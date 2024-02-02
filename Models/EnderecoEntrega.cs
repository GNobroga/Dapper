namespace DapperTesting.Models;

public class EnderecoEntrega
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public string? NomeEndereco { get; set; }

    public string? CEP { get; set; }

    public string? Cidade { get; set; }

    public string? Estado { get; set; }

    public string? Bairro { get; set; }

    public string? Endereco { get; set; }

    public int Numero { get; set; }

    public string? Complemento { get; set; }

    public Usuario Usuario { get; set; }
}