using CryptoService.Clients;
using CryptoService.Clients.Interfaces;
using CryptoService.HostedServices;
using CryptoService.Persistance;
using CryptoService.Services;
using CryptoService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configuration & Logging
builder.Services.AddOptions();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// DbContext
builder.Services.AddDbContext<CryptoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Http clients + Binance client
builder.Services.AddHttpClient(); // general
builder.Services.AddHttpClient<IBinanceClient, BinanceClient>();

// Services
builder.Services.AddScoped<ICryptoPaymentService, CryptoPaymentService>();

// Hosted background watcher
builder.Services.AddHostedService<PaymentWatcher>();

// Controllers
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
