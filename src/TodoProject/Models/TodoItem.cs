namespace TodoProject;

public class TodoItem(int id, string title)
{
    public int Id { get; } = id;
    public string Title { get; private set; } = ValidateTitle(title);
    public bool IsCompleted { get; private set; }


    private static string ValidateTitle(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        if (title.Contains('\r') || title.Contains('\n'))
        {
            throw new ArgumentException("Il titolo deve stare su una sola riga.", nameof(title));
        }

        if (!title.Any(char.IsLetterOrDigit))
        {
            throw new ArgumentException(
                "Il titolo deve contenere almeno una lettera o una cifra.", nameof(title));
        }

        return title.Trim();
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
