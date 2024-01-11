using CommentAPI.Models.DTOs;
using CommentAPI.Models;
using CommentAPI.Repositories;
using CommentAPI.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;

    public CommentService(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<IEnumerable<Comment>> GetAllCommentsAsync()
    {
        return await _commentRepository.GetAllCommentsAsync();
    }

    public async Task<IEnumerable<Comment>> GetAllCommentsInPostAsync(Guid postId)
    {
        return await _commentRepository.GetAllCommentsInPostAsync(postId);
    }
    public async Task<Comment> GetCommentByIdAsync(Guid commentId)
    {
        return await _commentRepository.GetCommentByIdAsync(commentId);
    }

    public async Task<Comment> CreateCommentAsync(Guid postId, CreateCommentDTO createCommentDTO, string authorId)
    {
        if (string.IsNullOrEmpty(createCommentDTO?.Content) || createCommentDTO.Content.Length < 3)
        {
            throw new ArgumentException("Comment content cannot be empty or smaller than 3 characters long.");
        }

        var comment = new Comment
        {
            Content = createCommentDTO.Content,
            AuthorId = authorId,
            PostId = postId
        };

        return await _commentRepository.CreateCommentAsync(comment);
    }

    public async Task<bool> UpdateCommentAsync(Guid id, CreateCommentDTO updatedCommentDTO, string userId)
    {
        var existingComment = await _commentRepository.GetCommentByIdAsync(id);
        if (existingComment == null)
        {
            return false; 
        }

        if (existingComment.AuthorId != userId)
        {
            throw new UnauthorizedAccessException("User is not authorized to update this comment.");
        }

        if (string.IsNullOrEmpty(updatedCommentDTO?.Content))
        {
            throw new ArgumentException("Comment content cannot be empty.");
        }

        existingComment.Content = updatedCommentDTO.Content;
        return await _commentRepository.UpdateCommentAsync(existingComment);
    }

    public async Task<bool> DeleteCommentAsync(Guid id, string userId)
    {
        var comment = await _commentRepository.GetCommentByIdAsync(id);
        if (comment == null)
        {
            return false;
        }

        if (comment.AuthorId != userId)
        {
            throw new UnauthorizedAccessException("User is not authorized to delete this comment.");
        }

        return await _commentRepository.DeleteCommentAsync(id);
    }
}
