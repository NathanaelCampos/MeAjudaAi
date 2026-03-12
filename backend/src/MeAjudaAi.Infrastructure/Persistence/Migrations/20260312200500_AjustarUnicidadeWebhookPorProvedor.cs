using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Infrastructure.Persistence.Migrations
{
    public partial class AjustarUnicidadeWebhookPorProvedor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_webhooks_pagamentos_impulsionamentos_eventos_EventoExternoId",
                table: "webhooks_pagamentos_impulsionamentos_eventos");

            migrationBuilder.CreateIndex(
                name: "IX_webhooks_pagamentos_impulsionamentos_eventos_Provedor_EventoExternoId",
                table: "webhooks_pagamentos_impulsionamentos_eventos",
                columns: new[] { "Provedor", "EventoExternoId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_webhooks_pagamentos_impulsionamentos_eventos_Provedor_EventoExternoId",
                table: "webhooks_pagamentos_impulsionamentos_eventos");

            migrationBuilder.CreateIndex(
                name: "IX_webhooks_pagamentos_impulsionamentos_eventos_EventoExternoId",
                table: "webhooks_pagamentos_impulsionamentos_eventos",
                column: "EventoExternoId",
                unique: true);
        }
    }
}
