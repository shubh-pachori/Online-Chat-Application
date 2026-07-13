using System;
using ChatApp.Api.Middlewares;
using ChatApp.Api.Hubs;
using ChatApp.Core.Interfaces;
using ChatApp.Infrastructure.Data;
using ChatApp.Infrastructure.Repositories;
using ChatApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Amazon.S3;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Configuration
builder.Services.AddDbContext<ChatAppContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dependency Injection
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<ITimeHelper, TimeHelper>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IS3Service, S3Service>();

// AWS S3
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();

// JWT Authentication
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"] ?? "super_secret_key_that_should_be_long_enough_for_hmac_sha256");
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy => policy
        .WithOrigins("http://localhost:5173") // Vite default
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<EmailExtractionMiddleware>();

app.MapControllers();
app.MapHub<ChatHub>("/chathub");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ChatAppContext>();
    context.Database.EnsureCreated();

    if (!context.Users.Any())
    {
        var timeHelper = scope.ServiceProvider.GetRequiredService<ITimeHelper>();
        var now = timeHelper.GetIstTime();

        context.Users.AddRange(
            new ChatApp.Core.Entities.User { Email = "admin@example.com", Username = "Admin", PasswordHash = "admin123", Role = "Admin", CreatedAt = now, UpdatedAt = now },
            new ChatApp.Core.Entities.User { Email = "user1@example.com", Username = "User One", PasswordHash = "user123", Role = "User", CreatedAt = now, UpdatedAt = now },
            new ChatApp.Core.Entities.User { Email = "dummy@example.com", Username = "Dummy", PasswordHash = "dummy123", Role = "User", CreatedAt = now, UpdatedAt = now }
        );
        context.SaveChanges();
    }
}

app.Run();
