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
