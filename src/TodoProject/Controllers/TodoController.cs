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
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TodoItemResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<TodoItemResponse> GetTodoItem(
            [FromRoute] int id
        )
        {
            var task = manager.GetTodoItem(id);

            if (task is null)
            {
                return NotFound();
            }

            return Ok(task);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<TodoItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<IReadOnlyList<TodoItemResponse>> GetTodoItems(
            [FromQuery] bool? isCompleted = null,
            [FromQuery] string? search = null)
        {
            var tasks = manager.GetTodoItems(isCompleted, search);

            return Ok(tasks);
        }

        [HttpPost]
        [ProducesResponseType(typeof(TodoItemResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<TodoItemResponse> CreateTodoItem(
            [FromBody] TodoTitleRequest request
        )
        {
            try
            {
                TodoItemResponse response = manager.AddTodoItem(request.Title);
                return StatusCode(StatusCodes.Status201Created, response);
            }
            catch (ArgumentException ex) when (ex.ParamName == "title")
            {
                ModelState.AddModelError(nameof(TodoTitleRequest.Title), ex.Message);
                return ValidationProblem(ModelState);
            }
        }


        [HttpPatch("{id:int}/title")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
            catch (ArgumentException ex) when (ex.ParamName == "title")
            {
                ModelState.AddModelError(nameof(TodoTitleRequest.Title), ex.Message);
                return ValidationProblem(ModelState);
            }
        }

        [HttpPatch("{id:int}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult SetStatusTodoItem(
            [FromRoute] int id,
            [FromBody] TodoStatusRequest status
        )
        {
            bool updated = manager.SetTodoCompletion(id, status.IsCompleted!.Value);

            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }


        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult DeleteTodoItem([FromRoute] int id)
        {
            RemoveTodoResult result = manager.RemoveTodoItem(id);

            switch (result)
            {
                case RemoveTodoResult.NotCompleted:
                    // conflict ritorna error 409: conflitto con lo stato del task che impedisce l'eliminazione
                    return Conflict();

                case RemoveTodoResult.NotFound:
                    return NotFound();

                case RemoveTodoResult.Removed:
                    return NoContent();

                default:
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
