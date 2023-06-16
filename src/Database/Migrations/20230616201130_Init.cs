using System;
using System.Collections.Generic;
using System.Net;
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
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Nickname = table.Column<string>(type: "text", nullable: false),
                    RealName = table.Column<string>(type: "text", nullable: true),
                    DiscordId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    SteamId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    DetectionDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TimeZone = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    JoinDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Rank = table.Column<short>(type: "smallint", nullable: true),
                    Score = table.Column<double>(type: "double precision", nullable: true),
                    OnReserve = table.Column<bool>(type: "boolean", nullable: true),
                    Relationship = table.Column<short>(type: "smallint", nullable: true),
                    RestorationAvailable = table.Column<bool>(type: "boolean", nullable: true),
                    LeaveDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LeaveReason = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.UniqueConstraint("AK_Players_Nickname", x => x.Nickname);
                    table.ForeignKey(
                        name: "FK_Players_AspNetUsers_IdentityId",
                        column: x => x.IdentityId,
                        principalSchema: "AndNet",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DbPlayerContact",
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
                    table.PrimaryKey("PK_DbPlayerContact", x => new { x.PlayerId, x.Type });
                    table.ForeignKey(
                        name: "FK_DbPlayerContact_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "AndNet",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Expeditions",
                schema: "AndNet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    During = table.Column<NpgsqlRange<DateTime>>(type: "tsrange", nullable: false),
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
                name: "Documents",
                schema: "AndNet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    CreatorId = table.Column<int>(type: "integer", nullable: false),
                    Prefix = table.Column<string>(type: "text", nullable: false, collation: "C"),
                    Body = table.Column<string>(type: "text", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    SearchVector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: false)
                        .Annotation("Npgsql:TsVectorConfig", "russian")
                        .Annotation("Npgsql:TsVectorProperties", new[] { "Body" }),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CancelDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    AcceptanceDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ReplacedById = table.Column<int>(type: "integer", nullable: true),
                    ProtocolType = table.Column<int>(type: "integer", nullable: true),
                    MinYesVotesPercent = table.Column<double>(type: "double precision", nullable: true),
                    IsExecuted = table.Column<bool>(type: "boolean", nullable: true),
                    ExecutorId = table.Column<int>(type: "integer", nullable: true),
                    ExecuteDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DirectiveId = table.Column<int>(type: "integer", nullable: true),
                    NewDirectiveId = table.Column<int>(type: "integer", nullable: true),
                    ExpeditionId = table.Column<int>(type: "integer", nullable: true),
                    PlayerId = table.Column<int>(type: "integer", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Recommendation = table.Column<string>(type: "text", nullable: true),
                    Hours = table.Column<float>(type: "real", nullable: true),
                    TimeZone = table.Column<string>(type: "text", nullable: true),
                    Age = table.Column<int>(type: "integer", nullable: true),
                    AwardType = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    PlayerLeaveReason = table.Column<int>(type: "integer", nullable: true),
                    ReportRange = table.Column<NpgsqlRange<DateTime>>(type: "tsrange", nullable: true),
                    ServerEndPoint = table.Column<ValueTuple<IPAddress, int>>(type: "cidr", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.UniqueConstraint("AK_Documents_CreationDate", x => x.CreationDate);
                    table.ForeignKey(
                        name: "FK_Documents_Documents_DirectiveId",
                        column: x => x.DirectiveId,
                        principalSchema: "AndNet",
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Documents_Documents_NewDirectiveId",
                        column: x => x.NewDirectiveId,
                        principalSchema: "AndNet",
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Documents_Documents_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "AndNet",
                        principalTable: "Documents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Documents_Documents_ReplacedById",
                        column: x => x.ReplacedById,
                        principalSchema: "AndNet",
                        principalTable: "Documents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Documents_Expeditions_ExpeditionId",
                        column: x => x.ExpeditionId,
                        principalSchema: "AndNet",
                        principalTable: "Expeditions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Documents_Players_CreatorId",
                        column: x => x.CreatorId,
                        principalSchema: "AndNet",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Documents_Players_ExecutorId",
                        column: x => x.ExecutorId,
                        principalSchema: "AndNet",
                        principalTable: "Players",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Documents_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "AndNet",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "Awards",
                schema: "AndNet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IssueDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AutomationId = table.Column<int>(type: "integer", nullable: true),
                    AwardType = table.Column<int>(type: "integer", nullable: false),
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
                name: "DbBattleCombatant",
                schema: "AndNet",
                columns: table => new
                {
                    BattleId = table.Column<int>(type: "integer", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Tag = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CommanderId = table.Column<int>(type: "integer", nullable: true),
                    UnknownPlayers = table.Column<List<string>>(type: "text[]", nullable: false),
                    Units = table.Column<List<string>>(type: "text[]", nullable: false),
                    Casualties = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbBattleCombatant", x => new { x.BattleId, x.Number });
                    table.ForeignKey(
                        name: "FK_DbBattleCombatant_Documents_BattleId",
                        column: x => x.BattleId,
                        principalSchema: "AndNet",
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DbBattleCombatant_Players_CommanderId",
                        column: x => x.CommanderId,
                        principalSchema: "AndNet",
                        principalTable: "Players",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DbDocumentDecisionCouncilExpeditionCreateDbPlayer",
                schema: "AndNet",
                columns: table => new
                {
                    DbDocumentDecisionCouncilExpeditionCreateId = table.Column<int>(type: "integer", nullable: false),
                    MembersId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbDocumentDecisionCouncilExpeditionCreateDbPlayer", x => new { x.DbDocumentDecisionCouncilExpeditionCreateId, x.MembersId });
                    table.ForeignKey(
                        name: "FK_DbDocumentDecisionCouncilExpeditionCreateDbPlayer_Documents~",
                        column: x => x.DbDocumentDecisionCouncilExpeditionCreateId,
                        principalSchema: "AndNet",
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DbDocumentDecisionCouncilExpeditionCreateDbPlayer_Players_M~",
                        column: x => x.MembersId,
                        principalSchema: "AndNet",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DbDocumentProtocolDbPlayer",
                schema: "AndNet",
                columns: table => new
                {
                    MembersId = table.Column<int>(type: "integer", nullable: false),
                    RelatedProtocolsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbDocumentProtocolDbPlayer", x => new { x.MembersId, x.RelatedProtocolsId });
                    table.ForeignKey(
                        name: "FK_DbDocumentProtocolDbPlayer_Documents_RelatedProtocolsId",
                        column: x => x.RelatedProtocolsId,
                        principalSchema: "AndNet",
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DbDocumentProtocolDbPlayer_Players_MembersId",
                        column: x => x.MembersId,
                        principalSchema: "AndNet",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DbVote",
                schema: "AndNet",
                columns: table => new
                {
                    DecisionId = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VoteType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbVote", x => new { x.DecisionId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_DbVote_Documents_DecisionId",
                        column: x => x.DecisionId,
                        principalSchema: "AndNet",
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DbVote_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "AndNet",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DbBattleCombatantDbPlayer",
                schema: "AndNet",
                columns: table => new
                {
                    PlayersId = table.Column<int>(type: "integer", nullable: false),
                    BattleCombatantsMemberBattleId = table.Column<int>(type: "integer", nullable: false),
                    BattleCombatantsMemberNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbBattleCombatantDbPlayer", x => new { x.PlayersId, x.BattleCombatantsMemberBattleId, x.BattleCombatantsMemberNumber });
                    table.ForeignKey(
                        name: "FK_DbBattleCombatantDbPlayer_DbBattleCombatant_BattleCombatant~",
                        columns: x => new { x.BattleCombatantsMemberBattleId, x.BattleCombatantsMemberNumber },
                        principalSchema: "AndNet",
                        principalTable: "DbBattleCombatant",
                        principalColumns: new[] { "BattleId", "Number" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DbBattleCombatantDbPlayer_Players_PlayersId",
                        column: x => x.PlayersId,
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
                name: "IX_DbBattleCombatant_BattleId",
                schema: "AndNet",
                table: "DbBattleCombatant",
                column: "BattleId")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_DbBattleCombatant_Casualties",
                schema: "AndNet",
                table: "DbBattleCombatant",
                column: "Casualties")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_DbBattleCombatant_CommanderId",
                schema: "AndNet",
                table: "DbBattleCombatant",
                column: "CommanderId")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_DbBattleCombatant_Name",
                schema: "AndNet",
                table: "DbBattleCombatant",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "BTree");

            migrationBuilder.CreateIndex(
                name: "IX_DbBattleCombatant_Tag",
                schema: "AndNet",
                table: "DbBattleCombatant",
                column: "Tag")
                .Annotation("Npgsql:IndexMethod", "BTree");

            migrationBuilder.CreateIndex(
                name: "IX_DbBattleCombatant_Units",
                schema: "AndNet",
                table: "DbBattleCombatant",
                column: "Units")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_DbBattleCombatant_UnknownPlayers",
                schema: "AndNet",
                table: "DbBattleCombatant",
                column: "UnknownPlayers")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_DbBattleCombatantDbPlayer_BattleCombatantsMemberBattleId_Ba~",
                schema: "AndNet",
                table: "DbBattleCombatantDbPlayer",
                columns: new[] { "BattleCombatantsMemberBattleId", "BattleCombatantsMemberNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_DbDocumentDecisionCouncilExpeditionCreateDbPlayer_MembersId",
                schema: "AndNet",
                table: "DbDocumentDecisionCouncilExpeditionCreateDbPlayer",
                column: "MembersId");

            migrationBuilder.CreateIndex(
                name: "IX_DbDocumentProtocolDbPlayer_RelatedProtocolsId",
                schema: "AndNet",
                table: "DbDocumentProtocolDbPlayer",
                column: "RelatedProtocolsId");

            migrationBuilder.CreateIndex(
                name: "IX_DbPlayerContact_IsMarkedForDelete",
                schema: "AndNet",
                table: "DbPlayerContact",
                column: "IsMarkedForDelete")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_DbPlayerContact_PlayerId",
                schema: "AndNet",
                table: "DbPlayerContact",
                column: "PlayerId")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_DbPlayerContact_Type",
                schema: "AndNet",
                table: "DbPlayerContact",
                column: "Type")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_DbVote_DecisionId",
                schema: "AndNet",
                table: "DbVote",
                column: "DecisionId")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_DbVote_PlayerId",
                schema: "AndNet",
                table: "DbVote",
                column: "PlayerId")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_DbVote_VoteType",
                schema: "AndNet",
                table: "DbVote",
                column: "VoteType")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_AcceptanceDate",
                schema: "AndNet",
                table: "Documents",
                column: "AcceptanceDate")
                .Annotation("Npgsql:IndexMethod", "BTree");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CancelDate",
                schema: "AndNet",
                table: "Documents",
                column: "CancelDate",
                descending: new bool[0])
                .Annotation("Npgsql:IndexMethod", "BTree");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CreatorId",
                schema: "AndNet",
                table: "Documents",
                column: "CreatorId")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Date",
                schema: "AndNet",
                table: "Documents",
                column: "Date",
                unique: true)
                .Annotation("Npgsql:IndexMethod", "BTree");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DirectiveId",
                schema: "AndNet",
                table: "Documents",
                column: "DirectiveId")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ExecuteDate",
                schema: "AndNet",
                table: "Documents",
                column: "ExecuteDate")
                .Annotation("Npgsql:IndexMethod", "BTree");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ExecutorId",
                schema: "AndNet",
                table: "Documents",
                column: "ExecutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ExpeditionId",
                schema: "AndNet",
                table: "Documents",
                column: "ExpeditionId")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_NewDirectiveId",
                schema: "AndNet",
                table: "Documents",
                column: "NewDirectiveId")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ParentId",
                schema: "AndNet",
                table: "Documents",
                column: "ParentId")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_PlayerId",
                schema: "AndNet",
                table: "Documents",
                column: "PlayerId")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Prefix",
                schema: "AndNet",
                table: "Documents",
                column: "Prefix")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ProtocolType",
                schema: "AndNet",
                table: "Documents",
                column: "ProtocolType")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ReplacedById",
                schema: "AndNet",
                table: "Documents",
                column: "ReplacedById")
                .Annotation("Npgsql:IndexMethod", "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ReportRange",
                schema: "AndNet",
                table: "Documents",
                column: "ReportRange")
                .Annotation("Npgsql:IndexMethod", "GiST");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_SearchVector",
                schema: "AndNet",
                table: "Documents",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "RUM");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ServerEndPoint",
                schema: "AndNet",
                table: "Documents",
                column: "ServerEndPoint")
                .Annotation("Npgsql:IndexMethod", "GiST")
                .Annotation("Npgsql:IndexOperators", new[] { "inet_ops" });

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
                name: "DbBattleCombatantDbPlayer",
                schema: "AndNet");

            migrationBuilder.DropTable(
                name: "DbDocumentDecisionCouncilExpeditionCreateDbPlayer",
                schema: "AndNet");

            migrationBuilder.DropTable(
                name: "DbDocumentProtocolDbPlayer",
                schema: "AndNet");

            migrationBuilder.DropTable(
                name: "DbPlayerContact",
                schema: "AndNet");

            migrationBuilder.DropTable(
                name: "DbVote",
                schema: "AndNet");

            migrationBuilder.DropTable(
                name: "ExpeditionsPlayers",
                schema: "AndNet");

            migrationBuilder.DropTable(
                name: "AspNetRoles",
                schema: "AndNet");

            migrationBuilder.DropTable(
                name: "DbBattleCombatant",
                schema: "AndNet");

            migrationBuilder.DropTable(
                name: "Documents",
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
