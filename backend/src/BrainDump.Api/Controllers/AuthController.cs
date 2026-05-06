using BrainDump.Application.Interfaces;
using BrainDump.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Globalization;
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
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;

    public AuthController(
        IMemoryCache cache,
        IConfiguration config,
        IUserRepository users,
        IPasswordHasher hasher)
    {
        _cache = cache;
        _config = config;
        _users = users;
        _hasher = hasher;
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
    public async Task<ActionResult<AuthorizeResponse>> Authorize(
        [FromBody] AuthorizeRequest request,
        CancellationToken ct)
    {
        // L2-031 #5: when local sign-in is disabled, the endpoint must be absent
        // (404), not return 401, so callers can tell "feature off" from "wrong creds".
        if (!_config.GetValue<bool>("Jwt:UseLocalAuth")) return NotFound();

        var user = await _users.FindByEmailAsync(request.Email, ct);
        if (user is null
            || user.PasswordHash is null
            || !_hasher.Verify(user.PasswordHash, request.Password))
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        if (!string.Equals(request.CodeChallengeMethod, "S256", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Only S256 code_challenge_method is supported." });

        var code = Guid.NewGuid().ToString("N");
        _cache.Set(code, new PendingAuth(user.Email, request.CodeChallenge), TimeSpan.FromMinutes(5));

        return Ok(new AuthorizeResponse(code));
    }

    [HttpPost("token")]
    public async Task<ActionResult<TokenResponse>> Token(
        [FromBody] TokenRequest request,
        CancellationToken ct)
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

        var user = await _users.FindByEmailAsync(pending.Email, ct);
        if (user is null) return Unauthorized(new { message = "Invalid or expired code." });

        return Ok(new TokenResponse(GenerateJwt(user), "Bearer", 3600));
    }

    private string GenerateJwt(User user)
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
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString(CultureInfo.InvariantCulture)),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture))
            ],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string Base64UrlEncode(byte[] bytes)
        => Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
}
