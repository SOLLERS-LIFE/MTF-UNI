using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MTF.Migrations.LogDB_
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cacheControls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    uid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    wfid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cacheControls", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChatHistory",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    dateIn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    uId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    msg = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    machineName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    appIdent = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatHistory", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClaimsLog",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    author = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    logged = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    viewed = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    viewedby = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    claimText = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimsLog", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "claimsLogcacheControl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    uid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    wfid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claimsLogcacheControl", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClaimsLogCatched",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    actorId = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    author = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    logged = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    viewed = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    viewedby = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    claimText = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    prevID = table.Column<int>(type: "int", nullable: false),
                    marked = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimsLogCatched", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "combinedFilters",
                columns: table => new
                {
                    uid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    subsys = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    filter = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_combinedFilters", x => new { x.uid, x.subsys });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GeneralLog",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    machineName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true, defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    appIdent = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true, defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    logged = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    _level = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false, defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    message = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: false, defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    logger = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true, defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    properties = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true, defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    callsite = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true, defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _exception = table.Column<string>(type: "varchar(4096)", maxLength: 4096, nullable: true, defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    url = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true, defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reqhost = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true, defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    uId = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true, defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralLog", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GeneralLogCatched",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    actorId = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    machineName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    appIdent = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    logged = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    _level = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    message = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    logger = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    properties = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    callsite = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _exception = table.Column<string>(type: "varchar(4096)", maxLength: 4096, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    url = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reqhost = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    uId = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    prevID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralLogCatched", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GeneralLogPermanent",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    appIdent = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    logged = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    _level = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    message = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralLogPermanent", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ChatHistory_appIdent",
                table: "ChatHistory",
                column: "appIdent");

            migrationBuilder.CreateIndex(
                name: "IX_ChatHistory_dateIn",
                table: "ChatHistory",
                column: "dateIn");

            migrationBuilder.CreateIndex(
                name: "IX_ChatHistory_machineName",
                table: "ChatHistory",
                column: "machineName");

            migrationBuilder.CreateIndex(
                name: "IX_ChatHistory_msg",
                table: "ChatHistory",
                column: "msg");

            migrationBuilder.CreateIndex(
                name: "IX_ChatHistory_uId",
                table: "ChatHistory",
                column: "uId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimsLog_author",
                table: "ClaimsLog",
                column: "author");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimsLog_claimText",
                table: "ClaimsLog",
                column: "claimText");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimsLog_logged_viewed",
                table: "ClaimsLog",
                columns: new[] { "logged", "viewed" });

            migrationBuilder.CreateIndex(
                name: "IX_ClaimsLogCatched_actorId",
                table: "ClaimsLogCatched",
                column: "actorId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimsLogCatched_logged_viewed",
                table: "ClaimsLogCatched",
                columns: new[] { "logged", "viewed" });

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLog_appIdent",
                table: "GeneralLog",
                column: "appIdent");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLog_logged",
                table: "GeneralLog",
                column: "logged");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLog_machineName",
                table: "GeneralLog",
                column: "machineName");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLog_message",
                table: "GeneralLog",
                column: "message");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLog_uId",
                table: "GeneralLog",
                column: "uId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLogCatched_actorId",
                table: "GeneralLogCatched",
                column: "actorId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLogCatched_appIdent",
                table: "GeneralLogCatched",
                column: "appIdent");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLogCatched_logged",
                table: "GeneralLogCatched",
                column: "logged");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLogCatched_machineName",
                table: "GeneralLogCatched",
                column: "machineName");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLogCatched_message",
                table: "GeneralLogCatched",
                column: "message");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLogPermanent_logged",
                table: "GeneralLogPermanent",
                column: "logged");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLogPermanent_message",
                table: "GeneralLogPermanent",
                column: "message");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cacheControls");

            migrationBuilder.DropTable(
                name: "ChatHistory");

            migrationBuilder.DropTable(
                name: "ClaimsLog");

            migrationBuilder.DropTable(
                name: "claimsLogcacheControl");

            migrationBuilder.DropTable(
                name: "ClaimsLogCatched");

            migrationBuilder.DropTable(
                name: "combinedFilters");

            migrationBuilder.DropTable(
                name: "GeneralLog");

            migrationBuilder.DropTable(
                name: "GeneralLogCatched");

            migrationBuilder.DropTable(
                name: "GeneralLogPermanent");
        }
    }
}
