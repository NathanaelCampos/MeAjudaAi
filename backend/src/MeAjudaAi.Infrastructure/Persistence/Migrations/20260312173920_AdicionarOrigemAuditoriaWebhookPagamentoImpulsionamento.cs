using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarOrigemAuditoriaWebhookPagamentoImpulsionamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeadersJson",
                table: "webhooks_pagamentos_impulsionamentos_eventos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IpOrigem",
                table: "webhooks_pagamentos_impulsionamentos_eventos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeadersJson",
                table: "webhooks_pagamentos_impulsionamentos_eventos");

            migrationBuilder.DropColumn(
                name: "IpOrigem",
                table: "webhooks_pagamentos_impulsionamentos_eventos");
        }
    }
}
