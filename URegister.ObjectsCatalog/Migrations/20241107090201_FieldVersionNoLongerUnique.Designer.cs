﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using URegister.ObjectsCatalog.Data;

#nullable disable

namespace URegister.ObjectsCatalog.Migrations
{
    [DbContext(typeof(ObjectCatalogDbContext))]
    [Migration("20241107090201_FieldVersionNoLongerUnique")]
    partial class FieldVersionNoLongerUnique
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("RulesEngine.Models.Rule", b =>
                {
                    b.Property<string>("RuleName")
                        .HasColumnType("text")
                        .HasColumnName("rule_name");

                    b.Property<string>("Actions")
                        .HasColumnType("text")
                        .HasColumnName("actions");

                    b.Property<bool>("Enabled")
                        .HasColumnType("boolean")
                        .HasColumnName("enabled");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("text")
                        .HasColumnName("error_message");

                    b.Property<string>("Expression")
                        .HasColumnType("text")
                        .HasColumnName("expression");

                    b.Property<string>("Operator")
                        .HasColumnType("text")
                        .HasColumnName("operator");

                    b.Property<string>("Properties")
                        .HasColumnType("text")
                        .HasColumnName("properties");

                    b.Property<int>("RuleExpressionType")
                        .HasColumnType("integer")
                        .HasColumnName("rule_expression_type");

                    b.Property<string>("RuleNameFK")
                        .HasColumnType("text")
                        .HasColumnName("rule_name_fk");

                    b.Property<string>("SuccessEvent")
                        .HasColumnType("text")
                        .HasColumnName("success_event");

                    b.Property<string>("WorkflowName")
                        .HasColumnType("text")
                        .HasColumnName("workflow_name");

                    b.HasKey("RuleName")
                        .HasName("pk_rules");

                    b.HasIndex("RuleNameFK")
                        .HasDatabaseName("ix_rules_rule_name_fk");

                    b.HasIndex("WorkflowName")
                        .HasDatabaseName("ix_rules_workflow_name");

                    b.ToTable("rules", (string)null);
                });

            modelBuilder.Entity("RulesEngine.Models.ScopedParam", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("Expression")
                        .HasColumnType("text")
                        .HasColumnName("expression");

                    b.Property<string>("RuleName")
                        .HasColumnType("text")
                        .HasColumnName("rule_name");

                    b.Property<string>("WorkflowName")
                        .HasColumnType("text")
                        .HasColumnName("workflow_name");

                    b.HasKey("Name")
                        .HasName("pk_scoped_param");

                    b.HasIndex("RuleName")
                        .HasDatabaseName("ix_scoped_param_rule_name");

                    b.HasIndex("WorkflowName")
                        .HasDatabaseName("ix_scoped_param_workflow_name");

                    b.ToTable("scoped_param", (string)null);
                });

            modelBuilder.Entity("RulesEngine.Models.Workflow", b =>
                {
                    b.Property<string>("WorkflowName")
                        .HasColumnType("text")
                        .HasColumnName("workflow_name");

                    b.Property<int>("RuleExpressionType")
                        .HasColumnType("integer")
                        .HasColumnName("rule_expression_type");

                    b.HasKey("WorkflowName")
                        .HasName("pk_workflows");

                    b.ToTable("workflows", (string)null);
                });

            modelBuilder.Entity("URegister.ObjectsCatalog.Data.Models.Field", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasComment("Идентификатор");

                    b.Property<string>("FieldData")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("field_data")
                        .HasComment("Информация за полето. Настройки по подразбиране");

                    b.Property<int>("FieldTypeId")
                        .HasColumnType("integer")
                        .HasColumnName("field_type_id")
                        .HasComment("Тип на поле");

                    b.Property<bool>("IsCurrent")
                        .HasColumnType("boolean")
                        .HasColumnName("is_current")
                        .HasComment("Дали полето е последна версия");

                    b.Property<string>("Label")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("label")
                        .HasComment("Етикет на поле");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("name")
                        .HasComment("Име на поле");

                    b.Property<int>("Version")
                        .HasColumnType("integer")
                        .HasColumnName("version")
                        .HasComment("Версия на полето");

                    b.HasKey("Id")
                        .HasName("pk_field");

                    b.HasIndex("FieldTypeId")
                        .HasDatabaseName("ix_field_field_type_id");

                    b.HasIndex("Name", "IsCurrent")
                        .HasDatabaseName("ix_field_name_is_current");

                    b.ToTable("field", null, t =>
                        {
                            t.HasComment("Полета");
                        });
                });

            modelBuilder.Entity("URegister.ObjectsCatalog.Data.Models.FieldType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .HasComment("Идентификатор");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("IsComplexField")
                        .HasColumnType("boolean")
                        .HasColumnName("is_complex_field")
                        .HasComment("Дали полето е комплексно");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("label")
                        .HasComment("Етикет на поле");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("name")
                        .HasComment("Име на поле");

                    b.Property<string>("Template")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("template")
                        .HasComment("Шаблон за визуализация");

                    b.HasKey("Id")
                        .HasName("pk_field_type");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("ix_field_type_name");

                    b.ToTable("field_type", null, t =>
                        {
                            t.HasComment("Типове полета");
                        });
                });

            modelBuilder.Entity("RulesEngine.Models.Rule", b =>
                {
                    b.HasOne("RulesEngine.Models.Rule", null)
                        .WithMany("Rules")
                        .HasForeignKey("RuleNameFK")
                        .HasConstraintName("fk_rules_rules_rule_name_fk");

                    b.HasOne("RulesEngine.Models.Workflow", null)
                        .WithMany("Rules")
                        .HasForeignKey("WorkflowName")
                        .HasConstraintName("fk_rules_workflows_workflow_name");
                });

            modelBuilder.Entity("RulesEngine.Models.ScopedParam", b =>
                {
                    b.HasOne("RulesEngine.Models.Rule", null)
                        .WithMany("LocalParams")
                        .HasForeignKey("RuleName")
                        .HasConstraintName("fk_scoped_param_rules_rule_name");

                    b.HasOne("RulesEngine.Models.Workflow", null)
                        .WithMany("GlobalParams")
                        .HasForeignKey("WorkflowName")
                        .HasConstraintName("fk_scoped_param_workflows_workflow_name");
                });

            modelBuilder.Entity("URegister.ObjectsCatalog.Data.Models.Field", b =>
                {
                    b.HasOne("URegister.ObjectsCatalog.Data.Models.FieldType", "FieldType")
                        .WithMany()
                        .HasForeignKey("FieldTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_field_field_type_field_type_id");

                    b.Navigation("FieldType");
                });

            modelBuilder.Entity("RulesEngine.Models.Rule", b =>
                {
                    b.Navigation("LocalParams");

                    b.Navigation("Rules");
                });

            modelBuilder.Entity("RulesEngine.Models.Workflow", b =>
                {
                    b.Navigation("GlobalParams");

                    b.Navigation("Rules");
                });
#pragma warning restore 612, 618
        }
    }
}
