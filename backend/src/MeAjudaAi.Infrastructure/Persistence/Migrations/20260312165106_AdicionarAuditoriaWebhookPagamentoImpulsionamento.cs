using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarAuditoriaWebhookPagamentoImpulsionamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "webhooks_pagamentos_impulsionamentos_eventos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventoExternoId = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CodigoReferenciaPagamento = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    StatusPagamento = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    ProcessadoComSucesso = table.Column<bool>(type: "boolean", nullable: false),
                    MensagemResultado = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ImpulsionamentoProfissionalId = table.Column<Guid>(type: "uuid", nullable: true),
                    StatusImpulsionamentoResultado = table.Column<int>(type: "integer", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_webhooks_pagamentos_impulsionamentos_eventos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_webhooks_pagamentos_impulsionamentos_eventos_impulsionament~",
                        column: x => x.ImpulsionamentoProfissionalId,
                        principalTable: "impulsionamentos_profissionais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_webhooks_pagamentos_impulsionamentos_eventos_CodigoReferenc~",
                table: "webhooks_pagamentos_impulsionamentos_eventos",
                column: "CodigoReferenciaPagamento");

            migrationBuilder.CreateIndex(
                name: "IX_webhooks_pagamentos_impulsionamentos_eventos_EventoExternoId",
                table: "webhooks_pagamentos_impulsionamentos_eventos",
                column: "EventoExternoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_webhooks_pagamentos_impulsionamentos_eventos_Impulsionament~",
                table: "webhooks_pagamentos_impulsionamentos_eventos",
                column: "ImpulsionamentoProfissionalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "webhooks_pagamentos_impulsionamentos_eventos");
        }
    }
}
