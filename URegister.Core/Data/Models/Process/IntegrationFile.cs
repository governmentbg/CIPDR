using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Infrastructure.Data.Common;

namespace URegister.Core.Data.Models.Process
{
    /// <summary>
    /// Файлове от ССЕВ
    /// </summary>
    [Comment("Файлове от ССЕВ")]
    public class IntegrationFile : EntityBaseWithLastModifiedInfo
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Вид връзка
        /// </summary>
        [Comment("Вид връзка")]
        public int SourceType { get; set; }


        /// <summary>
        /// Идентификатор на връзка
        /// </summary>
        [Comment("Идентификатор на връзка")]
        public Guid? SourceId { get; set; }


        /// <summary>
        /// Име на файл
        /// </summary>
        [Comment("Име на файл")]
        public string? FileName { get; set; }

        /// <summary>
        /// Идентификатор на файл в IntegrationsCatalog
        /// </summary>
        [Comment("Име на файл")]
        public Guid IntegrationFileId { get; set; }


        public Guid? FileMetadataId { get; set; }
    }
}
