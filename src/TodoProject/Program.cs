using TodoProject.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TodoManager>();
builder.Services.AddControllers();
builder.Services.AddHostedService<TodoCleanupService>();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Todo API",
        Version = "v1"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();

// Rende il punto di ingresso accessibile ai test di integrazione.
public partial class Program { }
