using System.ComponentModel.DataAnnotations;

namespace Garimpei.DTOs.Produto;

/// <summary>Dados de entrada para criar um produto.</summary>
public class ProdutoCreateDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(120)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descricao { get; set; }

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    public int CategoriaId { get; set; }

    [StringLength(20)]
    public string? Tamanho { get; set; }

    [StringLength(30)]
    public string? Cor { get; set; }

    [Range(0.01, 999999.99, ErrorMessage = "O preço deve ser maior que zero.")]
    public decimal Preco { get; set; }

    [Range(1, 3, ErrorMessage = "Estado inválido (1=Novo, 2=Seminovo, 3=Usado).")]
    public byte Estado { get; set; }
}

/// <summary>Dados de entrada para editar um produto. Status não é editável aqui (usar PATCH /status).</summary>
public class ProdutoUpdateDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(120)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descricao { get; set; }

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    public int CategoriaId { get; set; }

    [StringLength(20)]
    public string? Tamanho { get; set; }

    [StringLength(30)]
    public string? Cor { get; set; }

    [Range(0.01, 999999.99, ErrorMessage = "O preço deve ser maior que zero.")]
    public decimal Preco { get; set; }

    [Range(1, 3, ErrorMessage = "Estado inválido (1=Novo, 2=Seminovo, 3=Usado).")]
    public byte Estado { get; set; }
}

/// <summary>Alteração de disponibilidade: apenas 1 (Disponível) ou 3 (Inativo).</summary>
public class AlterarStatusDto
{
    [Range(1, 3)]
    public byte Status { get; set; }
}

/// <summary>Representação de saída de um produto.</summary>
public class ProdutoResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int CategoriaId { get; set; }
    public string CategoriaNome { get; set; } = string.Empty;
    public string? Tamanho { get; set; }
    public string? Cor { get; set; }
    public decimal Preco { get; set; }
    public byte Estado { get; set; }
    public string EstadoTexto { get; set; } = string.Empty;
    public byte Status { get; set; }
    public string StatusTexto { get; set; } = string.Empty;
    public DateTime DataCadastro { get; set; }
}
