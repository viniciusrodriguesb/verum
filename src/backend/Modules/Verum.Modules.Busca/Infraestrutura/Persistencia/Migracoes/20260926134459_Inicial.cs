using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Verum.Modules.Busca.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          migrationBuilder.EnsureSchema(
                name: "busca");

            migrationBuilder.CreateTable(
                name: "BUSCA",
                schema: "busca",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    CONTA_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    VISITANTE_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    CHAVE_IDEMPOTENCIA = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    CONSULTA_ORIGINAL = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    CONSULTA_NORMALIZADA = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    CONSULTA_ESTRUTURADA = table.Column<string>(type: "jsonb", nullable: true),
                    PRODUTO_PRINCIPAL_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    PRODUTO_VARIANTE_PRINCIPAL_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    STATUS = table.Column<short>(type: "smallint", nullable: false),
                    ETAPA_ATUAL = table.Column<short>(type: "smallint", nullable: false),
                    MOTIVO_SEM_RESULTADO = table.Column<short>(type: "smallint", nullable: true),
                    RESULTADO_PARCIAL = table.Column<bool>(type: "boolean", nullable: false),
                    CODIGO_ERRO = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    MENSAGEM_ERRO = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    INICIADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CONCLUIDA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EXPIRA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    QUANTIDADE_FONTES_CONSULTADAS = table.Column<int>(type: "integer", nullable: false),
                    QUANTIDADE_FONTES_COM_SUCESSO = table.Column<int>(type: "integer", nullable: false),
                    QUANTIDADE_FONTES_COM_FALHA = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_BUSCA", x => x.ID);

                    table.CheckConstraint("CK_BUSCA_CHAVE_IDEMPOTENCIA", "length(btrim(\"CHAVE_IDEMPOTENCIA\")) > 0");

                    table.CheckConstraint("CK_BUSCA_CODIGO_ERRO", "length(btrim(\"CODIGO_ERRO\")) > 0");

                    table.CheckConstraint("CK_BUSCA_CONSULTA_ESTRUTURADA", "jsonb_typeof(\"CONSULTA_ESTRUTURADA\") = 'object'");

                    table.CheckConstraint("CK_BUSCA_CONSULTA_NORMALIZADA", "length(btrim(\"CONSULTA_NORMALIZADA\")) > 0");

                    table.CheckConstraint("CK_BUSCA_CONSULTA_ORIGINAL", "length(btrim(\"CONSULTA_ORIGINAL\")) > 0");

                    table.CheckConstraint("CK_BUSCA_CONTA_ID", "\"CONTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_BUSCA_ETAPA_ATUAL", "\"ETAPA_ATUAL\" IN (1, 2, 3, 4, 5, 6)");

                    table.CheckConstraint("CK_BUSCA_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_BUSCA_MENSAGEM_ERRO", "length(btrim(\"MENSAGEM_ERRO\")) > 0");

                    table.CheckConstraint("CK_BUSCA_MOTIVO_SEM_RESULTADO", "\"MOTIVO_SEM_RESULTADO\" IN (1, 2, 3, 4)");

                    table.CheckConstraint("CK_BUSCA_PRODUTO_PRINCIPAL_ID", "\"PRODUTO_PRINCIPAL_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_BUSCA_PRODUTO_VARIANTE_PRINCIPAL_ID", "\"PRODUTO_VARIANTE_PRINCIPAL_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_BUSCA_QUANTIDADE_FONTES_COM_FALHA", "\"QUANTIDADE_FONTES_COM_FALHA\" >= 0 AND \"QUANTIDADE_FONTES_COM_FALHA\" <= 2147483647");

                    table.CheckConstraint("CK_BUSCA_QUANTIDADE_FONTES_COM_SUCESSO", "\"QUANTIDADE_FONTES_COM_SUCESSO\" >= 0 AND \"QUANTIDADE_FONTES_COM_SUCESSO\" <= 2147483647");

                    table.CheckConstraint("CK_BUSCA_QUANTIDADE_FONTES_CONSULTADAS", "\"QUANTIDADE_FONTES_CONSULTADAS\" >= 0 AND \"QUANTIDADE_FONTES_CONSULTADAS\" <= 2147483647");

                    table.CheckConstraint("CK_BUSCA_REGRA_17", "(\"CONTA_ID\" IS NULL) <> (\"VISITANTE_ID\" IS NULL)");

                    table.CheckConstraint("CK_BUSCA_REGRA_18", "\"EXPIRA_EM\" > \"INICIADA_EM\"");

                    table.CheckConstraint("CK_BUSCA_STATUS", "\"STATUS\" IN (1, 2, 3, 4, 5, 6)");

                    table.CheckConstraint("CK_BUSCA_VISITANTE_ID", "\"VISITANTE_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");
                });

            migrationBuilder.CreateTable(
                name: "BUSCA_ETAPA",
                schema: "busca",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BUSCA_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    ETAPA = table.Column<short>(type: "smallint", nullable: false),
                    STATUS = table.Column<short>(type: "smallint", nullable: false),
                    INICIADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CONCLUIDA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DETALHES = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_BUSCA_ETAPA", x => x.ID);

                    table.CheckConstraint("CK_BUSCA_ETAPA_BUSCA_ID", "\"BUSCA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_BUSCA_ETAPA_DETALHES", "jsonb_typeof(\"DETALHES\") = 'object'");

                    table.CheckConstraint("CK_BUSCA_ETAPA_ETAPA", "\"ETAPA\" IN (1, 2, 3, 4, 5, 6)");

                    table.CheckConstraint("CK_BUSCA_ETAPA_STATUS", "\"STATUS\" IN (1, 2, 3)");

                    table.ForeignKey(
                        name: "FK_BUSCA_ETAPA_BUSCA_BUSCA_ID",
                        column: x => x.BUSCA_ID,
                        principalSchema: "busca",
                        principalTable: "BUSCA",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RESULTADO_BUSCA",
                schema: "busca",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    BUSCA_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    VERSAO_RANKING = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    QUANTIDADE_ANALISADA = table.Column<int>(type: "integer", nullable: false),
                    QUANTIDADE_EXIBIDA = table.Column<int>(type: "integer", nullable: false),
                    PARCIAL = table.Column<bool>(type: "boolean", nullable: false),
                    GERADO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_RESULTADO_BUSCA", x => x.ID);

                    table.CheckConstraint("CK_RESULTADO_BUSCA_BUSCA_ID", "\"BUSCA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_RESULTADO_BUSCA_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_RESULTADO_BUSCA_QUANTIDADE_ANALISADA", "\"QUANTIDADE_ANALISADA\" >= 0 AND \"QUANTIDADE_ANALISADA\" <= 2147483647");

                    table.CheckConstraint("CK_RESULTADO_BUSCA_QUANTIDADE_EXIBIDA", "\"QUANTIDADE_EXIBIDA\" >= 0 AND \"QUANTIDADE_EXIBIDA\" <= 2147483647");

                    table.CheckConstraint("CK_RESULTADO_BUSCA_REGRA_5", "\"QUANTIDADE_EXIBIDA\" <= \"QUANTIDADE_ANALISADA\"");

                    table.CheckConstraint("CK_RESULTADO_BUSCA_VERSAO_RANKING", "length(btrim(\"VERSAO_RANKING\")) > 0");

                    table.ForeignKey(
                        name: "FK_RESULTADO_BUSCA_BUSCA_BUSCA_ID",
                        column: x => x.BUSCA_ID,
                        principalSchema: "busca",
                        principalTable: "BUSCA",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RESULTADO_OFERTA",
                schema: "busca",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RESULTADO_BUSCA_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    OFERTA_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    PRODUTO_VARIANTE_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    POSICAO = table.Column<short>(type: "smallint", nullable: false),
                    CLASSIFICACAO = table.Column<short>(type: "smallint", nullable: false),
                    PONTUACAO_FINAL = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    PONTUACAO_PRECO = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    PONTUACAO_CONFIANCA = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    PONTUACAO_ATUALIZACAO = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    EXPLICACAO = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    OFERTA_SNAPSHOT = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_RESULTADO_OFERTA", x => x.ID);

                    table.CheckConstraint("CK_RESULTADO_OFERTA_CLASSIFICACAO", "\"CLASSIFICACAO\" IN (1, 2, 3)");

                    table.CheckConstraint("CK_RESULTADO_OFERTA_EXPLICACAO", "length(btrim(\"EXPLICACAO\")) > 0");

                    table.CheckConstraint("CK_RESULTADO_OFERTA_OFERTA_ID", "\"OFERTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_RESULTADO_OFERTA_OFERTA_SNAPSHOT", "jsonb_typeof(\"OFERTA_SNAPSHOT\") = 'object'");

                    table.CheckConstraint("CK_RESULTADO_OFERTA_PONTUACAO_ATUALIZACAO", "\"PONTUACAO_ATUALIZACAO\" >= 0 AND \"PONTUACAO_ATUALIZACAO\" <= 100");

                    table.CheckConstraint("CK_RESULTADO_OFERTA_PONTUACAO_CONFIANCA", "\"PONTUACAO_CONFIANCA\" >= 0 AND \"PONTUACAO_CONFIANCA\" <= 100");

                    table.CheckConstraint("CK_RESULTADO_OFERTA_PONTUACAO_FINAL", "\"PONTUACAO_FINAL\" >= 0 AND \"PONTUACAO_FINAL\" <= 100");

                    table.CheckConstraint("CK_RESULTADO_OFERTA_PONTUACAO_PRECO", "\"PONTUACAO_PRECO\" >= 0 AND \"PONTUACAO_PRECO\" <= 100");

                    table.CheckConstraint("CK_RESULTADO_OFERTA_POSICAO", "\"POSICAO\" >= 1 AND \"POSICAO\" <= 32767");

                    table.CheckConstraint("CK_RESULTADO_OFERTA_PRODUTO_VARIANTE_ID", "\"PRODUTO_VARIANTE_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_RESULTADO_OFERTA_RESULTADO_BUSCA_ID", "\"RESULTADO_BUSCA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.ForeignKey(
                        name: "FK_RESULTADO_OFERTA_RESULTADO_BUSCA_RESULTADO_BUSCA_ID",
                        column: x => x.RESULTADO_BUSCA_ID,
                        principalSchema: "busca",
                        principalTable: "RESULTADO_BUSCA",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BUSCA_CONTA_ID_CHAVE_IDEMPOTENCIA",
                schema: "busca",
                table: "BUSCA",
                columns: new[] { "CONTA_ID", "CHAVE_IDEMPOTENCIA" },
                unique: true,
                filter: "\"CONTA_ID\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BUSCA_VISITANTE_ID_CHAVE_IDEMPOTENCIA",
                schema: "busca",
                table: "BUSCA",
                columns: new[] { "VISITANTE_ID", "CHAVE_IDEMPOTENCIA" },
                unique: true,
                filter: "\"VISITANTE_ID\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BUSCA_ETAPA_BUSCA_ID",
                schema: "busca",
                table: "BUSCA_ETAPA",
                column: "BUSCA_ID");

            migrationBuilder.CreateIndex(
                name: "IX_RESULTADO_BUSCA_BUSCA_ID",
                schema: "busca",
                table: "RESULTADO_BUSCA",
                column: "BUSCA_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RESULTADO_OFERTA_RESULTADO_BUSCA_ID_POSICAO",
                schema: "busca",
                table: "RESULTADO_OFERTA",
                columns: new[] { "RESULTADO_BUSCA_ID", "POSICAO" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
          migrationBuilder.DropTable(
                name: "BUSCA_ETAPA",
                schema: "busca");

            migrationBuilder.DropTable(
                name: "RESULTADO_OFERTA",
                schema: "busca");

            migrationBuilder.DropTable(
                name: "RESULTADO_BUSCA",
                schema: "busca");

            migrationBuilder.DropTable(
                name: "BUSCA",
                schema: "busca");
        }
    }
}
