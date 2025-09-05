using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace URegister.Infrastructure.Model.AuditLog
{
    public class AuditLogListItemVM
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор на потребител
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Имена на потребител
        /// </summary>
        public string? UserFullName { get; set; }

        /// <summary>
        /// Контролер на операцията
        /// </summary>
        public string Controller { get; set; }

        /// <summary>
        /// Действие на операцията
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Тип на операцията
        /// </summary>
        public string ActionType { get; set; }

        /// <summary>
        /// Дата и час на събитието (UTC)
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// IP Адрес на потребителя
        /// </summary>
        [JsonIgnore]
        public IPAddress IpAddress { get; set; }

        public string IpAddressStr
        {
            get
            {
                if (IpAddress != null)
                {
                    //return IpAddress.ToString();
                    return IpAddress.IsIPv4MappedToIPv6 ? IpAddress.MapToIPv4().ToString() : IpAddress.ToString();
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Модул, в който е възникнала операцията лейбъл
        /// </summary>
        public string AssemblyNameText { get; set; }

        /// <summary>
        /// Модул, в който е възникнала операцията
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// Флаг дали има нова и стара стойност
        /// </summary>
        public bool IsValues
        {
            get
            {
                var parameters = !string.IsNullOrEmpty(Parameters) ? Parameters.Replace(@"[", string.Empty).Replace(@"]", string.Empty) : string.Empty;
                return !string.IsNullOrEmpty(OldValues) ||
                        !string.IsNullOrEmpty(NewValues) ||
                        !string.IsNullOrEmpty(parameters);
            }
        }

        /// <summary>
        /// Стойности преди операцията
        /// </summary>
        public string OldValues { get; set; }

        /// <summary>
        /// Стойности след операцията
        /// </summary>
        public string NewValues { get; set; }

        /// <summary>
        /// Параметри
        /// </summary>
        public string? Parameters { get; set; }
    }
}
