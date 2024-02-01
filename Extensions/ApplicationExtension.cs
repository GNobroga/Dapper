using DapperTesting.Models;
using DapperTesting.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DapperTesting.Extensions;

public static class ApplicationExtension
{
    public static WebApplication UseApiEndpoints(this WebApplication app)
    {
        var endpoint = app.MapGroup("/api");

        endpoint.MapGet("/usuarios", async (HttpContext context, IUsuarioRepository repository) => {
            var result = await repository.FindAllAsync();
            return Results.Ok(result);
        });


        endpoint.MapGet("/usuarios/{id:int}", async (HttpContext context, IUsuarioRepository repository, int id) => {
            
            var result = await repository.FindByIdAsync(id);

            if (result is null) 
                return Results.BadRequest(new ProblemDetails {
                    Detail = "Usuário não encontrado."
                });

            return Results.Ok(await repository.FindByIdAsync(id));
        }).WithName("FindById");


        endpoint.MapPost("/usuarios", async (HttpContext context, IUsuarioRepository repository, [FromBody] Usuario usuario) => {
            var result = await repository.CreateAsync(usuario);
            return Results.CreatedAtRoute("FindById", new { usuario.Id }, result);
        });

        endpoint.MapPut("/usuarios/{id:int}", async (HttpContext context, IUsuarioRepository repository, [FromBody] Usuario usuario, int id) => {
            if (await repository.FindByIdAsync(id) is null) 
                return Results.BadRequest(new ProblemDetails {
                    Detail = "Usuário não encontrado."
                });

            usuario.Id = id;
            var result = await repository.UpdateAsync(usuario);
            return Results.NoContent();
        });

        endpoint.MapDelete("/usuarios/{id:int}", async (HttpContext context, IUsuarioRepository repository, int id) => {
            var result = await repository.DeleteAsync(id);
            return Results.Ok(new { Deleted = result });
        });
        
        return app;
    }
}
