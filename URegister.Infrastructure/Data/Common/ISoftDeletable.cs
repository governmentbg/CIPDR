using System;

namespace URegister.Infrastructure.Data.Common
{
    /// <summary>
    /// Позволява маркиране на записи като изтрити, 
    /// без да се трият физически от базата данни
    /// </summary>
    public interface ISoftDeletable
    {
        /// <summary>
        /// Дали записът е активен
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        /// Дата на изтриване
        /// </summary>
        DateTime? DeletedOn { get; set; }
    }
}
