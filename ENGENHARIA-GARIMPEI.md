# Garimpei — Sistema de Gestão para Brechós
## Documento de Engenharia de Projeto (Especificação Executável)

> **Para quem lê:** este documento é a fonte única de verdade do projeto. Ele foi escrito para que um agente de programação (Antigravity) implemente o sistema **seguindo o plano sem tomar decisões arquiteturais próprias**. Onde houver dúvida, este documento tem autoridade. Se algo não estiver aqui, **não invente complexidade** — consulte a Seção 26 (Decisões Importantes).

> **Versão do documento:** 1.0 · **Data:** 2026-09-05 · **Papel do autor:** Arquiteto de Software / Analista de Sistemas / Tech Lead.

---

## Índice

1. [Visão geral](#1-visão-geral)
2. [Objetivos](#2-objetivos)
3. [Escopo](#3-escopo)
4. [Funcionalidades](#4-funcionalidades)
5. [Arquitetura](#5-arquitetura)
6. [Stack (decisão)](#6-stack-decisão-definitiva)
7. [Estrutura do projeto](#7-estrutura-do-projeto)
8. [Banco de dados](#8-banco-de-dados)
9. [Modelo relacional](#9-modelo-relacional)
10. [SQL](#10-sql-do-banco)
11. [Models](#11-models-entidades)
12. [DTOs](#12-dtos)
13. [API REST](#13-api-rest)
14. [Regras de negócio](#14-regras-de-negócio)
15. [Validações](#15-validações)
16. [Telas](#16-telas)
17. [UX/UI](#17-uxui)
18. [Fluxos completos](#18-fluxos-completos)
19. [Tratamento de erros](#19-tratamento-de-erros)
20. [Segurança](#20-segurança)
21. [Plano de implementação](#21-plano-de-implementação-para-o-antigravity)
22. [Critérios de aceite](#22-critérios-de-aceite)
23. [Casos de teste](#23-casos-de-teste)
24. [Dados de demonstração](#24-dados-de-demonstração)
25. [README](#25-conteúdo-do-readme)
26. [Decisões importantes para o Antigravity](#26-decisões-importantes-para-o-antigravity)
27. [Checklist de execução](#27-checklist-de-execução-do-antigravity)

---

## 1. Visão geral

**Garimpei** é um sistema web de gestão para um pequeno brechó. Ele permite controlar **produtos (peças)**, **categorias**, **clientes**, **vendas** e apresenta um **dashboard** com indicadores resumidos do negócio.

A característica central de um brechó que molda todo o modelo de dados: **cada peça é única** (item de segunda mão, uma única unidade física). Não existe "quantidade em estoque" por produto — cada produto é uma unidade que está **Disponível**, **Vendida** ou **Inativa**. Isso simplifica drasticamente o controle de estoque: o "estoque" é simplesmente o conjunto de produtos com status `Disponível`.

O sistema é uma **aplicação web local** composta por:
- Um **backend ASP.NET Core Web API** (C#) que expõe endpoints REST em JSON.
- Um **frontend estático** (HTML + CSS + JavaScript puro) servido pelo próprio backend a partir da pasta `wwwroot`.
- Um banco de dados **SQL Server**.

Tudo roda a partir de um único processo (`dotnet run`), acessível pelo navegador em `http://localhost:5000`. Não há dependências externas além do .NET SDK e do SQL Server.

---

## 2. Objetivos

**Objetivo de produto:** entregar ao brechó uma ferramenta simples para cadastrar peças, registrar vendas e visualizar o resumo do negócio.

**Objetivo acadêmico (atividade de extensão):** ser simples de instalar, apresentar e explicar a professores.

**Objetivo de portfólio:** demonstrar competência real em **desenvolvimento Full Stack .NET** — modelagem de dados relacional, API REST bem desenhada, ORM (EF Core), boas práticas de validação e tratamento de erros, e um frontend que consome a API.

**Não-objetivos (explicitamente fora):** multiusuário com papéis, autenticação/autorização robusta, relatórios avançados, integração fiscal/pagamentos, app mobile nativo, deploy em nuvem, microserviços.

---

## 3. Escopo

### Dentro do escopo
- CRUD de **Produtos** (com filtros e busca).
- CRUD de **Categorias** (mínimo, para dar suporte aos produtos).
- CRUD de **Clientes**.
- **Vendas**: criar venda com um ou mais produtos, cliente opcional, cálculo de total no backend, cancelamento de venda.
- Regras de estoque: produto vendido não pode ser vendido de novo; venda muda status do produto para `Vendido`; cancelamento devolve para `Disponível`.
- **Dashboard** com indicadores e listas resumidas.
- Dados de demonstração (seed) para abrir o sistema já populado.

### Fora do escopo
- Login/senha, cadastro de usuários, controle de acesso.
- Upload de imagens de produtos (mencionado como *extensão opcional futura* na Seção 26; **não implementar** na entrega base).
- Pagamentos, emissão de nota fiscal, integração com gateways.
- Paginação server-side complexa, exportação para Excel/PDF, gráficos avançados.
- Testes automatizados (o projeto usa **testes manuais** documentados na Seção 23).

---

## 4. Funcionalidades

Resumo funcional (o detalhamento por tela está na Seção 16, por endpoint na Seção 13, e por regra na Seção 14).

### 4.1 Produtos
- Listar produtos (tabela).
- Buscar por nome (texto).
- Filtrar por categoria.
- Filtrar por status (Disponível / Vendido / Inativo).
- Cadastrar produto.
- Editar produto.
- Excluir produto (com regra de proteção — ver 14).
- Ver detalhes de um produto.
- Alterar disponibilidade (Disponível ↔ Inativo).

**Campos do produto (decisão final — apenas o necessário):**

| Campo | Necessário? | Justificativa |
|---|---|---|
| Nome | ✅ | Identifica a peça. |
| Descrição | ✅ (opcional no preenchimento) | Detalhes livres da peça. |
| Categoria | ✅ | Organização e filtro. |
| Tamanho | ✅ (opcional) | Relevante para roupas/calçados de brechó. |
| Cor | ✅ (opcional) | Ajuda a identificar a peça. |
| Preço | ✅ | Essencial para venda e faturamento. |
| Estado/Conservação | ✅ | Típico de brechó (Novo / Seminovo / Usado). |
| Status | ✅ (controlado pelo sistema) | Disponível / Vendido / Inativo. |
| Data de cadastro | ✅ (automático) | Ordenação e "cadastrados recentemente". |

> **Removidos por desnecessários** para um pequeno brechó: SKU/código de barras, marca, peso, dimensões, fornecedor, custo de aquisição, margem. Não incluir.

### 4.2 Categorias
- Listar categorias.
- Cadastrar categoria.
- Editar categoria.
- Excluir categoria (bloqueada se houver produtos vinculados — ver 14).

Campos: **Nome** (obrigatório, único), **DataCadastro** (automático).

### 4.3 Clientes
- Listar clientes.
- Buscar por nome.
- Cadastrar / editar / excluir cliente.

Campos: **Nome** (obrigatório), **Telefone** (opcional), **Email** (opcional), **DataCadastro** (automático). Mantido simples de propósito — brechó pequeno não precisa de endereço/CPF.

### 4.4 Vendas
- Listar vendas (com cliente, data, total, status).
- Criar nova venda: selecionar cliente (opcional) + um ou mais produtos disponíveis.
- Backend valida disponibilidade e calcula o total.
- Confirmar venda → produtos passam a `Vendido`.
- Ver detalhes da venda (itens).
- Cancelar venda → produtos voltam a `Disponível`, venda fica `Cancelada`.

### 4.5 Dashboard
Indicadores (cards) + duas listas (últimas vendas, produtos recentes). Detalhado na Seção 16.6 e no endpoint `GET /api/dashboard` (Seção 13.6).

---

## 5. Arquitetura

### 5.1 Diagrama de camadas

```text
┌─────────────────────────────────────────────┐
│  Navegador (usuário)                          │
│  HTML + CSS + JavaScript (fetch API)          │
└───────────────────────┬─────────────────────┘
                        │  HTTP / JSON (REST)
                        ▼
┌─────────────────────────────────────────────┐
│  ASP.NET Core Web API (Kestrel)               │
│                                               │
│  Middleware (Static Files, CORS, ErrorHandler)│
│        │                                      │
│  Controllers  ──►  Services  ──►  DbContext   │
│  (HTTP/JSON)      (regras)       (EF Core)     │
└───────────────────────┬─────────────────────┘
                        │  SQL (parametrizado via EF Core)
                        ▼
┌─────────────────────────────────────────────┐
│  SQL Server (banco GarimpeiDb)                │
└─────────────────────────────────────────────┘
```

O mesmo processo ASP.NET Core **serve os arquivos estáticos** (frontend) **e** a API. Não há dois servidores. Isso elimina a maior parte dos problemas de CORS e simplifica a execução (`dotnet run` e pronto).

### 5.2 Responsabilidade de cada camada

| Camada | Responsabilidade | O que NÃO faz |
|---|---|---|
| **Frontend (wwwroot)** | Renderizar telas, capturar entrada do usuário, validar para UX, chamar a API via `fetch`, exibir mensagens de sucesso/erro/loading. | Não contém regra de negócio "de verdade" (a autoridade é o backend). Não acessa o banco. |
| **Controllers** | Receber requisições HTTP, ler parâmetros/body (DTOs), chamar o Service adequado, traduzir o resultado em status HTTP + JSON. | Não contém regra de negócio nem acesso direto ao banco. Sem lógica de cálculo. |
| **Services** | Regras de negócio: validações de domínio, cálculo de total, transições de status, orquestração de transações. Usam o `DbContext`. | Não conhecem HTTP (não retornam `IActionResult`). Não montam JSON. |
| **DbContext (EF Core)** | Mapear entidades ↔ tabelas, executar queries parametrizadas, gerenciar transações. | Não contém regra de negócio. |
| **SQL Server** | Persistir dados, impor constraints (PK/FK/UNIQUE/CHECK), integridade referencial. | — |

> **Por que existe uma camada Service se o projeto é simples?** Porque ela separa "traduzir HTTP" (Controller) de "regra de negócio" (Service). É a única separação que agrega valor real aqui e é fácil de explicar na apresentação. **Não** introduzir Repository/UnitOfWork/CQRS — o `DbContext` do EF Core já é um Repository + Unit of Work. Ver Seção 26.

### 5.3 Comunicação frontend ↔ backend
- Frontend faz `fetch('/api/...')` com `Content-Type: application/json`.
- Backend responde JSON com convenção **camelCase** (padrão do ASP.NET Core `System.Text.Json`).
- Sucesso: status `200`/`201` + payload de dados.
- Erro: status `400`/`404`/`409`/`500` + `{ "sucesso": false, "mensagem": "..." }` (envelope de erro padronizado — Seção 19).

### 5.4 Fluxos resumidos (detalhados na Seção 18)
- **Consulta/Listagem:** JS chama `GET /api/x` → Controller → Service → DbContext → SQL → JSON → tabela renderizada.
- **Cadastro:** formulário → `POST /api/x` (body DTO) → validação → Service persiste → `201 Created` → mensagem de sucesso → redireciona/atualiza lista.
- **Edição:** `PUT /api/x/{id}` → valida existência + campos → atualiza → `200`.
- **Exclusão:** `DELETE /api/x/{id}` → valida regra de proteção → remove ou `409` → mensagem.
- **Venda:** `POST /api/vendas` com lista de `produtoIds` + `clienteId?` → Service abre transação, valida disponibilidade de todos os produtos, calcula total, cria Venda + ItensVenda, marca produtos como Vendido, commit → `201`.

---

## 6. Stack (decisão definitiva)

### 6.1 Comparação das opções

| Critério | Web Forms | MVC + Razor | **Web API + HTML/CSS/JS** | Blazor Server |
|---|---|---|---|---|
| Relevância no mercado | ❌ Legado (só .NET Framework) | 🟡 Boa | ✅ Alta (padrão de mercado) | 🟡 Crescente, nicho .NET |
| Valor para portfólio | ❌ Baixo/negativo | 🟡 Bom | ✅ Alto (demonstra API + consumo) | 🟡 Bom, mas menos "Full Stack clássico" |
| Demonstra Full Stack | ❌ Não | 🟡 Parcial (tudo server-side) | ✅ Sim (backend REST **e** frontend que consome) | 🟡 Esconde a fronteira cliente/servidor |
| Arquitetura limpa | ❌ Acoplada | ✅ Boa | ✅ Boa e explícita | ✅ Boa |
| Complexidade | 🟡 Média (mas obsoleta) | 🟡 Média | ✅ Baixa/controlada | 🟡 Média (SignalR, estado) |
| Integração SQL Server | ✅ | ✅ | ✅ (EF Core) | ✅ |
| Facilidade de explicar | 🟡 | ✅ | ✅ (camadas nítidas) | 🟡 (modelo de execução menos óbvio) |
| Tempo de desenvolvimento | ❌ Alto (tecnologia antiga) | ✅ Baixo | ✅ Baixo | 🟡 Médio |

### 6.2 Decisão

> ## ✅ Stack oficial: **ASP.NET Core Web API + HTML/CSS/JavaScript (vanilla) + Entity Framework Core + SQL Server**
>
> **.NET 8 (LTS)** · **C#** · **EF Core 8 (Code First)** · **SQL Server** (LocalDB no desenvolvimento) · Frontend **HTML5 + CSS3 + JavaScript ES6 puro** (sem framework).

### 6.3 Por que esta stack (objetivamente)

1. **É a arquitetura que o próprio projeto já desenha** (Navegador → HTML/CSS/JS → REST API → ASP.NET Core → SQL Server). É coerente com o objetivo.
2. **Máximo valor de portfólio pelo menor custo:** demonstra os dois lados do Full Stack de forma **explícita e separada** — um backend REST profissional **e** um frontend que o consome. Recrutadores enxergam claramente as duas competências. MVC/Razor esconde a fronteira; Web Forms é legado; Blazor mascara onde o código executa.
3. **Simplicidade real:** JavaScript puro com `fetch` é suficiente para 7 telas. Um framework (React/Angular) adicionaria build tooling, dependências e tempo sem ganho para o tamanho do sistema.
4. **.NET 8 é LTS e moderno**, com suporte a longo prazo — bom para o currículo e para a estabilidade do projeto.
5. **EF Core** dá acesso a dados seguro (queries parametrizadas → sem SQL Injection), Code First (modelo em C#) e é a prática dominante no mercado .NET — bom para demonstrar boas práticas.
6. **Um único processo serve frontend + API** → instalação e demonstração triviais (`dotnet run`).

### 6.4 O que destacar no README e no portfólio

**Tecnologias / palavras-chave:**
`C#` · `.NET 8` · `ASP.NET Core Web API` · `REST` · `Entity Framework Core` · `SQL Server` · `HTML5` · `CSS3` · `JavaScript (ES6, Fetch API)` · `Arquitetura em camadas` · `JSON`

**Competências a evidenciar:**
- Modelagem de banco de dados relacional (normalização, PK/FK, constraints, índices).
- Design de **API REST** (verbos HTTP corretos, status codes, DTOs, versionável).
- **ORM** com EF Core (Code First, migrations, relacionamentos, transações).
- **Separação de responsabilidades** (Controller / Service / Data).
- **Validação em duas camadas** (frontend para UX, backend como autoridade).
- **Tratamento de erros** padronizado.
- Frontend responsivo consumindo API assíncrona (`fetch`/`async-await`).
- Regras de negócio de domínio (controle de estoque de peças únicas, cálculo de total no servidor).

---

## 7. Estrutura do projeto

```text
Garimpei/
├── Garimpei.sln                       # (opcional) solution file
├── Garimpei.csproj                    # projeto Web API (.NET 8)
├── Program.cs                         # composição da app: DI, middleware, seed, static files
├── appsettings.json                   # connection string + config
├── appsettings.Development.json       # overrides de desenvolvimento
│
├── Controllers/                       # camada HTTP (só orquestra)
│   ├── ProdutosController.cs
│   ├── CategoriasController.cs
│   ├── ClientesController.cs
│   ├── VendasController.cs
│   └── DashboardController.cs
│
├── Models/                            # entidades de domínio (mapeadas por EF Core)
│   ├── Produto.cs
│   ├── Categoria.cs
│   ├── Cliente.cs
│   ├── Venda.cs
│   ├── ItemVenda.cs
│   └── Enums.cs                       # StatusProduto, EstadoConservacao, StatusVenda
│
├── Data/                              # acesso a dados
│   ├── GarimpeiDbContext.cs           # DbContext + Fluent API + relacionamentos
│   └── DbSeeder.cs                    # popular dados de demonstração
│
├── Services/                          # regras de negócio
│   ├── ProdutoService.cs
│   ├── CategoriaService.cs
│   ├── ClienteService.cs
│   ├── VendaService.cs
│   └── DashboardService.cs
│
├── DTOs/                              # objetos de entrada/saída da API
│   ├── Produto/ (ProdutoCreateDto, ProdutoUpdateDto, ProdutoResponseDto, ProdutoListItemDto)
│   ├── Categoria/ (CategoriaDto, CategoriaResponseDto)
│   ├── Cliente/ (ClienteDto, ClienteResponseDto)
│   ├── Venda/ (VendaCreateDto, VendaResponseDto, ItemVendaResponseDto)
│   └── Comum/ (ErroResponse.cs)
│
├── wwwroot/                           # frontend estático (servido pela própria API)
│   ├── index.html                     # shell / dashboard (rota /)
│   ├── css/
│   │   └── styles.css                 # design system único
│   ├── js/
│   │   ├── api.js                     # wrapper de fetch + tratamento de erro
│   │   ├── ui.js                      # helpers: toasts, modais, loading, formatação
│   │   ├── dashboard.js
│   │   ├── produtos.js
│   │   ├── clientes.js
│   │   └── vendas.js
│   └── pages/
│       ├── produtos.html
│       ├── produto-form.html          # cadastro E edição (via ?id=)
│       ├── clientes.html
│       ├── cliente-form.html
│       ├── vendas.html
│       └── venda-form.html            # nova venda
│
├── Scripts/
│   └── GarimpeiDb.sql                 # script SQL manual (alternativa às migrations)
│
├── Migrations/                        # geradas pelo EF Core (não editar à mão)
│
└── README.md
```

### 7.1 Responsabilidade de cada pasta

| Pasta/arquivo | Serve para | Deve conter | NÃO deve conter |
|---|---|---|---|
| `Program.cs` | Ponto de entrada; configura DI, middleware, arquivos estáticos, roda seed. | Registro de serviços, pipeline HTTP, `MapControllers`, `UseStaticFiles`, seed. | Regra de negócio, queries. |
| `Controllers/` | Fronteira HTTP. Um controller por recurso. | Ações que lêem DTO, chamam Service, retornam status+JSON. | Cálculos, `DbContext` direto, validação de regra de domínio. |
| `Models/` | Entidades persistidas. | Propriedades + navegações + enums. | Lógica de aplicação, atributos de API. |
| `Data/` | Configuração de persistência e seed. | `DbContext`, Fluent API, `DbSeeder`. | Regra de negócio de venda. |
| `Services/` | Regras de negócio. | Validações de domínio, transações, cálculo de total, transições de status. | `IActionResult`, dependência de HTTP. |
| `DTOs/` | Contratos da API (entrada/saída). | Classes simples (sem comportamento) + Data Annotations de validação. | Lógica, navegação EF, referências circulares. |
| `wwwroot/` | Frontend. | HTML/CSS/JS estáticos. | Segredos, connection string. |
| `Scripts/` | SQL manual para quem não usa migrations. | `CREATE DATABASE/TABLE`, seeds. | — |

---

## 8. Banco de dados

### 8.1 Entidades (análise de necessidade)

| Entidade | Necessária? | Motivo |
|---|---|---|
| **Categoria** | ✅ | Organiza e filtra produtos. Tabela separada evita repetição de texto e permite filtro consistente. |
| **Produto** | ✅ | Núcleo do sistema. Cada linha = uma peça única. |
| **Cliente** | ✅ | Registro de compradores (opcional na venda). |
| **Venda** | ✅ | Cabeçalho da transação (data, cliente, total, status). |
| **ItemVenda** | ✅ | Ligação N:N entre Venda e Produto. Necessária porque **uma venda pode conter vários produtos** e guarda o **preço no momento da venda** (snapshot). |

> **Por que ItemVenda existe mesmo com "peça única"?** Porque uma venda pode incluir várias peças de uma vez (ex.: cliente leva 3 itens). ItemVenda modela isso e preserva o preço histórico. **Não há campo `Quantidade`** porque cada produto é uma unidade única (quantidade sempre 1). Isso é uma simplificação legítima e defensável do domínio "brechó".

Nenhuma tabela extra ("Usuario", "Fornecedor", "Pagamento", "Estoque") — não são necessárias e violariam o princípio de simplicidade.

### 8.2 Tabelas detalhadas

Nome do banco: **`GarimpeiDb`**. Todas as tabelas usam `Id INT IDENTITY(1,1)` como PK.

#### Tabela `Categorias`
Objetivo: catálogo de categorias de peças.

| Coluna | Tipo SQL Server | Null | Chave | Regras |
|---|---|---|---|---|
| Id | INT IDENTITY(1,1) | NOT NULL | PK | — |
| Nome | NVARCHAR(60) | NOT NULL | UNIQUE | UNIQUE(Nome) |
| DataCadastro | DATETIME2 | NOT NULL | — | DEFAULT SYSUTCDATETIME() |

Índices: PK clustered em `Id`; índice UNIQUE em `Nome`.

#### Tabela `Clientes`
Objetivo: cadastro simples de clientes.

| Coluna | Tipo | Null | Chave | Regras |
|---|---|---|---|---|
| Id | INT IDENTITY(1,1) | NOT NULL | PK | — |
| Nome | NVARCHAR(120) | NOT NULL | — | — |
| Telefone | NVARCHAR(20) | NULL | — | — |
| Email | NVARCHAR(120) | NULL | — | — |
| DataCadastro | DATETIME2 | NOT NULL | — | DEFAULT SYSUTCDATETIME() |

Índices: PK em `Id`; índice não-único em `Nome` (para busca).

#### Tabela `Produtos`
Objetivo: peças do brechó (cada linha = uma unidade física única).

| Coluna | Tipo | Null | Chave | Regras |
|---|---|---|---|---|
| Id | INT IDENTITY(1,1) | NOT NULL | PK | — |
| Nome | NVARCHAR(120) | NOT NULL | — | — |
| Descricao | NVARCHAR(500) | NULL | — | — |
| CategoriaId | INT | NOT NULL | FK → Categorias(Id) | ON DELETE NO ACTION |
| Tamanho | NVARCHAR(20) | NULL | — | — |
| Cor | NVARCHAR(30) | NULL | — | — |
| Preco | DECIMAL(10,2) | NOT NULL | — | CHECK (Preco > 0) |
| Estado | TINYINT | NOT NULL | — | CHECK (Estado IN (1,2,3)) — 1=Novo,2=Seminovo,3=Usado |
| Status | TINYINT | NOT NULL | — | DEFAULT 1; CHECK (Status IN (1,2,3)) — 1=Disponível,2=Vendido,3=Inativo |
| DataCadastro | DATETIME2 | NOT NULL | — | DEFAULT SYSUTCDATETIME() |

Índices: PK em `Id`; índice em `CategoriaId` (FK/filtro); índice em `Status` (filtro do dashboard e listagem).
Relacionamentos: `Produto N → 1 Categoria`.

#### Tabela `Vendas`
Objetivo: cabeçalho da venda.

| Coluna | Tipo | Null | Chave | Regras |
|---|---|---|---|---|
| Id | INT IDENTITY(1,1) | NOT NULL | PK | — |
| ClienteId | INT | NULL | FK → Clientes(Id) | cliente opcional; ON DELETE NO ACTION |
| DataVenda | DATETIME2 | NOT NULL | — | DEFAULT SYSUTCDATETIME() |
| Total | DECIMAL(10,2) | NOT NULL | — | CHECK (Total >= 0); calculado pelo backend |
| Status | TINYINT | NOT NULL | — | DEFAULT 1; CHECK (Status IN (1,2)) — 1=Concluída,2=Cancelada |

Índices: PK em `Id`; índice em `ClienteId`; índice em `DataVenda` (ordenar "últimas vendas").

#### Tabela `ItensVenda`
Objetivo: itens (produtos) de uma venda, com preço-snapshot.

| Coluna | Tipo | Null | Chave | Regras |
|---|---|---|---|---|
| Id | INT IDENTITY(1,1) | NOT NULL | PK | — |
| VendaId | INT | NOT NULL | FK → Vendas(Id) | ON DELETE CASCADE |
| ProdutoId | INT | NOT NULL | FK → Produtos(Id) | ON DELETE NO ACTION |
| PrecoUnitario | DECIMAL(10,2) | NOT NULL | — | CHECK (PrecoUnitario >= 0); snapshot do preço no momento da venda |

Índices: PK em `Id`; índice em `VendaId`; índice UNIQUE em `ProdutoId` **filtrado** para itens de vendas ativas — na prática, como produto vendido não pode ser revendido, a regra é garantida na aplicação; para reforço no banco, criar índice em `ProdutoId`. (Ver nota no SQL.)

> **Decisão sobre CASCADE:** apagar uma `Venda` apaga seus `ItensVenda` (CASCADE) — mas a operação normal é **cancelar**, não excluir. Produtos **nunca** são apagados por cascata de venda (NO ACTION), preservando o histórico.

---

## 9. Modelo relacional

```text
                 ┌──────────────┐
                 │  Categorias  │
                 │  PK Id       │
                 └──────┬───────┘
                        │ 1
                        │
                        │ N
                 ┌──────▼───────┐         ┌──────────────┐
                 │   Produtos   │         │   Clientes   │
                 │  PK Id       │         │  PK Id       │
                 │  FK Categoria│         └──────┬───────┘
                 └──────┬───────┘                │ 1
                        │ 1                       │
                        │                         │ 0..N (opcional)
                        │ N                       │
                 ┌──────▼───────┐         ┌──────▼───────┐
                 │  ItensVenda  │  N   1  │    Vendas    │
                 │  PK Id       ├────────►│  PK Id       │
                 │  FK VendaId  │         │  FK ClienteId│ (nullable)
                 │  FK ProdutoId│         │  Total       │
                 └──────────────┘         └──────────────┘
```

**Cardinalidades:**
- `Categoria (1) —— (N) Produto`
- `Produto (1) —— (N) ItemVenda` (na prática, cada produto aparece em no máximo 1 item de venda concluída)
- `Venda (1) —— (N) ItemVenda`
- `Cliente (1) —— (0..N) Venda` (cliente opcional → FK nullable)

Correção em relação ao esboço do briefing: o cliente liga-se a **Venda**, não a Produto/ItemVenda; e ItemVenda é a tabela associativa entre Venda e Produto. O diagrama acima é o correto.

---

## 10. SQL do banco

> **Fonte de verdade em runtime:** o EF Core (Code First + Migrations) cria o schema. Este script SQL é fornecido como **alternativa manual** e para fins acadêmicos (demonstrar domínio de SQL). Ele produz o **mesmo schema** que as migrations. Use um **ou** outro — não os dois no mesmo banco. Arquivo: `Scripts/GarimpeiDb.sql`.

```sql
-- =============================================================
-- Garimpei — Script de criação do banco (SQL Server)
-- Executar no SSMS ou Azure Data Studio conectado ao SQL Server.
-- =============================================================

IF DB_ID('GarimpeiDb') IS NULL
    CREATE DATABASE GarimpeiDb;
GO
USE GarimpeiDb;
GO

-- Limpeza idempotente (ordem respeita FKs)
IF OBJECT_ID('dbo.ItensVenda','U') IS NOT NULL DROP TABLE dbo.ItensVenda;
IF OBJECT_ID('dbo.Vendas','U')     IS NOT NULL DROP TABLE dbo.Vendas;
IF OBJECT_ID('dbo.Produtos','U')   IS NOT NULL DROP TABLE dbo.Produtos;
IF OBJECT_ID('dbo.Clientes','U')   IS NOT NULL DROP TABLE dbo.Clientes;
IF OBJECT_ID('dbo.Categorias','U') IS NOT NULL DROP TABLE dbo.Categorias;
GO

-- ---------- Categorias ----------
CREATE TABLE dbo.Categorias (
    Id           INT IDENTITY(1,1) NOT NULL,
    Nome         NVARCHAR(60)      NOT NULL,
    DataCadastro DATETIME2         NOT NULL CONSTRAINT DF_Categorias_Data DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Categorias   PRIMARY KEY (Id),
    CONSTRAINT UQ_Categorias_Nome UNIQUE (Nome)
);
GO

-- ---------- Clientes ----------
CREATE TABLE dbo.Clientes (
    Id           INT IDENTITY(1,1) NOT NULL,
    Nome         NVARCHAR(120)     NOT NULL,
    Telefone     NVARCHAR(20)      NULL,
    Email        NVARCHAR(120)     NULL,
    DataCadastro DATETIME2         NOT NULL CONSTRAINT DF_Clientes_Data DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Clientes PRIMARY KEY (Id)
);
GO
CREATE INDEX IX_Clientes_Nome ON dbo.Clientes (Nome);
GO

-- ---------- Produtos ----------
CREATE TABLE dbo.Produtos (
    Id           INT IDENTITY(1,1) NOT NULL,
    Nome         NVARCHAR(120)     NOT NULL,
    Descricao    NVARCHAR(500)     NULL,
    CategoriaId  INT               NOT NULL,
    Tamanho      NVARCHAR(20)      NULL,
    Cor          NVARCHAR(30)      NULL,
    Preco        DECIMAL(10,2)     NOT NULL,
    Estado       TINYINT           NOT NULL,  -- 1=Novo, 2=Seminovo, 3=Usado
    Status       TINYINT           NOT NULL CONSTRAINT DF_Produtos_Status DEFAULT 1, -- 1=Disponível,2=Vendido,3=Inativo
    DataCadastro DATETIME2         NOT NULL CONSTRAINT DF_Produtos_Data DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Produtos PRIMARY KEY (Id),
    CONSTRAINT FK_Produtos_Categorias FOREIGN KEY (CategoriaId) REFERENCES dbo.Categorias(Id),
    CONSTRAINT CK_Produtos_Preco  CHECK (Preco > 0),
    CONSTRAINT CK_Produtos_Estado CHECK (Estado IN (1,2,3)),
    CONSTRAINT CK_Produtos_Status CHECK (Status IN (1,2,3))
);
GO
CREATE INDEX IX_Produtos_CategoriaId ON dbo.Produtos (CategoriaId);
CREATE INDEX IX_Produtos_Status      ON dbo.Produtos (Status);
GO

-- ---------- Vendas ----------
CREATE TABLE dbo.Vendas (
    Id        INT IDENTITY(1,1) NOT NULL,
    ClienteId INT               NULL,
    DataVenda DATETIME2         NOT NULL CONSTRAINT DF_Vendas_Data DEFAULT SYSUTCDATETIME(),
    Total     DECIMAL(10,2)     NOT NULL,
    Status    TINYINT           NOT NULL CONSTRAINT DF_Vendas_Status DEFAULT 1, -- 1=Concluída,2=Cancelada
    CONSTRAINT PK_Vendas PRIMARY KEY (Id),
    CONSTRAINT FK_Vendas_Clientes FOREIGN KEY (ClienteId) REFERENCES dbo.Clientes(Id),
    CONSTRAINT CK_Vendas_Total  CHECK (Total >= 0),
    CONSTRAINT CK_Vendas_Status CHECK (Status IN (1,2))
);
GO
CREATE INDEX IX_Vendas_ClienteId ON dbo.Vendas (ClienteId);
CREATE INDEX IX_Vendas_DataVenda ON dbo.Vendas (DataVenda DESC);
GO

-- ---------- ItensVenda ----------
CREATE TABLE dbo.ItensVenda (
    Id            INT IDENTITY(1,1) NOT NULL,
    VendaId       INT               NOT NULL,
    ProdutoId     INT               NOT NULL,
    PrecoUnitario DECIMAL(10,2)     NOT NULL,
    CONSTRAINT PK_ItensVenda PRIMARY KEY (Id),
    CONSTRAINT FK_ItensVenda_Vendas   FOREIGN KEY (VendaId)   REFERENCES dbo.Vendas(Id)   ON DELETE CASCADE,
    CONSTRAINT FK_ItensVenda_Produtos FOREIGN KEY (ProdutoId) REFERENCES dbo.Produtos(Id),
    CONSTRAINT CK_ItensVenda_Preco CHECK (PrecoUnitario >= 0)
);
GO
CREATE INDEX IX_ItensVenda_VendaId   ON dbo.ItensVenda (VendaId);
CREATE INDEX IX_ItensVenda_ProdutoId ON dbo.ItensVenda (ProdutoId);
GO

-- =============================================================
-- SEED — dados de demonstração (fictícios)
-- =============================================================

-- Categorias
INSERT INTO dbo.Categorias (Nome) VALUES
 (N'Camisetas'), (N'Vestidos'), (N'Calças'), (N'Calçados'), (N'Acessórios'), (N'Jaquetas');
GO

-- Clientes (fictícios)
INSERT INTO dbo.Clientes (Nome, Telefone, Email) VALUES
 (N'Ana Beatriz Souza', N'(11) 99999-1010', N'ana.souza@exemplo.com'),
 (N'Carlos Henrique Lima', N'(11) 98888-2020', N'carlos.lima@exemplo.com'),
 (N'Marina Oliveira', N'(21) 97777-3030', N'marina.oliveira@exemplo.com'),
 (N'João Pedro Alves', NULL, NULL),
 (N'Fernanda Costa', N'(31) 96666-4040', N'fernanda.costa@exemplo.com');
GO

-- Produtos (Estado: 1=Novo,2=Seminovo,3=Usado | Status: 1=Disponível,2=Vendido,3=Inativo)
INSERT INTO dbo.Produtos (Nome, Descricao, CategoriaId, Tamanho, Cor, Preco, Estado, Status) VALUES
 (N'Vestido Floral Vintage', N'Vestido midi estampado, tecido leve.', 2, N'M', N'Floral', 79.90, 2, 1),
 (N'Camiseta Básica Branca',  N'Algodão, gola redonda.',               1, N'G', N'Branco', 24.90, 1, 1),
 (N'Calça Jeans Reta',        N'Jeans clássico, cintura média.',        3, N'40', N'Azul',   89.00, 3, 1),
 (N'Tênis Casual',            N'Tênis de lona, pouco uso.',             4, N'38', N'Bege',  119.90, 2, 1),
 (N'Jaqueta Jeans',           N'Jaqueta oversized.',                    6, N'M', N'Azul',  149.90, 2, 1),
 (N'Bolsa de Couro',          N'Bolsa média, alça ajustável.',          5, NULL, N'Marrom', 99.90, 3, 1),
 (N'Vestido Longo Festa',     N'Vestido de festa, usado uma vez.',      2, N'P', N'Vinho', 199.90, 2, 1),
 (N'Camisa Social',           N'Camisa manga longa.',                   1, N'M', N'Azul',   59.90, 2, 1),
 (N'Sapatênis Marrom',        N'Confortável, sola nova.',               4, N'41', N'Marrom', 89.90, 2, 1),
 (N'Cinto de Couro',          N'Cinto marrom clássico.',                5, N'U', N'Marrom', 39.90, 3, 1),
 (N'Blusa de Tricô',          N'Blusa quentinha para o inverno.',       1, N'G', N'Cinza',  49.90, 2, 3),  -- Inativo (exemplo)
 (N'Saia Plissada',           N'Saia midi plissada.',                   2, N'M', N'Preto',  54.90, 2, 1);
GO

-- Vendas + Itens (para popular o dashboard e faturamento)
-- Venda 1: cliente Ana, 2 itens (produtos 2 e 8)
INSERT INTO dbo.Vendas (ClienteId, Total, Status, DataVenda) VALUES (1, 84.80, 1, DATEADD(DAY,-10,SYSUTCDATETIME()));
INSERT INTO dbo.ItensVenda (VendaId, ProdutoId, PrecoUnitario) VALUES
 (SCOPE_IDENTITY(), 2, 24.90), (SCOPE_IDENTITY(), 8, 59.90);
UPDATE dbo.Produtos SET Status = 2 WHERE Id IN (2,8);
GO

-- Venda 2: cliente Carlos, 1 item (produto 4)
INSERT INTO dbo.Vendas (ClienteId, Total, Status, DataVenda) VALUES (2, 119.90, 1, DATEADD(DAY,-5,SYSUTCDATETIME()));
INSERT INTO dbo.ItensVenda (VendaId, ProdutoId, PrecoUnitario) VALUES (SCOPE_IDENTITY(), 4, 119.90);
UPDATE dbo.Produtos SET Status = 2 WHERE Id = 4;
GO

-- Venda 3: sem cliente (venda avulsa), 1 item (produto 6)
INSERT INTO dbo.Vendas (ClienteId, Total, Status, DataVenda) VALUES (NULL, 99.90, 1, DATEADD(DAY,-2,SYSUTCDATETIME()));
INSERT INTO dbo.ItensVenda (VendaId, ProdutoId, PrecoUnitario) VALUES (SCOPE_IDENTITY(), 6, 99.90);
UPDATE dbo.Produtos SET Status = 2 WHERE Id = 6;
GO

-- Venda 4: cliente Marina, 1 item (produto 10)
INSERT INTO dbo.Vendas (ClienteId, Total, Status, DataVenda) VALUES (3, 39.90, 1, DATEADD(DAY,-1,SYSUTCDATETIME()));
INSERT INTO dbo.ItensVenda (VendaId, ProdutoId, PrecoUnitario) VALUES (SCOPE_IDENTITY(), 10, 39.90);
UPDATE dbo.Produtos SET Status = 2 WHERE Id = 10;
GO

PRINT 'Banco GarimpeiDb criado e populado com sucesso.';
GO
```

> Após esse seed: 12 produtos (5 vendidos, 6 disponíveis, 1 inativo), 5 clientes, 6 categorias, 4 vendas, faturamento ≈ R$ 344,50. Suficiente para demonstrar o dashboard. O EF Core `DbSeeder` (Seção 11/24) reproduz **exatamente** este conjunto quando o banco é criado por migrations.

---

## 11. Models (entidades)

C#, em `Models/`. Sem atributos de API — configuração fica no `DbContext` (Fluent API).

```csharp
// Models/Enums.cs
namespace Garimpei.Models;

public enum EstadoConservacao : byte { Novo = 1, Seminovo = 2, Usado = 3 }
public enum StatusProduto     : byte { Disponivel = 1, Vendido = 2, Inativo = 3 }
public enum StatusVenda       : byte { Concluida = 1, Cancelada = 2 }
```

```csharp
// Models/Categoria.cs
namespace Garimpei.Models;

public class Categoria
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime DataCadastro { get; set; }

    public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}
```

```csharp
// Models/Cliente.cs
namespace Garimpei.Models;

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public DateTime DataCadastro { get; set; }

    public ICollection<Venda> Vendas { get; set; } = new List<Venda>();
}
```

```csharp
// Models/Produto.cs
namespace Garimpei.Models;

public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int CategoriaId { get; set; }
    public string? Tamanho { get; set; }
    public string? Cor { get; set; }
    public decimal Preco { get; set; }
    public EstadoConservacao Estado { get; set; }
    public StatusProduto Status { get; set; } = StatusProduto.Disponivel;
    public DateTime DataCadastro { get; set; }

    public Categoria Categoria { get; set; } = null!;
    public ICollection<ItemVenda> ItensVenda { get; set; } = new List<ItemVenda>();
}
```

```csharp
// Models/Venda.cs
namespace Garimpei.Models;

public class Venda
{
    public int Id { get; set; }
    public int? ClienteId { get; set; }
    public DateTime DataVenda { get; set; }
    public decimal Total { get; set; }
    public StatusVenda Status { get; set; } = StatusVenda.Concluida;

    public Cliente? Cliente { get; set; }
    public ICollection<ItemVenda> Itens { get; set; } = new List<ItemVenda>();
}
```

```csharp
// Models/ItemVenda.cs
namespace Garimpei.Models;

public class ItemVenda
{
    public int Id { get; set; }
    public int VendaId { get; set; }
    public int ProdutoId { get; set; }
    public decimal PrecoUnitario { get; set; }

    public Venda Venda { get; set; } = null!;
    public Produto Produto { get; set; } = null!;
}
```

### 11.1 DbContext (Data/GarimpeiDbContext.cs)

```csharp
using Garimpei.Models;
using Microsoft.EntityFrameworkCore;

namespace Garimpei.Data;

public class GarimpeiDbContext : DbContext
{
    public GarimpeiDbContext(DbContextOptions<GarimpeiDbContext> options) : base(options) { }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Cliente>   Clientes   => Set<Cliente>();
    public DbSet<Produto>   Produtos   => Set<Produto>();
    public DbSet<Venda>     Vendas     => Set<Venda>();
    public DbSet<ItemVenda> ItensVenda => Set<ItemVenda>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Categoria
        mb.Entity<Categoria>(e =>
        {
            e.Property(x => x.Nome).HasMaxLength(60).IsRequired();
            e.HasIndex(x => x.Nome).IsUnique();
            e.Property(x => x.DataCadastro).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        // Cliente
        mb.Entity<Cliente>(e =>
        {
            e.Property(x => x.Nome).HasMaxLength(120).IsRequired();
            e.Property(x => x.Telefone).HasMaxLength(20);
            e.Property(x => x.Email).HasMaxLength(120);
            e.HasIndex(x => x.Nome);
            e.Property(x => x.DataCadastro).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        // Produto
        mb.Entity<Produto>(e =>
        {
            e.Property(x => x.Nome).HasMaxLength(120).IsRequired();
            e.Property(x => x.Descricao).HasMaxLength(500);
            e.Property(x => x.Tamanho).HasMaxLength(20);
            e.Property(x => x.Cor).HasMaxLength(30);
            e.Property(x => x.Preco).HasColumnType("decimal(10,2)");
            e.Property(x => x.Estado).HasConversion<byte>();
            e.Property(x => x.Status).HasConversion<byte>().HasDefaultValue(StatusProduto.Disponivel);
            e.Property(x => x.DataCadastro).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(x => x.CategoriaId);
            e.HasIndex(x => x.Status);
            e.HasOne(x => x.Categoria).WithMany(c => c.Produtos)
             .HasForeignKey(x => x.CategoriaId).OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t => t.HasCheckConstraint("CK_Produtos_Preco", "[Preco] > 0"));
        });

        // Venda
        mb.Entity<Venda>(e =>
        {
            e.Property(x => x.Total).HasColumnType("decimal(10,2)");
            e.Property(x => x.Status).HasConversion<byte>().HasDefaultValue(StatusVenda.Concluida);
            e.Property(x => x.DataVenda).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(x => x.DataVenda);
            e.HasOne(x => x.Cliente).WithMany(c => c.Vendas)
             .HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        });

        // ItemVenda
        mb.Entity<ItemVenda>(e =>
        {
            e.Property(x => x.PrecoUnitario).HasColumnType("decimal(10,2)");
            e.HasIndex(x => x.VendaId);
            e.HasIndex(x => x.ProdutoId);
            e.HasOne(x => x.Venda).WithMany(v => v.Itens)
             .HasForeignKey(x => x.VendaId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Produto).WithMany(p => p.ItensVenda)
             .HasForeignKey(x => x.ProdutoId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
```

---

## 12. DTOs

Regra: **a API nunca recebe nem devolve entidades diretamente** — sempre DTOs. Isso evita referências circulares na serialização e desacopla o contrato do modelo. DTOs usam Data Annotations para validação de entrada.

```csharp
// DTOs/Produto/ProdutoCreateDto.cs
using System.ComponentModel.DataAnnotations;
namespace Garimpei.DTOs.Produto;

public class ProdutoCreateDto
{
    [Required, StringLength(120)] public string Nome { get; set; } = "";
    [StringLength(500)]          public string? Descricao { get; set; }
    [Required]                   public int CategoriaId { get; set; }
    [StringLength(20)]           public string? Tamanho { get; set; }
    [StringLength(30)]           public string? Cor { get; set; }
    [Range(0.01, 999999.99)]     public decimal Preco { get; set; }
    [Range(1, 3)]                public byte Estado { get; set; } // 1=Novo,2=Seminovo,3=Usado
}

// ProdutoUpdateDto: idêntico ao Create (todos os campos editáveis). Status NÃO entra aqui;
// alteração de disponibilidade tem endpoint próprio (PATCH). Ver Seção 13.
```

```csharp
// DTOs/Produto/ProdutoResponseDto.cs
namespace Garimpei.DTOs.Produto;

public class ProdutoResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string? Descricao { get; set; }
    public int CategoriaId { get; set; }
    public string CategoriaNome { get; set; } = "";
    public string? Tamanho { get; set; }
    public string? Cor { get; set; }
    public decimal Preco { get; set; }
    public byte Estado { get; set; }
    public string EstadoTexto { get; set; } = "";   // "Novo"/"Seminovo"/"Usado"
    public byte Status { get; set; }
    public string StatusTexto { get; set; } = "";    // "Disponível"/"Vendido"/"Inativo"
    public DateTime DataCadastro { get; set; }
}
```

```csharp
// DTOs/Produto/AlterarStatusDto.cs
namespace Garimpei.DTOs.Produto;
public class AlterarStatusDto { public byte Status { get; set; } } // só 1 (Disponível) ou 3 (Inativo)
```

```csharp
// DTOs/Categoria
public class CategoriaDto        { [Required, StringLength(60)] public string Nome { get; set; } = ""; }
public class CategoriaResponseDto { public int Id { get; set; } public string Nome { get; set; } = ""; public int QtdProdutos { get; set; } }
```

```csharp
// DTOs/Cliente
public class ClienteDto {
    [Required, StringLength(120)] public string Nome { get; set; } = "";
    [StringLength(20)]  public string? Telefone { get; set; }
    [EmailAddress, StringLength(120)] public string? Email { get; set; }
}
public class ClienteResponseDto {
    public int Id; public string Nome=""; public string? Telefone; public string? Email; public DateTime DataCadastro;
}
```

```csharp
// DTOs/Venda/VendaCreateDto.cs
using System.ComponentModel.DataAnnotations;
namespace Garimpei.DTOs.Venda;

public class VendaCreateDto
{
    public int? ClienteId { get; set; }                     // opcional
    [Required, MinLength(1)] public List<int> ProdutoIds { get; set; } = new(); // >= 1
}
```

```csharp
// DTOs/Venda/VendaResponseDto.cs
namespace Garimpei.DTOs.Venda;

public class VendaResponseDto
{
    public int Id { get; set; }
    public int? ClienteId { get; set; }
    public string? ClienteNome { get; set; }   // null = "Consumidor não identificado"
    public DateTime DataVenda { get; set; }
    public decimal Total { get; set; }
    public byte Status { get; set; }
    public string StatusTexto { get; set; } = "";
    public List<ItemVendaResponseDto> Itens { get; set; } = new();
}

public class ItemVendaResponseDto
{
    public int ProdutoId { get; set; }
    public string ProdutoNome { get; set; } = "";
    public decimal PrecoUnitario { get; set; }
}
```

```csharp
// DTOs/Comum/ErroResponse.cs — envelope de erro padronizado
namespace Garimpei.DTOs.Comum;
public class ErroResponse
{
    public bool Sucesso { get; set; } = false;
    public string Mensagem { get; set; } = "";
    public ErroResponse(string mensagem) => Mensagem = mensagem;
}
```

```csharp
// DTOs/Dashboard/DashboardDto.cs
namespace Garimpei.DTOs.Dashboard;
public class DashboardDto
{
    public int TotalProdutos { get; set; }
    public int ProdutosDisponiveis { get; set; }
    public int ProdutosVendidos { get; set; }
    public int TotalVendas { get; set; }          // vendas concluídas
    public decimal FaturamentoTotal { get; set; }
    public List<VendaResumoDto> UltimasVendas { get; set; } = new();
    public List<ProdutoResumoDto> ProdutosRecentes { get; set; } = new();
}
public class VendaResumoDto   { public int Id; public string Cliente=""; public decimal Total; public DateTime Data; }
public class ProdutoResumoDto { public int Id; public string Nome=""; public decimal Preco; public string Categoria=""; }
```

---

## 13. API REST

**Base URL:** `/api` · **Formato:** JSON (camelCase) · **Convenções de status:** ver Seção 19.

### 13.1 Produtos

| Método | URL | Objetivo | Body | Sucesso | Erros |
|---|---|---|---|---|---|
| GET | `/api/produtos` | Listar/filtrar produtos | — | 200 `[ProdutoResponseDto]` | 500 |
| GET | `/api/produtos/{id}` | Detalhe | — | 200 `ProdutoResponseDto` | 404 |
| POST | `/api/produtos` | Criar | `ProdutoCreateDto` | 201 + DTO | 400, 404 (categoria) |
| PUT | `/api/produtos/{id}` | Editar | `ProdutoUpdateDto` | 200 + DTO | 400, 404 |
| PATCH | `/api/produtos/{id}/status` | Alterar disponibilidade (Disponível↔Inativo) | `AlterarStatusDto` | 200 | 400, 404, 409 (se Vendido) |
| DELETE | `/api/produtos/{id}` | Excluir | — | 200 | 404, 409 (se vendido/em venda) |

**Query parameters de `GET /api/produtos`:**
- `busca` (string) — filtra por `Nome` contendo o termo (case-insensitive).
- `categoriaId` (int) — filtra por categoria.
- `status` (byte: 1/2/3) — filtra por status.

Todos combináveis. Ex.: `GET /api/produtos?busca=vestido&categoriaId=2&status=1`.

**Exemplo request `POST /api/produtos`:**
```json
{
  "nome": "Vestido Floral",
  "descricao": "Vestido midi estampado",
  "categoriaId": 2,
  "tamanho": "M",
  "cor": "Floral",
  "preco": 79.90,
  "estado": 2
}
```
**Response `201`:**
```json
{
  "id": 13, "nome": "Vestido Floral", "descricao": "Vestido midi estampado",
  "categoriaId": 2, "categoriaNome": "Vestidos", "tamanho": "M", "cor": "Floral",
  "preco": 79.90, "estado": 2, "estadoTexto": "Seminovo",
  "status": 1, "statusTexto": "Disponível", "dataCadastro": "2026-09-05T12:00:00Z"
}
```

### 13.2 Categorias

| Método | URL | Objetivo | Body | Sucesso | Erros |
|---|---|---|---|---|---|
| GET | `/api/categorias` | Listar (com QtdProdutos) | — | 200 `[CategoriaResponseDto]` | 500 |
| GET | `/api/categorias/{id}` | Detalhe | — | 200 | 404 |
| POST | `/api/categorias` | Criar | `CategoriaDto` | 201 | 400, 409 (nome duplicado) |
| PUT | `/api/categorias/{id}` | Editar | `CategoriaDto` | 200 | 400, 404, 409 |
| DELETE | `/api/categorias/{id}` | Excluir | — | 200 | 404, 409 (tem produtos) |

### 13.3 Clientes

| Método | URL | Objetivo | Body | Sucesso | Erros |
|---|---|---|---|---|---|
| GET | `/api/clientes?busca=` | Listar/buscar por nome | — | 200 `[ClienteResponseDto]` | 500 |
| GET | `/api/clientes/{id}` | Detalhe | — | 200 | 404 |
| POST | `/api/clientes` | Criar | `ClienteDto` | 201 | 400 |
| PUT | `/api/clientes/{id}` | Editar | `ClienteDto` | 200 | 400, 404 |
| DELETE | `/api/clientes/{id}` | Excluir | — | 200 | 404, 409 (tem vendas) |

### 13.4 Vendas

| Método | URL | Objetivo | Body | Sucesso | Erros |
|---|---|---|---|---|---|
| GET | `/api/vendas` | Listar vendas (resumo) | — | 200 `[VendaResponseDto]` | 500 |
| GET | `/api/vendas/{id}` | Detalhe (com itens) | — | 200 | 404 |
| POST | `/api/vendas` | Criar venda | `VendaCreateDto` | 201 + DTO | 400, 404 (cliente/produto), 409 (produto indisponível) |
| PATCH | `/api/vendas/{id}/cancelar` | Cancelar venda | — | 200 | 404, 409 (já cancelada) |

**Exemplo `POST /api/vendas`:**
```json
{ "clienteId": 1, "produtoIds": [3, 5, 7] }
```
**Response `201`:**
```json
{
  "id": 5, "clienteId": 1, "clienteNome": "Ana Beatriz Souza",
  "dataVenda": "2026-09-05T12:30:00Z", "total": 438.80,
  "status": 1, "statusTexto": "Concluída",
  "itens": [
    { "produtoId": 3, "produtoNome": "Calça Jeans Reta", "precoUnitario": 89.00 },
    { "produtoId": 5, "produtoNome": "Jaqueta Jeans", "precoUnitario": 149.90 },
    { "produtoId": 7, "produtoNome": "Vestido Longo Festa", "precoUnitario": 199.90 }
  ]
}
```

### 13.5 Dashboard

| Método | URL | Objetivo | Sucesso |
|---|---|---|---|
| GET | `/api/dashboard` | Indicadores + listas resumidas | 200 `DashboardDto` |

O frontend do dashboard usa **um único endpoint** (`GET /api/dashboard`) que retorna tudo: os 5 números, as últimas 5 vendas e os últimos 5 produtos cadastrados. Isso evita múltiplas chamadas e simplifica a tela.

### 13.6 Contrato de erro (todos os endpoints)
Qualquer erro (400/404/409/500) retorna:
```json
{ "sucesso": false, "mensagem": "Descrição legível do erro." }
```

---

## 14. Regras de negócio

> Esta é a seção de referência para o Antigravity sobre **comportamento esperado**. Todas as regras são impostas no **backend (Service)**; o frontend replica as mais simples para UX.

### Produto
- **RN-P01** Produto deve ter `Nome` não vazio.
- **RN-P02** Produto deve ter `Preco` > 0.
- **RN-P03** Produto deve ter `CategoriaId` válido (categoria existente). Caso contrário, `404`/`400`.
- **RN-P04** `Estado` deve ser 1, 2 ou 3.
- **RN-P05** Ao criar, `Status` = Disponível e `DataCadastro` = agora (definidos pelo backend, ignorando qualquer valor enviado).
- **RN-P06** Produto com `Status = Vendido` **não** pode ser editado de volta para Disponível via PATCH de status (`409`). Só volta a Disponível pelo **cancelamento da venda**.
- **RN-P07** Alterar disponibilidade via PATCH só permite alternar entre Disponível (1) e Inativo (3).
- **RN-P08** **Exclusão:** produto que já participou de qualquer venda (existe `ItemVenda` referenciando-o) **não pode ser excluído** → `409` com mensagem "Produto vinculado a vendas não pode ser excluído. Considere torná-lo Inativo." Produtos nunca vendidos podem ser excluídos.
- **RN-P09** Produto `Inativo` não aparece como disponível e **não pode ser adicionado a uma venda**.

### Categoria
- **RN-C01** `Nome` obrigatório e **único** (case-insensitive) → duplicado retorna `409`.
- **RN-C02** Categoria com produtos vinculados **não pode ser excluída** → `409` "Categoria possui produtos e não pode ser excluída."

### Cliente
- **RN-CL01** `Nome` obrigatório.
- **RN-CL02** `Email`, se preenchido, deve ter formato válido.
- **RN-CL03** Cliente com vendas vinculadas **não pode ser excluído** → `409`. (Alternativa aceitável: bloquear; **não** implementar anonimização.)

### Venda
- **RN-V01** Uma venda deve ter **pelo menos 1 produto** (`produtoIds` não vazio) → senão `400`.
- **RN-V02** Cliente é **opcional**. Se `clienteId` informado, deve existir (`404` se não).
- **RN-V03** **Todos** os produtos da venda devem existir e estar `Disponível` no momento da confirmação. Se **qualquer** um estiver Vendido/Inativo ou não existir → **a venda inteira é recusada** (`409`/`404`), nada é persistido.
- **RN-V04** O **`Total` é calculado pelo backend** somando o `Preco` atual de cada produto — o frontend **nunca** envia o total. Cada `ItemVenda.PrecoUnitario` recebe o `Preco` do produto naquele instante (snapshot).
- **RN-V05** Ao confirmar: cria `Venda` (Status=Concluída) + `ItensVenda`, e marca **cada produto** como `Vendido`. Tudo em **uma transação**: ou tudo grava, ou nada.
- **RN-V06** `DataVenda` = agora (backend).
- **RN-V07** **Cancelamento:** venda Concluída pode ser cancelada → `Status` = Cancelada e **todos os produtos da venda voltam a Disponível**. Também transacional. Venda já Cancelada → `409`.
- **RN-V08** Venda **não é editada** (não há endpoint de edição de venda). O caminho de correção é **cancelar** e criar nova. (Simplicidade proposital.)
- **RN-V09** Vendas **não são excluídas** pela UI (preserva histórico e faturamento). Só cancelamento.

### Dashboard
- **RN-D01** `FaturamentoTotal` = soma de `Total` das vendas com Status = **Concluída** (canceladas não contam).
- **RN-D02** `TotalVendas` = contagem de vendas Concluídas.
- **RN-D03** `ProdutosDisponiveis` = produtos com Status=Disponível; `ProdutosVendidos` = Status=Vendido; `TotalProdutos` = todos (inclui Inativos).
- **RN-D04** `UltimasVendas` = 5 vendas Concluídas mais recentes (por `DataVenda` desc).
- **RN-D05** `ProdutosRecentes` = 5 produtos por `DataCadastro` desc.

---

## 15. Validações

Regra geral: **validação de frontend melhora a UX; a validação de backend é a autoridade final.** Nunca confiar apenas no cliente.

### 15.1 Frontend (antes de enviar)
- **Produto:** Nome não vazio; Categoria selecionada; Preço numérico > 0; Estado selecionado. Bloquear submit e destacar campo com mensagem inline.
- **Cliente:** Nome não vazio; Email (se preenchido) com formato válido (regex simples/`type="email"`).
- **Categoria:** Nome não vazio.
- **Venda:** pelo menos 1 produto selecionado; botão "Confirmar" desabilitado se lista vazia.
- Exibir **loading** durante a requisição e **desabilitar** o botão de submit para evitar duplo clique.

### 15.2 Backend (autoridade)
Validação em **duas linhas de defesa**:
1. **Data Annotations nos DTOs** (`[Required]`, `[Range]`, `[StringLength]`, `[EmailAddress]`) → o ASP.NET Core retorna `400` automaticamente com `ModelState` inválido. Padronizar essa resposta para o envelope de erro (Seção 19).
2. **Regras de domínio no Service** (as RN da Seção 14) → retornam exceções de negócio traduzidas em `404`/`409`.

Tabela de validações por entidade:

| Entidade | Campo | Regra backend | Falha → |
|---|---|---|---|
| Produto | Nome | obrigatório, ≤120 | 400 |
| Produto | Preco | > 0 | 400 |
| Produto | CategoriaId | existe | 400/404 |
| Produto | Estado | 1..3 | 400 |
| Categoria | Nome | obrigatório, único ≤60 | 400/409 |
| Cliente | Nome | obrigatório ≤120 | 400 |
| Cliente | Email | formato (se houver) | 400 |
| Venda | ProdutoIds | ≥1 item | 400 |
| Venda | ClienteId | existe (se houver) | 404 |
| Venda | Produtos | todos Disponíveis | 409 |

---

## 16. Telas

Padrão de navegação: **layout com barra lateral (sidebar)** fixa à esquerda em desktop, que colapsa em menu no topo em telas pequenas. Links: Dashboard, Produtos, Clientes, Vendas.

Rotas (arquivos estáticos servidos por `wwwroot`):

| Tela | Arquivo | Rota (URL) |
|---|---|---|
| Dashboard | `index.html` | `/` |
| Produtos (lista) | `pages/produtos.html` | `/pages/produtos.html` |
| Produto (novo/editar) | `pages/produto-form.html` | `/pages/produto-form.html` e `?id=` |
| Clientes (lista) | `pages/clientes.html` | `/pages/clientes.html` |
| Cliente (novo/editar) | `pages/cliente-form.html` | `/pages/cliente-form.html?id=` |
| Vendas (lista) | `pages/vendas.html` | `/pages/vendas.html` |
| Nova venda | `pages/venda-form.html` | `/pages/venda-form.html` |

> **Nota de roteamento:** para simplicidade, usamos páginas HTML separadas (multipáginas), não um SPA. Cadastro e edição compartilham o mesmo formulário; o modo é decidido pela presença de `?id=` na URL. Categorias são gerenciadas dentro da tela de Produtos (um modal "Gerenciar categorias") para não multiplicar telas.

### 16.1 Dashboard (`/`)
- **Objetivo:** visão geral do negócio.
- **Componentes:** 5 **cards** de indicadores (Total de produtos, Disponíveis, Vendidos, Total de vendas, Faturamento total); tabela "Últimas vendas" (5 linhas: cliente, data, total); tabela/lista "Produtos cadastrados recentemente" (5 linhas: nome, categoria, preço).
- **Ações:** cards e listas são somente leitura; botões de atalho "Nova venda" e "Novo produto".
- **Dados:** `GET /api/dashboard`.
- **Estados:** loading (skeleton/spinner nos cards); vazio ("Nenhuma venda registrada ainda").
- **Erro:** toast "Não foi possível carregar o dashboard."
- **Responsivo:** cards em grid 5→2→1 colunas conforme largura.

### 16.2 Produtos — lista (`/pages/produtos.html`)
- **Objetivo:** listar, filtrar e gerenciar produtos.
- **Componentes:** barra de filtros (input busca por nome; select categoria; select status); botão "Novo produto"; botão "Gerenciar categorias" (abre modal); **tabela**.
- **Colunas da tabela:** Nome · Categoria · Tamanho · Cor · Preço (R$) · Estado · Status (badge colorido) · Ações.
- **Ações por linha:** Editar (lápis), Excluir (lixeira, com confirmação), alternar disponibilidade (toggle Disponível/Inativo — desabilitado se Vendido).
- **Dados:** `GET /api/produtos?busca=&categoriaId=&status=` (refaz a chamada a cada mudança de filtro, com debounce ~300ms na busca).
- **Mensagens:** sucesso ao excluir ("Produto excluído."); erro 409 ("Produto vinculado a vendas...").
- **Estados:** loading (linha "Carregando..."); vazio ("Nenhum produto encontrado.").
- **Responsivo:** em telas pequenas, tabela vira **cards** empilhados ou permite scroll horizontal.

### 16.3 Produto — formulário (`/pages/produto-form.html`)
- **Objetivo:** cadastrar (sem `?id=`) ou editar (com `?id=`).
- **Campos:** Nome (text, obrigatório) · Descrição (textarea) · Categoria (select, obrigatório, populado por `GET /api/categorias`) · Tamanho (text) · Cor (text) · Preço (number, step 0.01, obrigatório) · Estado (select: Novo/Seminovo/Usado, obrigatório).
- **Botões:** Salvar (submit) · Cancelar (volta à lista).
- **Comportamento:** se `?id=`, carrega `GET /api/produtos/{id}` e preenche; Salvar faz `POST` (novo) ou `PUT` (edição).
- **Mensagens:** sucesso ("Produto salvo com sucesso.") → redireciona para lista; erro de validação inline + toast.
- **Responsivo:** formulário em coluna única, largura máx. ~600px centralizado.

### 16.4 Clientes — lista (`/pages/clientes.html`)
- **Componentes:** input busca por nome; botão "Novo cliente"; tabela (Nome · Telefone · Email · Ações Editar/Excluir).
- **Dados:** `GET /api/clientes?busca=`.
- **Erro exclusão:** 409 → "Cliente possui vendas e não pode ser excluído."
- Estados vazio/loading análogos.

### 16.5 Cliente — formulário (`/pages/cliente-form.html`)
- **Campos:** Nome (obrigatório) · Telefone · Email. Botões Salvar/Cancelar. POST/PUT conforme `?id=`.

### 16.6 Vendas — lista (`/pages/vendas.html`)
- **Componentes:** botão "Nova venda"; tabela (Nº · Data · Cliente · Qtd itens · Total · Status badge · Ações).
- **Ações por linha:** Ver detalhes (abre modal com itens); Cancelar (confirmação; desabilitado se já Cancelada).
- **Dados:** `GET /api/vendas`; detalhe `GET /api/vendas/{id}`; cancelar `PATCH /api/vendas/{id}/cancelar`.
- **Mensagens:** "Venda cancelada. Produtos devolvidos ao estoque."

### 16.7 Nova venda (`/pages/venda-form.html`)
- **Objetivo:** montar e confirmar uma venda.
- **Componentes:**
  - Select **Cliente** (opcional; opção padrão "Consumidor não identificado"; populado por `GET /api/clientes`).
  - Área **"Adicionar produto"**: select/busca listando **apenas produtos Disponíveis** (`GET /api/produtos?status=1`); botão "Adicionar".
  - **Lista de itens da venda** (carrinho): Nome · Preço · botão Remover.
  - **Total** (calculado no cliente **apenas para exibição**; o valor oficial vem do backend na resposta).
  - Botão **Confirmar venda** (desabilitado com 0 itens) · botão Cancelar.
- **Comportamento:** ao Confirmar → `POST /api/vendas` com `{clienteId?, produtoIds[]}`. Sucesso → toast "Venda registrada!" e redireciona para lista de vendas. Um produto já adicionado não pode ser adicionado de novo (removê-lo do select ou impedir duplicata).
- **Erros:** 409 (algum produto ficou indisponível) → "Um ou mais produtos não estão mais disponíveis. Atualize a lista."
- **Responsivo:** em desktop, seleção à esquerda e carrinho à direita; em mobile, empilhado.

---

## 17. UX/UI

### 17.1 Identidade visual
Estética de brechó: acolhedora, artesanal, "vintage moderno" — tons terrosos suaves com um destaque quente. Limpa e legível, sem exageros.

### 17.2 Paleta de cores (CSS variables)
```css
:root{
  --cor-primaria:      #7C5C3E;  /* marrom terroso (marca) */
  --cor-primaria-esc:  #5E4429;
  --cor-destaque:      #C89B6B;  /* caramelo / dourado suave */
  --cor-fundo:         #FAF6F0;  /* off-white quente */
  --cor-superficie:    #FFFFFF;  /* cards */
  --cor-texto:         #2E2A26;
  --cor-texto-suave:   #6B6259;
  --cor-borda:         #E7DECF;
  --cor-sucesso:       #4E7C59;  /* verde */
  --cor-erro:          #B04A3C;  /* terracota/vermelho */
  --cor-aviso:         #C08A2D;
  /* Badges de status */
  --status-disponivel: #4E7C59;
  --status-vendido:    #8A8178;  /* cinza */
  --status-inativo:    #B58A3C;
}
```

### 17.3 Tipografia
- Fonte: **`'Poppins'`** ou **`'Inter'`** para textos (Google Fonts) com fallback `system-ui, sans-serif`. Títulos podem usar `'Poppins', 600`.
- Escala: título de página `1.5rem`; seção `1.125rem`; corpo `0.95rem`; auxiliar `0.8rem`.
- Se preferir zero dependências externas, usar apenas `system-ui` — aceitável.

### 17.4 Espaçamento e layout
- Grid de espaçamento base **8px** (4, 8, 16, 24, 32).
- Container central máx. `1100px`. Sidebar `220px`.
- `border-radius` padrão **10px**; sombra sutil `0 1px 3px rgba(0,0,0,.08)`.

### 17.5 Componentes
- **Botões:** primário (fundo `--cor-primaria`, texto branco), secundário (borda + texto primário), perigo (`--cor-erro`). Hover com leve escurecimento; `cursor:pointer`; estado desabilitado opaco.
- **Cards:** superfície branca, borda `--cor-borda`, padding 16–24px, radius 10px.
- **Tabelas:** cabeçalho com fundo `--cor-fundo`, linhas com separador `--cor-borda`, hover de linha suave. Números (preço) alinhados à direita.
- **Formulários:** labels acima do campo; inputs com borda `--cor-borda`, foco com borda `--cor-primaria`; mensagem de erro inline em `--cor-erro` abaixo do campo.
- **Badges de status:** pílula colorida (Disponível=verde, Vendido=cinza, Inativo=âmbar; Venda Concluída=verde, Cancelada=cinza/vermelho).
- **Modais:** overlay escurecido, card central, botão fechar; usados para "Gerenciar categorias", "Detalhes da venda", e confirmações de exclusão.
- **Alertas / Toasts:** notificação no canto superior direito, some sozinha após ~3s; verde para sucesso, vermelho para erro.
- **Loading:** spinner central ou linha "Carregando..." em tabelas; botões mostram estado ocupado e ficam desabilitados.
- **Estados vazios:** ícone/emoji + texto amigável ("Nenhum produto por aqui ainda. Que tal cadastrar o primeiro?") + botão de ação.
- **Mensagens de erro:** sempre a partir do campo `mensagem` do envelope de erro do backend; nunca mostrar stack trace.

### 17.6 Responsividade
- Breakpoints: `>=1024px` desktop (sidebar visível); `768–1023px` tablet; `<768px` mobile (sidebar vira menu hambúrguer/topo; tabelas com scroll horizontal ou viram cards).
- Grid de cards do dashboard: `repeat(auto-fit, minmax(180px, 1fr))`.

---

## 18. Fluxos completos

### 18.1 Cadastro de produto
```text
Usuário preenche formulário (produto-form.html, sem ?id=)
   → JS valida campos (nome, categoria, preço, estado)
   → fetch POST /api/produtos  (body ProdutoCreateDto)
      → ProdutosController.Criar(dto)
         → ModelState válido? não → 400 (envelope)
         → ProdutoService.CriarAsync(dto)
            → valida categoria existe (RN-P03)
            → cria Produto (Status=Disponível, DataCadastro=agora)
            → SaveChangesAsync
         → 201 Created + ProdutoResponseDto
   → JS: toast "Produto salvo" → redirect produtos.html
```

### 18.2 Edição de produto
```text
produto-form.html?id=5 → JS GET /api/produtos/5 → preenche form
Usuário edita → PUT /api/produtos/5 (ProdutoUpdateDto)
   → Controller → Service.AtualizarAsync(5, dto)
      → busca produto; não achou → 404
      → valida categoria; atualiza campos; SaveChanges
   → 200 + DTO → toast → redirect
```

### 18.3 Exclusão de produto
```text
Lista → clica lixeira → modal "Confirmar exclusão?"
   → DELETE /api/produtos/5
      → Service.ExcluirAsync(5)
         → não existe → 404
         → existe ItemVenda com ProdutoId=5 (RN-P08) → 409
         → senão remove; SaveChanges
      → 200
   → toast "Produto excluído" → remove linha da tabela
```

### 18.4 Cadastro de cliente
```text
cliente-form.html → valida nome/email → POST /api/clientes
   → ClienteService.CriarAsync → SaveChanges → 201
   → toast → redirect clientes.html
```

### 18.5 Nova venda
```text
venda-form.html
  → JS carrega clientes (GET /api/clientes) e produtos disponíveis (GET /api/produtos?status=1)
  → usuário escolhe cliente (opcional) e adiciona produtos ao "carrinho"
  → Confirmar → POST /api/vendas { clienteId?, produtoIds[] }
     → VendasController.Criar(dto)
        → dto.ProdutoIds vazio → 400 (RN-V01)
        → VendaService.CriarAsync(dto):
           BEGIN TRANSACTION
           → se clienteId informado, valida existência (RN-V02) senão 404
           → carrega produtos por ids
           → algum inexistente → 404 ; algum Status != Disponível → 409 (RN-V03), ROLLBACK
           → Total = soma dos Preco (RN-V04)
           → cria Venda + ItensVenda (PrecoUnitario = Preco snapshot)
           → marca cada Produto como Vendido (RN-V05)
           → SaveChanges ; COMMIT
        → 201 + VendaResponseDto
  → toast "Venda registrada!" → redirect vendas.html
  (Qualquer erro → ROLLBACK, nada persistido; frontend mostra mensagem)
```

### 18.6 Cancelamento de venda
```text
vendas.html → clica Cancelar → modal confirmação
  → PATCH /api/vendas/5/cancelar
     → VendaService.CancelarAsync(5):
        → venda não existe → 404
        → já Cancelada → 409 (RN-V07)
        BEGIN TRANSACTION
        → Status = Cancelada
        → cada produto dos itens volta a Disponível
        → SaveChanges ; COMMIT
     → 200
  → toast "Venda cancelada. Produtos devolvidos ao estoque." → atualiza lista
```

### 18.7 Dashboard
```text
index.html carrega → GET /api/dashboard
   → DashboardService.ObterAsync():
      → conta produtos por status; conta vendas concluídas; soma faturamento
      → últimas 5 vendas; últimos 5 produtos
   → 200 DashboardDto → JS preenche cards e tabelas
```

---

## 19. Tratamento de erros

### 19.1 Envelope padrão de erro
Toda resposta de erro tem o corpo:
```json
{ "sucesso": false, "mensagem": "Texto legível." }
```
Respostas de **sucesso** retornam o payload de dados diretamente (não envelopadas), com o status apropriado.

### 19.2 Códigos de status usados

| Status | Quando | Exemplo de mensagem |
|---|---|---|
| **200 OK** | GET/PUT/PATCH/DELETE bem-sucedidos | — (dados) |
| **201 Created** | POST criou recurso (com header `Location`) | — (dados) |
| **400 Bad Request** | Validação de entrada falhou (DTO/ModelState, regra simples) | "Preço deve ser maior que zero." |
| **404 Not Found** | Recurso não existe | "Produto não encontrado." |
| **409 Conflict** | Conflito de regra de negócio/estado | "Produto já vendido não pode ser vendido novamente." |
| **500 Internal Server Error** | Exceção não tratada | "Ocorreu um erro inesperado. Tente novamente." |

### 19.3 Implementação (simples, sem overengineering)
- **Exceções de negócio** custom: `NotFoundException` e `ConflictException` (herdam de `Exception`) lançadas pelos Services.
- **Um middleware** de tratamento global (`ErroMiddleware`) captura essas exceções e as traduz para o status + envelope corretos; qualquer outra exceção vira `500` com mensagem genérica (o detalhe vai para o log do servidor, **nunca** para o cliente).
- **ModelState inválido** (Data Annotations): configurar `ApiBehaviorOptions.InvalidModelStateResponseFactory` para retornar o mesmo envelope com `400`, concatenando as mensagens de validação.

```csharp
// Esboço do middleware
public class ErroMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErroMiddleware> _log;
    public ErroMiddleware(RequestDelegate next, ILogger<ErroMiddleware> log){ _next=next; _log=log; }

    public async Task Invoke(HttpContext ctx)
    {
        try { await _next(ctx); }
        catch (NotFoundException ex) { await Write(ctx, 404, ex.Message); }
        catch (ConflictException ex) { await Write(ctx, 409, ex.Message); }
        catch (Exception ex)
        {
            _log.LogError(ex, "Erro não tratado");
            await Write(ctx, 500, "Ocorreu um erro inesperado. Tente novamente.");
        }
    }
    private static Task Write(HttpContext ctx, int status, string msg)
    {
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json";
        return ctx.Response.WriteAsJsonAsync(new { sucesso = false, mensagem = msg });
    }
}
```

---

## 20. Segurança

Projeto acadêmico local: **sem autenticação** (justificado — usuário único, execução local, escopo de extensão). Ainda assim, aplicar boas práticas básicas:

- **SQL Injection:** eliminado por design — todo acesso a dados usa **EF Core** com queries parametrizadas/LINQ. **Nunca** concatenar strings SQL. Não usar `FromSqlRaw` com interpolação de input.
- **Validação de entrada:** dupla camada (Seção 15). Backend é a autoridade. Tamanhos máximos em todos os campos string (evita payloads absurdos).
- **CORS:** como frontend e API compartilham a **mesma origem** (servidos pelo mesmo processo), CORS não é necessário para produção local. Se, apenas em desenvolvimento, o frontend for aberto de outra porta, configurar uma política `AllowSpecificOrigins` restrita a `http://localhost:<porta>` — **nunca** `AllowAnyOrigin` liberado com credenciais. Recomendação: **servir tudo pela mesma origem e não habilitar CORS**.
- **Connection String / credenciais:** ficam em `appsettings.json` (dev) — **não** commitar senhas reais; usar **Autenticação Integrada do Windows** (`Trusted_Connection=True`) quando possível, evitando senha no arquivo. Documentar no README que segredos reais não vão para o Git (`.gitignore` cobrindo `appsettings.*.json` sensíveis, se houver).
- **Não expor detalhes internos:** erros `500` retornam mensagem genérica; stack traces só no log do servidor (Seção 19).
- **HTTPS:** habilitar redirecionamento HTTPS em desenvolvimento é opcional para localhost; documentar como opcional.
- **Headers/JSON:** `System.Text.Json` por padrão não é vulnerável a overposting porque usamos **DTOs de entrada** (o cliente não consegue setar `Status`, `Id`, `DataCadastro` — eles não existem nos DTOs de criação).

> **Por que EF Core (e não ADO.NET puro) para segurança:** o ORM parametriza automaticamente, reduz superfície de erro humano em SQL e é a prática recomendada — bom argumento de "boas práticas" na apresentação.

---

## 21. Plano de implementação para o Antigravity

Executar **em ordem**. Cada fase tem entregáveis e critério de "pronto".

### Fase 0 — Pré-requisitos (ambiente)
1. Instalar **.NET 8 SDK** e **SQL Server** (Express/Developer) + SSMS ou Azure Data Studio.
2. Confirmar `dotnet --version` ≥ 8.0.
- **Pronto quando:** `dotnet --info` e conexão ao SQL Server funcionam.

### Fase 1 — Estrutura do projeto
1. `dotnet new webapi -n Garimpei` (ou `web` + adicionar controllers). Remover exemplos gerados (WeatherForecast).
2. Adicionar pacotes NuGet: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.Tools`.
3. Criar pastas: `Controllers/ Models/ Data/ Services/ DTOs/ wwwroot/ Scripts/`.
4. Configurar `Program.cs`: `AddControllers`, `AddDbContext`, registrar Services (DI, `AddScoped`), `UseStaticFiles()`, `UseDefaultFiles()` (para servir `index.html` em `/`), `MapControllers`, middleware de erro, chamada ao seed.
5. Definir `appsettings.json` com `ConnectionStrings:DefaultConnection`.
- **Pronto quando:** `dotnet run` sobe e serve `wwwroot/index.html` em `http://localhost:5000`.

### Fase 2 — Banco de dados
1. Criar `Models/` (Seção 11) e `Enums.cs`.
2. Criar `Data/GarimpeiDbContext.cs` com Fluent API (Seção 11.1).
3. Gerar migration: `dotnet ef migrations add InitialCreate`.
4. Aplicar: `dotnet ef database update` (cria `GarimpeiDb`).
5. Criar `Data/DbSeeder.cs` (Seção 24) e chamá-lo no startup **somente se o banco estiver vazio**.
6. (Paralelo/alternativa) Escrever `Scripts/GarimpeiDb.sql` (Seção 10) equivalente.
- **Pronto quando:** banco criado, tabelas e seed presentes (conferir no SSMS).

### Fase 3 — Backend (por recurso, nesta ordem)
Para **cada** recurso: DTOs → Service → Controller → testar no navegador/Swagger.
1. **Categorias** (mais simples; produtos dependem dela).
2. **Produtos** (CRUD + filtros + PATCH status).
3. **Clientes** (CRUD).
4. **Vendas** (criar transacional + cancelar). Implementar `NotFoundException`/`ConflictException` e o `ErroMiddleware` aqui (ou antes).
5. **Dashboard** (`GET /api/dashboard`).
6. Habilitar **Swagger** em desenvolvimento para testar a API sem frontend.
- **Pronto quando:** todos os endpoints da Seção 13 respondem corretamente via Swagger.

### Fase 4 — Frontend
1. `css/styles.css` com o design system (Seção 17).
2. `js/api.js` (wrapper `fetch` que trata o envelope de erro e lança) e `js/ui.js` (toasts, modal, loading, formatação de moeda/data).
3. Layout base (sidebar + área de conteúdo) reutilizado em todas as páginas.
4. Telas na ordem: **Dashboard → Produtos (lista+form) → Categorias (modal) → Clientes (lista+form) → Vendas (lista) → Nova venda.**
- **Pronto quando:** todas as telas da Seção 16 existem e navegam.

### Fase 5 — Integração
1. Ligar cada tela aos endpoints reais.
2. Implementar loading, toasts de sucesso/erro, estados vazios.
3. Garantir que mensagens de erro venham do campo `mensagem` do backend.
- **Pronto quando:** todos os fluxos da Seção 18 funcionam fim a fim.

### Fase 6 — Testes e ajustes
1. Executar todos os casos de teste da Seção 23.
2. Verificar critérios de aceite (Seção 22).
3. Testar responsividade (desktop/tablet/mobile).
4. Revisar código (nomes claros, remover código morto).
5. Escrever `README.md` (Seção 25).
- **Pronto quando:** todos os testes passam e o README permite subir o projeto do zero.

---

## 22. Critérios de aceite

### Produtos
- [ ] Cadastrar produto válido → aparece na listagem.
- [ ] Cadastrar sem nome/sem categoria/preço ≤ 0 → backend recusa (400) e frontend mostra erro.
- [ ] Editar produto → mudanças refletidas na lista.
- [ ] Excluir produto nunca vendido → some da lista.
- [ ] Excluir produto que está em uma venda → recusado (409) com mensagem.
- [ ] Alternar Disponível↔Inativo funciona; Vendido não pode ser alternado.
- [ ] Filtros por busca, categoria e status funcionam (isolados e combinados).

### Categorias
- [ ] Criar categoria; nome duplicado é recusado (409).
- [ ] Excluir categoria com produtos é recusado (409).

### Clientes
- [ ] CRUD completo funciona.
- [ ] Email inválido é recusado.
- [ ] Excluir cliente com vendas é recusado (409).

### Vendas
- [ ] Criar venda com 1+ produtos disponíveis → total calculado pelo backend; produtos passam a Vendido.
- [ ] Criar venda com cliente vazio (avulsa) funciona.
- [ ] Tentar vender produto já vendido → recusado (409), nada persistido.
- [ ] Cancelar venda → status Cancelada e produtos voltam a Disponível.
- [ ] Faturamento do dashboard ignora vendas canceladas.

### Dashboard
- [ ] Os 5 indicadores batem com os dados do banco.
- [ ] "Últimas vendas" e "Produtos recentes" listam 5 itens ordenados corretamente.

### Geral
- [ ] Todas as telas responsivas (sem quebra em mobile).
- [ ] Nenhum erro 500 em uso normal; erros mostram mensagem amigável.

---

## 23. Casos de teste (manuais)

```text
TESTE 01 — Cadastrar produto válido
Passos: Produtos → Novo → preencher nome, categoria, preço 50.00, estado Usado → Salvar.
Esperado: 201; toast de sucesso; produto aparece na lista como "Disponível".

TESTE 02 — Cadastrar produto sem nome
Passos: Novo produto → deixar nome vazio → Salvar.
Esperado: bloqueio no frontend; se forçado, backend retorna 400 "Nome é obrigatório".

TESTE 03 — Cadastrar produto com preço 0
Esperado: 400 "Preço deve ser maior que zero."

TESTE 04 — Editar produto
Passos: editar preço de um produto → Salvar.
Esperado: 200; novo preço na lista.

TESTE 05 — Excluir produto nunca vendido
Esperado: 200; sai da lista.

TESTE 06 — Excluir produto que está em uma venda
Esperado: 409 "Produto vinculado a vendas não pode ser excluído."

TESTE 07 — Alternar disponibilidade
Passos: produto Disponível → toggle → Inativo → toggle → Disponível.
Esperado: status alterna; produto Inativo não aparece na Nova Venda.

TESTE 08 — Filtrar produtos
Passos: aplicar busca "vestido", categoria "Vestidos", status "Disponível".
Esperado: lista filtrada corretamente.

TESTE 09 — Criar categoria duplicada
Esperado: 409 "Categoria já existe."

TESTE 10 — Excluir categoria com produtos
Esperado: 409.

TESTE 11 — Cadastrar cliente com email inválido
Esperado: 400.

TESTE 12 — Excluir cliente com vendas
Esperado: 409.

TESTE 13 — Nova venda com 2 produtos e cliente
Passos: Nova Venda → cliente Ana → adicionar 2 produtos disponíveis → Confirmar.
Esperado: 201; total = soma dos preços (conferir); produtos viram Vendido; venda na lista.

TESTE 14 — Nova venda sem cliente (avulsa)
Esperado: 201; venda mostra "Consumidor não identificado".

TESTE 15 — Nova venda sem produtos
Esperado: botão Confirmar desabilitado; se forçado, 400.

TESTE 16 — Vender produto já vendido (condição de corrida / lista desatualizada)
Passos: abrir Nova Venda, deixar aberto, vender o produto em outra aba, voltar e confirmar.
Esperado: 409; nenhuma venda criada; mensagem "produto não está mais disponível".

TESTE 17 — Cancelar venda
Passos: Vendas → Cancelar uma venda concluída.
Esperado: 200; status Cancelada; produtos voltam a Disponível; faturamento diminui.

TESTE 18 — Cancelar venda já cancelada
Esperado: 409.

TESTE 19 — Dashboard confere
Passos: abrir Dashboard após os testes.
Esperado: indicadores coerentes (total produtos, disponíveis, vendidos, total de vendas, faturamento sem canceladas).

TESTE 20 — Responsividade
Passos: reduzir a janela / DevTools mobile.
Esperado: sidebar colapsa; tabelas acessíveis; formulários usáveis.

TESTE 21 — Erro de servidor tratado
Passos: derrubar o SQL Server e tentar listar produtos.
Esperado: mensagem amigável (não stack trace); status 500 com envelope.
```

---

## 24. Dados de demonstração

Estratégia: **seed idempotente** no startup. Em `Program.cs`, após `database update`/`EnsureCreated`, chamar `DbSeeder.Seed(context)` que **só insere se as tabelas estiverem vazias** (`if (!context.Categorias.Any())`). Assim o sistema abre já populado e a re-execução não duplica dados.

Conteúdo do seed = o mesmo da Seção 10 (6 categorias, 5 clientes fictícios, 12 produtos, 4 vendas com itens, ~R$344,50 de faturamento, misturando produtos disponíveis/vendidos/inativo). Nomes e dados **claramente fictícios** (ex.: "Ana Beatriz Souza", emails `@exemplo.com`).

```csharp
// Data/DbSeeder.cs (esboço)
public static class DbSeeder
{
    public static void Seed(GarimpeiDbContext db)
    {
        if (db.Categorias.Any()) return; // idempotente

        var cats = new[] {
            new Categoria{Nome="Camisetas"}, new Categoria{Nome="Vestidos"},
            new Categoria{Nome="Calças"},   new Categoria{Nome="Calçados"},
            new Categoria{Nome="Acessórios"},new Categoria{Nome="Jaquetas"}
        };
        db.Categorias.AddRange(cats);
        db.SaveChanges();
        // ... produtos, clientes, vendas + itens (marcar produtos vendidos, calcular totais)
        db.SaveChanges();
    }
}
```

> O Antigravity deve garantir que o conjunto final de dados após o seed reproduza os números esperados do dashboard (Seção 10) para que a apresentação seja consistente.

---

## 25. Conteúdo do README

O `README.md` deve conter, nesta ordem:

1. **Título e descrição** — "Garimpei — Sistema de Gestão para Brechós" + 1-2 parágrafos.
2. **Objetivo** — atividade de extensão + gestão de brechó.
3. **Tecnologias** — C#, .NET 8, ASP.NET Core Web API, EF Core, SQL Server, HTML/CSS/JS, REST/JSON (badges opcionais).
4. **Arquitetura** — o diagrama de camadas (Seção 5) + 1 parágrafo.
5. **Funcionalidades** — bullet list (produtos, categorias, clientes, vendas, dashboard).
6. **Pré-requisitos** — .NET 8 SDK, SQL Server, ferramenta de banco (SSMS/ADS).
7. **Configuração do SQL Server** — como confirmar a instância e o nome do servidor.
8. **Connection string** — onde editar (`appsettings.json`), exemplos:
   - LocalDB (padrão de desenvolvimento): `Server=(localdb)\\MSSQLLocalDB;Database=GarimpeiDb;Trusted_Connection=True;TrustServerCertificate=True;`
   - SQL Server integrado: `Server=localhost;Database=GarimpeiDb;Trusted_Connection=True;TrustServerCertificate=True;`
   - Com usuário: `Server=localhost;Database=GarimpeiDb;User Id=sa;Password=***;TrustServerCertificate=True;`
9. **Como criar o banco** — duas opções: (a) `dotnet ef database update` (recomendado) **ou** (b) rodar `Scripts/GarimpeiDb.sql` no SSMS.
10. **Como executar o backend** — `dotnet run` (bloco de código com botão Run).
11. **Como acessar** — abrir `http://localhost:5000`.
12. **Dados de demonstração** — o sistema já vem populado via seed.
13. **Estrutura do projeto** — árvore de pastas resumida (Seção 7).
14. **Credenciais** — "Não há login; sistema de uso local sem autenticação (ver justificativa)."
15. **Observações** — limitações conhecidas e ideias de evolução (upload de imagem, etc.).

Comandos-chave a incluir no README:

```bash
dotnet restore
```
```bash
dotnet ef database update
```
```bash
dotnet run
```

---

## 26. DECISÕES IMPORTANTES PARA O ANTIGRAVITY

> Estas decisões são **finais**. O Antigravity **não deve alterá-las** sem instrução explícita do autor.

1. **Stack fixa:** ASP.NET Core Web API (.NET 8) + HTML/CSS/JavaScript **puro** + EF Core + SQL Server. **Não** trocar por MVC/Razor, Web Forms, Blazor, React, Angular, Vue, TypeScript, Node.
2. **Sem SPA framework e sem build tooling** (nada de npm/webpack/vite). JavaScript ES6 nativo com `fetch`.
3. **Banco:** SQL Server. **Não** trocar por SQLite/Postgres/MySQL.
4. **Acesso a dados:** EF Core (Code First). **Não** usar Dapper/ADO.NET, **nem** Repository Pattern, Unit of Work, CQRS, MediatR, AutoMapper (mapeamento manual DTO↔entidade é suficiente e mais legível).
5. **Sem autenticação/autorização** (Identity, JWT, cookies de login). Uso local, usuário único.
6. **Sem Docker, Kubernetes, microserviços, mensageria, cache distribuído, cloud.**
7. **Camadas:** apenas Controller → Service → DbContext. Não criar camadas extras.
8. **Modelo de dados fixo:** 5 tabelas (Categorias, Clientes, Produtos, Vendas, ItensVenda). **Não** adicionar tabelas (Usuario, Fornecedor, Pagamento, Estoque). **Não** adicionar campo `Quantidade` em ItemVenda (peças são únicas).
9. **Total da venda é sempre calculado no backend.** O frontend nunca envia o total.
10. **Vendas não são editadas nem excluídas pela UI** — apenas criadas e canceladas.
11. **Produto vendido não pode ser vendido novamente nem excluído** (torna-se histórico).
12. **Não implementar upload de imagens** na entrega base (fica como evolução futura documentada).
13. **Sem testes automatizados** — usar os testes manuais da Seção 23.
14. **Envelope de erro fixo:** `{ "sucesso": false, "mensagem": "..." }`. Sucesso retorna os dados diretamente.
15. **Não criar funcionalidades não solicitadas.** Prioridade absoluta: **código simples, legível e funcional.**
16. **Enums persistidos como `byte/TINYINT`** (mapeados por conversão), conforme Seção 11. Não usar strings no banco para status/estado.
17. **Frontend e API na mesma origem** (mesma porta, servido por `UseStaticFiles`). Evitar CORS.

---

## 27. CHECKLIST DE EXECUÇÃO DO ANTIGRAVITY

Executar estritamente nesta ordem. Marcar cada item ao concluir.

```text
FASE 1 — Estrutura
01. Criar projeto Web API .NET 8 (Garimpei) e remover exemplos gerados
02. Adicionar pacotes EF Core (SqlServer, Design, Tools)
03. Criar pastas: Controllers, Models, Data, Services, DTOs, wwwroot, Scripts
04. Configurar Program.cs (DI, DbContext, UseStaticFiles/UseDefaultFiles, MapControllers, middleware de erro)
05. Configurar appsettings.json (connection string)

FASE 2 — Banco
06. Criar Enums.cs e Models (Categoria, Cliente, Produto, Venda, ItemVenda)
07. Criar GarimpeiDbContext com Fluent API (constraints, índices, relacionamentos)
08. Gerar migration InitialCreate e aplicar (dotnet ef database update)
09. Criar DbSeeder idempotente e chamá-lo no startup
10. (Alternativa) Escrever Scripts/GarimpeiDb.sql equivalente

FASE 3 — Backend
11. Criar DTOs de todos os recursos + ErroResponse
12. Criar exceções NotFoundException/ConflictException e ErroMiddleware
13. Implementar Categorias (Service + Controller): GET, GET id, POST, PUT, DELETE
14. Implementar Produtos (Service + Controller): GET(filtros), GET id, POST, PUT, PATCH status, DELETE
15. Implementar Clientes (Service + Controller): CRUD + busca
16. Implementar Vendas (Service + Controller): GET, GET id, POST(transacional), PATCH cancelar
17. Implementar Dashboard (Service + Controller): GET /api/dashboard
18. Habilitar Swagger em Development e testar todos os endpoints

FASE 4 — Frontend
19. Criar css/styles.css (design system, responsivo)
20. Criar js/api.js (wrapper fetch + erro) e js/ui.js (toast, modal, loading, formatação)
21. Criar layout base (sidebar + conteúdo)
22. Criar Dashboard (index.html + dashboard.js)
23. Criar Produtos: lista (produtos.html) + formulário (produto-form.html) + modal categorias
24. Criar Clientes: lista + formulário
25. Criar Vendas: lista (vendas.html) + Nova venda (venda-form.html)

FASE 5 — Integração
26. Conectar todas as telas aos endpoints
27. Implementar loading, toasts, estados vazios e mensagens de erro do backend
28. Garantir validações de UX no frontend

FASE 6 — Testes e entrega
29. Executar casos de teste 01–21 (Seção 23)
30. Verificar critérios de aceite (Seção 22)
31. Testar responsividade (desktop/tablet/mobile)
32. Revisar código (legibilidade, remover código morto)
33. Escrever README.md (Seção 25)
```

---

### Fim do documento

**Resumo da decisão-chave:** ASP.NET Core Web API (.NET 8) + HTML/CSS/JS puro + EF Core + SQL Server — a melhor relação entre **simplicidade de implementação** e **valor de portfólio Full Stack**, mantendo o projeto propositalmente pequeno, profissional e academicamente defensável. Todas as 27 seções acima constituem a especificação executável a ser seguida pelo Antigravity sem necessidade de decisões arquiteturais adicionais.
