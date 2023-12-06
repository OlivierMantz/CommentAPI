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
                Id = 1,
                Content = "Nice post",
                UserId = 4,
                PostId = 1,
            },
            new()
            {
                Id = 1,
                Content = "Cool",
                UserId = 2,
                PostId = 1,
            }
            );
        }
    }
}
