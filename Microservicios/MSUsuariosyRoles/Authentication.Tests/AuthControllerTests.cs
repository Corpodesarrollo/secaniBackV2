using Authentication.Api.Controllers;
using Authentication.Application.Commands.Auth;
using Authentication.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Authentication.Tests
{
    public class AuthControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly AuthController _authController;

        public AuthControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _authController = new AuthController(_mediatorMock.Object);
        }

        [Fact]
        public async Task Login_ReturnsOkResult_WithAuthResponseDTO()
        {
            // Arrange
            var command = new AuthCommand
            {
                UserName = "test",
                Password = "test"
            };
            var expectedResponse = new AuthResponseDTO
            {
                Name = "test",
                Token = "test",
                UserId = "test"
            };
            _mediatorMock.Setup(m => m.Send(It.IsAny<AuthCommand>(), default)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _authController.Login(command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<AuthResponseDTO>(okResult.Value);
            Assert.NotEmpty(returnValue.Token);
        }
    }
}
