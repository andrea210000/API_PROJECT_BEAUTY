using API_PROJECT_BEAUTY.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // La URL de tu frontend
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Get the connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container.
// Configure Entity Framework Core to use MySQL
builder.Services.AddDbContext<ProjectBeautyContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddControllers();
var mvcBuilder = builder.Services.AddControllers();

mvcBuilder.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddSwaggerGen(); // Add Swagger for API documentation
builder.Services.AddSwaggerGen(); // Add Swagger for API documentation
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(); // Add OpenAPI support for API documentation

// 🔐 Extraer configuración de JWT desde appsettings.json
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

// 🛡️ Registrar servicios de Autenticación
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization(); // Registrar servicios de autorización

var app = builder.Build(); // Build the application

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS

app.UseRouting();

app.UseCors("AngularClient");

// ⚠️ El orden importa de arriba a abajo:
app.UseAuthentication(); // 1. ¿Quién eres? (Valida el Token)
app.UseAuthorization(); // 2. ¿Tienes permiso de entrar aquí?


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Map OpenAPI endpoints for API documentation
    app.UseSwagger(); //  Enable Swagger middleware for API documentation
    app.UseSwaggerUI(); // Enable Swagger UI for interactive API documentation

}

app.MapControllers(); // Map controller routes for API endpoints

app.Run(); // Run the application
