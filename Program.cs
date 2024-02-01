using DapperTesting.DatabaseFactories;
using DapperTesting.DatabaseFactories.Impl;
using DapperTesting.Extensions;
using DapperTesting.Repositories;
using DapperTesting.Repositories.Impl;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IConnectionFactory, SqliteConnectionFactory>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(exceptionHandlerApp => {
    exceptionHandlerApp.Run(async context => await Results.Problem().ExecuteAsync(context));
});

app.UseHttpsRedirection();

app.UseApiEndpoints();

app.MapFallback(() => Results.NotFound(new { Detail = "Rota não encontrada" }));

app.Run();



