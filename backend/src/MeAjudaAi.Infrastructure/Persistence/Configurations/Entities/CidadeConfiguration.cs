using MeAjudaAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Infrastructure.Persistence.Configurations.Entities;

public class CidadeConfiguration : IEntityTypeConfiguration<Cidade>
{
    public void Configure(EntityTypeBuilder<Cidade> builder)
    {
        builder.ToTable("cidades");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.CodigoIbge)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.Ativo)
            .IsRequired();

        builder.Property(x => x.DataCriacao)
            .IsRequired();

        builder.HasIndex(x => x.CodigoIbge).IsUnique();
        builder.HasIndex(x => new { x.EstadoId, x.Nome });

        builder.HasOne(x => x.Estado)
            .WithMany(x => x.Cidades)
            .HasForeignKey(x => x.EstadoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}