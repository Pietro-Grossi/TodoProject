using TodoProject.Models;
using TodoProject.Dtos;

namespace TodoProject.Services;

public class TodoManager
{
    private readonly List<TodoItem> _todos = [
        new TodoItem(1, "Restituire i libri in biblioteca"),
        new TodoItem(2, "Rinnovare la tessera della biblioteca"),
        new TodoItem(3, "Ritirare il pacco in posta"),
        new TodoItem(4, "Prenotare la revisione dell'auto"),
    ];
    private int _nextId;
    private readonly object _syncRoot = new();

    public TodoManager()
    {
        _nextId = _todos.Max(task => task.Id) + 1;
        _todos[1].SetCompletion(true);
    }

    public TodoItemResponse AddTodoItem(string title)
    {
        lock (_syncRoot)
        {
            TodoItem item = new TodoItem(_nextId, title);
            _todos.Add(item);
            _nextId++;

            return new TodoItemResponse(item.Id, item.Title, item.IsCompleted);
        }
    }

    public RemoveTodoResult RemoveTodoItem(int id)
    {
        lock (_syncRoot)
        {
            TodoItem? item = _todos.FirstOrDefault(task => task.Id == id);

            if (item is not null && item.IsCompleted)
            {
                _todos.Remove(item);
                return RemoveTodoResult.Removed;
            }
            else if (item is not null && !item.IsCompleted)
            {
                return RemoveTodoResult.NotCompleted;
            }
            else
            {
                return RemoveTodoResult.NotFound;
            }
        }
    }

    public bool RenameTodoItem(int id, string newTitle)
    {
        lock (_syncRoot)
        {
            TodoItem? item = _todos.FirstOrDefault(task => task.Id == id);

            if (item is not null)
            {
                item.Rename(newTitle);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public TodoItemResponse? GetTodoItem(int id)
    {
        lock (_syncRoot)
        {
            var item = _todos.Find(task => task.Id == id);

            if (item is null)
            {
                return null;
            }

            return new TodoItemResponse(item.Id, item.Title, item.IsCompleted);
        }
    }

    public IReadOnlyList<TodoItemResponse> GetTodoItems(bool? isCompleted = null, string? search = null)
    {
        lock (_syncRoot)
        {
            IEnumerable<TodoItem> tasks = _todos;

            if (isCompleted.HasValue)
            {
                tasks = tasks.Where(task => task.IsCompleted == isCompleted.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchText = search.Trim();

                tasks = tasks.Where(task =>
                    task.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            return tasks
                .Select(task => new TodoItemResponse(
                    task.Id,
                    task.Title,
                    task.IsCompleted))
                .ToList()
                .AsReadOnly();
        }
    }

    public bool SetTodoCompletion(int id, bool isCompleted)
    {
        lock (_syncRoot)
        {
            TodoItem? item = _todos.FirstOrDefault(task => task.Id == id);

            if (item is not null)
            {
                item.SetCompletion(isCompleted);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public int RemoveCompletedTodoItems()
    {
        lock (_syncRoot)
        {
            return _todos.RemoveAll(task => task.IsCompleted);
        }
    }
}
