using Garimpei.DTOs.Categoria;
using Garimpei.Services;
using Microsoft.AspNetCore.Mvc;

namespace Garimpei.Controllers;

[ApiController]
[Route("api/categorias")]
public class CategoriasController : ControllerBase
{
    private readonly CategoriaService _service;
    public CategoriasController(CategoriaService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Listar()
        => Ok(await _service.ListarAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id)
        => Ok(await _service.ObterAsync(id));

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CategoriaDto dto)
    {
        var criada = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(Obter), new { id = criada.Id }, criada);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] CategoriaDto dto)
        => Ok(await _service.AtualizarAsync(id, dto));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        await _service.ExcluirAsync(id);
        return Ok(new { sucesso = true, mensagem = "Categoria excluída." });
    }
}
