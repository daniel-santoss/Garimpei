using Garimpei.Data;
using Garimpei.DTOs.Produto;
using Garimpei.Exceptions;
using Garimpei.Models;
using Microsoft.EntityFrameworkCore;

namespace Garimpei.Services;

public class ProdutoService
{
    private readonly GarimpeiDbContext _db;
    public ProdutoService(GarimpeiDbContext db) => _db = db;

    public async Task<List<ProdutoResponseDto>> ListarAsync(string? busca, int? categoriaId, byte? status)
    {
        var query = _db.Produtos.Include(p => p.Categoria).AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(p => p.Nome.Contains(busca));

        if (categoriaId.HasValue)
            query = query.Where(p => p.CategoriaId == categoriaId.Value);

        if (status.HasValue)
        {
            var st = (StatusProduto)status.Value;
            query = query.Where(p => p.Status == st);
        }

        var produtos = await query.OrderByDescending(p => p.DataCadastro).ToListAsync();
        return produtos.Select(Map).ToList();
    }

    public async Task<ProdutoResponseDto> ObterAsync(int id)
    {
        var produto = await _db.Produtos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException("Produto não encontrado.");

        return Map(produto);
    }

    public async Task<ProdutoResponseDto> CriarAsync(ProdutoCreateDto dto)
    {
        // RN-P03: categoria deve existir
        var categoria = await _db.Categorias.FindAsync(dto.CategoriaId)
            ?? throw new ValidationException("Categoria informada não existe.");

        // RN-P05: Status e DataCadastro definidos pelo backend
        var produto = new Produto
        {
            Nome = dto.Nome.Trim(),
            Descricao = dto.Descricao,
            CategoriaId = dto.CategoriaId,
            Tamanho = dto.Tamanho,
            Cor = dto.Cor,
            Preco = dto.Preco,
            Estado = (EstadoConservacao)dto.Estado,
            Status = StatusProduto.Disponivel
        };

        _db.Produtos.Add(produto);
        await _db.SaveChangesAsync();

        produto.Categoria = categoria;
        return Map(produto);
    }

    public async Task<ProdutoResponseDto> AtualizarAsync(int id, ProdutoUpdateDto dto)
    {
        var produto = await _db.Produtos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException("Produto não encontrado.");

        var categoria = await _db.Categorias.FindAsync(dto.CategoriaId)
            ?? throw new ValidationException("Categoria informada não existe.");

        produto.Nome = dto.Nome.Trim();
        produto.Descricao = dto.Descricao;
        produto.CategoriaId = dto.CategoriaId;
        produto.Tamanho = dto.Tamanho;
        produto.Cor = dto.Cor;
        produto.Preco = dto.Preco;
        produto.Estado = (EstadoConservacao)dto.Estado;

        await _db.SaveChangesAsync();

        produto.Categoria = categoria;
        return Map(produto);
    }

    public async Task AlterarStatusAsync(int id, byte novoStatus)
    {
        // RN-P07: só alterna entre Disponível (1) e Inativo (3)
        if (novoStatus != (byte)StatusProduto.Disponivel && novoStatus != (byte)StatusProduto.Inativo)
            throw new ValidationException("Só é possível alternar entre Disponível e Inativo.");

        var produto = await _db.Produtos.FindAsync(id)
            ?? throw new NotFoundException("Produto não encontrado.");

        // RN-P06: produto vendido não pode ter disponibilidade alterada
        if (produto.Status == StatusProduto.Vendido)
            throw new ConflictException("Produto vendido não pode ter a disponibilidade alterada.");

        produto.Status = (StatusProduto)novoStatus;
        await _db.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id)
    {
        var produto = await _db.Produtos.FindAsync(id)
            ?? throw new NotFoundException("Produto não encontrado.");

        // RN-P08: produto que participou de vendas não pode ser excluído
        var vinculadoAVenda = await _db.ItensVenda.AnyAsync(i => i.ProdutoId == id);
        if (vinculadoAVenda)
            throw new ConflictException("Produto vinculado a vendas não pode ser excluído. Considere torná-lo Inativo.");

        _db.Produtos.Remove(produto);
        await _db.SaveChangesAsync();
    }

    private static ProdutoResponseDto Map(Produto p) => new()
    {
        Id = p.Id,
        Nome = p.Nome,
        Descricao = p.Descricao,
        CategoriaId = p.CategoriaId,
        CategoriaNome = p.Categoria?.Nome ?? string.Empty,
        Tamanho = p.Tamanho,
        Cor = p.Cor,
        Preco = p.Preco,
        Estado = (byte)p.Estado,
        EstadoTexto = p.Estado.Texto(),
        Status = (byte)p.Status,
        StatusTexto = p.Status.Texto(),
        DataCadastro = p.DataCadastro
    };
}
