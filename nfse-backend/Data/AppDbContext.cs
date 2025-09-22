using Microsoft.EntityFrameworkCore;
using nfse_backend.Models;
using System.Text.Json; // Adicionado para serialização/desserialização JSON
using Microsoft.EntityFrameworkCore.Storage.ValueConversion; // Adicionado para ValueConverter

namespace nfse_backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Nfse> Nfses { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Company> Companies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Nfse>().OwnsOne(n => n.Prestador);
        modelBuilder.Entity<Nfse>().OwnsOne(n => n.Tomador);
        
        // Remover OwnsMany e ToJson() para Servicos
        // Usar um ValueConverter para serializar/desserializar List<ServicoItem> para JSON string
        var servicosConverter = new ValueConverter<List<ServicoItem>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), // Serialize List<ServicoItem> to JSON string
            v => JsonSerializer.Deserialize<List<ServicoItem>>(v, (JsonSerializerOptions?)null) ?? new List<ServicoItem>() // Deserialize JSON string to List<ServicoItem>
        );

        modelBuilder.Entity<Nfse>()
            .Property(n => n.Servicos)
            .HasConversion(servicosConverter)
            .HasColumnType("json"); // Indicar ao MySQL que é uma coluna JSON

        modelBuilder.Entity<Nfse>().OwnsOne(n => n.InformacoesAdicionais);
    }
}