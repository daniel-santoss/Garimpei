using Garimpei.DTOs.Produto;
using Garimpei.Services;
using Microsoft.AspNetCore.Mvc;

namespace Garimpei.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController : ControllerBase
{
    private readonly ProdutoService _service;
    public ProdutosController(ProdutoService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busca,
        [FromQuery] int? categoriaId,
        [FromQuery] byte? status)
        => Ok(await _service.ListarAsync(busca, categoriaId, status));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id)
        => Ok(await _service.ObterAsync(id));

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] ProdutoCreateDto dto)
    {
        var criado = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] ProdutoUpdateDto dto)
        => Ok(await _service.AtualizarAsync(id, dto));

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> AlterarStatus(int id, [FromBody] AlterarStatusDto dto)
    {
        await _service.AlterarStatusAsync(id, dto.Status);
        return Ok(new { sucesso = true, mensagem = "Disponibilidade atualizada." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        await _service.ExcluirAsync(id);
        return Ok(new { sucesso = true, mensagem = "Produto excluído." });
    }
}
