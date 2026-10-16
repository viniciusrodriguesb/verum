using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verum.Modules.Acesso.Infraestrutura.Persistencia.Migracoes
{
  /// <inheritdoc />
  public partial class UsoBuscaAtomico : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateIndex(
          name: "IX_REGISTRO_USO_CONTA_ID_RECURSO_CODIGO_OCORRIDO_EM",
          schema: "acesso",
          table: "REGISTRO_USO",
          columns: new[] { "CONTA_ID", "RECURSO_CODIGO", "OCORRIDO_EM" });

      migrationBuilder.CreateIndex(
          name: "IX_REGISTRO_USO_RECURSO_CODIGO_REFERENCIA_ID",
          schema: "acesso",
          table: "REGISTRO_USO",
          columns: new[] { "RECURSO_CODIGO", "REFERENCIA_ID" },
          unique: true,
          filter: "\"REFERENCIA_ID\" IS NOT NULL");

      migrationBuilder.CreateIndex(
          name: "IX_REGISTRO_USO_VISITANTE_ID_RECURSO_CODIGO_OCORRIDO_EM",
          schema: "acesso",
          table: "REGISTRO_USO",
          columns: new[] { "VISITANTE_ID", "RECURSO_CODIGO", "OCORRIDO_EM" });

      // Preserva o consumo das buscas aceitas antes da adoção do módulo Acesso.
      migrationBuilder.Sql("""
                DO $$ BEGIN
                  IF to_regclass('busca."BUSCA"') IS NOT NULL THEN
                    INSERT INTO acesso."REGISTRO_USO" ("CONTA_ID", "VISITANTE_ID", "RECURSO_CODIGO", "QUANTIDADE", "REFERENCIA_ID", "DADOS", "OCORRIDO_EM")
                    SELECT "CONTA_ID", "VISITANTE_ID", 'BUSCA_EXECUTAR', 1, "ID", '{"origem":"migracao_busca"}'::jsonb, "INICIADA_EM"
                    FROM busca."BUSCA"
                    ON CONFLICT ("RECURSO_CODIGO", "REFERENCIA_ID") WHERE "REFERENCIA_ID" IS NOT NULL DO NOTHING;
                  END IF;
                END $$;
                """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropIndex(
          name: "IX_REGISTRO_USO_CONTA_ID_RECURSO_CODIGO_OCORRIDO_EM",
          schema: "acesso",
          table: "REGISTRO_USO");

      migrationBuilder.DropIndex(
          name: "IX_REGISTRO_USO_RECURSO_CODIGO_REFERENCIA_ID",
          schema: "acesso",
          table: "REGISTRO_USO");

      migrationBuilder.DropIndex(
          name: "IX_REGISTRO_USO_VISITANTE_ID_RECURSO_CODIGO_OCORRIDO_EM",
          schema: "acesso",
          table: "REGISTRO_USO");
    }
  }
}
