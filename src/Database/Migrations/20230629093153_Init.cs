using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AndNet.Manager.Database.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AndNet");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:AndNet.rum", ",,");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                schema: "AndNet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                schema: "AndNet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Elections",
                schema: "AndNet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    Stage = table.Column<byte>(type: "smallint", nullable: false),
                    ElectionEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CouncilCapacity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Elections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                schema: "AndNet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "AndNet",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                schema: "AndNet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "AndNet",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                schema: "AndNet",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "AndNet",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                schema: "AndNet",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "AndNet",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "AndNet",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                schema: "AndNet",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "AndNet",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                schema: "AndNet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdentityId = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    Nickname = table.Column<string>(type: "text", nullable: false),
                    RealName = table.Column<string>(type: "text", nullable: true),
                    DiscordId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    SteamId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    DetectionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimeZone = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    JoinDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Rank = table.Column<short>(type: "smallint", nullable: true),
                    Score = table.Column<double>(type: "double precision", nullable: true),
                    OnReserve = table.Column<bool>(type: "boolean", nullable: true),
                    Relationship = table.Column<short>(type: "smallint", nullable: true),
                    RestorationAvailable = table.Column<bool>(type: "boolean", nullable: true),
                    LeaveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LeaveReason = table.Column<byte>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_AspNetUsers_IdentityId",
                        column: x => x.IdentityId,
                        principalSchema: "AndNet",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                schema: "AndNet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuthorId = table.Column<int>(type: "integer", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    Views = table.Column<int>(type: "integer", nullable: false),
                    Info = table.Column<string>(type: "jsonb", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_Documents_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "AndNet",
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Players_AuthorId",
                        column: x => x.AuthorId,
                        principalSchema: "AndNet",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ElectionsCandidates",
                schema: "AndNet",
                columns: table => new
                {
                    ElectionId = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<int>(type: "integer", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    IsWinner = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectionsCandidates", x => new { x.ElectionId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_ElectionsCandidates_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalSchema: "AndNet",
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ElectionsCandidates_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "AndNet",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ElectionsVoters",
                schema: "AndNet",
                columns: table => new
                {
                    ElectionId = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<int>(type: "integer", nullable: false),
                    VoteDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectionsVoters", x => new { x.ElectionId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_ElectionsVoters_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalSchema: "AndNet",
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ElectionsVoters_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "AndNet",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Expeditions",
                schema: "AndNet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    During = table.Column<NpgsqlRange<DateTime>>(type: "tstzrange", nullable: false),
                    AccountablePlayerId = table.Column<int>(type: "integer", nullable: false),
                    DiscordRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    IsMarkedForDelete = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expeditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expeditions_Players_AccountablePlayerId",
                        column: x => x.AccountablePlayerId,
                        principalSchema: "AndNet",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerContacts",
                schema: "AndNet",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    IsMarkedForDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerContacts", x => new { x.PlayerId, x.Type });
                    table.ForeignKey(
                        name: "FK_PlayerContacts_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "AndNet",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerStats",
                schema: "AndNet",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<byte>(type: "smallint", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerStats", x => new { x.PlayerId, x.Date });
                    table.ForeignKey(
                        name: "FK_PlayerStats_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "AndNet",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Awards",
                schema: "AndNet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AutomationId = table.Column<int>(type: "integer", nullable: true),
                    AwardType = table.Column<short>(type: "smallint", nullable: false),
                    PlayerId = table.Column<int>(type: "integer", nullable: false),
                    IssuerId = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    IsMarkedForDelete = table.Column<bool>(type: "boolean", nullable: false),
                    AwardSheetId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Awards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Awards_Documents_AwardSheetId",
                        column: x => x.AwardSheetId,
                        principalSchema: "AndNet",
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Awards_Players_IssuerId",
                        column: x => x.IssuerId,
                        principalSchema: "AndNet",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Awards_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "AndNet",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentBodies",
                schema: "AndNet",
                columns: table => new
                {
                    DocId = table.Column<int>(type: "integer", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:Compression:", "pglz"),
                    SearchVector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: false)
                        .Annotation("Npgsql:TsVectorConfig", "russian")
                        .Annotation("Npgsql:TsVectorProperties", new[] { "Body" }),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentBodies", x => x.DocId);
                    table.ForeignKey(
                        name: "FK_DocumentBodies_Documents_DocId",
                        column: x => x.DocId,
                        principalSchema: "AndNet",
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExpeditionsPlayers",
                schema: "AndNet",
                columns: table => new
                {
                    ExpeditionsId = table.Column<int>(type: "integer", nullable: false),
                    MembersId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpeditionsPlayers", x => new { x.ExpeditionsId, x.MembersId });
                    table.ForeignKey(
                        name: "FK_ExpeditionsPlayers_Expeditions_ExpeditionsId",
                        column: x => x.ExpeditionsId,
                        principalSchema: "AndNet",
                        principalTable: "Expeditions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpeditionsPlayers_Players_MembersId",
                        column: x => x.MembersId,
                        principalSchema: "AndNet",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "AndNet",
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { -3, "b1debd2c3d2c4214a1ace0d9ddba5dff", "member", "MEMBER" },
                    { -2, "3a160416081c4744840bd246115d39b8", "advisor", "ADVISOR" },
                    { -1, "757dc6530e744053874ea3b66d54b90a", "first_advisor", "FIRST_ADVISOR" }
                });

            migrationBuilder.InsertData(
                schema: "AndNet",
                table: "Elections",
                columns: new[] { "Id", "CouncilCapacity", "ElectionEnd", "Stage" },
                values: new object[] { 1, 0, new DateTime(2023, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), (byte)0 });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                schema: "AndNet",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "AndNet",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                schema: "AndNet",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                schema: "AndNet",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                schema: "AndNet",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "AndNet",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "AndNet",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Awards_AutomationId",
                schema: "AndNet",
                table: "Awards",
                column: "AutomationId")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Awards_AwardSheetId",
                schema: "AndNet",
                table: "Awards",
                column: "AwardSheetId",
                unique: true)
                .Annotation("Npgsql:IndexMethod", "Btree");

            migrationBuilder.CreateIndex(
                name: "IX_Awards_AwardType",
                schema: "AndNet",
                table: "Awards",
                column: "AwardType")
                .Annotation("Npgsql:IndexMethod", "Btree");

            migrationBuilder.CreateIndex(
                name: "IX_Awards_IsMarkedForDelete",
                schema: "AndNet",
                table: "Awards",
                column: "IsMarkedForDelete")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Awards_IssuerId",
                schema: "AndNet",
                table: "Awards",
                column: "IssuerId");

            migrationBuilder.CreateIndex(
                name: "IX_Awards_PlayerId",
                schema: "AndNet",
                table: "Awards",
                column: "PlayerId")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Awards_PlayerId_AutomationId",
                schema: "AndNet",
                table: "Awards",
                columns: new[] { "PlayerId", "AutomationId" })
                .Annotation("Npgsql:IndexMethod", "BTree");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentBodies_SearchVector",
                schema: "AndNet",
                table: "DocumentBodies",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "RUM");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_AuthorId",
                schema: "AndNet",
                table: "Documents",
                column: "AuthorId")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CreationDate",
                schema: "AndNet",
                table: "Documents",
                column: "CreationDate")
                .Annotation("Npgsql:IndexMethod", "BTree");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Info",
                schema: "AndNet",
                table: "Documents",
                column: "Info")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "jsonb_path_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ParentId",
                schema: "AndNet",
                table: "Documents",
                column: "ParentId")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Elections_Stage",
                schema: "AndNet",
                table: "Elections",
                column: "Stage")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_ElectionsCandidates_ElectionId",
                schema: "AndNet",
                table: "ElectionsCandidates",
                column: "ElectionId",
                descending: new bool[0])
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_ElectionsCandidates_PlayerId",
                schema: "AndNet",
                table: "ElectionsCandidates",
                column: "PlayerId")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_ElectionsVoters_ElectionId",
                schema: "AndNet",
                table: "ElectionsVoters",
                column: "ElectionId",
                descending: new bool[0])
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_ElectionsVoters_PlayerId",
                schema: "AndNet",
                table: "ElectionsVoters",
                column: "PlayerId")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Expeditions_AccountablePlayerId",
                schema: "AndNet",
                table: "Expeditions",
                column: "AccountablePlayerId")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Expeditions_DiscordRoleId",
                schema: "AndNet",
                table: "Expeditions",
                column: "DiscordRoleId",
                unique: true)
                .Annotation("Npgsql:IndexMethod", "Btree");

            migrationBuilder.CreateIndex(
                name: "IX_Expeditions_During",
                schema: "AndNet",
                table: "Expeditions",
                column: "During")
                .Annotation("Npgsql:IndexMethod", "GiST");

            migrationBuilder.CreateIndex(
                name: "IX_Expeditions_IsMarkedForDelete",
                schema: "AndNet",
                table: "Expeditions",
                column: "IsMarkedForDelete")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_ExpeditionsPlayers_ExpeditionsId",
                schema: "AndNet",
                table: "ExpeditionsPlayers",
                column: "ExpeditionsId")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_ExpeditionsPlayers_MembersId",
                schema: "AndNet",
                table: "ExpeditionsPlayers",
                column: "MembersId")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerContacts_IsMarkedForDelete",
                schema: "AndNet",
                table: "PlayerContacts",
                column: "IsMarkedForDelete")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerContacts_PlayerId",
                schema: "AndNet",
                table: "PlayerContacts",
                column: "PlayerId")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerContacts_Type",
                schema: "AndNet",
                table: "PlayerContacts",
                column: "Type")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Players_DiscordId",
                schema: "AndNet",
                table: "Players",
                column: "DiscordId",
                unique: true)
                .Annotation("Npgsql:IndexMethod", "Btree");

            migrationBuilder.CreateIndex(
                name: "IX_Players_IdentityId",
                schema: "AndNet",
                table: "Players",
                column: "IdentityId",
                unique: true)
                .Annotation("Npgsql:IndexMethod", "Btree");

            migrationBuilder.CreateIndex(
                name: "IX_Players_Nickname",
                schema: "AndNet",
                table: "Players",
                column: "Nickname",
                unique: true)
                .Annotation("Npgsql:IndexMethod", "Btree");

            migrationBuilder.CreateIndex(
                name: "IX_Players_OnReserve",
                schema: "AndNet",
                table: "Players",
                column: "OnReserve")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Players_Rank",
                schema: "AndNet",
                table: "Players",
                column: "Rank")
                .Annotation("Npgsql:IndexMethod", "Btree");

            migrationBuilder.CreateIndex(
                name: "IX_Players_Relationship",
                schema: "AndNet",
                table: "Players",
                column: "Relationship")
                .Annotation("Npgsql:IndexMethod", "Btree");

            migrationBuilder.CreateIndex(
                name: "IX_Players_SteamId",
                schema: "AndNet",
                table: "Players",
                column: "SteamId",
                unique: true)
                .Annotation("Npgsql:IndexMethod", "Btree");

            migrationBuilder.CreateIndex(
                name: "IX_Players_TimeZone",
                schema: "AndNet",
                table: "Players",
                column: "TimeZone")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStats_Date",
                schema: "AndNet",
                table: "PlayerStats",
                column: "Date")
                .Annotation("Npgsql:IndexMethod", "BRIN");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStats_PlayerId",
                schema: "AndNet",
                table: "PlayerStats",
                column: "PlayerId")
                .Annotation("Npgsql:IndexMethod", "Hash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims",
                schema: "AndNet");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: "AndNet");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: "AndNet");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles",
                schema: "AndNet");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: "AndNet");

            migrationBuilder.DropTable(
                name: "Awards",
                schema: "AndNet");

            migrationBuilder.DropTable(
                name: "DocumentBodies",
                schema: "AndNet");

            migrationBuilder.DropTable(
                name: "ElectionsCandidates",
                schema: "AndNet");

            migrationBuilder.DropTable(
                name: "ElectionsVoters",
                schema: "AndNet");

            migrationBuilder.DropTable(
                name: "ExpeditionsPlayers",
                schema: "AndNet");

            migrationBuilder.DropTable(
                name: "PlayerContacts",
                schema: "AndNet");

            migrationBuilder.DropTable(
                name: "PlayerStats",
                schema: "AndNet");

            migrationBuilder.DropTable(
                name: "AspNetRoles",
                schema: "AndNet");

            migrationBuilder.DropTable(
                name: "Documents",
                schema: "AndNet");

            migrationBuilder.DropTable(
                name: "Elections",
                schema: "AndNet");

            migrationBuilder.DropTable(
                name: "Expeditions",
                schema: "AndNet");

            migrationBuilder.DropTable(
                name: "Players",
                schema: "AndNet");

            migrationBuilder.DropTable(
                name: "AspNetUsers",
                schema: "AndNet");
        }
    }
}
