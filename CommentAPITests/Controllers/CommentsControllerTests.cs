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
using Moq;

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
                new Comment { Content = "Beautiful", UserId = "2", PostId = 2 },
                new Comment { Content = "Great", UserId = "3", PostId = 2 } 
            };

            context.Comments.AddRange(testData);
            context.SaveChanges();
        }

        private void MockUserAuthentication(CommentsController controller, string userId = "1", string role = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            if (!string.IsNullOrEmpty(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.SetupGet(ctx => ctx.User).Returns(claimsPrincipal);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext.Object
            };
        }

        [Fact]
        public async Task GetAllComments_Admin()
        {
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            PopulateTestData(context);

            var commentRepository = new CommentRepository(context);
            var commentService = new CommentService(commentRepository);
            var controller = new CommentsController(commentService);

            MockUserAuthentication(controller, "adminUserId", "Admin");

            var result = await controller.GetAllCommentsAsync();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var comments = Assert.IsAssignableFrom<IEnumerable<CommentDTO>>(okResult.Value);
            Assert.True(comments.Any());
        }

        //[Theory]
        //[InlineData("1")]
        //public async Task GetAllComments_NotAdmin_Forbidden(string userId)
        //{
        //    var options = CreateNewContextOptions();
        //    using var context = new ApplicationDbContext(options);
        //    PopulateTestData(context);

        //    var commentRepository = new CommentRepository(context);
        //    var commentService = new CommentService(commentRepository);
        //    var controller = new CommentsController(commentService);

        //    MockUserAuthentication(controller, userId);

        //    var result = await controller.GetAllCommentsAsync();

        //    Assert.IsType<ForbidResult>(result.Result);
        //}

        [Fact]
        public async Task GetAllComments_InPost()
        {
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            PopulateTestData(context);

            var commentRepository = new CommentRepository(context);
            var commentService = new CommentService(commentRepository);
            var controller = new CommentsController(commentService);

            MockUserAuthentication(controller);

            long postId = 1;

            var result = await controller.GetAllCommentsInPost(postId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var comments = Assert.IsAssignableFrom<IEnumerable<CommentDTO>>(okResult.Value);
            Assert.True(comments.Any());
        }

        [Theory]
        [InlineData(999)]
        public async Task GetAllComments_InPost_InvalidPostId_ReturnsEmpty(long postId)
        {
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            PopulateTestData(context);

            var commentRepository = new CommentRepository(context);
            var commentService = new CommentService(commentRepository);
            var controller = new CommentsController(commentService);

            MockUserAuthentication(controller);

            var result = await controller.GetAllCommentsInPost(postId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var comments = Assert.IsAssignableFrom<IEnumerable<CommentDTO>>(okResult.Value);
            Assert.False(comments.Any());
        }

        [Theory]
        [InlineData(1)]
        public async Task Post_CreateComment(long postId)
        {
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            PopulateTestData(context);

            var commentRepository = new CommentRepository(context);
            var commentService = new CommentService(commentRepository);
            var controller = new CommentsController(commentService);

            MockUserAuthentication(controller);

            var createCommentDTO = new CreateCommentDTO
            {
                Content = "Great comment!"
            };

            var result = await controller.CreateCommentAsync(postId, createCommentDTO);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var commentDto = Assert.IsType<CommentDTO>(okResult.Value);

            Assert.Equal(createCommentDTO.Content, commentDto.Content);
            Assert.Equal(postId, commentDto.PostId);

            var savedComment = await context.Comments.FirstOrDefaultAsync(c => c.Content == createCommentDTO.Content && c.PostId == postId);
            Assert.NotNull(savedComment);
        }

        [Theory]
        [InlineData(1)]
        public async Task Post_CreateComment_InvalidData_ReturnsBadRequest(long postId)
        {
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            PopulateTestData(context);

            var commentRepository = new CommentRepository(context);
            var commentService = new CommentService(commentRepository);
            var controller = new CommentsController(commentService);

            MockUserAuthentication(controller);

            var createCommentDTO = new CreateCommentDTO
            {
                Content = ""
            };
            var result = await controller.CreateCommentAsync(postId, createCommentDTO);

            Assert.IsType<BadRequestResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public async Task DeleteComment_Admin(long commentId)
        {
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            PopulateTestData(context);

            var commentRepository = new CommentRepository(context);
            var commentService = new CommentService(commentRepository);
            var controller = new CommentsController(commentService);

            MockUserAuthentication(controller);

            var result = await controller.DeleteComment(commentId);

            Assert.IsType<NoContentResult>(result);
            var deletedComment = await context.Comments.FindAsync(commentId);
            Assert.Null(deletedComment);
        }

        [Theory]
        [InlineData(2, "2")]
        public async Task DeleteComment_OwnComment(long commentId, string userId)
        {
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);

            PopulateTestData(context);

            var commentRepository = new CommentRepository(context);
            var commentService = new CommentService(commentRepository);
            var controller = new CommentsController(commentService);

            MockUserAuthentication(controller, userId);

            // CommentId retrieved via debugging for now
            //var result1 = await controller.GetAllCommentsInPost(1);

            var result = await controller.DeleteComment(commentId);

            Assert.IsType<NoContentResult>(result);
            var deletedComment = await context.Comments.FindAsync(commentId);
            Assert.Null(deletedComment);
        }

        [Theory]
        [InlineData(4, "2")]
        public async Task DeleteComment_OtherUser_Forbidden(long commentId, string userId)
        {
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);

            PopulateTestData(context);

            var commentRepository = new CommentRepository(context);
            var commentService = new CommentService(commentRepository);
            var controller = new CommentsController(commentService);

            MockUserAuthentication(controller, userId);

            var result = await controller.DeleteComment(commentId);

            Assert.IsType<ForbidResult>(result);
        }

    }
}