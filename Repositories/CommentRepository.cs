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

        public async Task<IEnumerable<Comment>> GetCommentsAsync()
        {
            return await _context.Comments.ToListAsync();
        }
        public async Task<List<Comment>> GetAllCommentsInPostAsync(int postId)
        {
            return await _context.Comments
                                 .Where(c => c.PostId == postId)
                                 .ToListAsync();
        }
        public async Task<Comment> GetCommentByIdAsync(long id)
        {
            return await _context.Comments.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task CreateCommentAsync(Comment comment)
        {
            if (comment == null)
            {
                throw new ArgumentNullException(nameof(comment));
            }
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> PutCommentAsync(Comment comment)
        {
            _context.Comments.Update(comment);
            var updated = await _context.SaveChangesAsync();
            return updated > 0;
        }

        public async Task<bool> DeleteCommentAsync(long id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                var deleted = await _context.SaveChangesAsync();
                return deleted > 0;
            }

            return false;
        }

        public async Task<bool> CommentExistsAsync(long commentId)
        {
            return await _context.Comments.AnyAsync(u => u.Id == commentId);
        }
    }
}