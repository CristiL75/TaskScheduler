using TaskManager.Api.Services;
using TaskManager.Shared.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();                    // ← CONTROLLERS pentru REST API
builder.Services.AddOpenApi();                        // ← SWAGGER/OpenAPI documentation

// Configurăm serviciile noastre prin DEPENDENCY INJECTION
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();  // ← RabbitMQ pentru mesaje async
builder.Services.AddScoped<ITaskApiService, TaskApiService>();       // ← Service principal care orchestrează gRPC + RabbitMQ

// Configurăm CORS pentru a permite cereri de la diferite servicii (Postman, frontend, etc.)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                               // ← SWAGGER UI în development
}

app.UseCors();                                      // ← Activează CORS
app.UseRouting();                                   // ← Activează routing-ul
app.MapControllers();                               // ← Mapează controller-ele (TasksController)

// Endpoint de verificare că API-ul funcționează (health check)
app.MapGet("/", () => "TaskManager API is running!");

// Rulează API-ul pe localhost:5000
app.Run("http://localhost:5000");
