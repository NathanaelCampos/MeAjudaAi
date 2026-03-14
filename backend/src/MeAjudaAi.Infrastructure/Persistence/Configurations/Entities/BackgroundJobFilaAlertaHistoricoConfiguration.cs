using MeAjudaAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Infrastructure.Persistence.Configurations.Entities;

public class BackgroundJobFilaAlertaHistoricoConfiguration : IEntityTypeConfiguration<BackgroundJobFilaAlertaHistorico>
{
    public void Configure(EntityTypeBuilder<BackgroundJobFilaAlertaHistorico> builder)
    {
        builder.ToTable("background_job_fila_alerta_historico");

        builder.Property(x => x.JobId)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.NivelAlerta)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Mensagem)
            .IsRequired()
            .HasMaxLength(400);

        builder.Property(x => x.Cor)
            .IsRequired()
            .HasMaxLength(25);

        builder.Property(x => x.TempoMedioFilaSegundos).IsRequired();
        builder.Property(x => x.TempoMedioProcessamentoSegundos).IsRequired();
        builder.Property(x => x.TotalPendentes).IsRequired();
        builder.Property(x => x.TotalFalhas).IsRequired();
    }
}
