using TodoProject.Models;

namespace TodoProject.Tests;

public class TodoItemTest
{
    [Fact]
    public void Costruttore_ConTitoloValido_CreaUnTaskAperto()
    {
        var task = new TodoItem(42, "  Comprare il pane  ");

        Assert.Equal(42, task.Id);
        Assert.Equal("Comprare il pane", task.Title);
        Assert.False(task.IsCompleted);
    }

    [Fact]
    public void Costruttore_ConTitoloNullo_RifiutaLaCreazione()
    {
        Assert.Throws<ArgumentNullException>(() => new TodoItem(1, null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("!!!")]
    [InlineData("Prima\nSeconda")]
    [InlineData("Prima\rSeconda")]
    public void Costruttore_ConTitoloInvalido_RifiutaLaCreazione(string title)
    {
        Assert.Throws<ArgumentException>(() => new TodoItem(1, title));
    }

    [Fact]
    public void Rename_ConTitoloValido_SalvaIlTitoloSenzaSpaziAiBordi()
    {
        var task = new TodoItem(1, "Comprare il pane");

        task.Rename("  Preparare il colloquio  ");

        Assert.Equal("Preparare il colloquio", task.Title);
    }

    [Fact]
    public void Rename_ConTitoloInvalido_MantieneIlTitoloOriginale()
    {
        var task = new TodoItem(1, "Comprare il pane");

        Assert.Throws<ArgumentException>(() => task.Rename("!!!"));

        Assert.Equal("Comprare il pane", task.Title);
    }

    [Fact]
    public void SetCompletion_PermetteDiCompletareERiaprireIlTask()
    {
        var task = new TodoItem(1, "Comprare il pane");

        task.SetCompletion(true);
        Assert.True(task.IsCompleted);

        task.SetCompletion(false);
        Assert.False(task.IsCompleted);
    }
}
