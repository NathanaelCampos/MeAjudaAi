using MeAjudaAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Infrastructure.Persistence.Configurations.Entities;

public class AuditoriaAdminAcaoConfiguration : IEntityTypeConfiguration<AuditoriaAdminAcao>
{
    public void Configure(EntityTypeBuilder<AuditoriaAdminAcao> builder)
    {
        builder.ToTable("auditorias_admin_acoes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Entidade)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Acao)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Descricao)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.PayloadJson)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(x => x.Ativo)
            .IsRequired();

        builder.Property(x => x.DataCriacao)
            .IsRequired();

        builder.HasIndex(x => new { x.AdminUsuarioId, x.DataCriacao });
        builder.HasIndex(x => new { x.Entidade, x.EntidadeId, x.DataCriacao });
        builder.HasIndex(x => new { x.Acao, x.DataCriacao });

        builder.HasOne(x => x.AdminUsuario)
            .WithMany()
            .HasForeignKey(x => x.AdminUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
