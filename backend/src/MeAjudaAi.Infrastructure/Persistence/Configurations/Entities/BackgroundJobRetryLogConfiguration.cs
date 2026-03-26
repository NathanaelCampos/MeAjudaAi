using MeAjudaAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Infrastructure.Persistence.Configurations.Entities;

public class BackgroundJobRetryLogConfiguration : IEntityTypeConfiguration<BackgroundJobRetryLog>
{
    public void Configure(EntityTypeBuilder<BackgroundJobRetryLog> builder)
    {
        builder.ToTable("background_job_retry_logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.JobId).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Tipo).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Mensagem).IsRequired().HasMaxLength(500);
        builder.Property(x => x.DataCriacao).IsRequired();
        builder.Property(x => x.Ativo).IsRequired();
    }
}
