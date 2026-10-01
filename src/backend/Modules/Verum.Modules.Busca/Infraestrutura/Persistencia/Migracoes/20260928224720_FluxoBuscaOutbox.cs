using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Verum.Modules.Busca.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class FluxoBuscaOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "INBOX_STATE",
                schema: "busca",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MESSAGE_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    CONSUMER_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    LOCK_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    ROW_VERSION = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    RECEIVED = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RECEIVE_COUNT = table.Column<int>(type: "integer", nullable: false),
                    EXPIRATION_TIME = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CONSUMED = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DELIVERED = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LAST_SEQUENCE_NUMBER = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_INBOX_STATE", x => x.ID);
                    table.UniqueConstraint("AK_INBOX_STATE_MESSAGE_ID_CONSUMER_ID", x => new { x.MESSAGE_ID, x.CONSUMER_ID });
                });

            migrationBuilder.CreateTable(
                name: "OUTBOX_STATE",
                schema: "busca",
                columns: table => new
                {
                    OUTBOX_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    LOCK_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    ROW_VERSION = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    CREATED = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DELIVERED = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LAST_SEQUENCE_NUMBER = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OUTBOX_STATE", x => x.OUTBOX_ID);
                });

            migrationBuilder.CreateTable(
                name: "OUTBOX_MESSAGE",
                schema: "busca",
                columns: table => new
                {
                    SEQUENCE_NUMBER = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ENQUEUE_TIME = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SENT_TIME = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HEADERS = table.Column<string>(type: "text", nullable: true),
                    PROPERTIES = table.Column<string>(type: "text", nullable: true),
                    INBOX_MESSAGE_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    INBOX_CONSUMER_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    OUTBOX_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    MESSAGE_ID = table.Column<Guid>(type: "uuid", nullable: false),
                    CONTENT_TYPE = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    MESSAGE_TYPE = table.Column<string>(type: "text", nullable: false),
                    BODY = table.Column<string>(type: "text", nullable: false),
                    CONVERSATION_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    CORRELATION_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    INITIATOR_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    REQUEST_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    SOURCE_ADDRESS = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DESTINATION_ADDRESS = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RESPONSE_ADDRESS = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FAULT_ADDRESS = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EXPIRATION_TIME = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OUTBOX_MESSAGE", x => x.SEQUENCE_NUMBER);
                    table.ForeignKey(
                        name: "FK_OUTBOX_MESSAGE_INBOX_STATE_INBOX_MESSAGE_ID_INBOX_CONSUMER_~",
                        columns: x => new { x.INBOX_MESSAGE_ID, x.INBOX_CONSUMER_ID },
                        principalSchema: "busca",
                        principalTable: "INBOX_STATE",
                        principalColumns: new[] { "MESSAGE_ID", "CONSUMER_ID" });
                    table.ForeignKey(
                        name: "FK_OUTBOX_MESSAGE_OUTBOX_STATE_OUTBOX_ID",
                        column: x => x.OUTBOX_ID,
                        principalSchema: "busca",
                        principalTable: "OUTBOX_STATE",
                        principalColumn: "OUTBOX_ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_INBOX_STATE_DELIVERED",
                schema: "busca",
                table: "INBOX_STATE",
                column: "DELIVERED");

            migrationBuilder.CreateIndex(
                name: "IX_OUTBOX_MESSAGE_ENQUEUE_TIME",
                schema: "busca",
                table: "OUTBOX_MESSAGE",
                column: "ENQUEUE_TIME");

            migrationBuilder.CreateIndex(
                name: "IX_OUTBOX_MESSAGE_EXPIRATION_TIME",
                schema: "busca",
                table: "OUTBOX_MESSAGE",
                column: "EXPIRATION_TIME");

            migrationBuilder.CreateIndex(
                name: "IX_OUTBOX_MESSAGE_INBOX_MESSAGE_ID_INBOX_CONSUMER_ID_SEQUENCE_~",
                schema: "busca",
                table: "OUTBOX_MESSAGE",
                columns: new[] { "INBOX_MESSAGE_ID", "INBOX_CONSUMER_ID", "SEQUENCE_NUMBER" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OUTBOX_MESSAGE_OUTBOX_ID_SEQUENCE_NUMBER",
                schema: "busca",
                table: "OUTBOX_MESSAGE",
                columns: new[] { "OUTBOX_ID", "SEQUENCE_NUMBER" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OUTBOX_STATE_CREATED",
                schema: "busca",
                table: "OUTBOX_STATE",
                column: "CREATED");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OUTBOX_MESSAGE",
                schema: "busca");

            migrationBuilder.DropTable(
                name: "INBOX_STATE",
                schema: "busca");

            migrationBuilder.DropTable(
                name: "OUTBOX_STATE",
                schema: "busca");
        }
    }
}
