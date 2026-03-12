using MeAjudaAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Infrastructure.Persistence.Configurations.Entities;

public class AvaliacaoConfiguration : IEntityTypeConfiguration<Avaliacao>
{
    public void Configure(EntityTypeBuilder<Avaliacao> builder)
    {
        builder.ToTable("avaliacoes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.NotaAtendimento)
            .IsRequired();

        builder.Property(x => x.NotaServico)
            .IsRequired();

        builder.Property(x => x.NotaPreco)
            .IsRequired();

        builder.Property(x => x.Comentario)
            .HasMaxLength(1000);

        builder.Property(x => x.StatusModeracaoComentario)
            .IsRequired();

        builder.Property(x => x.Ativo)
            .IsRequired();

        builder.Property(x => x.DataCriacao)
            .IsRequired();

        builder.HasIndex(x => x.ServicoId).IsUnique();
        builder.HasIndex(x => new { x.ProfissionalId, x.DataCriacao });

        builder.HasOne(x => x.Cliente)
            .WithMany(x => x.Avaliacoes)
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Profissional)
            .WithMany(x => x.Avaliacoes)
            .HasForeignKey(x => x.ProfissionalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}