using Garimpei.Data;
using Garimpei.DTOs.Cliente;
using Garimpei.Exceptions;
using Garimpei.Models;
using Microsoft.EntityFrameworkCore;

namespace Garimpei.Services;

public class ClienteService
{
    private readonly GarimpeiDbContext _db;
    public ClienteService(GarimpeiDbContext db) => _db = db;

    public async Task<List<ClienteResponseDto>> ListarAsync(string? busca)
    {
        var query = _db.Clientes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(c => c.Nome.Contains(busca));

        return await query
            .OrderBy(c => c.Nome)
            .Select(c => new ClienteResponseDto
            {
                Id = c.Id,
                Nome = c.Nome,
                Telefone = c.Telefone,
                Email = c.Email,
                DataCadastro = c.DataCadastro
            })
            .ToListAsync();
    }

    public async Task<ClienteResponseDto> ObterAsync(int id)
    {
        var cliente = await _db.Clientes.FindAsync(id)
            ?? throw new NotFoundException("Cliente não encontrado.");
        return Map(cliente);
    }

    public async Task<ClienteResponseDto> CriarAsync(ClienteDto dto)
    {
        var cliente = new Cliente
        {
            Nome = dto.Nome.Trim(),
            Telefone = string.IsNullOrWhiteSpace(dto.Telefone) ? null : dto.Telefone.Trim(),
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim()
        };

        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();
        return Map(cliente);
    }

    public async Task<ClienteResponseDto> AtualizarAsync(int id, ClienteDto dto)
    {
        var cliente = await _db.Clientes.FindAsync(id)
            ?? throw new NotFoundException("Cliente não encontrado.");

        cliente.Nome = dto.Nome.Trim();
        cliente.Telefone = string.IsNullOrWhiteSpace(dto.Telefone) ? null : dto.Telefone.Trim();
        cliente.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();

        await _db.SaveChangesAsync();
        return Map(cliente);
    }

    public async Task ExcluirAsync(int id)
    {
        var cliente = await _db.Clientes.FindAsync(id)
            ?? throw new NotFoundException("Cliente não encontrado.");

        // RN-CL03: cliente com vendas não pode ser excluído
        if (await _db.Vendas.AnyAsync(v => v.ClienteId == id))
            throw new ConflictException("Cliente possui vendas e não pode ser excluído.");

        _db.Clientes.Remove(cliente);
        await _db.SaveChangesAsync();
    }

    private static ClienteResponseDto Map(Cliente c) => new()
    {
        Id = c.Id,
        Nome = c.Nome,
        Telefone = c.Telefone,
        Email = c.Email,
        DataCadastro = c.DataCadastro
    };
}
