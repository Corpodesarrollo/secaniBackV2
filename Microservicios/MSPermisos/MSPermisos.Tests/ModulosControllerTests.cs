namespace MSPermisos.Api.Tests
{
    public class ModulosControllerTests
    {
        //private readonly Mock<IModuloService> _mockService;
        //private readonly ModulosController _controller;

        //public ModulosControllerTests()
        //{
        //    _mockService = new Mock<IModuloService>();
        //    _controller = new ModulosController(_mockService.Object);
        //}

        //[Fact]
        //public async Task GetAll_ReturnsOkResult_WithListOfModulos()
        //{
        //    // Arrange
        //    var modulos = new List<ModuloResponseDTO> { new ModuloResponseDTO { Id = 1, Nombre = "Modulo 1" } };
        //    _mockService.Setup(s => s.GetAllAsync(default)).ReturnsAsync(modulos);

        //    // Act
        //    var result = await _controller.GetAll();

        //    // Assert
        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var returnValue = Assert.IsType<List<ModuloResponseDTO>>(okResult.Value);
        //    Assert.Single(returnValue);
        //}

        //[Fact]
        //public async Task GetById_ReturnsOkResult_WithModulo()
        //{
        //    // Arrange
        //    var modulo = new ModuloResponseDTO { Id = 1, Nombre = "Modulo 1" };
        //    _mockService.Setup(s => s.GetByIdAsync(1, default)).ReturnsAsync(modulo);

        //    // Act
        //    var result = await _controller.GetById(1);

        //    // Assert
        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var returnValue = Assert.IsType<ModuloResponseDTO>(okResult.Value);
        //    Assert.Equal(1, returnValue.Id);
        //}

        //[Fact]
        //public async Task Add_ReturnsCreatedAtActionResult()
        //{
        //    // Arrange
        //    var dto = new ModuloRequestDTO { Nombre = "New Modulo" };
        //    var response = new ModuloResponseDTO { Id = 1, Nombre = "New Modulo" };
        //    _mockService.Setup(s => s.AddAsync(dto, default)).ReturnsAsync((true, response));

        //    // Act
        //    var result = await _controller.Add(dto);

        //    // Assert
        //    var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        //    var returnValue = Assert.IsType<ModuloResponseDTO>(createdAtActionResult.Value);
        //    Assert.Equal(1, returnValue.Id);
        //    Assert.Equal("New Modulo", returnValue.Nombre);
        //}

        //[Fact]
        //public async Task Update_ReturnsNoContentResult()
        //{
        //    // Arrange
        //    var dto = new ModuloResponseDTO { Id = 1, Nombre = "Updated Modulo" };
        //    _mockService.Setup(s => s.UpdateAsync(dto, default)).ReturnsAsync((true, dto));

        //    // Act
        //    var result = await _controller.Update(dto);

        //    // Assert
        //    Assert.IsType<NoContentResult>(result);
        //}

        //[Fact]
        //public async Task Delete_ReturnsNoContentResult_WhenModuloExists()
        //{
        //    // Arrange
        //    var modulo = new ModuloResponseDTO { Id = 1, Nombre = "Modulo 1" };
        //    _mockService.Setup(s => s.GetByIdAsync(1, default)).ReturnsAsync(modulo);
        //    _mockService.Setup(s => s.DeleteAsync(modulo, default)).Returns(Task.FromResult(true));

        //    // Act
        //    var result = await _controller.Delete(1);

        //    // Assert
        //    Assert.IsType<NoContentResult>(result);
        //}

        //[Fact]
        //public async Task Delete_ReturnsNotFoundResult_WhenModuloDoesNotExist()
        //{
        //    // Arrange
        //    _mockService.Setup(s => s.GetByIdAsync(1, default)).ReturnsAsync((ModuloResponseDTO)null);

        //    // Act
        //    var result = await _controller.Delete(1);

        //    // Assert
        //    Assert.IsType<NotFoundResult>(result);
        //}
    }
}