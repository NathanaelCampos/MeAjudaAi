using MeAjudaAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Infrastructure.Persistence.Configurations.Entities;

public class FormaRecebimentoConfiguration : IEntityTypeConfiguration<FormaRecebimento>
{
    public void Configure(EntityTypeBuilder<FormaRecebimento> builder)
    {
        builder.ToTable("formas_recebimento");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TipoFormaRecebimento)
            .IsRequired();

        builder.Property(x => x.Descricao)
            .HasMaxLength(300);

        builder.Property(x => x.Ativo)
            .IsRequired();

        builder.Property(x => x.DataCriacao)
            .IsRequired();

        builder.HasIndex(x => new { x.ProfissionalId, x.TipoFormaRecebimento });

        builder.HasOne(x => x.Profissional)
            .WithMany(x => x.FormasRecebimento)
            .HasForeignKey(x => x.ProfissionalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}