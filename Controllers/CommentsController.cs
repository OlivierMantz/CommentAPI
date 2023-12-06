using Microsoft.AspNetCore.Mvc;
using CommentAPI.Models;
using CommentAPI.Models.DTOs;
using CommentAPI.Repositories;

namespace CommentAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CommentsController : ControllerBase
{

    private readonly ICommentRepository _commentRepository;
    public CommentsController(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_commentRepository.GetCommentsAsync());
    }
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var commentDeleted = _commentRepository.DeleteComment(id);
        return commentDeleted ? NoContent() : NotFound();
    }
}
