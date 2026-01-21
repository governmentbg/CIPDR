// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Net;
using System.Reflection;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Contracts;

namespace URegister.Infrastructure.Models
{
    /// <summary>
    /// Обща информация за заявката
    /// </summary>
    public class AuditInfo : IAuditInfo
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public bool IsSaved { get; set; }

        /// <summary>
        /// Идентификатор на потребител
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Пълно име на потребител
        /// </summary>
        public string? UserFullName { get; set; }


        public string? UserFullNameBase64 { 
            get {
                var userName = string.Empty;
                if (!string.IsNullOrEmpty(UserFullName))
                {
                    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(UserFullName);
                    userName = System.Convert.ToBase64String(plainTextBytes);
                }
                return userName;
            } 
            set{
                if (string.IsNullOrEmpty(value))
                {
                    UserFullName = value;
                }
                else
                {
                    var base64EncodedBytes = System.Convert.FromBase64String(value);
                    UserFullName = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
                }
            } 
        }

        /// <summary>
        /// Идентификатор на операция
        /// </summary>
        public string ActivityId { get; set; } = null!;

        // <summary>
        /// Идентификатор на операция в GRPC
        /// </summary>
        public string? ActivityFromId { get; set; }

        /// <summary>
        /// Допълнителна информация към операцията
        /// </summary>
        public string? ActivityTags { get; set; }

        /// <summary>
        /// Контролер на операцията
        /// </summary>
        public string Controller { get; set; }

        /// <summary>
        /// Действие на операцията
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Тип на действието
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Параметри на post заявката
        /// </summary>
        public string? PostData { get; set; }

        /// <summary>
        /// Параметри на операцията
        /// </summary>
        public string? Parameters { get; set; }

        /// <summary>
        /// IP Адрес на потребителя
        /// </summary>
        public IPAddress IpAddress { get; set; }

        public Guid AdministrationId { get; set; }
        public int RegisterId { get; set; }

        public string ProjectName { get; set; } = null!;

        /// <summary>
        /// Ако е автоматична задача,
        /// не прави запис в одит лога
        /// </summary>
        public TypeAuditTask TypeAuditTask { get; set; } 

        /// <summary>
        /// Модул, в който е възникнала операцията
        /// </summary>
        public string AssemblyName { get; set; } = null!;

        public void SetAuditInfoForQuartz(string queue, string method)
        {
            Guid userId = Guid.Empty;
            int regiserId = 0;
            var administrationId = Guid.Empty;
            
            Action = queue;
            ActivityId = "Job";
            ActivityFromId = method;
            AssemblyName = Assembly.GetExecutingAssembly()?.GetName()?.Name!;
            Method = method;
            Controller = "QUARTZ";
            IpAddress = null!;
            UserId = userId != Guid.Empty ? userId : null;
            AdministrationId = administrationId != Guid.Empty ? administrationId : Guid.Empty;
            RegisterId = regiserId;
        }
    }
}
