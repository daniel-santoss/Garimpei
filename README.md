# Garimpei — Sistema de Gestão para Brechós

Sistema web para um pequeno brechó controlar **produtos, categorias, clientes, vendas** e um **dashboard** resumido. Projeto de atividade de extensão.

> Este README cobre como **rodar o esqueleto atual**. A especificação completa (todas as regras, endpoints, telas e o plano de implementação para o agente Antigravity) está em [`ENGENHARIA-GARIMPEI.md`](ENGENHARIA-GARIMPEI.md).

## Tecnologias

`C#` · `.NET 8` · `ASP.NET Core Web API` · `REST` · `Entity Framework Core` · `SQL Server` · `HTML5` · `CSS3` · `JavaScript (ES6, Fetch API)`

## Arquitetura

```text
Navegador (HTML/CSS/JS)  →  REST/JSON  →  ASP.NET Core Web API
   →  Controllers → Services → DbContext (EF Core)  →  SQL Server
```

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download) — a versão está fixada em [`global.json`](global.json)
- SQL Server **ou** SQL Server LocalDB (já incluso no Visual Studio)
- As EF Core Tools já estão configuradas como **ferramenta local** ([`.config/dotnet-tools.json`](.config/dotnet-tools.json)); basta `dotnet tool restore`

## Configuração

A connection string fica em [`appsettings.json`](appsettings.json). Padrão de desenvolvimento (LocalDB, sem instalar nada):

```json
"Server=(localdb)\\MSSQLLocalDB;Database=GarimpeiDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

Para um SQL Server completo, troque por:

```json
"Server=localhost;Database=GarimpeiDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

Ou com usuário/senha:

```json
"Server=localhost;Database=GarimpeiDb;User Id=sa;Password=SUA_SENHA;TrustServerCertificate=True;"
```

## Como rodar

Restaurar dependências e ferramentas:

```bash
dotnet restore
```
```bash
dotnet tool restore
```

Rodar a aplicação (a migration `InitialCreate` já existe; ela é aplicada e os dados de demonstração são populados automaticamente no startup):

```bash
dotnet run
```

Acessar no navegador: **http://localhost:5000** (a porta exata aparece no console ao iniciar).

### Alternativa: criar o banco por script SQL

Em vez das migrations, é possível rodar [`Scripts/GarimpeiDb.sql`](Scripts/GarimpeiDb.sql) no SSMS/Azure Data Studio. Nesse caso, remova a linha `db.Database.Migrate();` de `Program.cs` (ou deixe-a: será no-op se o schema já existir e não houver migrations).

## Estado atual do projeto — completo ✅

✅ **Fases 1–2 (estrutura + banco):** projeto, `Program.cs`, Models, `DbContext`, DTOs, exceções, middleware de erro, seed de demonstração, script SQL, migration.

✅ **Fase 3 (backend):** os 5 Services e 5 Controllers com todos os endpoints `/api/...` (Categorias, Produtos, Clientes, Vendas, Dashboard) e todas as regras de negócio da Seção 14 — implementados e testados. Documentação interativa em **`/swagger`** (desenvolvimento).

✅ **Fase 4 (frontend):** todas as telas em `wwwroot/` (Dashboard, Produtos + formulário, Clientes + formulário, Vendas, Nova venda, modal de categorias) — design system, responsivo, consumindo a API via `fetch`.

O sistema está pronto para uso e apresentação: abra **http://localhost:5000** para o app completo, ou **/swagger** para testar a API isoladamente. Envelope de erro padrão: `{ "sucesso": false, "mensagem": "..." }`.

## Observações

- Sem autenticação: uso local, usuário único (justificado no documento de engenharia).
- Cada peça é única — o "estoque" é o conjunto de produtos com status `Disponível`.
- Não commitar senhas reais; `appsettings.Production.json` está no `.gitignore`.
