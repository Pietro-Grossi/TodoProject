using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TodoProject.Dtos;
using TodoProject.Services;

namespace TodoProject.Controllers
{
    [Route("api/tasks")]
    [ApiController]
    public class TodoController(TodoManager manager) : ControllerBase
    {

        [HttpGet]
        public ActionResult<IReadOnlyList<TodoItemResponse>> GetTodoItems(
            [FromQuery] bool? isCompleted = null,
            [FromQuery] string? search = null)
        {
            var tasks = manager.GetTodoItems(isCompleted, search);

            return Ok(tasks);
        }


        [HttpPost]
        public ActionResult<TodoItemResponse> CreateTodoItem(
            [FromBody] TodoTitleRequest request
        )
        {
            try
            {
                TodoItemResponse response = manager.AddTodoItem(request.Title);
                return StatusCode(StatusCodes.Status201Created, response);    
            }
            catch(ArgumentException exception)
            {
                return BadRequest(new{ error = exception.Message});
            }    
        }


        [HttpPatch("{id:int}/title")]
        public IActionResult RenameTodoItem(
            [FromRoute] int id,
            [FromBody] TodoTitleRequest title
        )
        {
            try
            {
                bool updated = manager.RenameTodoItem(id, title.Title);  

                if (!updated)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch(ArgumentException exception)
            {
                return BadRequest(new{error = exception.Message});
            }
        }


        [HttpPatch("{id:int}/status")]
        public IActionResult SetStatusTodoItem(
            [FromRoute] int id,
            [FromBody] TodoStatusRequest status
        ){
            bool updated = manager.SetTodoCompletion(id, status.Status.Value);

            if (!updated)
            {
                return NotFound();
            }
            return NoContent();
        }


        [HttpDelete("{id:int}")]
        public IActionResult DeleteTodoItem([FromRoute] int id)
        {
            RemoveTodoResult result = manager.RemoveTodoItem(id);

            switch (result)
            {
                case RemoveTodoResult.NotCompleted:
                    // conflict ritorna error 409: conflitto con lo stato del task che impedisce l'eliminazione
                    return Conflict(new
                    {
                        error = "È possibile eliminare solo i task completati."
                    });

                case RemoveTodoResult.NotFound:
                    // 404 dato che non si trova l'id
                    return NotFound();

                case RemoveTodoResult.Removed:
                    // 204 indica che la rimozione è avvenuta e non c'è nessun oggetto ritornato
                    return NoContent();

                default:
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }





    }
}
