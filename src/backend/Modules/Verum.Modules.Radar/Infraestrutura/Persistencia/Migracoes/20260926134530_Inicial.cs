using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verum.Modules.Radar.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          migrationBuilder.EnsureSchema(
                name: "radar");

            migrationBuilder.CreateTable(
                name: "MONITORAMENTO",
                schema: "radar",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    CONTA_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    PRODUTO_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    PRODUTO_VARIANTE_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    NOME = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false),
                    PRECO_INICIAL = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    PRECO_ALVO = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    MENOR_PRECO_ATUAL = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true),
                    ULTIMA_OFERTA_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    ULTIMA_OBSERVACAO_ID = table.Column<long>(type: "bigint", nullable: true),
                    STATUS = table.Column<short>(type: "smallint", nullable: false),
                    ULTIMA_VERIFICACAO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PROXIMA_VERIFICACAO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CRIADO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ATUALIZADO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PAUSADO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EXCLUIDO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ULTIMO_ALERTA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ULTIMO_PRECO_ALERTADO = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_MONITORAMENTO", x => x.ID);

                    table.CheckConstraint("CK_MONITORAMENTO_CONTA_ID", "\"CONTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_MONITORAMENTO_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_MONITORAMENTO_MENOR_PRECO_ATUAL", "\"MENOR_PRECO_ATUAL\" >= 0 AND \"MENOR_PRECO_ATUAL\" <= 999999999999.99");

                    table.CheckConstraint("CK_MONITORAMENTO_NOME", "length(btrim(\"NOME\")) > 0");

                    table.CheckConstraint("CK_MONITORAMENTO_PRECO_ALVO", "\"PRECO_ALVO\" >= 0.01 AND \"PRECO_ALVO\" <= 999999999999.99");

                    table.CheckConstraint("CK_MONITORAMENTO_PRECO_INICIAL", "\"PRECO_INICIAL\" >= 0.01 AND \"PRECO_INICIAL\" <= 999999999999.99");

                    table.CheckConstraint("CK_MONITORAMENTO_PRODUTO_ID", "\"PRODUTO_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_MONITORAMENTO_PRODUTO_VARIANTE_ID", "\"PRODUTO_VARIANTE_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_MONITORAMENTO_STATUS", "\"STATUS\" IN (1, 2, 3)");

                    table.CheckConstraint("CK_MONITORAMENTO_ULTIMA_OBSERVACAO_ID", "\"ULTIMA_OBSERVACAO_ID\" >= 1 AND \"ULTIMA_OBSERVACAO_ID\" <= 9223372036854775807");

                    table.CheckConstraint("CK_MONITORAMENTO_ULTIMA_OFERTA_ID", "\"ULTIMA_OFERTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_MONITORAMENTO_ULTIMO_PRECO_ALERTADO", "\"ULTIMO_PRECO_ALERTADO\" >= 0 AND \"ULTIMO_PRECO_ALERTADO\" <= 999999999999.99");
                });

            migrationBuilder.CreateTable(
                name: "OPORTUNIDADE",
                schema: "radar",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    MONITORAMENTO_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    OFERTA_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    OFERTA_OBSERVACAO_ID = table.Column<long>(type: "bigint", nullable: false),
                    PRECO_OBSERVADO = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    PRECO_ALVO = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    ECONOMIA_DESDE_CRIACAO = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true),
                    PERCENTUAL_REDUCAO = table.Column<decimal>(type: "numeric(7,4)", precision: 7, scale: 4, nullable: true),
                    OFERTA_SNAPSHOT = table.Column<string>(type: "jsonb", nullable: false),
                    STATUS = table.Column<short>(type: "smallint", nullable: false),
                    DETECTADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    VISUALIZADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EXPIRA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_OPORTUNIDADE", x => x.ID);

                    table.CheckConstraint("CK_OPORTUNIDADE_ECONOMIA_DESDE_CRIACAO", "\"ECONOMIA_DESDE_CRIACAO\" >= -999999999999.99 AND \"ECONOMIA_DESDE_CRIACAO\" <= 999999999999.99");

                    table.CheckConstraint("CK_OPORTUNIDADE_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_OPORTUNIDADE_MONITORAMENTO_ID", "\"MONITORAMENTO_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_OPORTUNIDADE_OFERTA_ID", "\"OFERTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_OPORTUNIDADE_OFERTA_OBSERVACAO_ID", "\"OFERTA_OBSERVACAO_ID\" >= 1 AND \"OFERTA_OBSERVACAO_ID\" <= 9223372036854775807");

                    table.CheckConstraint("CK_OPORTUNIDADE_OFERTA_SNAPSHOT", "jsonb_typeof(\"OFERTA_SNAPSHOT\") = 'object'");

                    table.CheckConstraint("CK_OPORTUNIDADE_PERCENTUAL_REDUCAO", "\"PERCENTUAL_REDUCAO\" >= -999 AND \"PERCENTUAL_REDUCAO\" <= 100");

                    table.CheckConstraint("CK_OPORTUNIDADE_PRECO_ALVO", "\"PRECO_ALVO\" >= 0.01 AND \"PRECO_ALVO\" <= 999999999999.99");

                    table.CheckConstraint("CK_OPORTUNIDADE_PRECO_OBSERVADO", "\"PRECO_OBSERVADO\" >= 0.01 AND \"PRECO_OBSERVADO\" <= 999999999999.99");

                    table.CheckConstraint("CK_OPORTUNIDADE_REGRA_10", "\"PRECO_OBSERVADO\" <= \"PRECO_ALVO\"");

                    table.CheckConstraint("CK_OPORTUNIDADE_REGRA_11", "\"EXPIRA_EM\" IS NULL OR \"EXPIRA_EM\" > \"DETECTADA_EM\"");

                    table.CheckConstraint("CK_OPORTUNIDADE_STATUS", "\"STATUS\" IN (1, 2, 3)");

                    table.ForeignKey(
                        name: "FK_OPORTUNIDADE_MONITORAMENTO_MONITORAMENTO_ID",
                        column: x => x.MONITORAMENTO_ID,
                        principalSchema: "radar",
                        principalTable: "MONITORAMENTO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OPORTUNIDADE_MONITORAMENTO_ID_OFERTA_OBSERVACAO_ID",
                schema: "radar",
                table: "OPORTUNIDADE",
                columns: new[] { "MONITORAMENTO_ID", "OFERTA_OBSERVACAO_ID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
          migrationBuilder.DropTable(
                name: "OPORTUNIDADE",
                schema: "radar");

            migrationBuilder.DropTable(
                name: "MONITORAMENTO",
                schema: "radar");
        }
    }
}
