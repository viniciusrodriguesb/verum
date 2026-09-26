using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verum.Modules.Notificacoes.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          migrationBuilder.EnsureSchema(
                name: "notificacoes");

            migrationBuilder.CreateTable(
                name: "NOTIFICACAO",
                schema: "notificacoes",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    CONTA_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    TIPO = table.Column<short>(type: "smallint", nullable: false),
                    TITULO = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    MENSAGEM = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    DADOS = table.Column<string>(type: "jsonb", nullable: true),
                    STATUS = table.Column<short>(type: "smallint", nullable: false),
                    CRIADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LIDA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_NOTIFICACAO", x => x.ID);

                    table.CheckConstraint("CK_NOTIFICACAO_CONTA_ID", "\"CONTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_NOTIFICACAO_DADOS", "jsonb_typeof(\"DADOS\") = 'object'");

                    table.CheckConstraint("CK_NOTIFICACAO_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_NOTIFICACAO_MENSAGEM", "length(btrim(\"MENSAGEM\")) > 0");

                    table.CheckConstraint("CK_NOTIFICACAO_STATUS", "\"STATUS\" IN (1, 2)");

                    table.CheckConstraint("CK_NOTIFICACAO_TIPO", "\"TIPO\" IN (1, 2)");

                    table.CheckConstraint("CK_NOTIFICACAO_TITULO", "length(btrim(\"TITULO\")) > 0");
                });

            migrationBuilder.CreateTable(
                name: "PREFERENCIA_NOTIFICACAO",
                schema: "notificacoes",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    CONTA_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    CANAL = table.Column<short>(type: "smallint", nullable: false),
                    HABILITADA = table.Column<bool>(type: "boolean", nullable: false),
                    CRIADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ATUALIZADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_PREFERENCIA_NOTIFICACAO", x => x.ID);

                    table.CheckConstraint("CK_PREFERENCIA_NOTIFICACAO_CANAL", "\"CANAL\" IN (1, 2, 3)");

                    table.CheckConstraint("CK_PREFERENCIA_NOTIFICACAO_CONTA_ID", "\"CONTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_PREFERENCIA_NOTIFICACAO_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");
                });

            migrationBuilder.CreateTable(
                name: "ENTREGA_NOTIFICACAO",
                schema: "notificacoes",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    NOTIFICACAO_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    CANAL = table.Column<short>(type: "smallint", nullable: false),
                    STATUS = table.Column<short>(type: "smallint", nullable: false),
                    QUANTIDADE_TENTATIVAS = table.Column<short>(type: "smallint", nullable: false),
                    PROXIMA_TENTATIVA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ULTIMA_TENTATIVA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ENTREGUE_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CODIGO_ERRO = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    DETALHES_ERRO = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    REFERENCIA_PROVEDOR = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_ENTREGA_NOTIFICACAO", x => x.ID);

                    table.CheckConstraint("CK_ENTREGA_NOTIFICACAO_CANAL", "\"CANAL\" IN (1, 2, 3)");

                    table.CheckConstraint("CK_ENTREGA_NOTIFICACAO_CODIGO_ERRO", "length(btrim(\"CODIGO_ERRO\")) > 0");

                    table.CheckConstraint("CK_ENTREGA_NOTIFICACAO_DETALHES_ERRO", "length(btrim(\"DETALHES_ERRO\")) > 0");

                    table.CheckConstraint("CK_ENTREGA_NOTIFICACAO_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_ENTREGA_NOTIFICACAO_NOTIFICACAO_ID", "\"NOTIFICACAO_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_ENTREGA_NOTIFICACAO_QUANTIDADE_TENTATIVAS", "\"QUANTIDADE_TENTATIVAS\" >= 0 AND \"QUANTIDADE_TENTATIVAS\" <= 32767");

                    table.CheckConstraint("CK_ENTREGA_NOTIFICACAO_REFERENCIA_PROVEDOR", "length(btrim(\"REFERENCIA_PROVEDOR\")) > 0");

                    table.CheckConstraint("CK_ENTREGA_NOTIFICACAO_STATUS", "\"STATUS\" IN (1, 2, 3, 4, 5, 6)");

                    table.ForeignKey(
                        name: "FK_ENTREGA_NOTIFICACAO_NOTIFICACAO_NOTIFICACAO_ID",
                        column: x => x.NOTIFICACAO_ID,
                        principalSchema: "notificacoes",
                        principalTable: "NOTIFICACAO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ENTREGA_NOTIFICACAO_NOTIFICACAO_ID_CANAL",
                schema: "notificacoes",
                table: "ENTREGA_NOTIFICACAO",
                columns: new[] { "NOTIFICACAO_ID", "CANAL" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PREFERENCIA_NOTIFICACAO_CONTA_ID_CANAL",
                schema: "notificacoes",
                table: "PREFERENCIA_NOTIFICACAO",
                columns: new[] { "CONTA_ID", "CANAL" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
          migrationBuilder.DropTable(
                name: "ENTREGA_NOTIFICACAO",
                schema: "notificacoes");

            migrationBuilder.DropTable(
                name: "PREFERENCIA_NOTIFICACAO",
                schema: "notificacoes");

            migrationBuilder.DropTable(
                name: "NOTIFICACAO",
                schema: "notificacoes");
        }
    }
}
