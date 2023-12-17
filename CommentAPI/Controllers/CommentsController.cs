using Microsoft.AspNetCore.Mvc;
using CommentAPI.Models;
using CommentAPI.Models.DTOs;
using CommentAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using CommentAPI.Services;
using CommentAPI.Data;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace CommentAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{

    private readonly ICommentService _commentService;
    private string GetCurrentUserId()
    {
        return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    }

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
    public async Task<ActionResult<IEnumerable<CommentDTO>>> GetAllCommentsAsync()
    {
        var comments = await _commentService.GetAllCommentsAsync();
        var commentDtos = comments.Select(CommentToDto).ToList();
        return Ok(commentDtos);
    }

    [HttpGet("post/{postId:long}")]
    public async Task<ActionResult<IEnumerable<CommentDTO>>> GetAllCommentsInPost(long postId)
    {
        var comments = await _commentService.GetAllCommentsInPostAsync(postId);
        var commentDtos = comments.Select(CommentToDto).ToList();
        return Ok(commentDtos);
    }

    [Authorize(Roles = "User, Admin")]
    [HttpPost("post/{postId}")]
    public async Task<IActionResult> CreateCommentAsync(long postId, [FromBody] CreateCommentDTO createCommentDTO)
    {
        if (string.IsNullOrEmpty(createCommentDTO.Content) || postId <= 0)
        {
            return BadRequest();
        }

        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var comment = new Comment
        {
            Content = createCommentDTO.Content,
            UserId = userId,
            PostId = postId
        };

        var createdComment = await _commentService.CreateCommentAsync(comment);
        if (createdComment == null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        var createdCommentDto = CommentToDto(createdComment);
        return Ok(createdCommentDto);
    }

    // PUT


    //DELETE api/Comments/3
    [Authorize(Roles = "User, Admin")]
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteComment(long id)
    {
        var existingComment = await _commentService.GetCommentByIdAsync(id);
        if (existingComment == null)
        {
            return NotFound();
        }

        var currentUserId = GetCurrentUserId();


        if (existingComment.UserId != currentUserId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        await _commentService.DeleteCommentAsync(id);

        return NoContent();
    }
}