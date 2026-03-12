using MeAjudaAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Infrastructure.Persistence.Configurations.Entities;

public class PortfolioFotoConfiguration : IEntityTypeConfiguration<PortfolioFoto>
{
    public void Configure(EntityTypeBuilder<PortfolioFoto> builder)
    {
        builder.ToTable("portfolio_fotos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UrlArquivo)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Legenda)
            .HasMaxLength(300);

        builder.Property(x => x.Ordem)
            .IsRequired();

        builder.Property(x => x.Ativo)
            .IsRequired();

        builder.Property(x => x.DataCriacao)
            .IsRequired();

        builder.HasIndex(x => new { x.ProfissionalId, x.Ordem });

        builder.HasOne(x => x.Profissional)
            .WithMany(x => x.PortfolioFotos)
            .HasForeignKey(x => x.ProfissionalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}