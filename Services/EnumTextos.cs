using Garimpei.Models;

namespace Garimpei.Services;

/// <summary>Converte os enums de domínio em texto amigável para exibição.</summary>
public static class EnumTextos
{
    public static string Texto(this EstadoConservacao e) => e switch
    {
        EstadoConservacao.Novo => "Novo",
        EstadoConservacao.Seminovo => "Seminovo",
        EstadoConservacao.Usado => "Usado",
        _ => "Desconhecido"
    };

    public static string Texto(this StatusProduto s) => s switch
    {
        StatusProduto.Disponivel => "Disponível",
        StatusProduto.Vendido => "Vendido",
        StatusProduto.Inativo => "Inativo",
        _ => "Desconhecido"
    };

    public static string Texto(this StatusVenda s) => s switch
    {
        StatusVenda.Concluida => "Concluída",
        StatusVenda.Cancelada => "Cancelada",
        _ => "Desconhecido"
    };
}
