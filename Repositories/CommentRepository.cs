using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommentAPI.Models;
using CommentAPI.Data;
using Microsoft.EntityFrameworkCore;
using CommentAPI.Repositories;

namespace BackEnd.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private static List<Comment> _comments = new()
        {
            new ()
            {
                PostId = 1, UserId = 4, Content = "Nice post" 
            },
            new ()
            {
                PostId = 1, UserId = 2, Content = "Cool"
            },

        };

        public List<Comment> GetComments() => _comments;

        private readonly CommentContext _commentContext;

        public CommentRepository(CommentContext commentContext)
        {
            _commentContext = commentContext;
        }

        public async Task<List<Comment>> GetCommentsAsync()
        {
            return await _commentContext.Comment.ToListAsync();
        }

        public async Task<Comment> GetCommentByIdAsync(long id)
        {
            return await _commentContext.Comment.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task PostCommentAsync(Comment comment)
        {
            if (comment == null)
            {
                throw new ArgumentNullException(nameof(comment));
            }
            await _commentContext.Comment.AddAsync(comment);
            await _commentContext.SaveChangesAsync();
        }

        public async Task<bool> PutCommentAsync(Comment comment)
        {
            _commentContext.Comment.Update(comment);
            var updated = await _commentContext.SaveChangesAsync();
            return updated > 0;
        }

        public async Task<bool> DeleteCommentAsync(long id)
        {
            var comment = await _commentContext.Comment.FindAsync(id);
            if (comment != null)
            {
                _commentContext.Comment.Remove(comment);
                var deleted = await _commentContext.SaveChangesAsync();
                return deleted > 0;
            }

            return false;
        }

        public async Task<bool> CommentExistsAsync(long commentId)
        {
            return await _commentContext.Comment.AnyAsync(u => u.Id == commentId);
        }
    }
}