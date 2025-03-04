﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using URegister.Core.Data;

#nullable disable

namespace URegister.Core.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20241202144919_AdministrationIdGuid")]
    partial class AdministrationIdGuid
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("FriendlyName")
                        .HasColumnType("text")
                        .HasColumnName("friendly_name");

                    b.Property<string>("Xml")
                        .HasColumnType("text")
                        .HasColumnName("xml");

                    b.HasKey("Id")
                        .HasName("pk_data_protection_keys");

                    b.ToTable("data_protection_keys", (string)null);
                });

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

            modelBuilder.Entity("URegister.Infrastructure.Data.Common.Models.Form", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .HasComment("Индентификатор");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("DeletedOn")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("deleted_on")
                        .HasComment("Дата на изтриване");

                    b.Property<string>("FieldConfiguration")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("field_configuration")
                        .HasComment("Конфигурация на полетата");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean")
                        .HasColumnName("is_active")
                        .HasComment("Дали записът е активен");

                    b.Property<string>("ModifiedByUserId")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)")
                        .HasColumnName("modified_by_user_id")
                        .HasComment("Идентификатор на потребилет променил последно записа");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("timestamptz")
                        .HasColumnName("modified_on")
                        .HasComment("Дата на последна промяна");

                    b.Property<int?>("ParentId")
                        .HasColumnType("integer")
                        .HasColumnName("parent_id")
                        .HasComment("Идентификатор на първата версия на формата");

                    b.Property<string>("Purpose")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("character varying(250)")
                        .HasColumnName("purpose")
                        .HasComment("Предназначение на формата");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("character varying(150)")
                        .HasColumnName("title")
                        .HasComment("Заглавие на формата");

                    b.HasKey("Id")
                        .HasName("pk_forms");

                    b.HasIndex("ParentId")
                        .HasDatabaseName("ix_forms_parent_id");

                    b.ToTable("forms", null, t =>
                        {
                            t.HasComment("Съдържа конфигурацияна на полетата във форма");
                        });
                });

            modelBuilder.Entity("URegister.Infrastructure.Data.Common.Models.Service", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .HasComment("Индентификатор");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("ServiceTypeId")
                        .HasColumnType("integer")
                        .HasColumnName("service_type_id")
                        .HasComment("Идентификатор на тип услуга");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("character varying(150)")
                        .HasColumnName("title")
                        .HasComment("Име на услугата");

                    b.HasKey("Id")
                        .HasName("pk_services");

                    b.ToTable("services", null, t =>
                        {
                            t.HasComment("Услуга в регистъра");
                        });
                });

            modelBuilder.Entity("URegister.Infrastructure.Data.Common.Models.ServiceStep", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .HasComment("Индентификатор");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("FormParentId")
                        .HasColumnType("integer")
                        .HasColumnName("form_parent_id")
                        .HasComment("Идентификатор на тип форма");

                    b.Property<int>("OrderNum")
                        .HasColumnType("integer")
                        .HasColumnName("order_num")
                        .HasComment("Поредност");

                    b.Property<int>("ServiceId")
                        .HasColumnType("integer")
                        .HasColumnName("service_id")
                        .HasComment("Идентификатор на услугата от регистъра");

                    b.Property<int>("StepId")
                        .HasColumnType("integer")
                        .HasColumnName("step_id")
                        .HasComment("Идентификатор на стъпка");

                    b.Property<string>("Title")
                        .HasMaxLength(150)
                        .HasColumnType("character varying(150)")
                        .HasColumnName("title")
                        .HasComment("Име на стъпката");

                    b.HasKey("Id")
                        .HasName("pk_service_steps");

                    b.HasIndex("ServiceId")
                        .HasDatabaseName("ix_service_steps_service_id");

                    b.ToTable("service_steps", null, t =>
                        {
                            t.HasComment("Стъпка от услуга в регистъра");
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

            modelBuilder.Entity("URegister.Infrastructure.Data.Common.Models.Form", b =>
                {
                    b.HasOne("URegister.Infrastructure.Data.Common.Models.Form", "Parent")
                        .WithMany()
                        .HasForeignKey("ParentId")
                        .HasConstraintName("fk_forms_forms_parent_id");

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("URegister.Infrastructure.Data.Common.Models.ServiceStep", b =>
                {
                    b.HasOne("URegister.Infrastructure.Data.Common.Models.Service", "Service")
                        .WithMany("ServiceSteps")
                        .HasForeignKey("ServiceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_service_steps_services_service_id");

                    b.Navigation("Service");
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

            modelBuilder.Entity("URegister.Infrastructure.Data.Common.Models.Service", b =>
                {
                    b.Navigation("ServiceSteps");
                });
#pragma warning restore 612, 618
        }
    }
}
