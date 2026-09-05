using Garimpei.Models;
using Microsoft.EntityFrameworkCore;

namespace Garimpei.Data;

public class GarimpeiDbContext : DbContext
{
    public GarimpeiDbContext(DbContextOptions<GarimpeiDbContext> options) : base(options) { }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<ItemVenda> ItensVenda => Set<ItemVenda>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ---------- Categoria ----------
        mb.Entity<Categoria>(e =>
        {
            e.Property(x => x.Nome).HasMaxLength(60).IsRequired();
            e.HasIndex(x => x.Nome).IsUnique();
            e.Property(x => x.DataCadastro).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        // ---------- Cliente ----------
        mb.Entity<Cliente>(e =>
        {
            e.Property(x => x.Nome).HasMaxLength(120).IsRequired();
            e.Property(x => x.Telefone).HasMaxLength(20);
            e.Property(x => x.Email).HasMaxLength(120);
            e.HasIndex(x => x.Nome);
            e.Property(x => x.DataCadastro).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        // ---------- Produto ----------
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
            e.HasOne(x => x.Categoria)
             .WithMany(c => c.Produtos)
             .HasForeignKey(x => x.CategoriaId)
             .OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Produtos_Preco", "[Preco] > 0");
                t.HasCheckConstraint("CK_Produtos_Estado", "[Estado] IN (1,2,3)");
                t.HasCheckConstraint("CK_Produtos_Status", "[Status] IN (1,2,3)");
            });
        });

        // ---------- Venda ----------
        mb.Entity<Venda>(e =>
        {
            e.Property(x => x.Total).HasColumnType("decimal(10,2)");
            e.Property(x => x.Status).HasConversion<byte>().HasDefaultValue(StatusVenda.Concluida);
            e.Property(x => x.DataVenda).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(x => x.ClienteId);
            e.HasIndex(x => x.DataVenda);
            e.HasOne(x => x.Cliente)
             .WithMany(c => c.Vendas)
             .HasForeignKey(x => x.ClienteId)
             .OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Vendas_Total", "[Total] >= 0");
                t.HasCheckConstraint("CK_Vendas_Status", "[Status] IN (1,2)");
            });
        });

        // ---------- ItemVenda ----------
        mb.Entity<ItemVenda>(e =>
        {
            e.Property(x => x.PrecoUnitario).HasColumnType("decimal(10,2)");
            e.HasIndex(x => x.VendaId);
            e.HasIndex(x => x.ProdutoId);
            e.HasOne(x => x.Venda)
             .WithMany(v => v.Itens)
             .HasForeignKey(x => x.VendaId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Produto)
             .WithMany(p => p.ItensVenda)
             .HasForeignKey(x => x.ProdutoId)
             .OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t => t.HasCheckConstraint("CK_ItensVenda_Preco", "[PrecoUnitario] >= 0"));
        });
    }
}
