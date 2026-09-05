namespace Garimpei.DTOs.Dashboard;

/// <summary>Indicadores e listas resumidas do dashboard.</summary>
public class DashboardDto
{
    public int TotalProdutos { get; set; }
    public int ProdutosDisponiveis { get; set; }
    public int ProdutosVendidos { get; set; }
    public int TotalVendas { get; set; }
    public decimal FaturamentoTotal { get; set; }
    public List<VendaResumoDto> UltimasVendas { get; set; } = new();
    public List<ProdutoResumoDto> ProdutosRecentes { get; set; } = new();
}

public class VendaResumoDto
{
    public int Id { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime Data { get; set; }
}

public class ProdutoResumoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public string Categoria { get; set; } = string.Empty;
}
