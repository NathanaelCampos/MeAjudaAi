using MeAjudaAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Infrastructure.Persistence.Configurations.Entities;

public class ImpulsionamentoProfissionalConfiguration : IEntityTypeConfiguration<ImpulsionamentoProfissional>
{
    public void Configure(EntityTypeBuilder<ImpulsionamentoProfissional> builder)
    {
        builder.ToTable("impulsionamentos_profissionais");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DataInicio)
            .IsRequired();

        builder.Property(x => x.DataFim)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.ValorPago)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.CodigoReferenciaPagamento)
            .HasMaxLength(150);

        builder.Property(x => x.Ativo)
            .IsRequired();

        builder.Property(x => x.DataCriacao)
            .IsRequired();

        builder.HasIndex(x => new { x.ProfissionalId, x.Status });
        builder.HasIndex(x => new { x.PlanoImpulsionamentoId, x.Status });

        builder.HasOne(x => x.Profissional)
            .WithMany(x => x.Impulsionamentos)
            .HasForeignKey(x => x.ProfissionalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.PlanoImpulsionamento)
            .WithMany(x => x.Impulsionamentos)
            .HasForeignKey(x => x.PlanoImpulsionamentoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}