// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using URegister.Infrastructure.Constants;

namespace URegister.Infrastructure.Data.AuditLog
{
    /// <summary>
    /// Одитен лог
    /// </summary>
    [Comment("Одитен лог")]
    [Index("UserId", Name = "ix_audit_user_id")]
    [Index("Created", Name = "ix_audit_created")]
    public class Audit
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        [Key]
        [Comment("Идентификатор на запис")]
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор на потребител
        /// </summary>
        [Comment("Идентификатор на потребител")]
        public Guid? UserId { get; set; }

        /// <summary>
        /// Идентификатор на операция
        /// </summary>
        [Comment("Идентификатор на операция")]
        [StringLength(100)]
        public string ActivityId { get; set; }

        /// <summary>
        /// Идентификатор на операция
        /// </summary>
        [Comment("Идентификатор на операция")]
        [StringLength(100)]
        public string? ActivityFromId { get; set; }

        /// <summary>
        /// Допълнителна информация към операцията
        /// </summary>
        [Column(TypeName = AttributeConstants.Jsonb)]
        [Comment("Допълнителна информация към операцията")]
        public string? ActivityTags { get; set; }

        /// <summary>
        /// Модул, в който е възникнала операцията
        /// </summary>
        [Comment("Модул, в който е възникнала операцията")]
        [StringLength(200)]
        [Required]
        public string AssemblyName { get; set; } = null!;

        /// <summary>
        /// Контролер на операцията
        /// </summary>
        [Comment("Контролер на операцията")]
        [Required]
        [StringLength(100)]
        public string Controller { get; set; } = null!;

        /// <summary>
        /// Действие на операцията
        /// </summary>
        [Comment("Действие на операцията")]
        [Required]
        [StringLength(100)]
        public string Action { get; set; } = null!;

        /// <summary>
        /// Тип на действието
        /// </summary>
        [Comment("Тип на действието")]
        [Required]
        [StringLength(10)]
        public string Method { get; set; } = null!;

        /// <summary>
        /// Параметри на операцията
        /// </summary>
        [Comment("Параметри на операцията")]
        [Column(TypeName = AttributeConstants.Jsonb)]
        public string? Parameters { get; set; }

        /// <summary>
        /// Параметри на операцията
        /// </summary>
        [Comment("Параметри на post заявката")]
        [Column(TypeName = AttributeConstants.Jsonb)]
        public string? PostData { get; set; }


        /// <summary>
        /// Дата и час на събитието (UTC)
        /// </summary>
        [Required]
        [Comment("Дата и час на събитието (UTC)")]
        [Column(TypeName = "timestamptz")]
        public DateTime Created { get; set; }

        /// <summary>
        /// IP Адрес на потребителя
        /// </summary>
        [Comment("IP Адрес на потребителя")]
        public IPAddress IpAddress { get; set; } = null!;

        /// <summary>
        /// Идентификатор на администрация
        /// </summary>
        [Comment("Идентификатор на администрация")]
        public Guid TenantId { get; set; }


        /// <summary>
        /// Идентификатор на регистър
        /// </summary>
        [Comment("Идентификатор на регистър")]
        public int? RegisterId { get; set; }

        /// <summary>
        /// Пълно име на потребител
        /// </summary>
        [Comment("Пълно име на потребител")]
        public string? UserFullName { get; set; }

        public List<AuditEntity> AuditEntities { get; set; } = null!;
    }
}
