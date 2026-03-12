using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "estados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UF = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    CodigoIbge = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "planos_impulsionamento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    TipoPeriodo = table.Column<int>(type: "integer", nullable: false),
                    QuantidadePeriodo = table.Column<int>(type: "integer", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planos_impulsionamento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "profissoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Slug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profissoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SenhaHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TipoPerfil = table.Column<int>(type: "integer", nullable: false),
                    DataUltimoLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cidades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EstadoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CodigoIbge = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cidades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cidades_estados_EstadoId",
                        column: x => x.EstadoId,
                        principalTable: "estados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "especialidades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfissaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_especialidades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_especialidades_profissoes_ProfissaoId",
                        column: x => x.ProfissaoId,
                        principalTable: "profissoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    NomeExibicao = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_clientes_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "profissionais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    NomeExibicao = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    WhatsApp = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Instagram = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Facebook = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OutraFormaContato = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AceitaContatoPeloApp = table.Column<bool>(type: "boolean", nullable: false),
                    PerfilVerificado = table.Column<bool>(type: "boolean", nullable: false),
                    NotaMediaAtendimento = table.Column<decimal>(type: "numeric", nullable: true),
                    NotaMediaServico = table.Column<decimal>(type: "numeric", nullable: true),
                    NotaMediaPreco = table.Column<decimal>(type: "numeric", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profissionais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_profissionais_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bairros",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CidadeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bairros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bairros_cidades_CidadeId",
                        column: x => x.CidadeId,
                        principalTable: "cidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "formas_recebimento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfissionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipoFormaRecebimento = table.Column<int>(type: "integer", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_formas_recebimento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_formas_recebimento_profissionais_ProfissionalId",
                        column: x => x.ProfissionalId,
                        principalTable: "profissionais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "impulsionamentos_profissionais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfissionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanoImpulsionamentoId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataFim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ValorPago = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CodigoReferenciaPagamento = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_impulsionamentos_profissionais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_impulsionamentos_profissionais_planos_impulsionamento_Plano~",
                        column: x => x.PlanoImpulsionamentoId,
                        principalTable: "planos_impulsionamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_impulsionamentos_profissionais_profissionais_ProfissionalId",
                        column: x => x.ProfissionalId,
                        principalTable: "profissionais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "portfolio_fotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfissionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    UrlArquivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Legenda = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portfolio_fotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_portfolio_fotos_profissionais_ProfissionalId",
                        column: x => x.ProfissionalId,
                        principalTable: "profissionais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "profissionais_especialidades",
                columns: table => new
                {
                    ProfissionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    EspecialidadeId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profissionais_especialidades", x => new { x.ProfissionalId, x.EspecialidadeId });
                    table.ForeignKey(
                        name: "FK_profissionais_especialidades_especialidades_EspecialidadeId",
                        column: x => x.EspecialidadeId,
                        principalTable: "especialidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_profissionais_especialidades_profissionais_ProfissionalId",
                        column: x => x.ProfissionalId,
                        principalTable: "profissionais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "profissionais_profissoes",
                columns: table => new
                {
                    ProfissionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfissaoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profissionais_profissoes", x => new { x.ProfissionalId, x.ProfissaoId });
                    table.ForeignKey(
                        name: "FK_profissionais_profissoes_profissionais_ProfissionalId",
                        column: x => x.ProfissionalId,
                        principalTable: "profissionais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_profissionais_profissoes_profissoes_ProfissaoId",
                        column: x => x.ProfissaoId,
                        principalTable: "profissoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "areas_atendimento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfissionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    CidadeId = table.Column<Guid>(type: "uuid", nullable: false),
                    BairroId = table.Column<Guid>(type: "uuid", nullable: true),
                    CidadeInteira = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_areas_atendimento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_areas_atendimento_bairros_BairroId",
                        column: x => x.BairroId,
                        principalTable: "bairros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_areas_atendimento_cidades_CidadeId",
                        column: x => x.CidadeId,
                        principalTable: "cidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_areas_atendimento_profissionais_ProfissionalId",
                        column: x => x.ProfissionalId,
                        principalTable: "profissionais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "servicos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfissionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfissaoId = table.Column<Guid>(type: "uuid", nullable: true),
                    EspecialidadeId = table.Column<Guid>(type: "uuid", nullable: true),
                    CidadeId = table.Column<Guid>(type: "uuid", nullable: false),
                    BairroId = table.Column<Guid>(type: "uuid", nullable: true),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ValorCombinado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataAceite = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataConclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataCancelamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_servicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_servicos_bairros_BairroId",
                        column: x => x.BairroId,
                        principalTable: "bairros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_servicos_cidades_CidadeId",
                        column: x => x.CidadeId,
                        principalTable: "cidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_servicos_clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_servicos_especialidades_EspecialidadeId",
                        column: x => x.EspecialidadeId,
                        principalTable: "especialidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_servicos_profissionais_ProfissionalId",
                        column: x => x.ProfissionalId,
                        principalTable: "profissionais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_servicos_profissoes_ProfissaoId",
                        column: x => x.ProfissaoId,
                        principalTable: "profissoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "avaliacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfissionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    NotaAtendimento = table.Column<int>(type: "integer", nullable: false),
                    NotaServico = table.Column<int>(type: "integer", nullable: false),
                    NotaPreco = table.Column<int>(type: "integer", nullable: false),
                    Comentario = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    StatusModeracaoComentario = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_avaliacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_avaliacoes_clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_avaliacoes_profissionais_ProfissionalId",
                        column: x => x.ProfissionalId,
                        principalTable: "profissionais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_avaliacoes_servicos_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "servicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_areas_atendimento_BairroId",
                table: "areas_atendimento",
                column: "BairroId");

            migrationBuilder.CreateIndex(
                name: "IX_areas_atendimento_CidadeId",
                table: "areas_atendimento",
                column: "CidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_areas_atendimento_ProfissionalId_CidadeId_BairroId",
                table: "areas_atendimento",
                columns: new[] { "ProfissionalId", "CidadeId", "BairroId" });

            migrationBuilder.CreateIndex(
                name: "IX_avaliacoes_ClienteId",
                table: "avaliacoes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_avaliacoes_ProfissionalId_DataCriacao",
                table: "avaliacoes",
                columns: new[] { "ProfissionalId", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_avaliacoes_ServicoId",
                table: "avaliacoes",
                column: "ServicoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bairros_CidadeId_Nome",
                table: "bairros",
                columns: new[] { "CidadeId", "Nome" });

            migrationBuilder.CreateIndex(
                name: "IX_cidades_CodigoIbge",
                table: "cidades",
                column: "CodigoIbge",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cidades_EstadoId_Nome",
                table: "cidades",
                columns: new[] { "EstadoId", "Nome" });

            migrationBuilder.CreateIndex(
                name: "IX_clientes_UsuarioId",
                table: "clientes",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_especialidades_ProfissaoId_Nome",
                table: "especialidades",
                columns: new[] { "ProfissaoId", "Nome" });

            migrationBuilder.CreateIndex(
                name: "IX_estados_CodigoIbge",
                table: "estados",
                column: "CodigoIbge",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_estados_UF",
                table: "estados",
                column: "UF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_formas_recebimento_ProfissionalId_TipoFormaRecebimento",
                table: "formas_recebimento",
                columns: new[] { "ProfissionalId", "TipoFormaRecebimento" });

            migrationBuilder.CreateIndex(
                name: "IX_impulsionamentos_profissionais_PlanoImpulsionamentoId_Status",
                table: "impulsionamentos_profissionais",
                columns: new[] { "PlanoImpulsionamentoId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_impulsionamentos_profissionais_ProfissionalId_Status",
                table: "impulsionamentos_profissionais",
                columns: new[] { "ProfissionalId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_planos_impulsionamento_Nome",
                table: "planos_impulsionamento",
                column: "Nome");

            migrationBuilder.CreateIndex(
                name: "IX_portfolio_fotos_ProfissionalId_Ordem",
                table: "portfolio_fotos",
                columns: new[] { "ProfissionalId", "Ordem" });

            migrationBuilder.CreateIndex(
                name: "IX_profissionais_UsuarioId",
                table: "profissionais",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_profissionais_especialidades_EspecialidadeId",
                table: "profissionais_especialidades",
                column: "EspecialidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_profissionais_profissoes_ProfissaoId",
                table: "profissionais_profissoes",
                column: "ProfissaoId");

            migrationBuilder.CreateIndex(
                name: "IX_profissoes_Nome",
                table: "profissoes",
                column: "Nome");

            migrationBuilder.CreateIndex(
                name: "IX_profissoes_Slug",
                table: "profissoes",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_servicos_BairroId",
                table: "servicos",
                column: "BairroId");

            migrationBuilder.CreateIndex(
                name: "IX_servicos_CidadeId",
                table: "servicos",
                column: "CidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_servicos_ClienteId_Status",
                table: "servicos",
                columns: new[] { "ClienteId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_servicos_EspecialidadeId",
                table: "servicos",
                column: "EspecialidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_servicos_ProfissaoId",
                table: "servicos",
                column: "ProfissaoId");

            migrationBuilder.CreateIndex(
                name: "IX_servicos_ProfissionalId_Status",
                table: "servicos",
                columns: new[] { "ProfissionalId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Email",
                table: "usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "areas_atendimento");

            migrationBuilder.DropTable(
                name: "avaliacoes");

            migrationBuilder.DropTable(
                name: "formas_recebimento");

            migrationBuilder.DropTable(
                name: "impulsionamentos_profissionais");

            migrationBuilder.DropTable(
                name: "portfolio_fotos");

            migrationBuilder.DropTable(
                name: "profissionais_especialidades");

            migrationBuilder.DropTable(
                name: "profissionais_profissoes");

            migrationBuilder.DropTable(
                name: "servicos");

            migrationBuilder.DropTable(
                name: "planos_impulsionamento");

            migrationBuilder.DropTable(
                name: "bairros");

            migrationBuilder.DropTable(
                name: "clientes");

            migrationBuilder.DropTable(
                name: "especialidades");

            migrationBuilder.DropTable(
                name: "profissionais");

            migrationBuilder.DropTable(
                name: "cidades");

            migrationBuilder.DropTable(
                name: "profissoes");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "estados");
        }
    }
}
