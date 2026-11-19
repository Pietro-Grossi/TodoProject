using System.ComponentModel.DataAnnotations;

namespace TodoProject.Dtos;

public class TodoStatusRequest()
{
    [Required]
    public bool? Status{get; init;} 
}
