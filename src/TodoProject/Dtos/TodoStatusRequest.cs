using System.ComponentModel.DataAnnotations;

namespace TodoProject.Dtos;

public class TodoStatusRequest()
{
    [Required]
    public bool? IsCompleted { get; init; }
}
