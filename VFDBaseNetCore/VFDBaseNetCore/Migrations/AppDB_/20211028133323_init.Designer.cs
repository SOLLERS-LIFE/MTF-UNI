﻿// <auto-generated />
using System;
using MTF.Areas.ApplicationDB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MTF.Migrations.AppDB_
{
    [DbContext(typeof(AppDB_Context))]
    [Migration("20211028133323_init")]
    partial class init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.11");

            modelBuilder.Entity("MTF.Areas.ApplicationDB.Models.bs_objects_protocol", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("_type")
                        .HasColumnType("int");

                    b.Property<string>("action_dscr")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<int>("id_object")
                        .HasColumnType("int");

                    b.Property<string>("id_user")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime>("logged")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.HasIndex("_type", "id_object")
                        .HasDatabaseName("IX_bs_protocol_type_id_object");

                    b.ToTable("bs_objects_protocol");
                });

            modelBuilder.Entity("MTF.Areas.ApplicationDB.Models.bs_security_disable", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("_state")
                        .HasColumnType("int");

                    b.Property<string>("id_user")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("id_user")
                        .HasDatabaseName("IX_bs_security_disable_id_user");

                    b.ToTable("bs_security_disable");
                });

            modelBuilder.Entity("MTF.Areas.ApplicationDB.Models.sa_global_configs", b =>
                {
                    b.Property<string>("cnfName")
                        .HasMaxLength(15)
                        .HasColumnType("varchar(15)");

                    b.Property<string>("cnfValue")
                        .IsRequired()
                        .HasMaxLength(127)
                        .HasColumnType("varchar(127)");

                    b.HasKey("cnfName");

                    b.ToTable("sa_global_configs");
                });

            modelBuilder.Entity("MTF.Areas.ApplicationDB.Models.sa_tech_ticks_protocol", b =>
                {
                    b.Property<DateTime>("logged")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("_type")
                        .HasColumnType("int");

                    b.Property<string>("dscr")
                        .IsRequired()
                        .HasMaxLength(127)
                        .HasColumnType("varchar(127)");

                    b.HasKey("logged");

                    b.ToTable("sa_tech_ticks_protocol");
                });

            modelBuilder.Entity("MTF.Areas.ApplicationDB.Models.sa_uidTranslator", b =>
                {
                    b.Property<string>("id_user")
                        .HasMaxLength(64)
                        .HasColumnType("VARCHAR(64)");

                    b.Property<int>("uid")
                        .HasColumnType("int");

                    b.HasKey("id_user");

                    b.HasIndex("uid")
                        .IsUnique();

                    b.ToTable("sa_uidTranslator");
                });

            modelBuilder.Entity("MTF.Areas.ApplicationDB.Models.sa_user_configs", b =>
                {
                    b.Property<string>("uid")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("cnfName")
                        .HasMaxLength(15)
                        .HasColumnType("varchar(15)");

                    b.Property<string>("cnfValue")
                        .IsRequired()
                        .HasMaxLength(127)
                        .HasColumnType("varchar(127)");

                    b.HasKey("uid", "cnfName");

                    b.ToTable("sa_user_configs");
                });

            modelBuilder.Entity("MTF.Areas.ApplicationDB.Pages.BS.Models.bs_invite_traps", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<sbyte>("_state")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint")
                        .HasDefaultValue((sbyte)0);

                    b.Property<string>("change_id_user")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime>("change_logged")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("id_bst")
                        .HasColumnType("int");

                    b.Property<string>("user_email")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("user_email", "_state")
                        .HasDatabaseName("IX_bs_invite_traps_fnd");

                    b.ToTable("bs_invite_traps");
                });

            modelBuilder.Entity("MTF.Areas.ApplicationDB.Pages.BS.Models.bs_marks", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("name")
                        .IsRequired()
                        .HasMaxLength(127)
                        .HasColumnType("varchar(127)");

                    b.HasKey("Id");

                    b.HasIndex("name")
                        .IsUnique();

                    b.ToTable("bs_marks");
                });

            modelBuilder.Entity("MTF.Areas.ApplicationDB.Pages.BS.Models.bs_marks_to_users", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("id_mark")
                        .HasColumnType("int");

                    b.Property<int>("id_team")
                        .HasColumnType("int");

                    b.Property<string>("id_user")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("id_mark", "id_user")
                        .IsUnique()
                        .HasDatabaseName("IX__bs_marks_to_users_id_mark_id_user");

                    b.ToTable("bs_marks_to_users");
                });

            modelBuilder.Entity("MTF.Areas.ApplicationDB.Pages.BS.Models.bs_team_configs", b =>
                {
                    b.Property<int>("id_cnf")
                        .HasColumnType("int");

                    b.Property<int>("id_team")
                        .HasColumnType("int");

                    b.Property<string>("cnfValue")
                        .IsRequired()
                        .HasMaxLength(127)
                        .HasColumnType("varchar(127)");

                    b.HasKey("id_cnf", "id_team");

                    b.ToTable("bs_team_configs");
                });

            modelBuilder.Entity("MTF.Areas.ApplicationDB.Pages.BS.Models.bs_team_configs_avl", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("cnfDefault")
                        .IsRequired()
                        .HasMaxLength(127)
                        .HasColumnType("varchar(127)");

                    b.Property<string>("cnfName")
                        .IsRequired()
                        .HasMaxLength(127)
                        .HasColumnType("varchar(127)");

                    b.HasKey("Id");

                    b.HasIndex("cnfName")
                        .IsUnique();

                    b.ToTable("bs_team_configs_avl");
                });

            modelBuilder.Entity("MTF.Areas.ApplicationDB.Pages.BS.Models.bs_team_marks", b =>
                {
                    b.Property<int>("id_bst")
                        .HasColumnType("int");

                    b.Property<int>("id_mark")
                        .HasColumnType("int");

                    b.HasKey("id_bst", "id_mark");

                    b.ToTable("bs_team_marks");
                });

            modelBuilder.Entity("MTF.Areas.ApplicationDB.Pages.BS.Models.bs_team_users", b =>
                {
                    b.Property<int>("id_bst")
                        .HasColumnType("int");

                    b.Property<string>("id_user")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<sbyte>("_role")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint")
                        .HasDefaultValue((sbyte)0);

                    b.HasKey("id_bst", "id_user");

                    b.ToTable("bs_team_users");
                });

            modelBuilder.Entity("MTF.Areas.ApplicationDB.Pages.BS.Models.bs_teams", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("name")
                        .IsRequired()
                        .HasMaxLength(127)
                        .HasColumnType("varchar(127)");

                    b.HasKey("Id");

                    b.HasIndex("name")
                        .IsUnique();

                    b.ToTable("bs_teams");
                });

            modelBuilder.Entity("MTF.Pages.LookingForward.Models.TGO_trivial_table", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("ActionDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("ActorId")
                        .IsRequired()
                        .HasMaxLength(127)
                        .HasColumnType("varchar(127)");

                    b.Property<DateTime>("ExactDate")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("Selected")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("Id");

                    b.HasIndex("ActorId");

                    b.ToTable("TGO_trivial_table");
                });
#pragma warning restore 612, 618
        }
    }
}
