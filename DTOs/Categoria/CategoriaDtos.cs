using System.ComponentModel.DataAnnotations;

namespace Garimpei.DTOs.Categoria;

/// <summary>Entrada para criar/editar categoria.</summary>
public class CategoriaDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(60)]
    public string Nome { get; set; } = string.Empty;
}

/// <summary>Saída de categoria (com contagem de produtos).</summary>
public class CategoriaResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int QtdProdutos { get; set; }
}
