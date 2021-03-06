// <auto-generated />
using System;
using MTF.Areas.CommonDB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MTF.Migrations.CommonDB_
{
    [DbContext(typeof(CommonDB_Context))]
    [Migration("20211028133248_init")]
    partial class init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.11");

            modelBuilder.Entity("MTF.Areas.CommonDB.Models.ExchangeStore", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ObjectDescr")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("varchar(1024)");

                    b.Property<string>("UserUID")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.HasKey("ID");

                    b.HasIndex("UserUID");

                    b.ToTable("ExchangeStore");
                });

            modelBuilder.Entity("MTF.Areas.Identity.Pages.SURoom.Models.announcements", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("DateIn")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("content")
                        .IsRequired()
                        .HasMaxLength(16920)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("_announcements");
                });

            modelBuilder.Entity("MTF.Areas.Identity.Pages.SURoom.Models.cacheControl", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("uid")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("wfid")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("uid");

                    b.ToTable("cacheControls");
                });

            modelBuilder.Entity("MTF.Areas.Identity.Pages.SURoom.Models.cachedCommonUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ActorId")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(95)
                        .HasColumnType("varchar(95)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("FullName")
                        .HasMaxLength(95)
                        .HasColumnType("varchar(95)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("PhoneNumber")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("Selected")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("toBeAddressed")
                        .HasMaxLength(127)
                        .HasColumnType("varchar(127)");

                    b.HasKey("Id");

                    b.HasIndex("ActorId");

                    b.ToTable("CachedCommonUsers");
                });

            modelBuilder.Entity("MTF.Areas.Identity.Pages.SURoom.Models.loginsdisablers", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("DateIn")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("content")
                        .IsRequired()
                        .HasMaxLength(16920)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("_loginsdisablers");
                });
#pragma warning restore 612, 618
        }
    }
}
