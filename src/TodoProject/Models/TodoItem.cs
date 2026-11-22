namespace TodoProject.Models;

public class TodoItem(int id, string title)
{
    private const int MaxTitleLength = 200;

    public int Id { get; } = id;
    public string Title { get; private set; } = ValidateTitle(title);
    public bool IsCompleted { get; private set; }

    private static string ValidateTitle(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        title = title.Trim();

        if (title.Length > MaxTitleLength)
        {
            throw new ArgumentException(
                $"Il titolo può contenere al massimo {MaxTitleLength} caratteri.", nameof(title));
        }

        if (title.Any(character => char.IsControl(character)
            || character == '\u2028'
            || character == '\u2029'))
        {
            throw new ArgumentException(
                "Il titolo deve stare su una sola riga e non contenere caratteri di controllo.", nameof(title));
        }

        if (!title.Any(char.IsLetterOrDigit))
        {
            throw new ArgumentException(
                "Il titolo deve contenere almeno una lettera o una cifra.", nameof(title));
        }

        return title;
    }

    public void Rename(string newTitle)
    {
        Title = ValidateTitle(newTitle);
    }

    public void SetCompletion(bool isCompleted)
    {
        IsCompleted = isCompleted;
    }
}
