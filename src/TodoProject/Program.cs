using TodoProject.Services;

var builder = WebApplication.CreateBuilder(args);

// registrazione del Singleton
builder.Services.AddSingleton<TodoManager>();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
