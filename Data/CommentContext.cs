using System.Collections.Generic;
using CommentAPI.Models;
using Microsoft.EntityFrameworkCore;
using CommentAPI.Models.DTOs;

namespace CommentAPI.Data
{
    public class CommentContext : DbContext
    {
        public CommentContext(DbContextOptions<CommentContext> options) : base(options)
        {
        }
        public DbSet<Comment> Comment { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>().HasData(
            new()
            {
                PostId = 1,
                CommentId = 4,
                Content = "Nice post"
            },
            new()
            {
                PostId = 1,
                CommentId = 2,
                Content = "Cool"
            }
            );
        }
    }
}
