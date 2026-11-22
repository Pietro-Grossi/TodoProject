using System.ComponentModel.DataAnnotations;

namespace TodoProject.Dtos;

public class TodoTitleRequest()
{
    [Required(ErrorMessage = "Il titolo è obbligatorio.")]
    public string Title { get; init; } = string.Empty;
}
