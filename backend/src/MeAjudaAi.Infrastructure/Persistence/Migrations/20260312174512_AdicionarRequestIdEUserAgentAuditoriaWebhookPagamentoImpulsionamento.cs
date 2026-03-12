using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarRequestIdEUserAgentAuditoriaWebhookPagamentoImpulsionamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestId",
                table: "webhooks_pagamentos_impulsionamentos_eventos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "webhooks_pagamentos_impulsionamentos_eventos",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_webhooks_pagamentos_impulsionamentos_eventos_RequestId",
                table: "webhooks_pagamentos_impulsionamentos_eventos",
                column: "RequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_webhooks_pagamentos_impulsionamentos_eventos_RequestId",
                table: "webhooks_pagamentos_impulsionamentos_eventos");

            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "webhooks_pagamentos_impulsionamentos_eventos");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "webhooks_pagamentos_impulsionamentos_eventos");
        }
    }
}
