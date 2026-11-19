using TodoProject.Models;
using TodoProject.Dtos;

namespace TodoProject.Services;

public class TodoManager
{
    // readonly: il campo può essere assegnato solo durante la dichiarazione o dal costruttore
    private readonly List<TodoItem> _todos = [
        new TodoItem(1, "Contattare il commercialista per spiegazioni"),
        new TodoItem(2, "Andare in palestra e chiedere al pt la nuova scheda"),
        new TodoItem(3, "Ricordarsi di fare gli auguri di anniversario ai genitori"),
    ];
    private int _nextId;
    private readonly object _syncRoot = new();

    public TodoManager()
    {
        // se la lista fosse vuota solleva un eccezione
        _nextId = _todos.Max( task => task.Id ) + 1;
        // Il preset include un task completato per provare filtri ed eliminazione.
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
            // potrebbe contenere il task o null: uso ?
            TodoItem? item = _todos.FirstOrDefault(task => task.Id == id);

            if(item is not null && item.IsCompleted)
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

            if(item is not null)
            {
                // validazione stringa nel metodo Rename, in caso contrario solleva un eccezione e il titolo rimane invariato
                item.Rename(newTitle);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public IReadOnlyList<TodoItemResponse> GetTodoItems(bool? isCompleted = null,string? search = null)
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
        lock(_syncRoot)
        {
            TodoItem? item = _todos.FirstOrDefault( task => task.Id == id);
            if(item is not null)
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
