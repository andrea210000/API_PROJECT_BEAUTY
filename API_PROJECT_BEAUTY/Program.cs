using API_PROJECT_BEAUTY.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Get the connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container.
// Configure Entity Framework Core to use MySQL
builder.Services.AddDbContext<ProjectBeautyContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddControllers(); // Enable controllers for API endpoints
builder.Services.AddSwaggerGen(); // Add Swagger for API documentation
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(); // Add OpenAPI support for API documentation

var app = builder.Build(); // Build the application

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Map OpenAPI endpoints for API documentation
    app.UseSwagger(); //  Enable Swagger middleware for API documentation
    app.UseSwaggerUI(); // Enable Swagger UI for interactive API documentation

}

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS

app.UseAuthorization(); // Enable authorization middleware

app.MapControllers(); // Map controller routes for API endpoints

app.Run(); // Run the application
