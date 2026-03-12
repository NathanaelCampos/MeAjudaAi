using MeAjudaAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Infrastructure.Persistence.Configurations.Entities;

public class PlanoImpulsionamentoConfiguration : IEntityTypeConfiguration<PlanoImpulsionamento>
{
    public void Configure(EntityTypeBuilder<PlanoImpulsionamento> builder)
    {
        builder.ToTable("planos_impulsionamento");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.TipoPeriodo)
            .IsRequired();

        builder.Property(x => x.QuantidadePeriodo)
            .IsRequired();

        builder.Property(x => x.Valor)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Ativo)
            .IsRequired();

        builder.Property(x => x.DataCriacao)
            .IsRequired();

        builder.HasIndex(x => x.Nome);
    }
}