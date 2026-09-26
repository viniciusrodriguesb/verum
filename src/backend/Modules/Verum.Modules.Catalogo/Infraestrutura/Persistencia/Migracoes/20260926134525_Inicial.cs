using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verum.Modules.Catalogo.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          migrationBuilder.EnsureSchema(
                name: "catalogo");

            migrationBuilder.CreateTable(
                name: "CATEGORIA",
                schema: "catalogo",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    CATEGORIA_PAI_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    NOME = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    SLUG = table.Column<string>(type: "varchar(140)", maxLength: 140, nullable: false),
                    ATIVA = table.Column<bool>(type: "boolean", nullable: false),
                    CRIADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ATUALIZADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_CATEGORIA", x => x.ID);

                    table.CheckConstraint("CK_CATEGORIA_CATEGORIA_PAI_ID", "\"CATEGORIA_PAI_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_CATEGORIA_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_CATEGORIA_NOME", "length(btrim(\"NOME\")) > 0");

                    table.CheckConstraint("CK_CATEGORIA_REGRA_4", "\"CATEGORIA_PAI_ID\" IS NULL OR \"CATEGORIA_PAI_ID\" <> \"ID\"");

                    table.CheckConstraint("CK_CATEGORIA_SLUG", "length(btrim(\"SLUG\")) > 0");

                    table.ForeignKey(
                        name: "FK_CATEGORIA_CATEGORIA_CATEGORIA_PAI_ID",
                        column: x => x.CATEGORIA_PAI_ID,
                        principalSchema: "catalogo",
                        principalTable: "CATEGORIA",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MARCA",
                schema: "catalogo",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    NOME = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    NOME_NORMALIZADO = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    SLUG = table.Column<string>(type: "varchar(140)", maxLength: 140, nullable: false),
                    ATIVA = table.Column<bool>(type: "boolean", nullable: false),
                    CRIADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_MARCA", x => x.ID);

                    table.CheckConstraint("CK_MARCA_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_MARCA_NOME", "length(btrim(\"NOME\")) > 0");

                    table.CheckConstraint("CK_MARCA_NOME_NORMALIZADO", "length(btrim(\"NOME_NORMALIZADO\")) > 0");

                    table.CheckConstraint("CK_MARCA_SLUG", "length(btrim(\"SLUG\")) > 0");
                });

            migrationBuilder.CreateTable(
                name: "PRODUTO",
                schema: "catalogo",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    CATEGORIA_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    MARCA_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    NOME = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false),
                    NOME_NORMALIZADO = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false),
                    MODELO = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    SLUG = table.Column<string>(type: "varchar(280)", maxLength: 280, nullable: false),
                    DESCRICAO = table.Column<string>(type: "text", nullable: true),
                    ATRIBUTOS = table.Column<string>(type: "jsonb", nullable: false),
                    STATUS = table.Column<short>(type: "smallint", nullable: false),
                    CRIADO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ATUALIZADO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_PRODUTO", x => x.ID);

                    table.CheckConstraint("CK_PRODUTO_ATRIBUTOS", "jsonb_typeof(\"ATRIBUTOS\") = 'object'");

                    table.CheckConstraint("CK_PRODUTO_CATEGORIA_ID", "\"CATEGORIA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_PRODUTO_DESCRICAO", "length(btrim(\"DESCRICAO\")) > 0");

                    table.CheckConstraint("CK_PRODUTO_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_PRODUTO_MARCA_ID", "\"MARCA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_PRODUTO_MODELO", "length(btrim(\"MODELO\")) > 0");

                    table.CheckConstraint("CK_PRODUTO_NOME", "length(btrim(\"NOME\")) > 0");

                    table.CheckConstraint("CK_PRODUTO_NOME_NORMALIZADO", "length(btrim(\"NOME_NORMALIZADO\")) > 0");

                    table.CheckConstraint("CK_PRODUTO_SLUG", "length(btrim(\"SLUG\")) > 0");

                    table.CheckConstraint("CK_PRODUTO_STATUS", "\"STATUS\" IN (1, 2, 3)");

                    table.ForeignKey(
                        name: "FK_PRODUTO_CATEGORIA_CATEGORIA_ID",
                        column: x => x.CATEGORIA_ID,
                        principalSchema: "catalogo",
                        principalTable: "CATEGORIA",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_PRODUTO_MARCA_MARCA_ID",
                        column: x => x.MARCA_ID,
                        principalSchema: "catalogo",
                        principalTable: "MARCA",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PRODUTO_VARIANTE",
                schema: "catalogo",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    PRODUTO_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    NOME = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    NOME_NORMALIZADO = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    SLUG = table.Column<string>(type: "varchar(320)", maxLength: 320, nullable: false),
                    ATRIBUTOS = table.Column<string>(type: "jsonb", nullable: false),
                    STATUS = table.Column<short>(type: "smallint", nullable: false),
                    CRIADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ATUALIZADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_PRODUTO_VARIANTE", x => x.ID);

                    table.UniqueConstraint("AK_PRODUTO_VARIANTE_ID_PRODUTO_ID", x => new { x.ID, x.PRODUTO_ID });

                    table.CheckConstraint("CK_PRODUTO_VARIANTE_ATRIBUTOS", "jsonb_typeof(\"ATRIBUTOS\") = 'object'");

                    table.CheckConstraint("CK_PRODUTO_VARIANTE_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_PRODUTO_VARIANTE_NOME", "length(btrim(\"NOME\")) > 0");

                    table.CheckConstraint("CK_PRODUTO_VARIANTE_NOME_NORMALIZADO", "length(btrim(\"NOME_NORMALIZADO\")) > 0");

                    table.CheckConstraint("CK_PRODUTO_VARIANTE_PRODUTO_ID", "\"PRODUTO_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_PRODUTO_VARIANTE_SLUG", "length(btrim(\"SLUG\")) > 0");

                    table.CheckConstraint("CK_PRODUTO_VARIANTE_STATUS", "\"STATUS\" IN (1, 2, 3)");

                    table.ForeignKey(
                        name: "FK_PRODUTO_VARIANTE_PRODUTO_PRODUTO_ID",
                        column: x => x.PRODUTO_ID,
                        principalSchema: "catalogo",
                        principalTable: "PRODUTO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PRODUTO_IDENTIFICADOR",
                schema: "catalogo",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    PRODUTO_VARIANTE_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    TIPO = table.Column<short>(type: "smallint", nullable: false),
                    VALOR = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    CRIADO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_PRODUTO_IDENTIFICADOR", x => x.ID);

                    table.CheckConstraint("CK_PRODUTO_IDENTIFICADOR_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_PRODUTO_IDENTIFICADOR_PRODUTO_VARIANTE_ID", "\"PRODUTO_VARIANTE_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_PRODUTO_IDENTIFICADOR_TIPO", "\"TIPO\" IN (1, 2, 3, 4, 5)");

                    table.CheckConstraint("CK_PRODUTO_IDENTIFICADOR_VALOR", "length(btrim(\"VALOR\")) > 0");

                    table.ForeignKey(
                        name: "FK_PRODUTO_IDENTIFICADOR_PRODUTO_VARIANTE_PRODUTO_VARIANTE_ID",
                        column: x => x.PRODUTO_VARIANTE_ID,
                        principalSchema: "catalogo",
                        principalTable: "PRODUTO_VARIANTE",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PRODUTO_IMAGEM",
                schema: "catalogo",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    PRODUTO_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    PRODUTO_VARIANTE_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    URL = table.Column<string>(type: "text", nullable: false),
                    ORIGEM = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    PRINCIPAL = table.Column<bool>(type: "boolean", nullable: false),
                    ORDEM = table.Column<short>(type: "smallint", nullable: false),
                    CRIADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_PRODUTO_IMAGEM", x => x.ID);

                    table.CheckConstraint("CK_PRODUTO_IMAGEM_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_PRODUTO_IMAGEM_ORDEM", "\"ORDEM\" >= 0 AND \"ORDEM\" <= 32767");

                    table.CheckConstraint("CK_PRODUTO_IMAGEM_ORIGEM", "length(btrim(\"ORIGEM\")) > 0");

                    table.CheckConstraint("CK_PRODUTO_IMAGEM_PRODUTO_ID", "\"PRODUTO_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_PRODUTO_IMAGEM_PRODUTO_VARIANTE_ID", "\"PRODUTO_VARIANTE_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_PRODUTO_IMAGEM_URL", "length(btrim(\"URL\")) > 0");

                    table.ForeignKey(
                        name: "FK_PRODUTO_IMAGEM_PRODUTO_PRODUTO_ID",
                        column: x => x.PRODUTO_ID,
                        principalSchema: "catalogo",
                        principalTable: "PRODUTO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_PRODUTO_IMAGEM_PRODUTO_VARIANTE_PRODUTO_VARIANTE_ID_PRODUTO~",
                        columns: x => new { x.PRODUTO_VARIANTE_ID, x.PRODUTO_ID },
                        principalSchema: "catalogo",
                        principalTable: "PRODUTO_VARIANTE",
                        principalColumns: new[] { "ID", "PRODUTO_ID" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PRODUTO_TERMO_BUSCA",
                schema: "catalogo",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    PRODUTO_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    PRODUTO_VARIANTE_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    TERMO_ORIGINAL = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    TERMO_NORMALIZADO = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    ORIGEM = table.Column<short>(type: "smallint", nullable: false),
                    CONFIANCA = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    CRIADO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_PRODUTO_TERMO_BUSCA", x => x.ID);

                    table.CheckConstraint("CK_PRODUTO_TERMO_BUSCA_CONFIANCA", "\"CONFIANCA\" >= 0 AND \"CONFIANCA\" <= 1");

                    table.CheckConstraint("CK_PRODUTO_TERMO_BUSCA_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_PRODUTO_TERMO_BUSCA_ORIGEM", "\"ORIGEM\" IN (1, 2, 3, 4)");

                    table.CheckConstraint("CK_PRODUTO_TERMO_BUSCA_PRODUTO_ID", "\"PRODUTO_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_PRODUTO_TERMO_BUSCA_PRODUTO_VARIANTE_ID", "\"PRODUTO_VARIANTE_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_PRODUTO_TERMO_BUSCA_TERMO_NORMALIZADO", "length(btrim(\"TERMO_NORMALIZADO\")) > 0");

                    table.CheckConstraint("CK_PRODUTO_TERMO_BUSCA_TERMO_ORIGINAL", "length(btrim(\"TERMO_ORIGINAL\")) > 0");

                    table.ForeignKey(
                        name: "FK_PRODUTO_TERMO_BUSCA_PRODUTO_PRODUTO_ID",
                        column: x => x.PRODUTO_ID,
                        principalSchema: "catalogo",
                        principalTable: "PRODUTO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_PRODUTO_TERMO_BUSCA_PRODUTO_VARIANTE_PRODUTO_VARIANTE_ID_PR~",
                        columns: x => new { x.PRODUTO_VARIANTE_ID, x.PRODUTO_ID },
                        principalSchema: "catalogo",
                        principalTable: "PRODUTO_VARIANTE",
                        principalColumns: new[] { "ID", "PRODUTO_ID" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CATEGORIA_CATEGORIA_PAI_ID",
                schema: "catalogo",
                table: "CATEGORIA",
                column: "CATEGORIA_PAI_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CATEGORIA_SLUG",
                schema: "catalogo",
                table: "CATEGORIA",
                column: "SLUG",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MARCA_NOME_NORMALIZADO",
                schema: "catalogo",
                table: "MARCA",
                column: "NOME_NORMALIZADO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MARCA_SLUG",
                schema: "catalogo",
                table: "MARCA",
                column: "SLUG",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PRODUTO_CATEGORIA_ID",
                schema: "catalogo",
                table: "PRODUTO",
                column: "CATEGORIA_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUTO_MARCA_ID",
                schema: "catalogo",
                table: "PRODUTO",
                column: "MARCA_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUTO_SLUG",
                schema: "catalogo",
                table: "PRODUTO",
                column: "SLUG",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PRODUTO_IDENTIFICADOR_PRODUTO_VARIANTE_ID",
                schema: "catalogo",
                table: "PRODUTO_IDENTIFICADOR",
                column: "PRODUTO_VARIANTE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUTO_IDENTIFICADOR_TIPO_VALOR",
                schema: "catalogo",
                table: "PRODUTO_IDENTIFICADOR",
                columns: new[] { "TIPO", "VALOR" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PRODUTO_IMAGEM_PRODUTO_ID",
                schema: "catalogo",
                table: "PRODUTO_IMAGEM",
                column: "PRODUTO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUTO_IMAGEM_PRODUTO_VARIANTE_ID_PRODUTO_ID",
                schema: "catalogo",
                table: "PRODUTO_IMAGEM",
                columns: new[] { "PRODUTO_VARIANTE_ID", "PRODUTO_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_PRODUTO_TERMO_BUSCA_PRODUTO_ID",
                schema: "catalogo",
                table: "PRODUTO_TERMO_BUSCA",
                column: "PRODUTO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUTO_TERMO_BUSCA_PRODUTO_VARIANTE_ID_PRODUTO_ID",
                schema: "catalogo",
                table: "PRODUTO_TERMO_BUSCA",
                columns: new[] { "PRODUTO_VARIANTE_ID", "PRODUTO_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_PRODUTO_VARIANTE_PRODUTO_ID",
                schema: "catalogo",
                table: "PRODUTO_VARIANTE",
                column: "PRODUTO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUTO_VARIANTE_SLUG",
                schema: "catalogo",
                table: "PRODUTO_VARIANTE",
                column: "SLUG",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
          migrationBuilder.DropTable(
                name: "PRODUTO_IDENTIFICADOR",
                schema: "catalogo");

            migrationBuilder.DropTable(
                name: "PRODUTO_IMAGEM",
                schema: "catalogo");

            migrationBuilder.DropTable(
                name: "PRODUTO_TERMO_BUSCA",
                schema: "catalogo");

            migrationBuilder.DropTable(
                name: "PRODUTO_VARIANTE",
                schema: "catalogo");

            migrationBuilder.DropTable(
                name: "PRODUTO",
                schema: "catalogo");

            migrationBuilder.DropTable(
                name: "CATEGORIA",
                schema: "catalogo");

            migrationBuilder.DropTable(
                name: "MARCA",
                schema: "catalogo");
        }
    }
}
