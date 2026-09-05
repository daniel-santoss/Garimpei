namespace Garimpei.Models;

public class Categoria
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime DataCadastro { get; set; }

    public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}
