// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using URegister.Infrastructure.Constants;

namespace URegister.AuditLog.Data
{
    /// <summary>
    /// Одитен лог записи в таблица
    /// </summary>
    [Comment("Одитен лог записи в таблица ")]
    public class AuditEntity
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        [Key]
        [Comment("Идентификатор на запис")]
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор на запис от Одитен лог
        /// </summary>
        [Comment("Идентификатор на запис заявка")]
        public Guid AuditId { get; set; }

       

        /// <summary>
        /// Име на обект
        /// </summary>
        [Comment("Име на обект")]
        [StringLength(100)]
        public string TableName { get; set; } = null!;
        
        /// <summary>
        /// Тип на операцията
        /// </summary>
        [Comment("Тип на операцията")]
        [Required]
        [StringLength(10)]
        public string Type { get; set; } = null!;

        /// <summary>
        /// Стойности преди операцията
        /// </summary>
        [Comment("Стойности преди операцията")]
        [Column(TypeName = AttributeConstants.Jsonb)]
        public string? OldValues { get; set; }

        /// <summary>
        /// Стойности след операцията
        /// </summary>
        [Comment("Стойности след операцията")]
        [Column(TypeName = AttributeConstants.Jsonb)]
        public string? NewValues { get; set; }

        /// <summary>
        /// Засегнати данни
        /// </summary>
        [Comment("Засегнати данни")]
        [Column(TypeName = AttributeConstants.Jsonb)]
        public string? AffectedColumns { get; set; }

        /// <summary>
        /// Идентификатор на обект
        /// </summary>
        [Comment("Идентификатор на обект")]
        [Column(TypeName = AttributeConstants.Jsonb)]
        public string? PrimaryKey { get; set; }

        [ForeignKey(nameof(AuditId))]
        public Audit Audit { get; set; } = null!; 
    }
}
