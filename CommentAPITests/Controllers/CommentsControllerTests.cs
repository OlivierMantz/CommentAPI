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
using Microsoft.Extensions.Logging;

namespace CommentAPI.Controllers.Tests
{
    public class CommentsControllerIntegrationTests
    {
        private Guid CommentId1;
        private Guid CommentId2;
        private Guid CommentId3;
        private Guid PostId1;
        private Guid PostId2;
        private Guid PostId3;
        private readonly Mock<ILogger<CommentsController>> _mockLogger;

        public CommentsControllerIntegrationTests()
        {
            _mockLogger = new Mock<ILogger<CommentsController>>();
            SetupTestData();
        }
        private void SetupTestData()
        {
            CommentId1 = Guid.NewGuid();
            CommentId2 = Guid.NewGuid();
            CommentId3 = Guid.NewGuid();
            PostId1 = Guid.NewGuid();
            PostId2 = Guid.NewGuid();
            PostId3 = Guid.NewGuid();

            using var context = new ApplicationDbContext(CreateNewContextOptions());
            PopulateTestData(context);
        }
        private DbContextOptions<ApplicationDbContext> CreateNewContextOptions()
        {
            // Create a unique name for each in-memory database
            string dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return options;
        }

        private void PopulateTestData(ApplicationDbContext context)
        {
            var testData = new[]
            {
                new Comment { Id = CommentId1, Content = "Nice image", AuthorId = "1", PostId = PostId1 },
                new Comment { Id = CommentId2, Content = "Cool", AuthorId = "2", PostId = PostId2 },
                new Comment { Id = CommentId3, Content = "Cool", AuthorId = "3", PostId = PostId3 },
            };

            context.Comments.AddRange(testData);
            context.SaveChanges();
        }

        private void MockUserAuthentication(CommentsController controller, string authorId = "1", string role = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, authorId)
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
            var controller = new CommentsController(commentService, _mockLogger.Object);

            MockUserAuthentication(controller, "AdminUserId", "Admin");

            var result = await controller.GetAllCommentsAsync();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var comments = Assert.IsAssignableFrom<IEnumerable<CommentDTO>>(okResult.Value);
            Assert.True(comments.Any());
        }
        [Fact]
        public async Task GetAllCommentsAdmin_ServiceThrowsException_ReturnsInternalServerError()
        {
            var commentService = new Mock<ICommentService>();
            commentService.Setup(s => s.GetAllCommentsAsync()).ThrowsAsync(new Exception());
            var controller = new CommentsController(commentService.Object, _mockLogger.Object);

            MockUserAuthentication(controller, "AdminUserId", "Admin");

            var result = await controller.GetAllCommentsAsync();

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
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
            var controller = new CommentsController(commentService, _mockLogger.Object);

            MockUserAuthentication(controller);

            var result = await controller.GetAllCommentsInPost(PostId1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var comments = Assert.IsAssignableFrom<IEnumerable<CommentDTO>>(okResult.Value);
            Assert.True(comments.Any());
        }

        [Theory]
        [InlineData()]
        public async Task GetAllComments_InPost_InvalidPostId_ReturnsEmpty()
        {
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            PopulateTestData(context);

            var commentRepository = new CommentRepository(context);
            var commentService = new CommentService(commentRepository);
            var controller = new CommentsController(commentService, _mockLogger.Object);

            MockUserAuthentication(controller);

            var result = await controller.GetAllCommentsInPost(new Guid());

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var comments = Assert.IsAssignableFrom<IEnumerable<CommentDTO>>(okResult.Value);
            Assert.False(comments.Any());
        }

        [Theory]
        [InlineData("1")]
        public async Task Post_CreateComment(string userId)
        {
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            PopulateTestData(context);

            var commentRepository = new CommentRepository(context);
            var commentService = new CommentService(commentRepository);
            var controller = new CommentsController(commentService, _mockLogger.Object);

            MockUserAuthentication(controller, userId);

            var createCommentDTO = new CreateCommentDTO
            {
                Content = "Great comment!"
            };

            var result = await controller.CreateCommentAsync(PostId1, createCommentDTO);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var commentDto = Assert.IsType<CommentDTO>(okResult.Value);
            Assert.Equal(createCommentDTO.Content, commentDto.Content);
            Assert.Equal(PostId1, commentDto.PostId);
        }

        [Fact]
        public async Task Post_CreateComment_InvalidData_ReturnsBadRequest()
        {
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            PopulateTestData(context);

            var commentRepository = new CommentRepository(context);
            var commentService = new CommentService(commentRepository);
            var controller = new CommentsController(commentService, _mockLogger.Object);

            MockUserAuthentication(controller);

            var createCommentDTO = new CreateCommentDTO
            {
                Content = ""
            };
            var result = await controller.CreateCommentAsync(PostId1, createCommentDTO);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task CreateCommentAsync_NoAuthorId_ReturnsUnauthorized()
        {
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            var commentService = new Mock<ICommentService>();
            var controller = new CommentsController(commentService.Object, _mockLogger.Object);

            MockUserAuthentication(controller, "");

            var createCommentDTO = new CreateCommentDTO { Content = "Test Content" };

            var result = await controller.CreateCommentAsync(Guid.NewGuid(), createCommentDTO);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Theory]
        [InlineData("1")]
        public async Task UpdateComment(string userId)
        {
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            PopulateTestData(context);

            var commentRepository = new CommentRepository(context);
            var commentService = new CommentService(commentRepository);
            var controller = new CommentsController(commentService, _mockLogger.Object);

            MockUserAuthentication(controller, userId);

            var updatedCommentDTO = new CreateCommentDTO { Content = "Updated Content" };
            var result = await controller.UpdateCommentAsync(CommentId1, updatedCommentDTO);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Comment updated successfully.", okResult.Value);
        }


        [Fact]
        public async Task UpdateComment_UnauthorizedAccess()
        {
            var commentService = new Mock<ICommentService>();
            commentService.Setup(s => s.UpdateCommentAsync(It.IsAny<Guid>(), It.IsAny<CreateCommentDTO>(), It.IsAny<string>()))
                          .Throws<UnauthorizedAccessException>();

            var controller = new CommentsController(commentService.Object, _mockLogger.Object);
            MockUserAuthentication(controller, "NonAuthorUserId");

            var updatedCommentDTO = new CreateCommentDTO { Content = "Updated Content" };

            var result = await controller.UpdateCommentAsync(CommentId1, updatedCommentDTO);

            Assert.IsType<ForbidResult>(result);
        }


        [Theory]
        [InlineData("1")]
        public async Task DeleteComment_Admin(string userId)
        {
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            PopulateTestData(context);

            var commentRepository = new CommentRepository(context);
            var commentService = new CommentService(commentRepository);
            var controller = new CommentsController(commentService, _mockLogger.Object);

            MockUserAuthentication(controller, userId);

            var result = await controller.DeleteComment(CommentId1);
            Assert.IsType<NoContentResult>(result);
        }

        [Theory]
        [InlineData("2")]
        public async Task DeleteComment_OwnComment(string authorId)
        {
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);

            PopulateTestData(context);

            var commentRepository = new CommentRepository(context);
            var commentService = new CommentService(commentRepository);
            var controller = new CommentsController(commentService, _mockLogger.Object);

            MockUserAuthentication(controller, authorId);

            // CommentId retrieved via debugging for now
            //var result1 = await controller.GetAllCommentsInPost(1);

            var result = await controller.DeleteComment(CommentId2);

            Assert.IsType<NoContentResult>(result);
            var deletedComment = await context.Comments.FindAsync(CommentId2);
            Assert.Null(deletedComment);
        }

        [Theory]
        [InlineData("2")]
        public async Task DeleteComment_OtherUser_Forbidden(string authorId)
        {
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);

            PopulateTestData(context);

            var commentRepository = new CommentRepository(context);
            var commentService = new CommentService(commentRepository);
            var controller = new CommentsController(commentService, _mockLogger.Object);

            MockUserAuthentication(controller, authorId);

            var result = await controller.DeleteComment(CommentId1);

            Assert.IsType<ForbidResult>(result);
        }
    }
}