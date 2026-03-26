using MeAjudaAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Infrastructure.Persistence.Configurations.Entities;

public class BackgroundJobFilaAlertaNotificacaoEstadoConfiguration : IEntityTypeConfiguration<BackgroundJobFilaAlertaNotificacaoEstado>
{
    public void Configure(EntityTypeBuilder<BackgroundJobFilaAlertaNotificacaoEstado> builder)
    {
        builder.ToTable("background_job_fila_alerta_notificacao_estado");

        builder.Property(x => x.Chave)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.UltimaNotificacaoEm)
            .IsRequired();
    }
}
