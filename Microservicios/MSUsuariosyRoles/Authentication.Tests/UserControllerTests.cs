using Authentication.Api.Controllers;
using Authentication.Application.Commands.User.Create;
using Authentication.Application.Commands.User.Delete;
using Authentication.Application.Commands.User.Update;
using Authentication.Application.DTOs;
using Authentication.Application.Queries.User;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Authentication.Tests
{


    public class UserControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly UserController _userController;

        public UserControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _userController = new UserController(_mediatorMock.Object);
        }

        [Fact]
        public async Task CreateUser_ReturnsOkResult_WithInt()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "pTqoG@example.com",
                Identificacion = "test",
                ConfirmarIdentificacion = "test",
                FullName = "Test Test",
                Roles = new List<string> { "Admin" }
            };
            var expectedResponse = 1;
            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateUserCommand>(), default)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _userController.CreateUser(command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<int>(okResult.Value);
            Assert.Equal(expectedResponse, returnValue);
        }

        [Fact]
        public async Task GetAllUserAsync_ReturnsOkResult_WithListOfUserResponseDTO()
        {
            // Arrange
            var expectedResponse = new List<UserResponseDTO>
            {
                new UserResponseDTO { UserName = "Test", Email = "pTqoG@example.com", FullName = "Test Test" },
                new UserResponseDTO { UserName = "Test", Email = "pTqoG@example.com", FullName = "Test Test" }
            };
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserQuery>(), default)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _userController.GetAllUserAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<UserResponseDTO>>(okResult.Value);
            Assert.Equal(expectedResponse, returnValue);
        }

        [Fact]
        public async Task DeleteUser_ReturnsOkResult_WithInt()
        {
            // Arrange
            var userId = "1";
            var expectedResponse = 1;
            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteUserCommand>(), default)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _userController.DeleteUser(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<int>(okResult.Value);
            Assert.Equal(expectedResponse, returnValue);
        }

        [Fact]
        public async Task GetUserDetails_ReturnsOkResult_WithUserDetailsResponseDTO()
        {
            // Arrange
            var userId = "1";
            var expectedResponse = new UserDetailsResponseDTO
            {
                UserName = "Test",
                Email = "pTqoG@example.com",
                FullName = "Test Test"
            };
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserDetailsQuery>(), default)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _userController.GetUserDetails(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<UserDetailsResponseDTO>(okResult.Value);
            Assert.Equal(expectedResponse, returnValue);
        }

        [Fact]
        public async Task GetUserDetailsByUserName_ReturnsOkResult_WithUserDetailsResponseDTO()
        {
            // Arrange
            var userName = "1";
            var expectedResponse = new UserDetailsResponseDTO
            {
                Id = "1",
                UserName = "Test",
                Email = "pTqoG@example.com",
                FullName = "Test Test",
                Roles = new List<string> { "Admin" }
            };
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserDetailsByUserNameQuery>(), default)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _userController.GetUserDetailsByUserName(userName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<UserDetailsResponseDTO>(okResult.Value);
            Assert.Equal(expectedResponse, returnValue);
        }

        [Fact]
        public async Task AssignRoles_ReturnsOkResult_WithInt()
        {
            // Arrange
            var command = new AssignUsersRoleCommand
            {
                UserName = "test",
                Roles = new List<string> { "Admin" }
            };
            var expectedResponse = 1;
            _mediatorMock.Setup(m => m.Send(It.IsAny<AssignUsersRoleCommand>(), default)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _userController.AssignRoles(command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<int>(okResult.Value);
            Assert.Equal(expectedResponse, returnValue);
        }

        [Fact]
        public async Task EditUserRoles_ReturnsOkResult_WithInt()
        {
            // Arrange
            var command = new UpdateUserRolesCommand
            {
                userName = "test",
                Roles = new List<string> { "Admin" }
            };
            var expectedResponse = 1;
            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserRolesCommand>(), default)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _userController.EditUserRoles(command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<int>(okResult.Value);
            Assert.Equal(expectedResponse, returnValue);
        }

        //[Fact]
        //public async Task GetAllUserDetails_ReturnsOkResult_WithUserDetailsResponseDTO()
        //{
        //    // Arrange
        //    var expectedResponse = new UserDetailsResponseDTO
        //    {
        //        Id = "1",
        //        FullName = "Test Test",
        //        UserName = "Test",
        //        Email = "pTqoG@example.com",
        //        Roles = new List<string> { "Admin" }
        //    };

        //    _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllUsersDetailsQuery>(), default)).ReturnsAsync(expectedResponse);

        //    // Act
        //    var result = await _userController.GetAllUserDetails();

        //    // Assert
        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var returnValue = Assert.IsType<UserDetailsResponseDTO>(okResult.Value);
        //    Assert.Equal(expectedResponse, returnValue);
        //}

        [Fact]
        public async Task EditUserProfile_ReturnsOkResult_WhenIdMatches()
        {
            // Arrange
            var id = "some-id";
            var command = new EditUserProfileCommand
            {
                Id = id,
                // Initialize other properties here
            };
            var expectedResponse = 1;
            _mediatorMock.Setup(m => m.Send(It.IsAny<EditUserProfileCommand>(), default)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _userController.EditUserProfile(id, command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<int>(okResult.Value);
            Assert.Equal(expectedResponse, returnValue);
        }

        [Fact]
        public async Task EditUserProfile_ReturnsBadRequest_WhenIdDoesNotMatch()
        {
            // Arrange
            var id = "some-id";
            var command = new EditUserProfileCommand
            {
                Id = "different-id",
                // Initialize other properties here
            };

            // Act
            var result = await _userController.EditUserProfile(id, command);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }
    }

}
