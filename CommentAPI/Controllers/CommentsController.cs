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
            AuthorId = Comment.AuthorId,
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

    [HttpGet("post/{postId:Guid}")]
    public async Task<ActionResult<IEnumerable<CommentDTO>>> GetAllCommentsInPost(Guid postId)
    {
        var comments = await _commentService.GetAllCommentsInPostAsync(postId);
        var commentDtos = comments.Select(CommentToDto).ToList();
        return Ok(commentDtos);
    }

    [Authorize(Roles = "User, Admin")]
    [HttpPost("post/{postId:guid}")]
    public async Task<IActionResult> CreateCommentAsync(Guid postId, [FromBody] CreateCommentDTO createCommentDTO)
    {
        if (string.IsNullOrEmpty(createCommentDTO.Content))
        {
            return BadRequest();
        }

        var authorId = GetCurrentUserId();
        if (string.IsNullOrEmpty(authorId))
        {
            return Unauthorized();
        }

        var comment = new Comment
        {
            Content = createCommentDTO.Content,
            AuthorId = authorId,
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

    // PUT api/Comments/3
    [Authorize(Roles = "User, Admin")]
    [HttpPut("{id:Guid}")]
    public async Task<IActionResult> UpdateCommentAsync(Guid id, [FromBody] CreateCommentDTO updatedCommentDTO)
    {
        var existingComment = await _commentService.GetCommentByIdAsync(id);
        if (existingComment == null)
        {
            return NotFound();
        }

        var currentUserId = GetCurrentUserId();
        if (existingComment.AuthorId != currentUserId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        if (string.IsNullOrEmpty(updatedCommentDTO.Content))
        {
            return BadRequest("Comment content cannot be empty.");
        }

        existingComment.Content = updatedCommentDTO.Content;
        var success = await _commentService.PutCommentAsync(existingComment);
        if (!success)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        var updatedCommentDto = CommentToDto(existingComment);
        return Ok(updatedCommentDto);
    }


    //DELETE api/Comments/3
    [Authorize(Roles = "User, Admin")]
    [HttpDelete("{id:Guid}")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        var existingComment = await _commentService.GetCommentByIdAsync(id);
        if (existingComment == null)
        {
            return NotFound();
        }

        var currentAuthorId = GetCurrentUserId();


        if (existingComment.AuthorId != currentAuthorId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        await _commentService.DeleteCommentAsync(id);

        return NoContent();
    }
}