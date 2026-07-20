using Api.Common;
using Api.Configurations;
using Api.Data;
using Api.Middlewares;
using Api.Services;
using Api.Services.Interfaces;
using Api.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Bases de datos y herramientas de .NET

builder.Services.AddExceptionHandler<GlobalExceptionHandler>(); // Manejador de errores global

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PortafolioDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention()
);

builder.Services.AddCustomCors(builder.Configuration);
builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ValidacionFilter>();
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var todosLosErrores = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            string mensajeDetalle = "Error de validación en la petición.";

            bool esErrorDeJson = todosLosErrores.Any(e =>
                e.Contains("could not be converted") ||
                e.Contains("JSON value") ||
                e.Contains("deserialized"));

            if (esErrorDeJson)
            {
                mensajeDetalle = "Uno o más campos tienen un tipo de dato incorrecto (ej. un número donde se esperaba texto).";
            }
            else if (todosLosErrores.Any(e => e.Contains("is required")))
            {
                mensajeDetalle = "Faltan campos obligatorios en la petición o el cuerpo está vacío.";
            }
            else if (todosLosErrores.Count > 0)
            {
                mensajeDetalle = todosLosErrores[0];
            }

            var respuestaPersonalizada = ApiResponse<object>.Fail(mensajeDetalle);

            return new BadRequestObjectResult(respuestaPersonalizada);
        };
    });

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

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(
            ApiResponse<object>.Fail("Demasiados intentos. Vuelve a intentarlo en unos momentos."),
            cancellationToken);
    };
});

// 3. Servicios: Contratos (Interfaces), Implementaciones y Validadores
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IComboService, ComboService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<IFileService, FileService>();

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
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference(options => // Levanta la UI interactiva en /scalar/v1
    {
        options.WithTitle("Portafolio Backend - API 1.0.0");
        options.WithTheme(ScalarTheme.DeepSpace);
    }).AllowAnonymous();
}

app.UseExceptionHandler(_ => { });

app.UseHttpsRedirection();

// Orden de los middlewares de CORS, autenticación y autorización
app.UseCustomCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Endpoint raíz: estado del API (excluido de la documentación OpenAPI).
var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
app.MapGet("/", () => Results.Ok(new
{
    nombre = "Portafolio Backend API",
    estado = "OK",
    version,
    entorno = app.Environment.EnvironmentName,
    timestampUtc = DateTime.UtcNow,
    documentacion = app.Environment.IsDevelopment()
        ? new { scalar = "/scalar/v1", openapi = "/openapi/v1.json" }
        : null
})).ExcludeFromDescription().AllowAnonymous();

app.Run();