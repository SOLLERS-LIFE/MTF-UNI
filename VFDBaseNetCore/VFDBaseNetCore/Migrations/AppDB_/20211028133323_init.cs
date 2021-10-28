using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MTF.Migrations.AppDB_
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bs_invite_traps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_bst = table.Column<int>(type: "int", nullable: false),
                    user_email = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _state = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValue: (sbyte)0),
                    change_id_user = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    change_logged = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bs_invite_traps", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bs_marks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(127)", maxLength: 127, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bs_marks", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bs_marks_to_users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_mark = table.Column<int>(type: "int", nullable: false),
                    id_team = table.Column<int>(type: "int", nullable: false),
                    id_user = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bs_marks_to_users", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bs_objects_protocol",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    _type = table.Column<int>(type: "int", nullable: false),
                    id_object = table.Column<int>(type: "int", nullable: false),
                    id_user = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    logged = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    action_dscr = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bs_objects_protocol", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bs_security_disable",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_user = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _state = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bs_security_disable", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bs_team_configs",
                columns: table => new
                {
                    id_team = table.Column<int>(type: "int", nullable: false),
                    id_cnf = table.Column<int>(type: "int", nullable: false),
                    cnfValue = table.Column<string>(type: "varchar(127)", maxLength: 127, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bs_team_configs", x => new { x.id_cnf, x.id_team });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bs_team_configs_avl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    cnfName = table.Column<string>(type: "varchar(127)", maxLength: 127, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cnfDefault = table.Column<string>(type: "varchar(127)", maxLength: 127, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bs_team_configs_avl", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bs_team_marks",
                columns: table => new
                {
                    id_bst = table.Column<int>(type: "int", nullable: false),
                    id_mark = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bs_team_marks", x => new { x.id_bst, x.id_mark });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bs_team_users",
                columns: table => new
                {
                    id_bst = table.Column<int>(type: "int", nullable: false),
                    id_user = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    _role = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValue: (sbyte)0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bs_team_users", x => new { x.id_bst, x.id_user });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bs_teams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(127)", maxLength: 127, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bs_teams", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sa_global_configs",
                columns: table => new
                {
                    cnfName = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cnfValue = table.Column<string>(type: "varchar(127)", maxLength: 127, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sa_global_configs", x => x.cnfName);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sa_tech_ticks_protocol",
                columns: table => new
                {
                    logged = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    _type = table.Column<int>(type: "int", nullable: false),
                    dscr = table.Column<string>(type: "varchar(127)", maxLength: 127, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sa_tech_ticks_protocol", x => x.logged);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sa_uidTranslator",
                columns: table => new
                {
                    id_user = table.Column<string>(type: "VARCHAR(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    uid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sa_uidTranslator", x => x.id_user);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sa_user_configs",
                columns: table => new
                {
                    uid = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cnfName = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cnfValue = table.Column<string>(type: "varchar(127)", maxLength: 127, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sa_user_configs", x => new { x.uid, x.cnfName });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TGO_trivial_table",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ActorId = table.Column<string>(type: "varchar(127)", maxLength: 127, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActionDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExactDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Selected = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TGO_trivial_table", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_bs_invite_traps_fnd",
                table: "bs_invite_traps",
                columns: new[] { "user_email", "_state" });

            migrationBuilder.CreateIndex(
                name: "IX_bs_marks_name",
                table: "bs_marks",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX__bs_marks_to_users_id_mark_id_user",
                table: "bs_marks_to_users",
                columns: new[] { "id_mark", "id_user" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bs_protocol_type_id_object",
                table: "bs_objects_protocol",
                columns: new[] { "_type", "id_object" });

            migrationBuilder.CreateIndex(
                name: "IX_bs_security_disable_id_user",
                table: "bs_security_disable",
                column: "id_user");

            migrationBuilder.CreateIndex(
                name: "IX_bs_team_configs_avl_cnfName",
                table: "bs_team_configs_avl",
                column: "cnfName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bs_teams_name",
                table: "bs_teams",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sa_uidTranslator_uid",
                table: "sa_uidTranslator",
                column: "uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TGO_trivial_table_ActorId",
                table: "TGO_trivial_table",
                column: "ActorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bs_invite_traps");

            migrationBuilder.DropTable(
                name: "bs_marks");

            migrationBuilder.DropTable(
                name: "bs_marks_to_users");

            migrationBuilder.DropTable(
                name: "bs_objects_protocol");

            migrationBuilder.DropTable(
                name: "bs_security_disable");

            migrationBuilder.DropTable(
                name: "bs_team_configs");

            migrationBuilder.DropTable(
                name: "bs_team_configs_avl");

            migrationBuilder.DropTable(
                name: "bs_team_marks");

            migrationBuilder.DropTable(
                name: "bs_team_users");

            migrationBuilder.DropTable(
                name: "bs_teams");

            migrationBuilder.DropTable(
                name: "sa_global_configs");

            migrationBuilder.DropTable(
                name: "sa_tech_ticks_protocol");

            migrationBuilder.DropTable(
                name: "sa_uidTranslator");

            migrationBuilder.DropTable(
                name: "sa_user_configs");

            migrationBuilder.DropTable(
                name: "TGO_trivial_table");
        }
    }
}
