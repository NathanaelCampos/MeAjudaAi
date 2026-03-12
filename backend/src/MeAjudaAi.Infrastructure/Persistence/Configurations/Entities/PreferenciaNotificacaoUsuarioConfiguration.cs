using MeAjudaAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Infrastructure.Persistence.Configurations.Entities;

public class PreferenciaNotificacaoUsuarioConfiguration : IEntityTypeConfiguration<PreferenciaNotificacaoUsuario>
{
    public void Configure(EntityTypeBuilder<PreferenciaNotificacaoUsuario> builder)
    {
        builder.ToTable("preferencias_notificacoes_usuarios");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Tipo)
            .IsRequired();

        builder.Property(x => x.AtivoInterno)
            .IsRequired();

        builder.Property(x => x.Ativo)
            .IsRequired();

        builder.Property(x => x.DataCriacao)
            .IsRequired();

        builder.HasIndex(x => new { x.UsuarioId, x.Tipo })
            .IsUnique();

        builder.HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
