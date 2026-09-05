using Garimpei.Models;

namespace Garimpei.Data;

/// <summary>
/// Popula o banco com dados fictícios de demonstração.
/// Idempotente: só insere se as tabelas estiverem vazias.
/// Reproduz os números esperados no dashboard (ver ENGENHARIA-GARIMPEI.md, Seção 10/24).
/// </summary>
public static class DbSeeder
{
    public static void Seed(GarimpeiDbContext db)
    {
        if (db.Categorias.Any()) return; // já populado

        // -------- Categorias --------
        var camisetas  = new Categoria { Nome = "Camisetas" };
        var vestidos   = new Categoria { Nome = "Vestidos" };
        var calcas     = new Categoria { Nome = "Calças" };
        var calcados   = new Categoria { Nome = "Calçados" };
        var acessorios = new Categoria { Nome = "Acessórios" };
        var jaquetas   = new Categoria { Nome = "Jaquetas" };
        db.Categorias.AddRange(camisetas, vestidos, calcas, calcados, acessorios, jaquetas);
        db.SaveChanges();

        // -------- Clientes (fictícios) --------
        var cAna    = new Cliente { Nome = "Ana Beatriz Souza",    Telefone = "(11) 99999-1010", Email = "ana.souza@exemplo.com" };
        var cCarlos = new Cliente { Nome = "Carlos Henrique Lima", Telefone = "(11) 98888-2020", Email = "carlos.lima@exemplo.com" };
        var cMarina = new Cliente { Nome = "Marina Oliveira",      Telefone = "(21) 97777-3030", Email = "marina.oliveira@exemplo.com" };
        var cJoao   = new Cliente { Nome = "João Pedro Alves" };
        var cFer    = new Cliente { Nome = "Fernanda Costa",       Telefone = "(31) 96666-4040", Email = "fernanda.costa@exemplo.com" };
        db.Clientes.AddRange(cAna, cCarlos, cMarina, cJoao, cFer);
        db.SaveChanges();

        // -------- Produtos --------
        var pVestidoFloral  = new Produto { Nome = "Vestido Floral Vintage", Descricao = "Vestido midi estampado, tecido leve.", CategoriaId = vestidos.Id,   Tamanho = "M",  Cor = "Floral", Preco = 79.90m,  Estado = EstadoConservacao.Seminovo, Status = StatusProduto.Disponivel };
        var pCamisetaBranca = new Produto { Nome = "Camiseta Básica Branca",  Descricao = "Algodão, gola redonda.",              CategoriaId = camisetas.Id,  Tamanho = "G",  Cor = "Branco", Preco = 24.90m,  Estado = EstadoConservacao.Novo,     Status = StatusProduto.Disponivel };
        var pCalcaJeans     = new Produto { Nome = "Calça Jeans Reta",        Descricao = "Jeans clássico, cintura média.",       CategoriaId = calcas.Id,     Tamanho = "40", Cor = "Azul",   Preco = 89.00m,  Estado = EstadoConservacao.Usado,    Status = StatusProduto.Disponivel };
        var pTenis          = new Produto { Nome = "Tênis Casual",            Descricao = "Tênis de lona, pouco uso.",            CategoriaId = calcados.Id,   Tamanho = "38", Cor = "Bege",   Preco = 119.90m, Estado = EstadoConservacao.Seminovo, Status = StatusProduto.Disponivel };
        var pJaqueta        = new Produto { Nome = "Jaqueta Jeans",           Descricao = "Jaqueta oversized.",                   CategoriaId = jaquetas.Id,   Tamanho = "M",  Cor = "Azul",   Preco = 149.90m, Estado = EstadoConservacao.Seminovo, Status = StatusProduto.Disponivel };
        var pBolsa          = new Produto { Nome = "Bolsa de Couro",          Descricao = "Bolsa média, alça ajustável.",         CategoriaId = acessorios.Id,                 Cor = "Marrom", Preco = 99.90m,  Estado = EstadoConservacao.Usado,    Status = StatusProduto.Disponivel };
        var pVestidoFesta   = new Produto { Nome = "Vestido Longo Festa",     Descricao = "Vestido de festa, usado uma vez.",     CategoriaId = vestidos.Id,   Tamanho = "P",  Cor = "Vinho",  Preco = 199.90m, Estado = EstadoConservacao.Seminovo, Status = StatusProduto.Disponivel };
        var pCamisaSocial   = new Produto { Nome = "Camisa Social",           Descricao = "Camisa manga longa.",                  CategoriaId = camisetas.Id,  Tamanho = "M",  Cor = "Azul",   Preco = 59.90m,  Estado = EstadoConservacao.Seminovo, Status = StatusProduto.Disponivel };
        var pSapatenis      = new Produto { Nome = "Sapatênis Marrom",        Descricao = "Confortável, sola nova.",              CategoriaId = calcados.Id,   Tamanho = "41", Cor = "Marrom", Preco = 89.90m,  Estado = EstadoConservacao.Seminovo, Status = StatusProduto.Disponivel };
        var pCinto          = new Produto { Nome = "Cinto de Couro",          Descricao = "Cinto marrom clássico.",               CategoriaId = acessorios.Id, Tamanho = "U",  Cor = "Marrom", Preco = 39.90m,  Estado = EstadoConservacao.Usado,    Status = StatusProduto.Disponivel };
        var pBlusaTrico     = new Produto { Nome = "Blusa de Tricô",          Descricao = "Blusa quentinha para o inverno.",      CategoriaId = camisetas.Id,  Tamanho = "G",  Cor = "Cinza",  Preco = 49.90m,  Estado = EstadoConservacao.Seminovo, Status = StatusProduto.Inativo };
        var pSaia           = new Produto { Nome = "Saia Plissada",           Descricao = "Saia midi plissada.",                  CategoriaId = vestidos.Id,   Tamanho = "M",  Cor = "Preto",  Preco = 54.90m,  Estado = EstadoConservacao.Seminovo, Status = StatusProduto.Disponivel };

        db.Produtos.AddRange(
            pVestidoFloral, pCamisetaBranca, pCalcaJeans, pTenis, pJaqueta, pBolsa,
            pVestidoFesta, pCamisaSocial, pSapatenis, pCinto, pBlusaTrico, pSaia);
        db.SaveChanges();

        // -------- Vendas (marcam os produtos como Vendido) --------
        var v1 = CriarVenda(cAna.Id,    DateTime.UtcNow.AddDays(-10), (pCamisetaBranca, 24.90m), (pCamisaSocial, 59.90m));
        var v2 = CriarVenda(cCarlos.Id, DateTime.UtcNow.AddDays(-5),  (pTenis, 119.90m));
        var v3 = CriarVenda(null,       DateTime.UtcNow.AddDays(-2),  (pBolsa, 99.90m));
        var v4 = CriarVenda(cMarina.Id, DateTime.UtcNow.AddDays(-1),  (pCinto, 39.90m));
        db.Vendas.AddRange(v1, v2, v3, v4);
        db.SaveChanges();
    }

    private static Venda CriarVenda(int? clienteId, DateTime data, params (Produto produto, decimal preco)[] itens)
    {
        var venda = new Venda
        {
            ClienteId = clienteId,
            DataVenda = data,
            Status = StatusVenda.Concluida,
            Total = 0m
        };

        foreach (var (produto, preco) in itens)
        {
            venda.Itens.Add(new ItemVenda { Produto = produto, PrecoUnitario = preco });
            produto.Status = StatusProduto.Vendido;
            venda.Total += preco;
        }

        return venda;
    }
}
