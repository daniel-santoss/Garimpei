using Garimpei.DTOs.Cliente;
using Garimpei.Services;
using Microsoft.AspNetCore.Mvc;

namespace Garimpei.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly ClienteService _service;
    public ClientesController(ClienteService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] string? busca)
        => Ok(await _service.ListarAsync(busca));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id)
        => Ok(await _service.ObterAsync(id));

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] ClienteDto dto)
    {
        var criado = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] ClienteDto dto)
        => Ok(await _service.AtualizarAsync(id, dto));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        await _service.ExcluirAsync(id);
        return Ok(new { sucesso = true, mensagem = "Cliente excluído." });
    }
}
