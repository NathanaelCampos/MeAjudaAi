using MeAjudaAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Infrastructure.Persistence.Configurations.Entities;

public class WebhookPagamentoImpulsionamentoEventoConfiguration : IEntityTypeConfiguration<WebhookPagamentoImpulsionamentoEvento>
{
    public void Configure(EntityTypeBuilder<WebhookPagamentoImpulsionamentoEvento> builder)
    {
        builder.ToTable("webhooks_pagamentos_impulsionamentos_eventos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provedor)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.EventoExternoId)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.CodigoReferenciaPagamento)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.StatusPagamento)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.PayloadJson)
            .IsRequired();

        builder.Property(x => x.HeadersJson)
            .IsRequired();

        builder.Property(x => x.IpOrigem)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.RequestId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.UserAgent)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.ProcessadoComSucesso)
            .IsRequired();

        builder.Property(x => x.MensagemResultado)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.Ativo)
            .IsRequired();

        builder.Property(x => x.DataCriacao)
            .IsRequired();

        builder.HasIndex(x => new { x.Provedor, x.EventoExternoId })
            .IsUnique();

        builder.HasIndex(x => x.Provedor);
        builder.HasIndex(x => x.CodigoReferenciaPagamento);
        builder.HasIndex(x => x.RequestId);

        builder.HasOne(x => x.ImpulsionamentoProfissional)
            .WithMany()
            .HasForeignKey(x => x.ImpulsionamentoProfissionalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
