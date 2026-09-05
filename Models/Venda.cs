namespace Garimpei.Models;

public class Venda
{
    public int Id { get; set; }
    public int? ClienteId { get; set; }
    public DateTime DataVenda { get; set; }
    public decimal Total { get; set; }
    public StatusVenda Status { get; set; } = StatusVenda.Concluida;

    public Cliente? Cliente { get; set; }
    public ICollection<ItemVenda> Itens { get; set; } = new List<ItemVenda>();
}
