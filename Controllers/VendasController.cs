using Garimpei.DTOs.Venda;
using Garimpei.Services;
using Microsoft.AspNetCore.Mvc;

namespace Garimpei.Controllers;

[ApiController]
[Route("api/vendas")]
public class VendasController : ControllerBase
{
    private readonly VendaService _service;
    public VendasController(VendaService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Listar()
        => Ok(await _service.ListarAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id)
        => Ok(await _service.ObterAsync(id));

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] VendaCreateDto dto)
    {
        var criada = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(Obter), new { id = criada.Id }, criada);
    }

    [HttpPatch("{id:int}/cancelar")]
    public async Task<IActionResult> Cancelar(int id)
    {
        await _service.CancelarAsync(id);
        return Ok(new { sucesso = true, mensagem = "Venda cancelada. Produtos devolvidos ao estoque." });
    }
}
