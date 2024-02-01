using System.Text.Json.Serialization;

namespace DapperTesting.Models;

public class Contato 
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public string? Telefone { get; set; }

    public string? Celular { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Usuario? Usuario  { get; set; }

}