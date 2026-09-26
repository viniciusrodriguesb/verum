using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Verum.Modules.Assinaturas.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          migrationBuilder.EnsureSchema(
                name: "assinaturas");

            migrationBuilder.CreateTable(
                name: "CLIENTE_GATEWAY",
                schema: "assinaturas",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    CONTA_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    GATEWAY = table.Column<short>(type: "smallint", nullable: false),
                    IDENTIFICADOR_EXTERNO = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    CRIADO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ATUALIZADO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_CLIENTE_GATEWAY", x => x.ID);

                    table.UniqueConstraint("AK_CLIENTE_GATEWAY_ID_CONTA_ID_GATEWAY", x => new { x.ID, x.CONTA_ID, x.GATEWAY });

                    table.CheckConstraint("CK_CLIENTE_GATEWAY_CONTA_ID", "\"CONTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_CLIENTE_GATEWAY_GATEWAY", "\"GATEWAY\" IN (1)");

                    table.CheckConstraint("CK_CLIENTE_GATEWAY_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_CLIENTE_GATEWAY_IDENTIFICADOR_EXTERNO", "length(btrim(\"IDENTIFICADOR_EXTERNO\")) > 0");
                });

            migrationBuilder.CreateTable(
                name: "EVENTO_GATEWAY",
                schema: "assinaturas",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GATEWAY = table.Column<short>(type: "smallint", nullable: false),
                    IDENTIFICADOR_EXTERNO = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    TIPO = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    CONTEUDO = table.Column<string>(type: "jsonb", nullable: false),
                    STATUS = table.Column<short>(type: "smallint", nullable: false),
                    RECEBIDO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PROCESSADO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TENTATIVAS = table.Column<short>(type: "smallint", nullable: false),
                    ULTIMO_ERRO = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_EVENTO_GATEWAY", x => x.ID);

                    table.CheckConstraint("CK_EVENTO_GATEWAY_CONTEUDO", "jsonb_typeof(\"CONTEUDO\") = 'object'");

                    table.CheckConstraint("CK_EVENTO_GATEWAY_GATEWAY", "\"GATEWAY\" IN (1)");

                    table.CheckConstraint("CK_EVENTO_GATEWAY_IDENTIFICADOR_EXTERNO", "length(btrim(\"IDENTIFICADOR_EXTERNO\")) > 0");

                    table.CheckConstraint("CK_EVENTO_GATEWAY_STATUS", "\"STATUS\" IN (1, 2, 3)");

                    table.CheckConstraint("CK_EVENTO_GATEWAY_TENTATIVAS", "\"TENTATIVAS\" >= 0 AND \"TENTATIVAS\" <= 32767");

                    table.CheckConstraint("CK_EVENTO_GATEWAY_TIPO", "length(btrim(\"TIPO\")) > 0");

                    table.CheckConstraint("CK_EVENTO_GATEWAY_ULTIMO_ERRO", "length(btrim(\"ULTIMO_ERRO\")) > 0");
                });

            migrationBuilder.CreateTable(
                name: "PLANO",
                schema: "assinaturas",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    CODIGO = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    NOME = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    DESCRICAO = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    PACOTE_ACESSO_CODIGO = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    VALOR_ATUAL = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    MOEDA = table.Column<string>(type: "char(3)", fixedLength: true, maxLength: 3, nullable: false),
                    PERIODICIDADE = table.Column<short>(type: "smallint", nullable: false),
                    ATIVO = table.Column<bool>(type: "boolean", nullable: false),
                    CRIADO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ATUALIZADO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_PLANO", x => x.ID);

                    table.CheckConstraint("CK_PLANO_CODIGO", "length(btrim(\"CODIGO\")) > 0");

                    table.CheckConstraint("CK_PLANO_DESCRICAO", "length(btrim(\"DESCRICAO\")) > 0");

                    table.CheckConstraint("CK_PLANO_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_PLANO_MOEDA", "length(btrim(\"MOEDA\")) > 0");

                    table.CheckConstraint("CK_PLANO_NOME", "length(btrim(\"NOME\")) > 0");

                    table.CheckConstraint("CK_PLANO_PACOTE_ACESSO_CODIGO", "length(btrim(\"PACOTE_ACESSO_CODIGO\")) > 0");

                    table.CheckConstraint("CK_PLANO_PERIODICIDADE", "\"PERIODICIDADE\" IN (1, 2)");

                    table.CheckConstraint("CK_PLANO_VALOR_ATUAL", "\"VALOR_ATUAL\" >= 0.01 AND \"VALOR_ATUAL\" <= 999999999999.99");
                });

            migrationBuilder.CreateTable(
                name: "ASSINATURA",
                schema: "assinaturas",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    CONTA_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    PLANO_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    CLIENTE_GATEWAY_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    GATEWAY = table.Column<short>(type: "smallint", nullable: false),
                    IDENTIFICADOR_EXTERNO = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    STATUS = table.Column<short>(type: "smallint", nullable: false),
                    VALOR_CONTRATADO = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    MOEDA = table.Column<string>(type: "char(3)", fixedLength: true, maxLength: 3, nullable: false),
                    PERIODO_INICIADO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PERIODO_TERMINA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CANCELAMENTO_SOLICITADO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CANCELADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CRIADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ATUALIZADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_ASSINATURA", x => x.ID);

                    table.CheckConstraint("CK_ASSINATURA_CLIENTE_GATEWAY_ID", "\"CLIENTE_GATEWAY_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_ASSINATURA_CONTA_ID", "\"CONTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_ASSINATURA_GATEWAY", "\"GATEWAY\" IN (1)");

                    table.CheckConstraint("CK_ASSINATURA_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_ASSINATURA_IDENTIFICADOR_EXTERNO", "length(btrim(\"IDENTIFICADOR_EXTERNO\")) > 0");

                    table.CheckConstraint("CK_ASSINATURA_MOEDA", "length(btrim(\"MOEDA\")) > 0");

                    table.CheckConstraint("CK_ASSINATURA_PLANO_ID", "\"PLANO_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_ASSINATURA_STATUS", "\"STATUS\" IN (1, 2, 3, 4, 5)");

                    table.CheckConstraint("CK_ASSINATURA_VALOR_CONTRATADO", "\"VALOR_CONTRATADO\" >= 0.01 AND \"VALOR_CONTRATADO\" <= 999999999999.99");

                    table.ForeignKey(
                        name: "FK_ASSINATURA_CLIENTE_GATEWAY_CLIENTE_GATEWAY_ID_CONTA_ID_GATE~",
                        columns: x => new { x.CLIENTE_GATEWAY_ID, x.CONTA_ID, x.GATEWAY },
                        principalSchema: "assinaturas",
                        principalTable: "CLIENTE_GATEWAY",
                        principalColumns: new[] { "ID", "CONTA_ID", "GATEWAY" },
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_ASSINATURA_PLANO_PLANO_ID",
                        column: x => x.PLANO_ID,
                        principalSchema: "assinaturas",
                        principalTable: "PLANO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PAGAMENTO",
                schema: "assinaturas",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    ASSINATURA_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    IDENTIFICADOR_EXTERNO = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    STATUS = table.Column<short>(type: "smallint", nullable: false),
                    VALOR = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    MOEDA = table.Column<string>(type: "char(3)", fixedLength: true, maxLength: 3, nullable: false),
                    METODO = table.Column<short>(type: "smallint", nullable: true),
                    VENCIMENTO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PAGO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CRIADO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ATUALIZADO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_PAGAMENTO", x => x.ID);

                    table.CheckConstraint("CK_PAGAMENTO_ASSINATURA_ID", "\"ASSINATURA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_PAGAMENTO_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_PAGAMENTO_IDENTIFICADOR_EXTERNO", "length(btrim(\"IDENTIFICADOR_EXTERNO\")) > 0");

                    table.CheckConstraint("CK_PAGAMENTO_METODO", "\"METODO\" IN (1, 2, 3)");

                    table.CheckConstraint("CK_PAGAMENTO_MOEDA", "length(btrim(\"MOEDA\")) > 0");

                    table.CheckConstraint("CK_PAGAMENTO_STATUS", "\"STATUS\" IN (1, 2, 3, 4, 5)");

                    table.CheckConstraint("CK_PAGAMENTO_VALOR", "\"VALOR\" >= 0.01 AND \"VALOR\" <= 999999999999.99");

                    table.ForeignKey(
                        name: "FK_PAGAMENTO_ASSINATURA_ASSINATURA_ID",
                        column: x => x.ASSINATURA_ID,
                        principalSchema: "assinaturas",
                        principalTable: "ASSINATURA",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ASSINATURA_CLIENTE_GATEWAY_ID_CONTA_ID_GATEWAY",
                schema: "assinaturas",
                table: "ASSINATURA",
                columns: new[] { "CLIENTE_GATEWAY_ID", "CONTA_ID", "GATEWAY" });

            migrationBuilder.CreateIndex(
                name: "IX_ASSINATURA_GATEWAY_IDENTIFICADOR_EXTERNO",
                schema: "assinaturas",
                table: "ASSINATURA",
                columns: new[] { "GATEWAY", "IDENTIFICADOR_EXTERNO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ASSINATURA_PLANO_ID",
                schema: "assinaturas",
                table: "ASSINATURA",
                column: "PLANO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CLIENTE_GATEWAY_CONTA_ID_GATEWAY",
                schema: "assinaturas",
                table: "CLIENTE_GATEWAY",
                columns: new[] { "CONTA_ID", "GATEWAY" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CLIENTE_GATEWAY_GATEWAY_IDENTIFICADOR_EXTERNO",
                schema: "assinaturas",
                table: "CLIENTE_GATEWAY",
                columns: new[] { "GATEWAY", "IDENTIFICADOR_EXTERNO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EVENTO_GATEWAY_GATEWAY_IDENTIFICADOR_EXTERNO",
                schema: "assinaturas",
                table: "EVENTO_GATEWAY",
                columns: new[] { "GATEWAY", "IDENTIFICADOR_EXTERNO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PAGAMENTO_ASSINATURA_ID_IDENTIFICADOR_EXTERNO",
                schema: "assinaturas",
                table: "PAGAMENTO",
                columns: new[] { "ASSINATURA_ID", "IDENTIFICADOR_EXTERNO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PLANO_CODIGO",
                schema: "assinaturas",
                table: "PLANO",
                column: "CODIGO",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
          migrationBuilder.DropTable(
                name: "EVENTO_GATEWAY",
                schema: "assinaturas");

            migrationBuilder.DropTable(
                name: "PAGAMENTO",
                schema: "assinaturas");

            migrationBuilder.DropTable(
                name: "ASSINATURA",
                schema: "assinaturas");

            migrationBuilder.DropTable(
                name: "CLIENTE_GATEWAY",
                schema: "assinaturas");

            migrationBuilder.DropTable(
                name: "PLANO",
                schema: "assinaturas");
        }
    }
}
