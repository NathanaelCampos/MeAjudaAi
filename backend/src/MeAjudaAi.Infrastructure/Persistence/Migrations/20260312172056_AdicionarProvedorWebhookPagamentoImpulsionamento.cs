using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarProvedorWebhookPagamentoImpulsionamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Provedor",
                table: "webhooks_pagamentos_impulsionamentos_eventos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_webhooks_pagamentos_impulsionamentos_eventos_Provedor",
                table: "webhooks_pagamentos_impulsionamentos_eventos",
                column: "Provedor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_webhooks_pagamentos_impulsionamentos_eventos_Provedor",
                table: "webhooks_pagamentos_impulsionamentos_eventos");

            migrationBuilder.DropColumn(
                name: "Provedor",
                table: "webhooks_pagamentos_impulsionamentos_eventos");
        }
    }
}
