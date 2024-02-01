namespace DapperTesting.Models;

public class UsuarioDepartamento
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public int DepartamentoId { get; set; }

    public Usuario? Usuario { get; set; }

    public Departamento? Departamento { get; set; }
}