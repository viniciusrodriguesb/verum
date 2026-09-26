using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Verum.Modules.Acesso.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          migrationBuilder.EnsureSchema(
                name: "acesso");

            migrationBuilder.CreateTable(
                name: "PACOTE_ACESSO",
                schema: "acesso",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    CODIGO = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    NOME = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    ATIVO = table.Column<bool>(type: "boolean", nullable: false),
                    CRIADO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ATUALIZADO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_PACOTE_ACESSO", x => x.ID);

                    table.CheckConstraint("CK_PACOTE_ACESSO_CODIGO", "length(btrim(\"CODIGO\")) > 0");

                    table.CheckConstraint("CK_PACOTE_ACESSO_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_PACOTE_ACESSO_NOME", "length(btrim(\"NOME\")) > 0");
                });

            migrationBuilder.CreateTable(
                name: "RECURSO",
                schema: "acesso",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    CODIGO = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    NOME = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    DESCRICAO = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    TIPO_LIMITE = table.Column<short>(type: "smallint", nullable: false),
                    ATIVO = table.Column<bool>(type: "boolean", nullable: false),
                    CRIADO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_RECURSO", x => x.ID);

                    table.CheckConstraint("CK_RECURSO_CODIGO", "length(btrim(\"CODIGO\")) > 0");

                    table.CheckConstraint("CK_RECURSO_DESCRICAO", "length(btrim(\"DESCRICAO\")) > 0");

                    table.CheckConstraint("CK_RECURSO_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_RECURSO_NOME", "length(btrim(\"NOME\")) > 0");

                    table.CheckConstraint("CK_RECURSO_TIPO_LIMITE", "\"TIPO_LIMITE\" IN (1, 2)");
                });

            migrationBuilder.CreateTable(
                name: "REGISTRO_USO",
                schema: "acesso",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CONTA_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    VISITANTE_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    RECURSO_CODIGO = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    QUANTIDADE = table.Column<int>(type: "integer", nullable: false),
                    REFERENCIA_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    DADOS = table.Column<string>(type: "jsonb", nullable: true),
                    OCORRIDO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_REGISTRO_USO", x => x.ID);

                    table.CheckConstraint("CK_REGISTRO_USO_CONTA_ID", "\"CONTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_REGISTRO_USO_DADOS", "jsonb_typeof(\"DADOS\") = 'object'");

                    table.CheckConstraint("CK_REGISTRO_USO_QUANTIDADE", "\"QUANTIDADE\" >= 1 AND \"QUANTIDADE\" <= 2147483647");

                    table.CheckConstraint("CK_REGISTRO_USO_RECURSO_CODIGO", "length(btrim(\"RECURSO_CODIGO\")) > 0");

                    table.CheckConstraint("CK_REGISTRO_USO_REFERENCIA_ID", "\"REFERENCIA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_REGISTRO_USO_REGRA_6", "(\"CONTA_ID\" IS NULL) <> (\"VISITANTE_ID\" IS NULL)");

                    table.CheckConstraint("CK_REGISTRO_USO_VISITANTE_ID", "\"VISITANTE_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");
                });

            migrationBuilder.CreateTable(
                name: "CONCESSAO_PACOTE",
                schema: "acesso",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    CONTA_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    PACOTE_ACESSO_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    ORIGEM = table.Column<short>(type: "smallint", nullable: false),
                    REFERENCIA_ORIGEM_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    VALIDA_DE = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    VALIDA_ATE = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    REVOGADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CRIADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_CONCESSAO_PACOTE", x => x.ID);

                    table.CheckConstraint("CK_CONCESSAO_PACOTE_CONTA_ID", "\"CONTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_CONCESSAO_PACOTE_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_CONCESSAO_PACOTE_ORIGEM", "\"ORIGEM\" IN (1, 2, 3, 4)");

                    table.CheckConstraint("CK_CONCESSAO_PACOTE_PACOTE_ACESSO_ID", "\"PACOTE_ACESSO_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_CONCESSAO_PACOTE_REFERENCIA_ORIGEM_ID", "\"REFERENCIA_ORIGEM_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_CONCESSAO_PACOTE_REGRA_5", "\"VALIDA_ATE\" IS NULL OR \"VALIDA_ATE\" > \"VALIDA_DE\"");

                    table.ForeignKey(
                        name: "FK_CONCESSAO_PACOTE_PACOTE_ACESSO_PACOTE_ACESSO_ID",
                        column: x => x.PACOTE_ACESSO_ID,
                        principalSchema: "acesso",
                        principalTable: "PACOTE_ACESSO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PACOTE_RECURSO",
                schema: "acesso",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    PACOTE_ACESSO_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    RECURSO_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    HABILITADO = table.Column<bool>(type: "boolean", nullable: false),
                    LIMITE_QUANTIDADE = table.Column<int>(type: "integer", nullable: true),
                    PERIODICIDADE = table.Column<short>(type: "smallint", nullable: true),
                    CONFIGURACAO = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_PACOTE_RECURSO", x => x.ID);

                    table.CheckConstraint("CK_PACOTE_RECURSO_CONFIGURACAO", "jsonb_typeof(\"CONFIGURACAO\") = 'object'");

                    table.CheckConstraint("CK_PACOTE_RECURSO_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_PACOTE_RECURSO_LIMITE_QUANTIDADE", "\"LIMITE_QUANTIDADE\" >= 0 AND \"LIMITE_QUANTIDADE\" <= 2147483647");

                    table.CheckConstraint("CK_PACOTE_RECURSO_PACOTE_ACESSO_ID", "\"PACOTE_ACESSO_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_PACOTE_RECURSO_PERIODICIDADE", "\"PERIODICIDADE\" IN (1, 2, 3)");

                    table.CheckConstraint("CK_PACOTE_RECURSO_RECURSO_ID", "\"RECURSO_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.ForeignKey(
                        name: "FK_PACOTE_RECURSO_PACOTE_ACESSO_PACOTE_ACESSO_ID",
                        column: x => x.PACOTE_ACESSO_ID,
                        principalSchema: "acesso",
                        principalTable: "PACOTE_ACESSO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_PACOTE_RECURSO_RECURSO_RECURSO_ID",
                        column: x => x.RECURSO_ID,
                        principalSchema: "acesso",
                        principalTable: "RECURSO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CONCESSAO_PACOTE_PACOTE_ACESSO_ID",
                schema: "acesso",
                table: "CONCESSAO_PACOTE",
                column: "PACOTE_ACESSO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PACOTE_ACESSO_CODIGO",
                schema: "acesso",
                table: "PACOTE_ACESSO",
                column: "CODIGO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PACOTE_RECURSO_PACOTE_ACESSO_ID_RECURSO_ID",
                schema: "acesso",
                table: "PACOTE_RECURSO",
                columns: new[] { "PACOTE_ACESSO_ID", "RECURSO_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PACOTE_RECURSO_RECURSO_ID",
                schema: "acesso",
                table: "PACOTE_RECURSO",
                column: "RECURSO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_RECURSO_CODIGO",
                schema: "acesso",
                table: "RECURSO",
                column: "CODIGO",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
          migrationBuilder.DropTable(
                name: "CONCESSAO_PACOTE",
                schema: "acesso");

            migrationBuilder.DropTable(
                name: "PACOTE_RECURSO",
                schema: "acesso");

            migrationBuilder.DropTable(
                name: "REGISTRO_USO",
                schema: "acesso");

            migrationBuilder.DropTable(
                name: "PACOTE_ACESSO",
                schema: "acesso");

            migrationBuilder.DropTable(
                name: "RECURSO",
                schema: "acesso");
        }
    }
}
