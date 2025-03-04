﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using URegister.NumberGenerator.Data;

#nullable disable

namespace URegister.NumberGenerator.Migrations
{
    [DbContext(typeof(NumberGeneratorDbContext))]
    [Migration("20240925074023_EBKRemovedFromNumerator")]
    partial class EBKRemovedFromNumerator
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("URegister.NumberGenerator.Data.Models.NumberArchive", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id")
                        .HasComment("Идентификатор");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<Guid>("InitialDocumentId")
                        .HasColumnType("uuid")
                        .HasColumnName("initial_document_id")
                        .HasComment("Идентификатор на инициращият документ");

                    b.Property<long>("Number")
                        .HasColumnType("bigint")
                        .HasColumnName("number")
                        .HasComment("Номер");

                    b.Property<string>("Register")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)")
                        .HasColumnName("register")
                        .HasComment("Код на регистър");

                    b.HasKey("Id")
                        .HasName("pk_number_archives");

                    b.HasIndex("Number")
                        .IsUnique()
                        .HasDatabaseName("ix_number_archives_number");

                    b.ToTable("number_archives", null, t =>
                        {
                            t.HasComment("Архив на номератора");
                        });
                });

            modelBuilder.Entity("URegister.NumberGenerator.Data.Models.Numerator", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id")
                        .HasComment("Идентификатор");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int>("Prefix")
                        .HasColumnType("integer")
                        .HasColumnName("prefix")
                        .HasComment("Префикс във формат yyddd");

                    b.Property<int>("Sequence")
                        .HasColumnType("integer")
                        .HasColumnName("sequence")
                        .HasComment("Последователност");

                    b.HasKey("Id")
                        .HasName("pk_numerators");

                    b.HasIndex("Prefix")
                        .IsUnique()
                        .HasDatabaseName("ix_numerators_prefix");

                    b.ToTable("numerators", null, t =>
                        {
                            t.HasComment("Номератор на универсалния регистър");
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
