using Garimpei.Data;
using Garimpei.DTOs.Dashboard;
using Garimpei.Models;
using Microsoft.EntityFrameworkCore;

namespace Garimpei.Services;

public class DashboardService
{
    private readonly GarimpeiDbContext _db;
    public DashboardService(GarimpeiDbContext db) => _db = db;

    public async Task<DashboardDto> ObterAsync()
    {
        var totalProdutos = await _db.Produtos.CountAsync();                                   // RN-D03
        var disponiveis = await _db.Produtos.CountAsync(p => p.Status == StatusProduto.Disponivel);
        var vendidos = await _db.Produtos.CountAsync(p => p.Status == StatusProduto.Vendido);

        var vendasConcluidas = _db.Vendas.Where(v => v.Status == StatusVenda.Concluida);        // RN-D01/D02
        var totalVendas = await vendasConcluidas.CountAsync();
        var faturamento = await vendasConcluidas.SumAsync(v => (decimal?)v.Total) ?? 0m;

        // RN-D04: 5 vendas concluídas mais recentes
        var ultimasVendas = await _db.Vendas
            .Where(v => v.Status == StatusVenda.Concluida)
            .Include(v => v.Cliente)
            .OrderByDescending(v => v.DataVenda)
            .Take(5)
            .Select(v => new VendaResumoDto
            {
                Id = v.Id,
                Cliente = v.Cliente != null ? v.Cliente.Nome : "Consumidor não identificado",
                Total = v.Total,
                Data = v.DataVenda
            })
            .ToListAsync();

        // RN-D05: 5 produtos mais recentes
        var produtosRecentes = await _db.Produtos
            .Include(p => p.Categoria)
            .OrderByDescending(p => p.DataCadastro)
            .Take(5)
            .Select(p => new ProdutoResumoDto
            {
                Id = p.Id,
                Nome = p.Nome,
                Preco = p.Preco,
                Categoria = p.Categoria.Nome
            })
            .ToListAsync();

        return new DashboardDto
        {
            TotalProdutos = totalProdutos,
            ProdutosDisponiveis = disponiveis,
            ProdutosVendidos = vendidos,
            TotalVendas = totalVendas,
            FaturamentoTotal = faturamento,
            UltimasVendas = ultimasVendas,
            ProdutosRecentes = produtosRecentes
        };
    }
}
