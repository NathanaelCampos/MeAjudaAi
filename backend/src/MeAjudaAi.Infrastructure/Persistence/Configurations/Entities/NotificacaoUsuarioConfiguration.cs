using MeAjudaAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Infrastructure.Persistence.Configurations.Entities;

public class NotificacaoUsuarioConfiguration : IEntityTypeConfiguration<NotificacaoUsuario>
{
    public void Configure(EntityTypeBuilder<NotificacaoUsuario> builder)
    {
        builder.ToTable("notificacoes_usuarios");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Titulo)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(x => x.Mensagem)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Tipo)
            .IsRequired();

        builder.Property(x => x.Ativo)
            .IsRequired();

        builder.Property(x => x.DataCriacao)
            .IsRequired();

        builder.HasIndex(x => new { x.UsuarioId, x.DataCriacao });
        builder.HasIndex(x => new { x.UsuarioId, x.DataLeitura });

        builder.HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
