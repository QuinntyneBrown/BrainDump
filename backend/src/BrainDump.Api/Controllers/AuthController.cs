using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BrainDump.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _config;

    public AuthController(IMemoryCache cache, IConfiguration config)
    {
        _cache = cache;
        _config = config;
    }

    public record AuthorizeRequest(
        string Email,
        string Password,
        string CodeChallenge,
        string CodeChallengeMethod);

    public record AuthorizeResponse(string Code);

    public record TokenRequest(
        string Code,
        string CodeVerifier,
        string GrantType);

    public record TokenResponse(
        string AccessToken,
        string TokenType,
        int ExpiresIn);

    private record PendingAuth(string Email, string CodeChallenge);

    [HttpPost("authorize")]
    public ActionResult<AuthorizeResponse> Authorize([FromBody] AuthorizeRequest request)
    {
        // L2-031 #5: when local sign-in is disabled, the endpoint must be absent
        // (404), not return 401, so callers can tell "feature off" from "wrong creds".
        if (!_config.GetValue<bool>("Jwt:UseLocalAuth")) return NotFound();

        var devEmail = _config["Jwt:LocalAuth:DevEmail"];
        var devPassword = _config["Jwt:LocalAuth:DevPassword"];

        if (!string.Equals(request.Email, devEmail, StringComparison.OrdinalIgnoreCase)
            || request.Password != devPassword)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        if (!string.Equals(request.CodeChallengeMethod, "S256", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Only S256 code_challenge_method is supported." });

        var code = Guid.NewGuid().ToString("N");
        _cache.Set(code, new PendingAuth(request.Email, request.CodeChallenge), TimeSpan.FromMinutes(5));

        return Ok(new AuthorizeResponse(code));
    }

    [HttpPost("token")]
    public ActionResult<TokenResponse> Token([FromBody] TokenRequest request)
    {
        if (!_config.GetValue<bool>("Jwt:UseLocalAuth")) return NotFound();

        if (!string.Equals(request.GrantType, "authorization_code", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Unsupported grant_type." });

        if (!_cache.TryGetValue<PendingAuth>(request.Code, out var pending) || pending is null)
            return Unauthorized(new { message = "Invalid or expired code." });

        _cache.Remove(request.Code);

        var computed = Base64UrlEncode(SHA256.HashData(Encoding.ASCII.GetBytes(request.CodeVerifier)));
        if (computed != pending.CodeChallenge)
            return Unauthorized(new { message = "Code verifier does not match." });

        return Ok(new TokenResponse(GenerateJwt(pending.Email), "Bearer", 3600));
    }

    private string GenerateJwt(string email)
    {
        var signingKey = _config["Jwt:LocalAuth:SigningKey"]!;
        var issuer = _config["Jwt:LocalAuth:Issuer"]!;
        var audience = _config["Jwt:LocalAuth:Audience"]!;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: [
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, email)
            ],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string Base64UrlEncode(byte[] bytes)
        => Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
}
