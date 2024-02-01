using DapperTesting.Models;

namespace DapperTesting.Repositories;

public interface IUsuarioRepository 
{
    Task<Usuario> CreateAsync(Usuario usuario);
    Task<List<Usuario>> FindAllAsync();

    Task<Usuario?> FindByIdAsync(int id);

    Task<bool> DeleteAsync(int id);

    Task<Usuario> UpdateAsync(Usuario usuario);
}