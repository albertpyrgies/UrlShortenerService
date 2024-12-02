using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using UrlShortenerService.Data;
using UrlShortenerService.Repository;
using UrlShortenerService.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Logging configuration
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Check configuration to determine which URL shortener service to use
bool useDatabase = builder.Configuration.GetValue<bool>("UseDatabase");
//builder.Services.AddDbContext<AppDbContext>(options =>
//        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
if (useDatabase)
{
    // Register DbContext and the database service
    
    builder.Services.AddScoped<IUrlShortenerService, DbUrlShortenerService>();
}
else
{
    // Register the ciphered URL shortener service
    builder.Services.AddScoped<IUrlShortenerService, CipheredUrlShortenerService>(provider =>
    {
        var configuration = provider.GetRequiredService<IConfiguration>();
        var encryptionKey = configuration["EncryptionSettings:EncryptionKey"];
        return new CipheredUrlShortenerService(encryptionKey);
    });
}

// Register Swagger services
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "URL Shortener API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "URL Shortener API V1");
        c.RoutePrefix = "swagger"; // Set the UI at the app's root
    });
}

// Ensure HTTPS redirection is configured before endpoints
app.UseHttpsRedirection();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

// Uncomment if you want to define a minimal API endpoint
// app.MapPost("/api/urls", async (UrlMapping urlMapping, AppDbContext db) =>
// { ... 

app.Run();

