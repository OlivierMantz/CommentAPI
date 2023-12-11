using Xunit;
using CommentAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommentAPI.Models;
using Microsoft.EntityFrameworkCore;
using BackEnd.Repositories;
using CommentAPI.Data;
using CommentAPI.Models.DTOs;
using CommentAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Security.Principal;

namespace CommentAPI.Controllers.Tests
{
    public class CommentsControllerIntegrationTests
    {
        private DbContextOptions<ApplicationDbContext> CreateNewContextOptions()
        {
            // Create options for ApplicationDbContext
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryAppDatabase")
                .Options;

            return options;
        }

        private void PopulateTestData(ApplicationDbContext context)
        {
            var testData = new[]
            {
                new Comment { Content = "Nice image", UserId = "1", PostId = 1 },
                new Comment { Content = "Cool", UserId = "2", PostId = 1 },
                new Comment { Content = "Beautiful", UserId = "2", PostId = 2 }
            };

            context.Comments.AddRange(testData);
            context.SaveChanges();
        }
        private void MockUserAuthentication(CommentsController controller, string userId = "1")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task Post_CreateComment_ReturnsCreatedComment()
        {
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            PopulateTestData(context);

            var commentRepository = new CommentRepository(context);
            var commentService = new CommentService(commentRepository);
            var controller = new CommentsController(commentService);

            MockUserAuthentication(controller); // Mock authentication

            long postId = 1; // postId is given through the URL
            var createCommentDTO = new CreateCommentDTO
            {
                Content = "Great comment!"
            };

            var result = await controller.CreateCommentAsync(postId, createCommentDTO);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var commentDto = Assert.IsType<CommentDTO>(okResult.Value);

            Assert.Equal(createCommentDTO.Content, commentDto.Content);
            Assert.Equal(postId, commentDto.PostId); // Ensure the postId matches

            var savedComment = await context.Comments.FirstOrDefaultAsync(c => c.Content == createCommentDTO.Content && c.PostId == postId);
            Assert.NotNull(savedComment);
        }
    }
}