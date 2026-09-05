namespace Garimpei.DTOs.Comum;

/// <summary>Envelope padrão de erro retornado pela API.</summary>
public class ErroResponse
{
    public bool Sucesso { get; set; } = false;
    public string Mensagem { get; set; } = string.Empty;

    public ErroResponse() { }
    public ErroResponse(string mensagem) => Mensagem = mensagem;
}
