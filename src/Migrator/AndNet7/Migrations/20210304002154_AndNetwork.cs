using System;
using AndNet.Migrator.AndNet7.AndNet7.Shared;
using AndNet.Migrator.AndNet7.AndNet7.Shared.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace AndNetwork.Server.Migrations
{
    public partial class AndNetwork : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase().Annotation("Npgsql:Enum:clan_award_type_enum", "none,bronze,silver,gold,hero").Annotation("Npgsql:Enum:clan_department_enum", "reserve,beginners_pool,none,infrastructure,research,military,agitation").Annotation("Npgsql:Enum:clan_druzhina_position_enum", "none,troop,lieutenant,captain").Annotation("Npgsql:Enum:clan_elections_stage_enum", "none,registration,voting,announcement,ended").Annotation("Npgsql:Enum:clan_member_rank_enum", "outcast,enemy,guest,diplomat,ally,candidate,none,neophyte,trainee,assistant,junior_employee,employee,senior_employee,specialist,defender,lieutenant,captain,advisor,first_advisor").Annotation("Npgsql:Enum:discord_channel_type_enum", "text,voice").Annotation("Npgsql:Enum:discord_permissions_flags", "none,view,read,write,priority,moderator,all");

            migrationBuilder.CreateTable("ChannelCategories", table => new
                                                                       {
                                                                           Position = table.Column<int>("integer", nullable: false),
                                                                           DiscordId = table.Column<decimal>("numeric(20,0)", nullable: false),
                                                                           Name = table.Column<string>("text", nullable: true),
                                                                       }, constraints: table => { table.PrimaryKey("PK_ChannelCategories", x => x.Position); });

            migrationBuilder.CreateTable("Druzhinas", table => new
                                                               {
                                                                   Id = table.Column<int>("integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                                                                   Name = table.Column<string>("text", nullable: false),
                                                                   Department = table.Column<ClanDepartmentEnum>("clan_department_enum", nullable: false),
                                                                   CreationDate = table.Column<DateTime>("date", nullable: false),
                                                                   DisbandDate = table.Column<DateTime>("date", nullable: true),
                                                               }, constraints: table => { table.PrimaryKey("PK_Druzhinas", x => x.Id); });

            migrationBuilder.CreateTable("Elections", table => new
                                                               {
                                                                   Id = table.Column<int>("integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                                                                   AdvisorsStartDate = table.Column<DateTime>("date", nullable: false),
                                                                   Stage = table.Column<ClanElectionsStageEnum>("clan_elections_stage_enum", nullable: false),
                                                               }, constraints: table => { table.PrimaryKey("PK_Elections", x => x.Id); });

            migrationBuilder.CreateTable("Members", table => new
                                                             {
                                                                 Id = table.Column<int>("integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                                                                 SteamId = table.Column<decimal>("numeric(20,0)", nullable: false),
                                                                 DiscordId = table.Column<decimal>("numeric(20,0)", nullable: false),
                                                                 VkId = table.Column<long>("bigint", nullable: true),
                                                                 TelegramId = table.Column<long>("bigint", nullable: true),
                                                                 Nickname = table.Column<string>("text", nullable: false),
                                                                 RealName = table.Column<string>("text", nullable: true),
                                                                 JoinDate = table.Column<DateTime>("date", nullable: false),
                                                                 Rank = table.Column<ClanMemberRankEnum>("clan_member_rank_enum", nullable: false),
                                                                 Department = table.Column<ClanDepartmentEnum>("clan_department_enum", nullable: false),
                                                                 DruzhinaId = table.Column<int>("integer", nullable: true),
                                                             }, constraints: table =>
            {
                table.PrimaryKey("PK_Members", x => x.Id);
                table.UniqueConstraint("AK_Members_DiscordId", x => x.DiscordId);
                table.UniqueConstraint("AK_Members_Nickname", x => x.Nickname);
                table.UniqueConstraint("AK_Members_SteamId", x => x.SteamId);
                table.ForeignKey("FK_Members_Druzhinas_DruzhinaId", x => x.DruzhinaId, "Druzhinas", "Id", onDelete: ReferentialAction.Restrict);
            });

            migrationBuilder.CreateTable("ClanElectionsVoting", table => new
                                                                         {
                                                                             ElectionsId = table.Column<int>("integer", nullable: false),
                                                                             Department = table.Column<ClanDepartmentEnum>("clan_department_enum", nullable: false),
                                                                             AgainstAll = table.Column<int>("integer", nullable: false),
                                                                         }, constraints: table =>
            {
                table.PrimaryKey("PK_ClanElectionsVoting", x => new
                                                                {
                                                                    x.ElectionsId,
                                                                    x.Department,
                                                                });
                table.ForeignKey("FK_ClanElectionsVoting_Elections_ElectionsId", x => x.ElectionsId, "Elections", "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("ClanAward", table => new
                                                               {
                                                                   Id = table.Column<int>("integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                                                                   Type = table.Column<ClanAwardTypeEnum>("clan_award_type_enum", nullable: false),
                                                                   MemberId = table.Column<int>("integer", nullable: false),
                                                                   Date = table.Column<DateTime>("date", nullable: false),
                                                                   Description = table.Column<string>("text", nullable: false),
                                                               }, constraints: table =>
            {
                table.PrimaryKey("PK_ClanAward", x => x.Id);
                table.ForeignKey("FK_ClanAward_Members_MemberId", x => x.MemberId, "Members", "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("ClanDruzhinaMember", table => new
                                                                        {
                                                                            DruzhinaId = table.Column<int>("integer", nullable: false),
                                                                            MemberId = table.Column<int>("integer", nullable: false),
                                                                            JoinDate = table.Column<DateTime>("date", nullable: false),
                                                                            LeaveDate = table.Column<DateTime>("date", nullable: true),
                                                                            Position = table.Column<ClanDruzhinaPositionEnum>("clan_druzhina_position_enum", nullable: false),
                                                                        }, constraints: table =>
            {
                table.PrimaryKey("PK_ClanDruzhinaMember", x => new
                                                               {
                                                                   x.DruzhinaId,
                                                                   x.MemberId,
                                                               });
                table.ForeignKey("FK_ClanDruzhinaMember_Druzhinas_DruzhinaId", x => x.DruzhinaId, "Druzhinas", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_ClanDruzhinaMember_Members_MemberId", x => x.MemberId, "Members", "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("Programs", table => new
                                                              {
                                                                  Id = table.Column<int>("integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                                                                  Name = table.Column<string>("text", nullable: false),
                                                                  InitiatorId = table.Column<int>("integer", nullable: true),
                                                                  CreationDate = table.Column<DateTime>("date", nullable: false),
                                                                  DisbandDate = table.Column<DateTime>("date", nullable: false),
                                                              }, constraints: table =>
            {
                table.PrimaryKey("PK_Programs", x => x.Id);
                table.ForeignKey("FK_Programs_Members_InitiatorId", x => x.InitiatorId, "Members", "Id", onDelete: ReferentialAction.Restrict);
            });

            migrationBuilder.CreateTable("ClanElectionsMember", table => new
                                                                         {
                                                                             ElectionsId = table.Column<int>("integer", nullable: false),
                                                                             Department = table.Column<ClanDepartmentEnum>("clan_department_enum", nullable: false),
                                                                             MemberId = table.Column<int>("integer", nullable: false),
                                                                             Votes = table.Column<int>("integer", nullable: true),
                                                                             VoterId = table.Column<Guid>("uuid", nullable: false),
                                                                             Voted = table.Column<bool>("boolean", nullable: false),
                                                                         }, constraints: table =>
            {
                table.PrimaryKey("PK_ClanElectionsMember", x => new
                                                                {
                                                                    x.ElectionsId,
                                                                    x.Department,
                                                                    x.MemberId,
                                                                });
                table.ForeignKey("FK_ClanElectionsMember_ClanElectionsVoting_ElectionsId_Departm~", x => new
                                                                                                         {
                                                                                                             x.ElectionsId,
                                                                                                             x.Department,
                                                                                                         }, "ClanElectionsVoting", new[]
                                                                                                                                   {
                                                                                                                                       "ElectionsId",
                                                                                                                                       "Department",
                                                                                                                                   }, onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_ClanElectionsMember_Members_MemberId", x => x.MemberId, "Members", "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("Channels", table => new
                                                              {
                                                                  DiscordId = table.Column<decimal>("numeric(20,0)", nullable: false),
                                                                  Name = table.Column<string>("text", nullable: true),
                                                                  CategoryPosition = table.Column<int>("integer", nullable: true),
                                                                  ChannelPosition = table.Column<int>("integer", nullable: false),
                                                                  Type = table.Column<DiscordChannelTypeEnum>("discord_channel_type_enum", nullable: false),
                                                                  EveryonePermissions = table.Column<DiscordPermissionsFlags>("discord_permissions_flags", nullable: false),
                                                                  MemberPermissions = table.Column<DiscordPermissionsFlags>("discord_permissions_flags", nullable: false),
                                                                  AdvisorPermissions = table.Column<DiscordPermissionsFlags>("discord_permissions_flags", nullable: false),
                                                                  DruzhinaId = table.Column<int>("integer", nullable: true),
                                                                  ProgramId = table.Column<int>("integer", nullable: true),
                                                              }, constraints: table =>
            {
                table.PrimaryKey("PK_Channels", x => x.DiscordId);
                table.ForeignKey("FK_Channels_ChannelCategories_CategoryPosition", x => x.CategoryPosition, "ChannelCategories", "Position", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Channels_Druzhinas_DruzhinaId", x => x.DruzhinaId, "Druzhinas", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Channels_Programs_ProgramId", x => x.ProgramId, "Programs", "Id", onDelete: ReferentialAction.Restrict);
            });

            migrationBuilder.CreateTable("ClanMemberClanProgram", table => new
                                                                           {
                                                                               MembersId = table.Column<int>("integer", nullable: false),
                                                                               ProgramsId = table.Column<int>("integer", nullable: false),
                                                                           }, constraints: table =>
            {
                table.PrimaryKey("PK_ClanMemberClanProgram", x => new
                                                                  {
                                                                      x.MembersId,
                                                                      x.ProgramsId,
                                                                  });
                table.ForeignKey("FK_ClanMemberClanProgram_Members_MembersId", x => x.MembersId, "Members", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_ClanMemberClanProgram_Programs_ProgramsId", x => x.ProgramsId, "Programs", "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("ClanProgramTask", table => new
                                                                     {
                                                                         ProgramId = table.Column<int>("integer", nullable: false),
                                                                         TaskNumber = table.Column<int>("integer", nullable: false),
                                                                         Status = table.Column<bool>("boolean", nullable: true),
                                                                         TaskDescription = table.Column<string>("text", nullable: false),
                                                                         FinalDescription = table.Column<string>("text", nullable: true),
                                                                     }, constraints: table =>
            {
                table.PrimaryKey("PK_ClanProgramTask", x => new
                                                            {
                                                                x.ProgramId,
                                                                x.TaskNumber,
                                                            });
                table.ForeignKey("FK_ClanProgramTask_Programs_ProgramId", x => x.ProgramId, "Programs", "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("DiscordDepartmentPermissions", table => new
                                                                                  {
                                                                                      ChannelId = table.Column<decimal>("numeric(20,0)", nullable: false),
                                                                                      Department = table.Column<ClanDepartmentEnum>("clan_department_enum", nullable: false),
                                                                                      Permissions = table.Column<DiscordPermissionsFlags>("discord_permissions_flags", nullable: false),
                                                                                  }, constraints: table =>
            {
                table.PrimaryKey("PK_DiscordDepartmentPermissions", x => new
                                                                         {
                                                                             x.ChannelId,
                                                                             x.Department,
                                                                         });
                table.ForeignKey("FK_DiscordDepartmentPermissions_Channels_ChannelId", x => x.ChannelId, "Channels", "DiscordId", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_ChannelCategories_Name", "ChannelCategories", "Name");

            migrationBuilder.CreateIndex("IX_Channels_CategoryPosition", "Channels", "CategoryPosition");

            migrationBuilder.CreateIndex("IX_Channels_DruzhinaId", "Channels", "DruzhinaId");

            migrationBuilder.CreateIndex("IX_Channels_Name", "Channels", "Name");

            migrationBuilder.CreateIndex("IX_Channels_ProgramId", "Channels", "ProgramId");

            migrationBuilder.CreateIndex("IX_ClanAward_MemberId", "ClanAward", "MemberId");

            migrationBuilder.CreateIndex("IX_ClanDruzhinaMember_MemberId", "ClanDruzhinaMember", "MemberId");

            migrationBuilder.CreateIndex("IX_ClanElectionsMember_MemberId", "ClanElectionsMember", "MemberId");

            migrationBuilder.CreateIndex("IX_ClanMemberClanProgram_ProgramsId", "ClanMemberClanProgram", "ProgramsId");

            migrationBuilder.CreateIndex("IX_Druzhinas_Name", "Druzhinas", "Name");

            migrationBuilder.CreateIndex("IX_Members_DiscordId", "Members", "DiscordId", unique: true);

            migrationBuilder.CreateIndex("IX_Members_DruzhinaId", "Members", "DruzhinaId");

            migrationBuilder.CreateIndex("IX_Members_Nickname", "Members", "Nickname", unique: true);

            migrationBuilder.CreateIndex("IX_Programs_InitiatorId", "Programs", "InitiatorId");

            migrationBuilder.CreateIndex("IX_Programs_Name", "Programs", "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("ClanAward");

            migrationBuilder.DropTable("ClanDruzhinaMember");

            migrationBuilder.DropTable("ClanElectionsMember");

            migrationBuilder.DropTable("ClanMemberClanProgram");

            migrationBuilder.DropTable("ClanProgramTask");

            migrationBuilder.DropTable("DiscordDepartmentPermissions");

            migrationBuilder.DropTable("ClanElectionsVoting");

            migrationBuilder.DropTable("Channels");

            migrationBuilder.DropTable("Elections");

            migrationBuilder.DropTable("ChannelCategories");

            migrationBuilder.DropTable("Programs");

            migrationBuilder.DropTable("Members");

            migrationBuilder.DropTable("Druzhinas");
        }
    }
}
