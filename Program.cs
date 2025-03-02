using System.Text;
using System.Text.Json.Serialization;
using FinalMarzo.net.Data;
using FinalMarzo.net.Middlewares;
using FinalMarzo.net.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// Crear el builder
var builder = WebApplication.CreateBuilder(args);

// Configurar la cadena de conexión a la base de datos
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 21))
    )
);

// Configurar HTTPS redirection indicando explícitamente el puerto HTTPS (7206)
builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 7206;
});

// Configurar JWT
var secretKey = builder.Configuration["TokenAuthentication:SecretKey"];
if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("TokenAuthentication:SecretKey is not configured.");
}
var key = Encoding.ASCII.GetBytes(secretKey);
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = builder.Configuration["TokenAuthentication:Issuer"],
            ValidAudience = builder.Configuration["TokenAuthentication:Audience"],
            ValidateLifetime = true,
        };
    });

// Habilitar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAllOrigins",
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
    );
});

// Configurar JSON para manejar ciclos de referencia
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Configuración de Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Parking Management API", Version = "v1" });
});

// Agregar servicios personalizados
builder.Services.AddScoped<TokenService>();
var salt = builder.Configuration["Salt"];
if (string.IsNullOrEmpty(salt))
{
    throw new InvalidOperationException("Salt is not configured.");
}
builder.Services.AddScoped<PasswordService>(_ => new PasswordService(salt));
var smtpHost = builder.Configuration["SMTP_Host"];
if (string.IsNullOrEmpty(smtpHost))
{
    throw new InvalidOperationException("SMTP_Host is not configured.");
}
var smtpUser = builder.Configuration["SMTP_User"];
if (string.IsNullOrEmpty(smtpUser))
{
    throw new InvalidOperationException("SMTP_User is not configured.");
}
var smtpPass = builder.Configuration["SMTP_Pass"];
if (string.IsNullOrEmpty(smtpPass))
{
    throw new InvalidOperationException("SMTP_Pass is not configured.");
}

// Registrar EmailService usando IConfiguration y ILogger
builder.Services.AddScoped<EmailService>(sp => new EmailService(
    sp.GetRequiredService<IConfiguration>(),
    sp.GetRequiredService<ILogger<EmailService>>()
));

// Agregar el servicio de limpieza automática de reservas
builder.Services.AddHostedService<ReservaCleanupService>();

// Construir la aplicación
var app = builder.Build();

// Configuración para el entorno de desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Parking Management API V1");
    });
}

// Registrar Middlewares personalizados
app.UseMiddleware<ExceptionMiddleware>(); // Middleware para manejo de excepciones
app.UseMiddleware<LoggingMiddleware>(); // Middleware para logging

// Configurar el middleware
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAllOrigins");
app.UseAuthentication();
app.UseAuthorization();

// Mapear controladores
app.MapControllers();

// Endpoint básico (ejemplo de prueba)
var summaries = new[]
{
    "Freezing",
    "Bracing",
    "Chilly",
    "Cool",
    "Mild",
    "Warm",
    "Balmy",
    "Hot",
    "Sweltering",
    "Scorching",
};

app.MapGet(
        "/weatherforecast",
        () =>
        {
            var forecast = Enumerable
                .Range(1, 5)
                .Select(index => new WeatherForecast(
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();
            return forecast;
        }
    )
    .WithName("GetWeatherForecast")
    .WithOpenApi();

// Asegurar que la base de datos se cree y las migraciones se apliquen
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    dbContext.Database.Migrate();
}

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
