using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VGManager.Library.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KeyVault_copy",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    OriginalKeyVault = table.Column<string>(type: "text", nullable: false),
                    DestinationKeyVault = table.Column<string>(type: "text", nullable: false),
                    User = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyVault_copy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Secret_changes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    KeyVaultName = table.Column<string>(type: "text", nullable: false),
                    SecretNameRegex = table.Column<string>(type: "text", nullable: false),
                    ChangeType = table.Column<int>(type: "integer", nullable: false),
                    User = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Secret_changes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Variable_additions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    User = table.Column<string>(type: "text", nullable: false),
                    Organization = table.Column<string>(type: "text", nullable: false),
                    Project = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VariableGroupFilter = table.Column<string>(type: "text", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Variable_additions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Variable_deletions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    User = table.Column<string>(type: "text", nullable: false),
                    Organization = table.Column<string>(type: "text", nullable: false),
                    Project = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VariableGroupFilter = table.Column<string>(type: "text", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Variable_deletions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Variable_editions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    NewValue = table.Column<string>(type: "text", nullable: false),
                    User = table.Column<string>(type: "text", nullable: false),
                    Organization = table.Column<string>(type: "text", nullable: false),
                    Project = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VariableGroupFilter = table.Column<string>(type: "text", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Variable_editions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KeyVault_copy");

            migrationBuilder.DropTable(
                name: "Secret_changes");

            migrationBuilder.DropTable(
                name: "Variable_additions");

            migrationBuilder.DropTable(
                name: "Variable_deletions");

            migrationBuilder.DropTable(
                name: "Variable_editions");
        }
    }
}
