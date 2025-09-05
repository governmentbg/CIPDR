// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Contracts;
using URegister.Infrastructure.Data.AuditLog;

namespace URegister.Infrastructure.Models
{
    /// <summary>
    /// Трансформира данните за одитния лог
    /// </summary>
    public class AuditEntry
    {
        /// <summary>
        /// Обект от обектния модел
        /// </summary>
        private readonly EntityEntry entry;

        /// <summary>
        /// Обща информация за заявката
        /// </summary>
        private readonly IAuditInfo info;

        /// <summary>
        /// Инициализация на модела
        /// </summary>
        /// <param name="_entry">Обект от обектния модел</param>
        /// <param name="_info">Обща информация за заявката</param>
        public AuditEntry(EntityEntry _entry, IAuditInfo _info)
        {
            entry = _entry;
            info = _info;
        }

        /// <summary>
        /// Стойности на първичния ключ
        /// </summary>
        public Dictionary<string, object?> KeyValues { get; } = new Dictionary<string, object?>();

        /// <summary>
        /// Данни преди операцията
        /// </summary>
        public Dictionary<string, object?> OldValues { get; } = new Dictionary<string, object?>();

        /// <summary>
        /// Данни след операцията
        /// </summary>
        public Dictionary<string, object?> NewValues { get; } = new Dictionary<string, object?>();

        /// <summary>
        /// Тип на операцията
        /// </summary>
        public AuditType AuditType { get; set; }

        /// <summary>
        /// Променени свойства
        /// </summary>
        public List<string> ChangedColumns { get; } = new List<string>();

        /// <summary>
        /// Конвертира данните към обект от одитния лог
        /// </summary>
        /// <returns></returns>
        public AuditEntity ToAuditDataItem()
        {
            return new AuditEntity()
            {
                AuditId = info.Id,
                AffectedColumns = ChangedColumns.Count == 0 ? null : JsonConvert.SerializeObject(ChangedColumns),
                NewValues = NewValues.Count == 0 ? null : JsonConvert.SerializeObject(NewValues),
                OldValues = OldValues.Count == 0 ? null : JsonConvert.SerializeObject(OldValues),
                PrimaryKey = JsonConvert.SerializeObject(KeyValues),
                TableName = entry.Entity.GetType().Name,
                Type = AuditType.ToString(),
            };
        }

        public string GetTableName()
        {
            return entry.Entity.GetType().Name;
        }
    }
}
