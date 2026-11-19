using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using TodoProject.Dtos;

namespace TodoProject.Tests.Integration;

public class TodoApiTests : IDisposable
{
    // xUnit crea una nuova istanza della classe per ogni caso di test.
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public TodoApiTests()
    {
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Get_SenzaFiltri_RestituisceITaskDelPreset()
    {
        var tasks = await GetTodosAsync();

        Assert.Equal(3, tasks.Length);
        Assert.Contains(tasks, task => task.IsCompleted);
        Assert.Contains(tasks, task => !task.IsCompleted);
    }

    [Fact]
    public async Task Post_ConTitoloValido_CreaUnTaskVisibileNelGetSuccessivo()
    {
        var createdTask = await CreateTodoAsync("  Preparare il colloquio  ");

        Assert.True(createdTask.Id > 0);
        Assert.Equal("Preparare il colloquio", createdTask.Title);
        Assert.False(createdTask.IsCompleted);

        var tasks = await GetTodosAsync();
        var savedTask = Assert.Single(tasks, task => task.Id == createdTask.Id);
        Assert.Equal(createdTask.Title, savedTask.Title);
        Assert.Equal(createdTask.IsCompleted, savedTask.IsCompleted);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"title\":null}")]
    [InlineData("{\"title\":\"\"}")]
    [InlineData("{\"title\":\"   \"}")]
    [InlineData("{\"title\":\"!!!\"}")]
    [InlineData("{\"title\":\"Prima\\nSeconda\"}")]
    [InlineData("{invalid")]
    public async Task Post_ConRichiestaInvalida_RestituisceBadRequestSenzaCreareTask(string json)
    {
        var initialTasks = await GetTodosAsync();
        using var body = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _client.PostAsync("/api/tasks", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var currentTasks = await GetTodosAsync();
        Assert.Equal(initialTasks.Select(task => task.Id), currentTasks.Select(task => task.Id));
    }

    [Theory]
    [InlineData("search=%20RIPASSARE%20", new[] { "Ripassare C#", "Ripassare SQL" })]
    [InlineData("isCompleted=true&search=RIPASSARE", new[] { "Ripassare C#" })]
    [InlineData("isCompleted=false&search=RIPASSARE", new[] { "Ripassare SQL" })]
    [InlineData("isCompleted=true&search=SQL", new string[] { })]
    public async Task Get_ConFiltri_RestituisceSoloITaskCorrispondenti(
        string query, string[] expectedTitles)
    {
        var completedTask = await CreateTodoAsync("Ripassare C#");
        await CreateTodoAsync("Ripassare SQL");
        using var completionResponse = await _client.PatchAsJsonAsync(
            $"/api/tasks/{completedTask.Id}/status", new { status = true });
        Assert.Equal(HttpStatusCode.NoContent, completionResponse.StatusCode);

        var tasks = await GetTodosAsync($"/api/tasks?{query}");

        Assert.Equal(expectedTitles, tasks.Select(task => task.Title));
    }

    [Fact]
    public async Task Get_ConFiltroStatoInvalido_RestituisceBadRequest()
    {
        using var response = await _client.GetAsync("/api/tasks?isCompleted=invalid");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PatchTitle_ConTitoloValido_AggiornaIlTaskERestituisceNoContent()
    {
        var createdTask = await CreateTodoAsync("Preparare il colloquio");

        using var response = await _client.PatchAsJsonAsync(
            $"/api/tasks/{createdTask.Id}/title", new { title = "  Ripassare C#  " });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
        var tasks = await GetTodosAsync();
        var updatedTask = Assert.Single(tasks, task => task.Id == createdTask.Id);
        Assert.Equal("Ripassare C#", updatedTask.Title);
        Assert.False(updatedTask.IsCompleted);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"title\":null}")]
    [InlineData("{\"title\":\"   \"}")]
    [InlineData("{\"title\":\"!!!\"}")]
    [InlineData("{\"title\":\"Prima\\nSeconda\"}")]
    public async Task PatchTitle_ConTitoloInvalido_RestituisceBadRequestEMantieneIlTitolo(string json)
    {
        var createdTask = await CreateTodoAsync("Preparare il colloquio");
        using var body = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _client.PatchAsync($"/api/tasks/{createdTask.Id}/title", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var tasks = await GetTodosAsync();
        var savedTask = Assert.Single(tasks, task => task.Id == createdTask.Id);
        Assert.Equal(createdTask.Title, savedTask.Title);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task PatchStatus_ConStatoEsplicito_CompletaORiapreIlTask(bool requestedState)
    {
        var createdTask = await CreateTodoAsync("Preparare il colloquio");
        string path = $"/api/tasks/{createdTask.Id}/status";
        using var initialResponse = await _client.PatchAsJsonAsync(path, new { status = true });
        Assert.Equal(HttpStatusCode.NoContent, initialResponse.StatusCode);

        using var response = await _client.PatchAsJsonAsync(path, new { status = requestedState });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
        var tasks = await GetTodosAsync();
        var updatedTask = Assert.Single(tasks, task => task.Id == createdTask.Id);
        Assert.Equal(requestedState, updatedTask.IsCompleted);
        Assert.Equal(createdTask.Title, updatedTask.Title);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"status\":null}")]
    [InlineData("{\"status\":\"true\"}")]
    public async Task PatchStatus_ConStatoMancanteOInvalido_RestituisceBadRequestSenzaModificareIlTask(
        string json)
    {
        var createdTask = await CreateTodoAsync("Preparare il colloquio");
        string path = $"/api/tasks/{createdTask.Id}/status";
        using var completionResponse = await _client.PatchAsJsonAsync(path, new { status = true });
        Assert.Equal(HttpStatusCode.NoContent, completionResponse.StatusCode);
        using var body = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _client.PatchAsync(path, body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var tasks = await GetTodosAsync();
        var savedTask = Assert.Single(tasks, task => task.Id == createdTask.Id);
        Assert.True(savedTask.IsCompleted);
    }

    [Theory]
    [InlineData("title", "{\"title\":\"Ripassare C#\"}")]
    [InlineData("status", "{\"status\":true}")]
    public async Task Patch_ConIdInesistente_RestituisceNotFound(string field, string json)
    {
        using var body = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _client.PatchAsync($"/api/tasks/{int.MaxValue}/{field}", body);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ConsenteLaRimozioneSoloDopoIlCompletamento()
    {
        var createdTask = await CreateTodoAsync("Preparare il colloquio");
        string path = $"/api/tasks/{createdTask.Id}";

        using var refusedResponse = await _client.DeleteAsync(path);
        Assert.Equal(HttpStatusCode.Conflict, refusedResponse.StatusCode);
        var tasksBeforeCompletion = await GetTodosAsync();
        Assert.Contains(tasksBeforeCompletion, task => task.Id == createdTask.Id);

        using var completionResponse = await _client.PatchAsJsonAsync(
            $"{path}/status", new { status = true });
        Assert.Equal(HttpStatusCode.NoContent, completionResponse.StatusCode);

        using var deletedResponse = await _client.DeleteAsync(path);
        Assert.Equal(HttpStatusCode.NoContent, deletedResponse.StatusCode);
        Assert.Equal(string.Empty, await deletedResponse.Content.ReadAsStringAsync());
        var remainingTasks = await GetTodosAsync();
        Assert.DoesNotContain(remainingTasks, task => task.Id == createdTask.Id);

        using var repeatedResponse = await _client.DeleteAsync(path);
        Assert.Equal(HttpStatusCode.NotFound, repeatedResponse.StatusCode);
    }

    private async Task<TodoItemResponse> CreateTodoAsync(string title)
    {
        using var response = await _client.PostAsJsonAsync("/api/tasks", new { title });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        return Assert.IsType<TodoItemResponse>(await response.Content.ReadFromJsonAsync<TodoItemResponse>());
    }

    private async Task<TodoItemResponse[]> GetTodosAsync(string path = "/api/tasks")
    {
        using var response = await _client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        return Assert.IsType<TodoItemResponse[]>(await response.Content.ReadFromJsonAsync<TodoItemResponse[]>());
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
