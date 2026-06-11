using BankAPI.Context;
using BankAPI.Helpers.HmacValidator;
using BankAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configure simple file logger - logs to Desktop
var logFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "BankAPI-Logs");
var fileLoggerProvider = new BankAPI.Logging.FileLoggerProvider(logFolder);

builder.Logging.ClearProviders();
builder.Logging.AddProvider(fileLoggerProvider);

// Register FileLoggerProvider in DI container
builder.Services.AddSingleton(fileLoggerProvider);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactLocalhost", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<BankingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddScoped<IHmacValidator, HmacValidator>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddDataProtection();
builder.Services.AddSingleton<BankAPI.Services.CardProtector.ICardProtector, BankAPI.Services.CardProtector.CardProtector>();

builder.Services.AddHttpClient<IPspClient, PspClient>(c =>
{
    c.BaseAddress = new Uri("https://localhost:5002"); // PSP API
});

// Configure Kestrel to load certificate if configured
var certPath = builder.Configuration["Kestrel:Certificates:Default:Path"];
var certPassword = builder.Configuration["Kestrel:Certificates:Default:Password"];
if (!string.IsNullOrEmpty(certPath) && File.Exists(certPath))
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ConfigureHttpsDefaults(https =>
        {
            https.ServerCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(certPath, certPassword);
        });
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("ReactLocalhost");

app.UseHttpsRedirection();

// Simple correlation id middleware
app.Use(async (context, next) =>
{
    if (!context.Request.Headers.ContainsKey("X-Correlation-ID"))
    {
        context.Request.Headers["X-Correlation-ID"] = Guid.NewGuid().ToString();
    }

    context.Response.Headers["X-Correlation-ID"] = context.Request.Headers["X-Correlation-ID"];
    {
        // Set provider's current scope for file logger to include correlation id
        var provider = app.Services.GetRequiredService<BankAPI.Logging.FileLoggerProvider>();
        provider.CurrentScope.Value = context.Request.Headers["X-Correlation-ID"].ToString();
        try
        {
            await next();
        }
        finally
        {
            provider.CurrentScope.Value = null;
        }
    }
});

app.UseAuthorization();

app.MapControllers();

// Apply any pending EF Core migrations and update the database at startup.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<BankingDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetService<ILogger<Program>>();
        logger?.LogError(ex, "An error occurred while migrating or initializing the database.");
        throw;
    }
}

app.Run();
