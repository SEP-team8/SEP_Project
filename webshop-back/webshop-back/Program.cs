using AutoMapper;
using Microsoft.EntityFrameworkCore;
using webshop_back.Data;
using webshop_back.Data.Mapping;
using webshop_back.Data.Seed; // DbInitializer (ako želiš automatski seed)
using webshop_back.Helpers;
using webshop_back.Service;
using webshop_back.Service.Interfaces;
using webshop_back.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

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

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)
        ),

        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = ClaimTypes.Role
    };
});


// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<TokenProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();


// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

// DbContext - koristi SQL Server (DefaultConnection iz appsettings.json)
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(conn))
{
    // fallback to in-memory for dev if not provided (optional)
    //builder.Services.AddDbContext<AppDbContext>(opts => opts.UseInMemoryDatabase("webshop_dev_db"));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(opts => opts.UseSqlServer(conn));
}

// Repository: EF implementation (scoped)
builder.Services.AddScoped<IRepository, EfRepository>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Use swagger in dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// serve wwwroot (images etc.)
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.UseAuthorization();
app.MapControllers();

// Optional: seed DB on startup (only for dev/test)
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var db = sp.GetRequiredService<AppDbContext>();
    // apply migrations (if you prefer automatic)
    db.Database.Migrate();

    // seed initial data if empty
    DbInitializer.Seed(db);
}

app.Run();
