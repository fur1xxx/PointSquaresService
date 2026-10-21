using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Persistence.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase().Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder
                .CreateTable(
                    name: "points_collections",
                    columns: table => new
                    {
                        id = table
                            .Column<long>(type: "bigint", nullable: false)
                            .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                        name = table.Column<string>(type: "varchar(200)", nullable: false),
                        points_count = table.Column<int>(type: "int", nullable: false),
                        squares_count = table.Column<long>(type: "bigint", nullable: false),
                        created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                        updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_points_collections", x => x.id);
                    }
                )
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder
                .CreateTable(
                    name: "points",
                    columns: table => new
                    {
                        collection_id = table.Column<long>(type: "bigint", nullable: false),
                        x = table.Column<int>(type: "int", nullable: false),
                        y = table.Column<int>(type: "int", nullable: false),
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey(
                            "PK_points",
                            x => new
                            {
                                x.collection_id,
                                x.x,
                                x.y,
                            }
                        );
                        table.ForeignKey(
                            name: "FK_points_points_collections_collection_id",
                            column: x => x.collection_id,
                            principalTable: "points_collections",
                            principalColumn: "id",
                            onDelete: ReferentialAction.Restrict
                        );
                    }
                )
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(name: "ix_points_collection_id_y_x", table: "points", columns: new[] { "collection_id", "y", "x" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "points");

            migrationBuilder.DropTable(name: "points_collections");
        }
    }
}
