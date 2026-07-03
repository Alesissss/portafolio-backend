using Api.Configurations;
using Api.Data;
using Api.Services;
using Api.Services.Interfaces;
using Api.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Bases de datos y herramientas de .NET
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PortafolioDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention()
);

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor(); // Clave para la auditoría posterior
builder.Services.AddOpenApi();

// 2. Seguridad JWT
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["Key"] ?? throw new InvalidOperationException("Falta la clave secreta 'Jwt:Key'.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // Conserva claims cortos como 'sub' y 'role'

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

// 3. Servicios: Contratos (Interfaces), Implementaciones y Validadores
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();


// Obliga a .NET a convertir todas las URL en minúscula
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// Con solo poner un Validator que hereda de AbstractValidator<T>, .NET implementa todos los Validators que heren de AbstractValidator<T> automáticamente
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

// 4. Construcción de la aplicación 
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // Levanta la UI interactiva en /scalar/v1
}

app.UseHttpsRedirection();

// Orden de los middlewares de autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();