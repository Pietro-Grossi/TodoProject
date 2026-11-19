namespace TodoProject.Dtos;

public class TodoItemResponse(int id, string title, bool isCompleted)
{
    public int Id { get; } = id;
    public string Title { get; } = title;
    public bool IsCompleted { get; } = isCompleted;
}