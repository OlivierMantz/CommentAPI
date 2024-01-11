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
        private readonly ApplicationDbContext _context;

        public CommentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Comment> GetCommentByIdAsync(Guid id)
        {
            return await _context.Comments.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IEnumerable<Comment>> GetAllCommentsAsync()
        {
            return await _context.Comments.ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetAllCommentsInPostAsync(Guid postId)
        {
            return await _context.Comments
                                 .Where(c => c.PostId == postId)
                                 .ToListAsync();
        }

        public async Task<Comment> CreateCommentAsync(Comment Comment)
        {
            if (Comment == null)
            {
                throw new ArgumentNullException(nameof(Comment));
            }

            await _context.Comments.AddAsync(Comment);
            await _context.SaveChangesAsync();

            return Comment;
        }

        public async Task<bool> UpdateCommentAsync(Comment Comment)
        {
            _context.Comments.Update(Comment);
            var updated = await _context.SaveChangesAsync();
            return updated > 0;
        }

        public async Task<bool> DeleteCommentAsync(Guid id)
        {
            var Comment = await _context.Comments.FindAsync(id);
            if (Comment != null)
            {
                _context.Comments.Remove(Comment);
                var deleted = await _context.SaveChangesAsync();
                return deleted > 0;
            }

            return false;
        }

        public async Task<bool> CommentExistsAsync(Guid commentId)
        {
            return await _context.Comments.AnyAsync(u => u.Id == commentId);
        }
    }
}