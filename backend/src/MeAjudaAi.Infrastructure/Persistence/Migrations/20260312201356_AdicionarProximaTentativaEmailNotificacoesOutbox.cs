using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarProximaTentativaEmailNotificacoesOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_emails_notificacoes_outbox_Status_DataCriacao",
                table: "emails_notificacoes_outbox");

            migrationBuilder.AddColumn<DateTime>(
                name: "ProximaTentativaEm",
                table: "emails_notificacoes_outbox",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_emails_notificacoes_outbox_Status_ProximaTentativaEm",
                table: "emails_notificacoes_outbox",
                columns: new[] { "Status", "ProximaTentativaEm" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_emails_notificacoes_outbox_Status_ProximaTentativaEm",
                table: "emails_notificacoes_outbox");

            migrationBuilder.DropColumn(
                name: "ProximaTentativaEm",
                table: "emails_notificacoes_outbox");

            migrationBuilder.CreateIndex(
                name: "IX_emails_notificacoes_outbox_Status_DataCriacao",
                table: "emails_notificacoes_outbox",
                columns: new[] { "Status", "DataCriacao" });
        }
    }
}
