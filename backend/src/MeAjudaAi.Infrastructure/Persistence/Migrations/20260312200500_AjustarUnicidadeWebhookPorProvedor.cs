using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260312200500_AjustarUnicidadeWebhookPorProvedor")]
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
