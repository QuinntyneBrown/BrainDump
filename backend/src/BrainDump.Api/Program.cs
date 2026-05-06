using BrainDump.Api.Auth;
using BrainDump.Api.Middleware;
using BrainDump.Application.Features.Tree;
using BrainDump.Application.Interfaces;
using BrainDump.Infrastructure;
using BrainDump.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Production-safety guard (L2-032): local sign-in must never be enabled outside
// the Development environment. Fail fast at startup before any request is served.
if (builder.Configuration.GetValue<bool>("Jwt:UseLocalAuth") && !builder.Environment.IsDevelopment())
{
    throw new InvalidOperationException(
        $"Jwt:UseLocalAuth is enabled but ASPNETCORE_ENVIRONMENT='{builder.Environment.EnvironmentName}'. " +
        "Local sign-in is for Development only — disable Jwt:UseLocalAuth or set the environment to Development.");
}

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetTreeQuery).Assembly));

builder.Services.AddInfrastructure(builder.Configuration);

// In-memory cache for PKCE authorization codes
builder.Services.AddMemoryCache();

// CORS – Angular dev server in development, configured allow-list in production.
var corsAllowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDev", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());

    options.AddPolicy("Production", policy =>
        policy.WithOrigins(corsAllowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// JWT Bearer authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        if (builder.Configuration.GetValue<bool>("Jwt:UseLocalAuth"))
        {
            // Development: validate against the symmetric key issued by AuthController
            var signingKey = builder.Configuration["Jwt:LocalAuth:SigningKey"]!;
            var issuer = builder.Configuration["Jwt:LocalAuth:Issuer"]!;
            var audience = builder.Configuration["Jwt:LocalAuth:Audience"]!;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        }
        else
        {
            options.Authority = builder.Configuration["Jwt:Authority"];
            options.Audience = builder.Configuration["Jwt:Audience"];
        }

        // Bootstrap a `user` row for every authenticated principal (local-auth and
        // Entra alike) so handlers can attribute work via ICurrentUser.
        options.Events ??= new JwtBearerEvents();
        options.Events.OnTokenValidated = UserBootstrapHandler.OnTokenValidated;
    });

builder.Services.AddAuthorization();

// Health checks: /healthz reflects DB connectivity so docker-compose readiness
// and L2-028 #2 ("GET /healthz returns 200 OK") have a real signal.
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors(app.Environment.IsDevelopment() ? "AngularDev" : "Production");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/healthz").AllowAnonymous();
app.MapControllers();

// First-deploy schema bootstrap. Replace with Database.MigrateAsync()
// once EF Core migrations are introduced.
if (app.Configuration.GetValue<bool>("Database:EnsureCreatedOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();

    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await LocalDevUserSeeder.SeedAsync(db, hasher, app.Configuration);
}

app.Run();

public partial class Program { }
