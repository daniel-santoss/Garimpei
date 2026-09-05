using System.ComponentModel.DataAnnotations;

namespace Garimpei.DTOs.Cliente;

/// <summary>Entrada para criar/editar cliente.</summary>
public class ClienteDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(120)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Telefone { get; set; }

    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(120)]
    public string? Email { get; set; }
}

/// <summary>Saída de cliente.</summary>
public class ClienteResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public DateTime DataCadastro { get; set; }
}
