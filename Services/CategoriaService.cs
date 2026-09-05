using Garimpei.Data;
using Garimpei.DTOs.Categoria;
using Garimpei.Exceptions;
using Garimpei.Models;
using Microsoft.EntityFrameworkCore;

namespace Garimpei.Services;

public class CategoriaService
{
    private readonly GarimpeiDbContext _db;
    public CategoriaService(GarimpeiDbContext db) => _db = db;

    public async Task<List<CategoriaResponseDto>> ListarAsync()
    {
        return await _db.Categorias
            .OrderBy(c => c.Nome)
            .Select(c => new CategoriaResponseDto
            {
                Id = c.Id,
                Nome = c.Nome,
                QtdProdutos = c.Produtos.Count
            })
            .ToListAsync();
    }

    public async Task<CategoriaResponseDto> ObterAsync(int id)
    {
        var c = await _db.Categorias
            .Include(x => x.Produtos)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Categoria não encontrada.");

        return new CategoriaResponseDto { Id = c.Id, Nome = c.Nome, QtdProdutos = c.Produtos.Count };
    }

    public async Task<CategoriaResponseDto> CriarAsync(CategoriaDto dto)
    {
        var nome = dto.Nome.Trim();

        // RN-C01: nome único (comparação case-insensitive pela collation padrão do SQL Server)
        if (await _db.Categorias.AnyAsync(c => c.Nome == nome))
            throw new ConflictException("Já existe uma categoria com esse nome.");

        var categoria = new Categoria { Nome = nome };
        _db.Categorias.Add(categoria);
        await _db.SaveChangesAsync();

        return new CategoriaResponseDto { Id = categoria.Id, Nome = categoria.Nome, QtdProdutos = 0 };
    }

    public async Task<CategoriaResponseDto> AtualizarAsync(int id, CategoriaDto dto)
    {
        var categoria = await _db.Categorias
            .Include(x => x.Produtos)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Categoria não encontrada.");

        var nome = dto.Nome.Trim();
        if (await _db.Categorias.AnyAsync(c => c.Nome == nome && c.Id != id))
            throw new ConflictException("Já existe uma categoria com esse nome.");

        categoria.Nome = nome;
        await _db.SaveChangesAsync();

        return new CategoriaResponseDto { Id = categoria.Id, Nome = categoria.Nome, QtdProdutos = categoria.Produtos.Count };
    }

    public async Task ExcluirAsync(int id)
    {
        var categoria = await _db.Categorias
            .Include(x => x.Produtos)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Categoria não encontrada.");

        // RN-C02: não excluir categoria com produtos
        if (categoria.Produtos.Any())
            throw new ConflictException("Categoria possui produtos e não pode ser excluída.");

        _db.Categorias.Remove(categoria);
        await _db.SaveChangesAsync();
    }
}
