using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Verum.Modules.Ofertas.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          migrationBuilder.EnsureSchema(
                name: "ofertas");

            migrationBuilder.CreateTable(
                name: "FONTE_OFERTA",
                schema: "ofertas",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    NOME = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    TIPO = table.Column<short>(type: "smallint", nullable: false),
                    CODIGO = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    ATIVA = table.Column<bool>(type: "boolean", nullable: false),
                    NIVEL_CONFIANCA = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    CRIADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_FONTE_OFERTA", x => x.ID);

                    table.CheckConstraint("CK_FONTE_OFERTA_CODIGO", "length(btrim(\"CODIGO\")) > 0");

                    table.CheckConstraint("CK_FONTE_OFERTA_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_FONTE_OFERTA_NIVEL_CONFIANCA", "\"NIVEL_CONFIANCA\" >= 0 AND \"NIVEL_CONFIANCA\" <= 100");

                    table.CheckConstraint("CK_FONTE_OFERTA_NOME", "length(btrim(\"NOME\")) > 0");

                    table.CheckConstraint("CK_FONTE_OFERTA_TIPO", "\"TIPO\" IN (1, 2, 3, 4, 5)");
                });

            migrationBuilder.CreateTable(
                name: "LOJA",
                schema: "ofertas",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    NOME = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false),
                    NOME_NORMALIZADO = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false),
                    DOMINIO = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    URL = table.Column<string>(type: "text", nullable: true),
                    VERIFICADA = table.Column<bool>(type: "boolean", nullable: false),
                    PONTUACAO_CONFIANCA = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    STATUS = table.Column<short>(type: "smallint", nullable: false),
                    CRIADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ATUALIZADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_LOJA", x => x.ID);

                    table.CheckConstraint("CK_LOJA_DOMINIO", "length(btrim(\"DOMINIO\")) > 0");

                    table.CheckConstraint("CK_LOJA_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_LOJA_NOME", "length(btrim(\"NOME\")) > 0");

                    table.CheckConstraint("CK_LOJA_NOME_NORMALIZADO", "length(btrim(\"NOME_NORMALIZADO\")) > 0");

                    table.CheckConstraint("CK_LOJA_PONTUACAO_CONFIANCA", "\"PONTUACAO_CONFIANCA\" >= 0 AND \"PONTUACAO_CONFIANCA\" <= 100");

                    table.CheckConstraint("CK_LOJA_STATUS", "\"STATUS\" IN (1, 2, 3)");

                    table.CheckConstraint("CK_LOJA_URL", "length(btrim(\"URL\")) > 0");
                });

            migrationBuilder.CreateTable(
                name: "EXECUCAO_CONSULTA_FONTE",
                schema: "ofertas",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    CORRELACAO_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    BUSCA_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    FONTE_OFERTA_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    TIPO_PROVEDOR = table.Column<short>(type: "smallint", nullable: false),
                    STATUS = table.Column<short>(type: "smallint", nullable: false),
                    QUANTIDADE_ITENS_ENCONTRADOS = table.Column<int>(type: "integer", nullable: false),
                    QUANTIDADE_ITENS_ACEITOS = table.Column<int>(type: "integer", nullable: false),
                    INICIADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FINALIZADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NUMERO_TENTATIVAS = table.Column<short>(type: "smallint", nullable: false),
                    CODIGO_ERRO = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    DETALHES_ERRO = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_EXECUCAO_CONSULTA_FONTE", x => x.ID);

                    table.CheckConstraint("CK_EXECUCAO_CONSULTA_FONTE_BUSCA_ID", "\"BUSCA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_EXECUCAO_CONSULTA_FONTE_CODIGO_ERRO", "length(btrim(\"CODIGO_ERRO\")) > 0");

                    table.CheckConstraint("CK_EXECUCAO_CONSULTA_FONTE_CORRELACAO_ID", "\"CORRELACAO_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_EXECUCAO_CONSULTA_FONTE_DETALHES_ERRO", "length(btrim(\"DETALHES_ERRO\")) > 0");

                    table.CheckConstraint("CK_EXECUCAO_CONSULTA_FONTE_FONTE_OFERTA_ID", "\"FONTE_OFERTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_EXECUCAO_CONSULTA_FONTE_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_EXECUCAO_CONSULTA_FONTE_NUMERO_TENTATIVAS", "\"NUMERO_TENTATIVAS\" >= 0 AND \"NUMERO_TENTATIVAS\" <= 32767");

                    table.CheckConstraint("CK_EXECUCAO_CONSULTA_FONTE_QUANTIDADE_ITENS_ACEITOS", "\"QUANTIDADE_ITENS_ACEITOS\" >= 0 AND \"QUANTIDADE_ITENS_ACEITOS\" <= 2147483647");

                    table.CheckConstraint("CK_EXECUCAO_CONSULTA_FONTE_QUANTIDADE_ITENS_ENCONTRADOS", "\"QUANTIDADE_ITENS_ENCONTRADOS\" >= 0 AND \"QUANTIDADE_ITENS_ENCONTRADOS\" <= 2147483647");

                    table.CheckConstraint("CK_EXECUCAO_CONSULTA_FONTE_STATUS", "\"STATUS\" IN (1, 2, 3, 4, 5, 6, 7)");

                    table.CheckConstraint("CK_EXECUCAO_CONSULTA_FONTE_TIPO_PROVEDOR", "\"TIPO_PROVEDOR\" IN (1, 2, 3, 4, 5)");

                    table.ForeignKey(
                        name: "FK_EXECUCAO_CONSULTA_FONTE_FONTE_OFERTA_FONTE_OFERTA_ID",
                        column: x => x.FONTE_OFERTA_ID,
                        principalSchema: "ofertas",
                        principalTable: "FONTE_OFERTA",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OFERTA",
                schema: "ofertas",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    PRODUTO_VARIANTE_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    LOJA_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    FONTE_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    IDENTIFICADOR_EXTERNO = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true),
                    TITULO_EXTERNO = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    URL = table.Column<string>(type: "text", nullable: false),
                    URL_HASH = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    PRECO_ATUAL = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    PRECO_PIX = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true),
                    PRECO_ANTERIOR = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true),
                    QUANTIDADE_PARCELAS = table.Column<short>(type: "smallint", nullable: true),
                    VALOR_PARCELA = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true),
                    CONDICAO_PRECO = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true),
                    DISPONIBILIDADE = table.Column<short>(type: "smallint", nullable: false),
                    CONDICAO_PRODUTO = table.Column<short>(type: "smallint", nullable: false),
                    IMAGEM_URL = table.Column<string>(type: "text", nullable: true),
                    OBSERVADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    VALIDA_ATE = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ULTIMA_CONFIRMACAO_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    STATUS = table.Column<short>(type: "smallint", nullable: false),
                    CRIADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ATUALIZADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_OFERTA", x => x.ID);

                    table.CheckConstraint("CK_OFERTA_CONDICAO_PRECO", "length(btrim(\"CONDICAO_PRECO\")) > 0");

                    table.CheckConstraint("CK_OFERTA_CONDICAO_PRODUTO", "\"CONDICAO_PRODUTO\" IN (1, 2, 3)");

                    table.CheckConstraint("CK_OFERTA_DISPONIBILIDADE", "\"DISPONIBILIDADE\" IN (1, 2, 3)");

                    table.CheckConstraint("CK_OFERTA_FONTE_ID", "\"FONTE_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_OFERTA_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_OFERTA_IDENTIFICADOR_EXTERNO", "length(btrim(\"IDENTIFICADOR_EXTERNO\")) > 0");

                    table.CheckConstraint("CK_OFERTA_IMAGEM_URL", "length(btrim(\"IMAGEM_URL\")) > 0");

                    table.CheckConstraint("CK_OFERTA_LOJA_ID", "\"LOJA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_OFERTA_PRECO_ANTERIOR", "\"PRECO_ANTERIOR\" >= 0.01 AND \"PRECO_ANTERIOR\" <= 999999999999.99");

                    table.CheckConstraint("CK_OFERTA_PRECO_ATUAL", "\"PRECO_ATUAL\" >= 0.01 AND \"PRECO_ATUAL\" <= 999999999999.99");

                    table.CheckConstraint("CK_OFERTA_PRECO_PIX", "\"PRECO_PIX\" >= 0.01 AND \"PRECO_PIX\" <= 999999999999.99");

                    table.CheckConstraint("CK_OFERTA_PRODUTO_VARIANTE_ID", "\"PRODUTO_VARIANTE_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_OFERTA_QUANTIDADE_PARCELAS", "\"QUANTIDADE_PARCELAS\" >= 1 AND \"QUANTIDADE_PARCELAS\" <= 32767");

                    table.CheckConstraint("CK_OFERTA_REGRA_18", "\"VALIDA_ATE\" > \"OBSERVADA_EM\"");

                    table.CheckConstraint("CK_OFERTA_REGRA_19", "(\"QUANTIDADE_PARCELAS\" IS NULL) = (\"VALOR_PARCELA\" IS NULL)");

                    table.CheckConstraint("CK_OFERTA_STATUS", "\"STATUS\" IN (1, 2, 3)");

                    table.CheckConstraint("CK_OFERTA_TITULO_EXTERNO", "length(btrim(\"TITULO_EXTERNO\")) > 0");

                    table.CheckConstraint("CK_OFERTA_URL", "length(btrim(\"URL\")) > 0");

                    table.CheckConstraint("CK_OFERTA_URL_HASH", "length(btrim(\"URL_HASH\")) > 0");

                    table.CheckConstraint("CK_OFERTA_VALOR_PARCELA", "\"VALOR_PARCELA\" >= 0.01 AND \"VALOR_PARCELA\" <= 999999999999.99");

                    table.ForeignKey(
                        name: "FK_OFERTA_FONTE_OFERTA_FONTE_ID",
                        column: x => x.FONTE_ID,
                        principalSchema: "ofertas",
                        principalTable: "FONTE_OFERTA",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_OFERTA_LOJA_LOJA_ID",
                        column: x => x.LOJA_ID,
                        principalSchema: "ofertas",
                        principalTable: "LOJA",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OFERTA_OBSERVACAO",
                schema: "ofertas",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OFERTA_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    FONTE_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    PRECO = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    PRECO_PIX = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true),
                    QUANTIDADE_PARCELAS = table.Column<short>(type: "smallint", nullable: true),
                    VALOR_PARCELA = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true),
                    CONDICAO_PRECO = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true),
                    DISPONIBILIDADE = table.Column<short>(type: "smallint", nullable: false),
                    OBSERVADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    HASH_CONTEUDO = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    EVIDENCIA = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_OFERTA_OBSERVACAO", x => x.ID);

                    table.CheckConstraint("CK_OFERTA_OBSERVACAO_CONDICAO_PRECO", "length(btrim(\"CONDICAO_PRECO\")) > 0");

                    table.CheckConstraint("CK_OFERTA_OBSERVACAO_DISPONIBILIDADE", "\"DISPONIBILIDADE\" IN (1, 2, 3)");

                    table.CheckConstraint("CK_OFERTA_OBSERVACAO_EVIDENCIA", "jsonb_typeof(\"EVIDENCIA\") = 'object'");

                    table.CheckConstraint("CK_OFERTA_OBSERVACAO_FONTE_ID", "\"FONTE_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_OFERTA_OBSERVACAO_HASH_CONTEUDO", "length(btrim(\"HASH_CONTEUDO\")) > 0");

                    table.CheckConstraint("CK_OFERTA_OBSERVACAO_OFERTA_ID", "\"OFERTA_ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_OFERTA_OBSERVACAO_PRECO", "\"PRECO\" >= 0.01 AND \"PRECO\" <= 999999999999.99");

                    table.CheckConstraint("CK_OFERTA_OBSERVACAO_PRECO_PIX", "\"PRECO_PIX\" >= 0.01 AND \"PRECO_PIX\" <= 999999999999.99");

                    table.CheckConstraint("CK_OFERTA_OBSERVACAO_QUANTIDADE_PARCELAS", "\"QUANTIDADE_PARCELAS\" >= 1 AND \"QUANTIDADE_PARCELAS\" <= 32767");

                    table.CheckConstraint("CK_OFERTA_OBSERVACAO_REGRA_10", "(\"QUANTIDADE_PARCELAS\" IS NULL) = (\"VALOR_PARCELA\" IS NULL)");

                    table.CheckConstraint("CK_OFERTA_OBSERVACAO_VALOR_PARCELA", "\"VALOR_PARCELA\" >= 0.01 AND \"VALOR_PARCELA\" <= 999999999999.99");

                    table.ForeignKey(
                        name: "FK_OFERTA_OBSERVACAO_FONTE_OFERTA_FONTE_ID",
                        column: x => x.FONTE_ID,
                        principalSchema: "ofertas",
                        principalTable: "FONTE_OFERTA",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_OFERTA_OBSERVACAO_OFERTA_OFERTA_ID",
                        column: x => x.OFERTA_ID,
                        principalSchema: "ofertas",
                        principalTable: "OFERTA",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EXECUCAO_CONSULTA_FONTE_FONTE_OFERTA_ID",
                schema: "ofertas",
                table: "EXECUCAO_CONSULTA_FONTE",
                column: "FONTE_OFERTA_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FONTE_OFERTA_CODIGO",
                schema: "ofertas",
                table: "FONTE_OFERTA",
                column: "CODIGO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OFERTA_FONTE_ID",
                schema: "ofertas",
                table: "OFERTA",
                column: "FONTE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_OFERTA_LOJA_ID_URL_HASH",
                schema: "ofertas",
                table: "OFERTA",
                columns: new[] { "LOJA_ID", "URL_HASH" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OFERTA_OBSERVACAO_FONTE_ID",
                schema: "ofertas",
                table: "OFERTA_OBSERVACAO",
                column: "FONTE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_OFERTA_OBSERVACAO_OFERTA_ID",
                schema: "ofertas",
                table: "OFERTA_OBSERVACAO",
                column: "OFERTA_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
          migrationBuilder.DropTable(
                name: "EXECUCAO_CONSULTA_FONTE",
                schema: "ofertas");

            migrationBuilder.DropTable(
                name: "OFERTA_OBSERVACAO",
                schema: "ofertas");

            migrationBuilder.DropTable(
                name: "OFERTA",
                schema: "ofertas");

            migrationBuilder.DropTable(
                name: "FONTE_OFERTA",
                schema: "ofertas");

            migrationBuilder.DropTable(
                name: "LOJA",
                schema: "ofertas");
        }
    }
}
