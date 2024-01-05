using BackEnd.Repositories;
using CommentAPI.Data;
using CommentAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CommentAPITests.UnitTests
{
    public class CommentRepositoryTests
    {
        private ApplicationDbContext _context;

        public CommentRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "CommentApiTest")
            .Options;

            _context = new ApplicationDbContext(options);

            // Populate test data
            _context.Comments.Add(new Comment { Id = Guid.NewGuid(), AuthorId="1", Content = "Test Comment" });
            _context.SaveChanges();
        }

        [Fact]
        public async Task CommentExistsAsync_Exists_ReturnsTrue()
        {
            var repository = new CommentRepository(_context);
            var existingCommentId = _context.Comments.First().Id;

            var result = await repository.CommentExistsAsync(existingCommentId);
            Assert.True(result);
        }

        [Fact]
        public async Task CommentExistsAsync_NotExists_ReturnsFalse()
        {
            var repository = new CommentRepository(_context);
            var nonExistingCommentId = Guid.NewGuid();

            var result = await repository.CommentExistsAsync(nonExistingCommentId);

            Assert.False(result);
        }
    }
}