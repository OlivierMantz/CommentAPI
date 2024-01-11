using Microsoft.AspNetCore.Mvc;
using CommentAPI.Models;
using CommentAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using CommentAPI.Services;
using System.Security.Claims;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CommentAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(ICommentService commentService, ILogger<CommentsController> logger)
        {
            _commentService = commentService;
            _logger = logger;
        }

        private string GetCurrentUserId()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                throw new InvalidOperationException("User ID not found in the current user's claims.");
            }
            return userId;
        }

        private static CommentDTO CommentToDto(Comment comment)
        {
            return new CommentDTO
            {
                Id = comment.Id,
                Content = comment.Content,
                AuthorId = comment.AuthorId,
                PostId = comment.PostId
            };
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentDTO>>> GetAllCommentsAsync()
        {
            try
            {
                var comments = await _commentService.GetAllCommentsAsync();
                var commentDtos = comments.Select(CommentToDto).ToList();
                return Ok(commentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all comments.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpGet("post/{postId:Guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CommentDTO>>> GetAllCommentsInPost(Guid postId)
        {
            try
            {
                var comments = await _commentService.GetAllCommentsInPostAsync(postId);
                var commentDtos = comments.Select(CommentToDto).ToList();
                return Ok(commentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all comments in post with Id of: {postId}", postId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPost("post/{postId:guid}")]
        public async Task<IActionResult> CreateCommentAsync(Guid postId, [FromBody] CreateCommentDTO createCommentDTO)
        {
            try
            {
                var authorId = GetCurrentUserId();
                if (string.IsNullOrEmpty(authorId))
                {
                    return Unauthorized();
                }

                var createdComment = await _commentService.CreateCommentAsync(postId, createCommentDTO, authorId);
                var createdCommentDto = CommentToDto(createdComment);

                return Ok(createdCommentDto);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid data provided.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a comment.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> UpdateCommentAsync(Guid id, [FromBody] CreateCommentDTO updatedCommentDTO)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized();
                }

                bool success = await _commentService.UpdateCommentAsync(id, updatedCommentDTO, currentUserId);
                if (!success)
                {
                    return NotFound("Comment not found or update failed.");
                }

                return Ok("Comment updated successfully.");
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating comment with commentId: {CommentId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [Authorize(Roles = "User, Admin")]
        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> DeleteComment(Guid id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized();
                }

                await _commentService.DeleteCommentAsync(id, currentUserId);

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting comment with commentId: {CommentId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
    }
}
