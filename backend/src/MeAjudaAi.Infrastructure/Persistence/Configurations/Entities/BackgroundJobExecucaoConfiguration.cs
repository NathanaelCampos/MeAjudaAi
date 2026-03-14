using MeAjudaAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Infrastructure.Persistence.Configurations.Entities;

public class BackgroundJobExecucaoConfiguration : IEntityTypeConfiguration<BackgroundJobExecucao>
{
    public void Configure(EntityTypeBuilder<BackgroundJobExecucao> builder)
    {
        builder.ToTable("background_jobs_execucoes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.JobId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.NomeJob)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Origem)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.TentativasProcessamento)
            .IsRequired();

        builder.Property(x => x.RegistrosProcessados)
            .IsRequired();

        builder.Property(x => x.MensagemResultado)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Ativo)
            .IsRequired();

        builder.Property(x => x.DataCriacao)
            .IsRequired();

        builder.HasIndex(x => new { x.Status, x.ProcessarAposUtc });
        builder.HasIndex(x => new { x.JobId, x.DataCriacao });

        builder.HasOne(x => x.SolicitadoPorAdminUsuario)
            .WithMany()
            .HasForeignKey(x => x.SolicitadoPorAdminUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
