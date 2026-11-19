using System.ComponentModel.DataAnnotations;

namespace TodoProject.Dtos;

public class TodoTitleRequest()
{
    [Required]
    // init permette di valorizzare la proprietà dal JSON durante la creazione,
    // poi nega la possibilità di riassegnare il campo
    public string Title{get; init;} = string.Empty;
}
