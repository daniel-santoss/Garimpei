using System.Diagnostics;
using System.Text.RegularExpressions;
using Garimpei.Data;
using Garimpei.Middleware;
using Garimpei.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- Serviços ----------
builder.Services.AddControllers();

// Swagger (apenas para testar a API em desenvolvimento)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Banco de dados (EF Core + SQL Server).
// EnableRetryOnFailure torna a conexão resiliente ao "cold start" do LocalDB.
builder.Services.AddDbContext<GarimpeiDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(6),
            errorNumbersToAdd: null)));

// Services de negócio (Fase 3)
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<ProdutoService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<VendaService>();
builder.Services.AddScoped<DashboardService>();

// Padroniza a resposta de validação de DTO (ModelState) no mesmo envelope de erro (HTTP 400)
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var mensagem = string.Join(" ", context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage));

        if (string.IsNullOrWhiteSpace(mensagem))
            mensagem = "Dados inválidos.";

        return new BadRequestObjectResult(new { sucesso = false, mensagem });
    };
});

var app = builder.Build();

// ---------- Pipeline HTTP ----------
// Tratamento global de erros (deve vir cedo no pipeline).
app.UseMiddleware<ErroMiddleware>();

// Swagger em desenvolvimento: UI em /swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Frontend estático (index.html em "/", demais páginas em /pages/...)
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// ---------- Banco: aplica migrations e popula dados de demonstração ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GarimpeiDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // Inicia o LocalDB proativamente para evitar o erro intermitente
    // "SQL Server process failed to start" (corrida no auto-start).
    GarantirLocalDbIniciado(builder.Configuration.GetConnectionString("DefaultConnection"), logger);

    // O LocalDB pode estar "frio" (parado) e demora a subir na primeira conexão.
    // Tentamos algumas vezes antes de desistir, com mensagem clara.
    const int maxTentativas = 4;
    for (var tentativa = 1; tentativa <= maxTentativas; tentativa++)
    {
        try
        {
            db.Database.Migrate();   // cria/atualiza o schema (GarimpeiDb)
            DbSeeder.Seed(db);       // popula dados fictícios se estiver vazio
            break;
        }
        catch (Exception ex) when (tentativa < maxTentativas)
        {
            logger.LogWarning("Não foi possível conectar ao banco (tentativa {Tentativa}/{Max}). " +
                "Nova tentativa em 4s... Detalhe: {Erro}", tentativa, maxTentativas, ex.Message);
            Thread.Sleep(4000);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao preparar o banco de dados. " +
                "Verifique se o SQL Server LocalDB está instalado e inicie-o com: sqllocaldb start MSSQLLocalDB");
            throw;
        }
    }
}

app.Run();


// Se a connection string usar LocalDB, executa "sqllocaldb start <instância>" antes de
// conectar. Falhas são ignoradas de propósito (o EF ainda tentará conectar com retry).
static void GarantirLocalDbIniciado(string? connectionString, ILogger logger)
{
    if (string.IsNullOrEmpty(connectionString)) return;

    var m = Regex.Match(connectionString, @"\(localdb\)\\([A-Za-z0-9_.\-]+)", RegexOptions.IgnoreCase);
    if (!m.Success) return; // não é LocalDB — nada a fazer

    var instancia = m.Groups[1].Value;
    try
    {
        var psi = new ProcessStartInfo("sqllocaldb", $"start {instancia}")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        using var proc = Process.Start(psi);
        proc?.WaitForExit(20000);
        logger.LogInformation("LocalDB '{Instancia}' verificado/iniciado.", instancia);
    }
    catch (Exception ex)
    {
        logger.LogWarning("Não foi possível iniciar o LocalDB via sqllocaldb ({Erro}). " +
            "Seguindo mesmo assim.", ex.Message);
    }
}
