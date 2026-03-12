using MeAjudaAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Infrastructure.Persistence.Configurations.Entities;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Telefone)
            .HasMaxLength(20);

        builder.Property(x => x.SenhaHash)
            .HasMaxLength(500);

        builder.Property(x => x.TipoPerfil)
            .IsRequired();

        builder.Property(x => x.Ativo)
            .IsRequired();

        builder.Property(x => x.DataCriacao)
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.HasOne(x => x.Cliente)
            .WithOne(x => x.Usuario)
            .HasForeignKey<Cliente>(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Profissional)
            .WithOne(x => x.Usuario)
            .HasForeignKey<Profissional>(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}