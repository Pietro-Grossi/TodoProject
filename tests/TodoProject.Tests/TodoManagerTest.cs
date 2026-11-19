using TodoProject.Services;

namespace TodoProject.Tests;

public class TodoManagerTest
{
    [Fact]
    public void AddTodoItem_ConTitoloValido_IncrementaIlNumeroDiTask()
    {
        var manager = new TodoManager();
        int initialCount = manager.GetTodoItems().Count;

        var createdTask = manager.AddTodoItem("  Preparare il colloquio  ");

        var tasks = manager.GetTodoItems();
        Assert.Equal(initialCount + 1, tasks.Count);
        var savedTask = Assert.Single(tasks, task => task.Id == createdTask.Id);
        Assert.Equal("Preparare il colloquio", savedTask.Title);
        Assert.False(savedTask.IsCompleted);
    }

    [Fact]
    public void AddTodoItem_ConTitoloInvalido_LasciaInvariatoIlNumeroDiTask()
    {
        var manager = new TodoManager();
        int initialCount = manager.GetTodoItems().Count;

        Assert.Throws<ArgumentException>(() => manager.AddTodoItem("!!!"));

        Assert.Equal(initialCount, manager.GetTodoItems().Count);
    }

    [Fact]
    public void AddTodoItem_ConDueCreazioni_AssegnaIdProgressiviDopoIlPreset()
    {
        var manager = new TodoManager();
        int highestInitialId = manager.GetTodoItems().Max(task => task.Id);

        var firstTask = manager.AddTodoItem("Preparare il colloquio");
        var secondTask = manager.AddTodoItem("Ripassare C#");

        Assert.Equal(highestInitialId + 1, firstTask.Id);
        Assert.Equal(firstTask.Id + 1, secondTask.Id);
    }

    [Fact]
    public void AddTodoItem_DopoUnaCreazioneInvalida_NonConsumaUnId()
    {
        var manager = new TodoManager();
        var firstTask = manager.AddTodoItem("Preparare il colloquio");

        Assert.Throws<ArgumentException>(() => manager.AddTodoItem("!!!"));
        var secondTask = manager.AddTodoItem("Ripassare C#");

        Assert.Equal(firstTask.Id + 1, secondTask.Id);
    }

    [Fact]
    public void AddTodoItem_DopoUnaRimozione_NonRiutilizzaGliIdDelPreset()
    {
        var manager = new TodoManager();
        var initialTasks = manager.GetTodoItems();
        int highestInitialId = initialTasks.Max(task => task.Id);
        var completedTask = initialTasks.First(task => task.IsCompleted);

        manager.RemoveTodoItem(completedTask.Id);
        var createdTask = manager.AddTodoItem("Ripassare C#");

        Assert.Equal(highestInitialId + 1, createdTask.Id);
    }

    [Fact]
    public void AddTodoItem_ConCreazioniConcorrenti_ConservaTuttiITaskConIdUnivoci()
    {
        var manager = new TodoManager();
        int initialCount = manager.GetTodoItems().Count;
        const int numberOfTasks = 100;

        Parallel.For(0, numberOfTasks, index =>
        {
            manager.AddTodoItem($"Attività {index}");
        });

        var tasks = manager.GetTodoItems();
        Assert.Equal(initialCount + numberOfTasks, tasks.Count);
        Assert.Equal(tasks.Count, tasks.Select(task => task.Id).Distinct().Count());
    }

    [Fact]
    public void RemoveTodoItem_ConTaskCompletato_RimuoveSoloIlTaskRichiesto()
    {
        var manager = new TodoManager();
        var initialTasks = manager.GetTodoItems();
        var completedTask = initialTasks.First(task => task.IsCompleted);

        var result = manager.RemoveTodoItem(completedTask.Id);

        Assert.Equal(RemoveTodoResult.Removed, result);
        var remainingTasks = manager.GetTodoItems();
        Assert.Equal(
            initialTasks.Where(task => task.Id != completedTask.Id).Select(task => task.Id),
            remainingTasks.Select(task => task.Id));
    }

    [Fact]
    public void RemoveTodoItem_ConTaskAperto_RifiutaLaRimozioneEMantieneIlTask()
    {
        var manager = new TodoManager();
        var createdTask = manager.AddTodoItem("Preparare il colloquio");
        int initialCount = manager.GetTodoItems().Count;

        var result = manager.RemoveTodoItem(createdTask.Id);

        Assert.Equal(RemoveTodoResult.NotCompleted, result);
        var tasks = manager.GetTodoItems();
        Assert.Equal(initialCount, tasks.Count);
        Assert.Contains(tasks, task => task.Id == createdTask.Id);
    }

    [Fact]
    public void RemoveTodoItem_ConIdInesistente_RestituisceNotFoundELasciaLaListaInvariata()
    {
        var manager = new TodoManager();
        var initialTasks = manager.GetTodoItems();
        int missingId = initialTasks.Max(task => task.Id) + 1;

        var result = manager.RemoveTodoItem(missingId);

        Assert.Equal(RemoveTodoResult.NotFound, result);
        Assert.Equal(
            initialTasks.Select(task => task.Id),
            manager.GetTodoItems().Select(task => task.Id));
    }

    [Fact]
    public void RemoveTodoItem_ConTaskGiaRimosso_RestituisceNotFound()
    {
        var manager = new TodoManager();
        var completedTask = manager.GetTodoItems().First(task => task.IsCompleted);
        manager.RemoveTodoItem(completedTask.Id);

        var result = manager.RemoveTodoItem(completedTask.Id);

        Assert.Equal(RemoveTodoResult.NotFound, result);
    }

    [Fact]
    public void RenameTodoItem_ConTitoloValido_AggiornaSoloIlTaskRichiesto()
    {
        var manager = new TodoManager();
        var createdTask = manager.AddTodoItem("Preparare il colloquio");
        var otherTasks = manager.GetTodoItems()
            .Where(task => task.Id != createdTask.Id)
            .ToList();

        bool result = manager.RenameTodoItem(createdTask.Id, "  Ripassare C#  ");

        Assert.True(result);
        var tasks = manager.GetTodoItems();
        var renamedTask = Assert.Single(tasks, task => task.Id == createdTask.Id);
        Assert.Equal("Ripassare C#", renamedTask.Title);
        Assert.False(renamedTask.IsCompleted);
        foreach (var originalTask in otherTasks)
        {
            var currentTask = Assert.Single(tasks, task => task.Id == originalTask.Id);
            Assert.Equal(originalTask.Title, currentTask.Title);
            Assert.Equal(originalTask.IsCompleted, currentTask.IsCompleted);
        }
    }

    [Fact]
    public void RenameTodoItem_ConIdInesistente_RestituisceFalse()
    {
        var manager = new TodoManager();
        int missingId = manager.GetTodoItems().Max(task => task.Id) + 1;

        bool result = manager.RenameTodoItem(missingId, "Ripassare C#");

        Assert.False(result);
    }

    [Fact]
    public void RenameTodoItem_ConTitoloInvalido_MantieneIlTitoloSalvato()
    {
        var manager = new TodoManager();
        var createdTask = manager.AddTodoItem("Preparare il colloquio");

        Assert.Throws<ArgumentException>(() => manager.RenameTodoItem(createdTask.Id, "!!!"));

        var savedTask = Assert.Single(manager.GetTodoItems(), task => task.Id == createdTask.Id);
        Assert.Equal("Preparare il colloquio", savedTask.Title);
    }

    [Fact]
    public void GetTodoItems_DopoUnaRinomina_MantieneInvariataLaRispostaPrecedente()
    {
        var manager = new TodoManager();
        var createdTask = manager.AddTodoItem("Preparare il colloquio");
        var previousResponse = manager.GetTodoItems();

        manager.RenameTodoItem(createdTask.Id, "Ripassare C#");

        var previousTask = Assert.Single(previousResponse, task => task.Id == createdTask.Id);
        Assert.Equal("Preparare il colloquio", previousTask.Title);
        var currentTask = Assert.Single(manager.GetTodoItems(), task => task.Id == createdTask.Id);
        Assert.Equal("Ripassare C#", currentTask.Title);
    }

    [Fact]
    public void GetTodoItems_DopoUnaRimozione_MantieneInvariataLaRispostaPrecedente()
    {
        var manager = new TodoManager();
        var previousResponse = manager.GetTodoItems();
        var completedTask = previousResponse.First(task => task.IsCompleted);

        manager.RemoveTodoItem(completedTask.Id);

        Assert.Contains(previousResponse, task => task.Id == completedTask.Id);
        Assert.DoesNotContain(manager.GetTodoItems(), task => task.Id == completedTask.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  \t  ")]
    public void GetTodoItems_ConRicercaVuota_RestituisceTuttiITask(string? search)
    {
        var manager = new TodoManager();
        manager.AddTodoItem("Preparare il colloquio");
        var allTasks = manager.GetTodoItems();

        var result = manager.GetTodoItems(search: search);

        Assert.Equal(
            allTasks.Select(task => task.Id),
            result.Select(task => task.Id));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetTodoItems_ConFiltroStato_RestituisceSoloITaskNelloStatoRichiesto(bool isCompleted)
    {
        var manager = new TodoManager();
        var createdTask = manager.AddTodoItem("Preparare il colloquio");
        manager.SetTodoCompletion(createdTask.Id, isCompleted);
        int expectedCount = manager.GetTodoItems().Count(task => task.IsCompleted == isCompleted);

        var result = manager.GetTodoItems(isCompleted: isCompleted);

        Assert.Equal(expectedCount, result.Count);
        Assert.All(result, task => Assert.Equal(isCompleted, task.IsCompleted));
        Assert.Contains(result, task => task.Id == createdTask.Id);
    }

    [Theory]
    [InlineData(null, "  RIPASSARE  ", new[] { "Ripassare C#", "Ripassare SQL" })]
    [InlineData(true, "  RIPASSARE  ", new[] { "Ripassare C#" })]
    [InlineData(false, "  RIPASSARE  ", new[] { "Ripassare SQL" })]
    [InlineData(true, "ripassare sql", new string[] { })]
    public void GetTodoItems_ConRicercaEStato_ApplicaEntrambiIFiltri(
        bool? isCompleted, string search, string[] expectedTitles)
    {
        var manager = new TodoManager();
        var completedTask = manager.AddTodoItem("Ripassare C#");
        manager.SetTodoCompletion(completedTask.Id, true);
        manager.AddTodoItem("Ripassare SQL");
        manager.AddTodoItem("Preparare il colloquio");

        var result = manager.GetTodoItems(isCompleted, search);

        Assert.Equal(expectedTitles, result.Select(task => task.Title));
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void SetTodoCompletion_ConTaskEsistente_ImpostaLoStatoRichiesto(
        bool initialState, bool requestedState)
    {
        var manager = new TodoManager();
        var createdTask = manager.AddTodoItem("Preparare il colloquio");
        manager.SetTodoCompletion(createdTask.Id, initialState);

        bool result = manager.SetTodoCompletion(createdTask.Id, requestedState);

        Assert.True(result);
        var savedTask = Assert.Single(manager.GetTodoItems(), task => task.Id == createdTask.Id);
        Assert.Equal(requestedState, savedTask.IsCompleted);
        Assert.Equal(createdTask.Title, savedTask.Title);
    }

    [Fact]
    public void SetTodoCompletion_ConIdInesistente_RestituisceFalseELasciaITaskInvariati()
    {
        var manager = new TodoManager();
        var initialTasks = manager.GetTodoItems();
        int missingId = initialTasks.Max(task => task.Id) + 1;

        bool result = manager.SetTodoCompletion(missingId, true);

        Assert.False(result);
        Assert.Equal(
            initialTasks.Select(task => (task.Id, task.Title, task.IsCompleted)),
            manager.GetTodoItems().Select(task => (task.Id, task.Title, task.IsCompleted)));
    }

    [Fact]
    public void SetTodoCompletion_CompletandoUnTask_NeConsenteLaRimozione()
    {
        var manager = new TodoManager();
        var createdTask = manager.AddTodoItem("Preparare il colloquio");

        manager.SetTodoCompletion(createdTask.Id, true);
        var result = manager.RemoveTodoItem(createdTask.Id);

        Assert.Equal(RemoveTodoResult.Removed, result);
        Assert.DoesNotContain(manager.GetTodoItems(), task => task.Id == createdTask.Id);
    }

    [Fact]
    public void ManagerDistinti_ConUnAggiunta_MantengonoListeIndipendenti()
    {
        var firstManager = new TodoManager();
        var secondManager = new TodoManager();
        int secondManagerCount = secondManager.GetTodoItems().Count;

        var createdTask = firstManager.AddTodoItem("Preparare il colloquio");

        var secondManagerTasks = secondManager.GetTodoItems();
        Assert.Equal(secondManagerCount, secondManagerTasks.Count);
        Assert.DoesNotContain(secondManagerTasks, task => task.Id == createdTask.Id);
    }
}
