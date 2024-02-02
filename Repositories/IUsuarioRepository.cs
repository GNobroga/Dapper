using DapperTesting.Models;

namespace DapperTesting.Repositories;

public interface IUsuarioRepository 
{
    Task<Usuario> CreateAsync(Usuario usuario, CancellationToken cancellationToken);
    Task<List<Usuario>> FindAllAsync(CancellationToken cancellationToken);

    Task<Usuario?> FindByIdAsync(int id, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);

    Task<Usuario> UpdateAsync(Usuario usuario,CancellationToken cancellationToken);
}