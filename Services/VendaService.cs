using Garimpei.Data;
using Garimpei.DTOs.Venda;
using Garimpei.Exceptions;
using Garimpei.Models;
using Microsoft.EntityFrameworkCore;

namespace Garimpei.Services;

public class VendaService
{
    private readonly GarimpeiDbContext _db;
    public VendaService(GarimpeiDbContext db) => _db = db;

    public async Task<List<VendaResponseDto>> ListarAsync()
    {
        var vendas = await _db.Vendas
            .Include(v => v.Cliente)
            .Include(v => v.Itens).ThenInclude(i => i.Produto)
            .OrderByDescending(v => v.DataVenda)
            .ToListAsync();

        return vendas.Select(Map).ToList();
    }

    public async Task<VendaResponseDto> ObterAsync(int id)
    {
        var venda = await _db.Vendas
            .Include(v => v.Cliente)
            .Include(v => v.Itens).ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(v => v.Id == id)
            ?? throw new NotFoundException("Venda não encontrada.");

        return Map(venda);
    }

    public async Task<VendaResponseDto> CriarAsync(VendaCreateDto dto)
    {
        // RN-V01: pelo menos um produto
        if (dto.ProdutoIds is null || dto.ProdutoIds.Count == 0)
            throw new ValidationException("A venda deve conter ao menos um produto.");

        // RN-V02: cliente é opcional; se informado, deve existir
        if (dto.ClienteId.HasValue && !await _db.Clientes.AnyAsync(c => c.Id == dto.ClienteId.Value))
            throw new NotFoundException("Cliente não encontrado.");

        var ids = dto.ProdutoIds.Distinct().ToList();
        var produtos = await _db.Produtos.Where(p => ids.Contains(p.Id)).ToListAsync();

        // Algum produto inexistente
        if (produtos.Count != ids.Count)
            throw new NotFoundException("Um ou mais produtos não foram encontrados.");

        // RN-V03: todos os produtos devem estar Disponíveis (senão a venda inteira é recusada)
        var indisponivel = produtos.FirstOrDefault(p => p.Status != StatusProduto.Disponivel);
        if (indisponivel is not null)
            throw new ConflictException($"O produto \"{indisponivel.Nome}\" não está disponível.");

        // RN-V04/V05/V06: total calculado no backend, snapshot de preço, produtos -> Vendido, tudo atômico
        var venda = new Venda
        {
            ClienteId = dto.ClienteId,
            DataVenda = DateTime.UtcNow,
            Status = StatusVenda.Concluida,
            Total = 0m
        };

        foreach (var produto in produtos)
        {
            venda.Itens.Add(new ItemVenda { ProdutoId = produto.Id, PrecoUnitario = produto.Preco });
            produto.Status = StatusProduto.Vendido;
            venda.Total += produto.Preco;
        }

        _db.Vendas.Add(venda);
        await _db.SaveChangesAsync(); // um único SaveChanges = transação atômica

        return await ObterAsync(venda.Id);
    }

    public async Task CancelarAsync(int id)
    {
        var venda = await _db.Vendas
            .Include(v => v.Itens).ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(v => v.Id == id)
            ?? throw new NotFoundException("Venda não encontrada.");

        // RN-V07: venda já cancelada
        if (venda.Status == StatusVenda.Cancelada)
            throw new ConflictException("Venda já está cancelada.");

        venda.Status = StatusVenda.Cancelada;
        foreach (var item in venda.Itens)
        {
            if (item.Produto is not null)
                item.Produto.Status = StatusProduto.Disponivel;
        }

        await _db.SaveChangesAsync();
    }

    private static VendaResponseDto Map(Venda v) => new()
    {
        Id = v.Id,
        ClienteId = v.ClienteId,
        ClienteNome = v.Cliente?.Nome,
        DataVenda = v.DataVenda,
        Total = v.Total,
        Status = (byte)v.Status,
        StatusTexto = v.Status.Texto(),
        Itens = v.Itens.Select(i => new ItemVendaResponseDto
        {
            ProdutoId = i.ProdutoId,
            ProdutoNome = i.Produto?.Nome ?? string.Empty,
            PrecoUnitario = i.PrecoUnitario
        }).ToList()
    };
}
