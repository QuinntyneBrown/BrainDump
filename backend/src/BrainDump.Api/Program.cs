using BrainDump.Api.Middleware;
using BrainDump.Application.Features.Tree;
using BrainDump.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// OpenAPI (built-in for .NET 9)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// MediatR (scans the Application assembly)
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetTreeQuery).Assembly));

// Infrastructure (DbContext + IAppDbContext)
builder.Services.AddInfrastructure(builder.Configuration);

// JWT Bearer authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Jwt:Authority"];
        options.Audience = builder.Configuration["Jwt:Audience"];
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Exception handling middleware (NotFoundException -> 404, ValidationException -> 400)
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program accessible to integration tests via WebApplicationFactory<Program>
public partial class Program { }
