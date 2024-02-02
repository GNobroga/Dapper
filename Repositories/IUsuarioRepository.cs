using DapperTesting.Models;

namespace DapperTesting.Repositories;

public interface IUsuarioRepository 
{
    Task<Usuario> CreateAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task<List<Usuario>> FindAllAsync(CancellationToken cancellationToken = default);

    Task<Usuario?> FindByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<Usuario> UpdateAsync(Usuario usuario,CancellationToken cancellationToken = default);
}