using Microsoft.AspNetCore.Mvc;
using CommentAPI.Models;
using CommentAPI.Models.DTOs;
using CommentAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using CommentAPI.Services;
using CommentAPI.Data;

namespace CommentAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CommentsController : ControllerBase
{

    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    private static CommentDTO CommentToDto(Comment Comment)
    {
        var CommentDto = new CommentDTO
        {
            Id = Comment.Id,
            Content = Comment.Content,
            UserId= Comment.UserId,
            PostId = Comment.PostId
        };
        return CommentDto;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommentDTO>>> GetComments()
    {
        var comments = await _commentService.GetCommentsAsync();
        var commentDtos = comments.Select(CommentToDto).ToList();
        return Ok(commentDtos);
    }

    [HttpGet("post/{postId}")]
    public async Task<ActionResult<IEnumerable<CommentDTO>>> GetAllCommentsInPost(int postId)
    {
        var comments = await _commentService.GetAllCommentsInPostAsync(postId);
        var commentDtos = comments.Select(CommentToDto).ToList();
        return Ok(commentDtos);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var existingComment = await _commentService.GetCommentByIdAsync(id);
        if (existingComment == null)
        {
            return NotFound();
        }

        await _commentService.DeleteCommentAsync(id);

        return NoContent();
    }
}
