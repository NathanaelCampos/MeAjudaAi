using MeAjudaAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Infrastructure.Persistence.Configurations.Entities;

public class EmailNotificacaoOutboxConfiguration : IEntityTypeConfiguration<EmailNotificacaoOutbox>
{
    public void Configure(EntityTypeBuilder<EmailNotificacaoOutbox> builder)
    {
        builder.ToTable("emails_notificacoes_outbox");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TipoNotificacao)
            .IsRequired();

        builder.Property(x => x.EmailDestino)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Assunto)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Corpo)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.TentativasProcessamento)
            .IsRequired();

        builder.Property(x => x.UltimaMensagemErro)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Ativo)
            .IsRequired();

        builder.Property(x => x.DataCriacao)
            .IsRequired();

        builder.HasIndex(x => new { x.Status, x.ProximaTentativaEm });
        builder.HasIndex(x => x.UsuarioId);

        builder.HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
