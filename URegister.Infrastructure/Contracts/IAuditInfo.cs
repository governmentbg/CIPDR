// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using URegister.Infrastructure.Constants;

namespace URegister.Infrastructure.Contracts
{
    /// <summary>
    /// Обща информация за заявката
    /// </summary>
    public interface IAuditInfo
    {
        Guid Id { get; set; }

        public bool IsSaved { get; set; }

        /// <summary>
        /// Идентификатор на потребител
        /// </summary>
        Guid? UserId { get; set; }


        /// <summary>
        /// Пълно име на потребител
        /// </summary>
        string? UserFullName { get; set; }

        // <summary>
        /// Пълно име на потребител Base64
        /// </summary>
        string? UserFullNameBase64 { get; set; }
        /// <summary>
        /// Идентификатор на операция
        /// </summary>
        string ActivityId { get; set; }

        // <summary>
        /// Идентификатор на операция в GRPC
        /// </summary>
        string? ActivityFromId { get; set; }

        /// <summary>
        /// Допълнителна информация към операцията
        /// </summary>
        string? ActivityTags { get; set; }

        /// <summary>
        /// Модул, в който е възникнала операцията
        /// </summary>
        string AssemblyName { get; set; }

        /// <summary>
        /// Контролер на операцията
        /// </summary>
        string Controller { get; set; }

        /// <summary>
        /// Действие на операцията
        /// </summary>
        string Action { get; set; }

        /// <summary>
        /// Тип на действието
        /// </summary>
        string Method { get; set; }

        /// <summary>
        /// Параметри на операцията
        /// </summary>
        string? Parameters { get; set; }

        /// <summary>
        /// Параметри на post заявката
        /// </summary>
        public string? PostData { get; set; }

        /// <summary>
        /// IP Адрес на потребителя
        /// </summary>
        IPAddress IpAddress { get; set; }

        /// <summary>
        /// Ако е автоматична задача,
        /// не прави запис в одит лога
        /// </summary>
        TypeAuditTask TypeAuditTask { get; set; }
        string ProjectName { get; set; }
        int RegisterId { get; set; }
        Guid AdministrationId { get; set; }
    }
}
