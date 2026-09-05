namespace Garimpei.Models;

/// <summary>Estado de conservação da peça.</summary>
public enum EstadoConservacao : byte
{
    Novo = 1,
    Seminovo = 2,
    Usado = 3
}

/// <summary>Situação do produto no estoque (peça única).</summary>
public enum StatusProduto : byte
{
    Disponivel = 1,
    Vendido = 2,
    Inativo = 3
}

/// <summary>Situação da venda.</summary>
public enum StatusVenda : byte
{
    Concluida = 1,
    Cancelada = 2
}
