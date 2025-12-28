using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using UniversityPaymentApi;
// using Microsoft.EntityFrameworkCore; // SQL kalktığı için buna gerek kalmadı

var builder = WebApplication.CreateBuilder(args);

// 1. CORS Ayarı (Tarayıcıdan erişim izni)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// 2. Firestore Anahtar Dosyası Yolu
string path = Path.Combine(Directory.GetCurrentDirectory(), "serviceAccountKey.json");
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

// DİKKAT: SQL Bağlantı kodunu sildim. Artık Firestore kullanıyoruz.

builder.Services.AddControllers();

// 3. Swagger Ayarları
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "University API",
        Version = "v1"
    });

    // Port sabitlemeyi kaldırdık, otomatik algılasın diye.
    
    // JWT için kilit simgesi ekleme
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 4. JWT Authentication Ayarları
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "VarsayilanUzunGizliAnahtar123!")
            ),
        };
    });

var app = builder.Build();

// 5. Middleware Sıralaması
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI();

// CORS
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();