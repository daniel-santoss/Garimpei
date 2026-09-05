using System.ComponentModel.DataAnnotations;

namespace Garimpei.DTOs.Venda;

/// <summary>Entrada para criar uma venda. Cliente opcional; pelo menos 1 produto.</summary>
public class VendaCreateDto
{
    public int? ClienteId { get; set; }

    [Required(ErrorMessage = "A venda deve conter ao menos um produto.")]
    [MinLength(1, ErrorMessage = "A venda deve conter ao menos um produto.")]
    public List<int> ProdutoIds { get; set; } = new();
}

/// <summary>Saída de uma venda com seus itens.</summary>
public class VendaResponseDto
{
    public int Id { get; set; }
    public int? ClienteId { get; set; }
    public string? ClienteNome { get; set; }
    public DateTime DataVenda { get; set; }
    public decimal Total { get; set; }
    public byte Status { get; set; }
    public string StatusTexto { get; set; } = string.Empty;
    public List<ItemVendaResponseDto> Itens { get; set; } = new();
}

/// <summary>Item de uma venda (saída).</summary>
public class ItemVendaResponseDto
{
    public int ProdutoId { get; set; }
    public string ProdutoNome { get; set; } = string.Empty;
    public decimal PrecoUnitario { get; set; }
}
