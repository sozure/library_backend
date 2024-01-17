﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using VGManager.Library.Repositories.DbContexts;

#nullable disable

namespace VGManager.Library.Migrations.PostgreSql.Migrations
{
    [DbContext(typeof(OperationsDbContext))]
    [Migration("20240117220122_Init")]
    partial class Init
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("VGManager.Library.Entities.SecretEntities.KeyVaultCopyEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("DestinationKeyVault")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("OriginalKeyVault")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("User")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("KeyVault_copy", (string)null);
                });

            modelBuilder.Entity("VGManager.Library.Entities.SecretEntities.SecretChangeEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text");

                    b.Property<int>("ChangeType")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("KeyVaultName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("SecretNameRegex")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("User")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Secret_changes", (string)null);
                });

            modelBuilder.Entity("VGManager.Library.Entities.VGEntities.VGAddEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Organization")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Project")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("User")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("VariableGroupFilter")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Variable_additions", (string)null);
                });

            modelBuilder.Entity("VGManager.Library.Entities.VGEntities.VGDeleteEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Organization")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Project")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("User")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("VariableGroupFilter")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Variable_deletions", (string)null);
                });

            modelBuilder.Entity("VGManager.Library.Entities.VGEntities.VGUpdateEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("NewValue")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Organization")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Project")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("User")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("VariableGroupFilter")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Variable_editions", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
