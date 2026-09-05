namespace Garimpei.Models;

public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int CategoriaId { get; set; }
    public string? Tamanho { get; set; }
    public string? Cor { get; set; }
    public decimal Preco { get; set; }
    public EstadoConservacao Estado { get; set; }
    public StatusProduto Status { get; set; } = StatusProduto.Disponivel;
    public DateTime DataCadastro { get; set; }

    public Categoria Categoria { get; set; } = null!;
    public ICollection<ItemVenda> ItensVenda { get; set; } = new List<ItemVenda>();
}
