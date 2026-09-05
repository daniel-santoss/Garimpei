-- =============================================================
-- Garimpei — Script de criação do banco (SQL Server)
-- Alternativa MANUAL às migrations do EF Core. Use um OU outro.
-- Produz o mesmo schema. Executar no SSMS ou Azure Data Studio.
-- (ver ENGENHARIA-GARIMPEI.md, Seção 10)
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
    CONSTRAINT PK_Categorias      PRIMARY KEY (Id),
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
    CONSTRAINT PK_Produtos            PRIMARY KEY (Id),
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
    CONSTRAINT PK_Vendas          PRIMARY KEY (Id),
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
    CONSTRAINT PK_ItensVenda          PRIMARY KEY (Id),
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

INSERT INTO dbo.Categorias (Nome) VALUES
 (N'Camisetas'), (N'Vestidos'), (N'Calças'), (N'Calçados'), (N'Acessórios'), (N'Jaquetas');
GO

INSERT INTO dbo.Clientes (Nome, Telefone, Email) VALUES
 (N'Ana Beatriz Souza',    N'(11) 99999-1010', N'ana.souza@exemplo.com'),
 (N'Carlos Henrique Lima', N'(11) 98888-2020', N'carlos.lima@exemplo.com'),
 (N'Marina Oliveira',      N'(21) 97777-3030', N'marina.oliveira@exemplo.com'),
 (N'João Pedro Alves',     NULL,               NULL),
 (N'Fernanda Costa',       N'(31) 96666-4040', N'fernanda.costa@exemplo.com');
GO

-- Estado: 1=Novo,2=Seminovo,3=Usado | Status: 1=Disponível,2=Vendido,3=Inativo
INSERT INTO dbo.Produtos (Nome, Descricao, CategoriaId, Tamanho, Cor, Preco, Estado, Status) VALUES
 (N'Vestido Floral Vintage', N'Vestido midi estampado, tecido leve.', 2, N'M',  N'Floral', 79.90, 2, 1),
 (N'Camiseta Básica Branca', N'Algodão, gola redonda.',               1, N'G',  N'Branco', 24.90, 1, 1),
 (N'Calça Jeans Reta',       N'Jeans clássico, cintura média.',       3, N'40', N'Azul',   89.00, 3, 1),
 (N'Tênis Casual',           N'Tênis de lona, pouco uso.',            4, N'38', N'Bege',  119.90, 2, 1),
 (N'Jaqueta Jeans',          N'Jaqueta oversized.',                   6, N'M',  N'Azul',  149.90, 2, 1),
 (N'Bolsa de Couro',         N'Bolsa média, alça ajustável.',         5, NULL, N'Marrom', 99.90, 3, 1),
 (N'Vestido Longo Festa',    N'Vestido de festa, usado uma vez.',     2, N'P',  N'Vinho', 199.90, 2, 1),
 (N'Camisa Social',          N'Camisa manga longa.',                  1, N'M',  N'Azul',   59.90, 2, 1),
 (N'Sapatênis Marrom',       N'Confortável, sola nova.',              4, N'41', N'Marrom', 89.90, 2, 1),
 (N'Cinto de Couro',         N'Cinto marrom clássico.',               5, N'U',  N'Marrom', 39.90, 3, 1),
 (N'Blusa de Tricô',         N'Blusa quentinha para o inverno.',      1, N'G',  N'Cinza',  49.90, 2, 3),
 (N'Saia Plissada',          N'Saia midi plissada.',                  2, N'M',  N'Preto',  54.90, 2, 1);
GO

-- Venda 1: Ana, 2 itens (produtos 2 e 8)
INSERT INTO dbo.Vendas (ClienteId, Total, Status, DataVenda) VALUES (1, 84.80, 1, DATEADD(DAY,-10,SYSUTCDATETIME()));
DECLARE @v1 INT = SCOPE_IDENTITY();
INSERT INTO dbo.ItensVenda (VendaId, ProdutoId, PrecoUnitario) VALUES (@v1, 2, 24.90), (@v1, 8, 59.90);
UPDATE dbo.Produtos SET Status = 2 WHERE Id IN (2,8);
GO

-- Venda 2: Carlos, 1 item (produto 4)
INSERT INTO dbo.Vendas (ClienteId, Total, Status, DataVenda) VALUES (2, 119.90, 1, DATEADD(DAY,-5,SYSUTCDATETIME()));
DECLARE @v2 INT = SCOPE_IDENTITY();
INSERT INTO dbo.ItensVenda (VendaId, ProdutoId, PrecoUnitario) VALUES (@v2, 4, 119.90);
UPDATE dbo.Produtos SET Status = 2 WHERE Id = 4;
GO

-- Venda 3: sem cliente (avulsa), 1 item (produto 6)
INSERT INTO dbo.Vendas (ClienteId, Total, Status, DataVenda) VALUES (NULL, 99.90, 1, DATEADD(DAY,-2,SYSUTCDATETIME()));
DECLARE @v3 INT = SCOPE_IDENTITY();
INSERT INTO dbo.ItensVenda (VendaId, ProdutoId, PrecoUnitario) VALUES (@v3, 6, 99.90);
UPDATE dbo.Produtos SET Status = 2 WHERE Id = 6;
GO

-- Venda 4: Marina, 1 item (produto 10)
INSERT INTO dbo.Vendas (ClienteId, Total, Status, DataVenda) VALUES (3, 39.90, 1, DATEADD(DAY,-1,SYSUTCDATETIME()));
DECLARE @v4 INT = SCOPE_IDENTITY();
INSERT INTO dbo.ItensVenda (VendaId, ProdutoId, PrecoUnitario) VALUES (@v4, 10, 39.90);
UPDATE dbo.Produtos SET Status = 2 WHERE Id = 10;
GO

PRINT 'Banco GarimpeiDb criado e populado com sucesso.';
GO
