namespace MSPermisos.Api.Tests
{
    public class FuncionalidadesControllerTests
    {
        //private readonly Mock<IFuncionalidadService> _mockService;
        //private readonly FuncionalidadesController _controller;

        //public FuncionalidadesControllerTests()
        //{
        //    _mockService = new Mock<IFuncionalidadService>();
        //    _controller = new FuncionalidadesController(_mockService.Object);
        //}

        //[Fact]
        //public async Task GetAll_ReturnsOkResult_WithListOfFuncionalidades()
        //{
        //    // Arrange
        //    var funcionalidades = new List<FuncionalidadResponseDTO> { new FuncionalidadResponseDTO { Id = 1, Nombre = "Funcionalidad 1", Descripcion = "Funcionalidad 1" } };
        //    _mockService.Setup(s => s.GetAllAsync(default)).ReturnsAsync(funcionalidades);

        //    // Act
        //    var result = await _controller.GetAll();

        //    // Assert
        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var returnValue = Assert.IsType<List<FuncionalidadResponseDTO>>(okResult.Value);
        //    Assert.Single(returnValue);
        //}

        //[Fact]
        //public async Task GetById_ReturnsOkResult_WithFuncionalidad()
        //{
        //    // Arrange
        //    var funcionalidad = new FuncionalidadResponseDTO { Id = 1, Nombre = "Funcionalidad 1", Descripcion = "Funcionalidad 1" };
        //    _mockService.Setup(s => s.GetByIdAsync(1, default)).ReturnsAsync(funcionalidad);

        //    // Act
        //    var result = await _controller.GetById(1);

        //    // Assert
        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var returnValue = Assert.IsType<FuncionalidadResponseDTO>(okResult.Value);
        //    Assert.Equal(1, returnValue.Id);
        //}

        //[Fact]
        //public async Task Add_ReturnsCreatedAtActionResult()
        //{
        //    // Arrange
        //    var dto = new FuncionalidadRequestDTO { Nombre = "New Funcionalidad" };
        //    var response = new FuncionalidadResponseDTO { Id = 1, Nombre = "New Funcionalidad" };
        //    _mockService.Setup(s => s.AddAsync(dto, default)).ReturnsAsync((true, response));

        //    // Act
        //    var result = await _controller.Add(dto);

        //    // Assert
        //    var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        //    var returnValue = Assert.IsType<FuncionalidadResponseDTO>(createdAtActionResult.Value);
        //    Assert.Equal(1, returnValue.Id);
        //    Assert.Equal("New Funcionalidad", returnValue.Nombre);
        //}

        //[Fact]
        //public async Task Update_ReturnsNoContentResult()
        //{
        //    // Arrange
        //    var dto = new FuncionalidadResponseDTO { Id = 1, Nombre = "Updated Funcionalidad" };
        //    _mockService.Setup(s => s.UpdateAsync(dto, default)).ReturnsAsync((true, dto));

        //    // Act
        //    var result = await _controller.Update(dto);

        //    // Assert
        //    Assert.IsType<NoContentResult>(result);
        //}

        //[Fact]
        //public async Task Delete_ReturnsNoContentResult_WhenFuncionalidadExists()
        //{
        //    // Arrange
        //    var funcionalidad = new FuncionalidadResponseDTO { Id = 1, Nombre = "Funcionalidad 1" };
        //    _mockService.Setup(s => s.GetByIdAsync(1, default)).ReturnsAsync(funcionalidad);
        //    _mockService.Setup(s => s.DeleteAsync(funcionalidad, default)).Returns(Task.FromResult(true));

        //    // Act
        //    var result = await _controller.Delete(1);

        //    // Assert
        //    Assert.IsType<NoContentResult>(result);
        //}

        //[Fact]
        //public async Task Delete_ReturnsNotFoundResult_WhenFuncionalidadDoesNotExist()
        //{
        //    // Arrange
        //    _mockService.Setup(s => s.GetByIdAsync(1, default)).ReturnsAsync((FuncionalidadResponseDTO)null);

        //    // Act
        //    var result = await _controller.Delete(1);

        //    // Assert
        //    Assert.IsType<NotFoundResult>(result);
        //}
    }
}