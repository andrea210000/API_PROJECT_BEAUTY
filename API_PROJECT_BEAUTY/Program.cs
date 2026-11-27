using API_PROJECT_BEAUTY.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy.AllowAnyOrigin()//permite que lleguen peticiones http de cualquier origen, pero pues con nuestra cookie se comprueba ese http para hacerlo o no
       .AllowAnyHeader()
      .AllowAnyMethod();

    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection en appsettings.json.");
}

var connectionBuilder = new MySqlConnectionStringBuilder(connectionString)
{
    Pooling = true,
    MinimumPoolSize = 1,
    MaximumPoolSize = 1,
    ConnectionIdleTimeout = 180,
    Keepalive = 60,
    ApplicationName = "BeautySpaApi"
};

connectionString = connectionBuilder.ConnectionString;
var serverVersionText = builder.Configuration["Database:ServerVersion"] ?? "8.0.45-mysql";
var serverVersion = ServerVersion.Parse(serverVersionText);

builder.Services.AddDbContext<ProjectBeautyContext>(options =>
    options.UseMySql(connectionString, serverVersion));

builder.Services.AddControllers();
var mvcBuilder = builder.Services.AddControllers();

mvcBuilder.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

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

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ProjectBeautyContext>();
    await context.Database.OpenConnectionAsync();
    await context.Database.CloseConnectionAsync();
}

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

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
