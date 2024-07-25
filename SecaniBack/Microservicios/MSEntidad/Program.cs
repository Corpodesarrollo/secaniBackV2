using MSEntidad.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.CustomConfigureServices();

var app = builder.Build();

app.RegisterMiddlewares();

app.Run();