using InvertedIndexSearchEngine.Server.Models;
using InvertedIndexSearchEngine.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1?? Add DbContext
builder.Services.AddDbContext<SearchDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2?? Register Services for Dependency Injection
builder.Services.AddScoped<IndexerService>();   // Handles indexing documents
builder.Services.AddScoped<SearchService>();    // Handles search logic

// 3?? Add controllers
builder.Services.AddControllers();

// 4?? Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 5?? Add CORS for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// 6?? Serve Angular static files
app.UseDefaultFiles();
app.UseStaticFiles();

// 7?? Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 8?? Apply CORS before authorization
app.UseCors("AllowAngular");

app.UseAuthorization();

// 9?? Map controllers
app.MapControllers();

// 10?? Fallback to Angular index.html
app.MapFallbackToFile("/index.html");

app.Run();
