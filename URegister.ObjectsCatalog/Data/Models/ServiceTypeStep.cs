using Microsoft.EntityFrameworkCore;

namespace URegister.ObjectsCatalog.Data.Models
{
    /// <summary>
    /// Стъпки към вид услуга
    /// </summary>
    [PrimaryKey(nameof(ServiceTypeId), nameof(StepId))]
    [Comment("Стъпки към вид услуга")]
    public class ServiceTypeStep
    {
        /// <summary>
        /// Идентификатор на вид услуга
        /// </summary>
        [Comment("Идентификатор на вид услуга")]
        public int ServiceTypeId { get; set; }

        /// <summary>
        /// Идентификатор на стъпка
        /// </summary>
        [Comment("Идентификатор на стъпка")]
        public int StepId { get; set; }

        /// <summary>
        /// Тип на услугата
        /// </summary>
        [Comment("Тип на услугата")]
        public ServiceType ServiceType { get; set; } = null!;

        /// <summary>
        /// Стъпка
        /// </summary>
        [Comment("Стъпка")]
        public Step Step { get; set; } = null!;
    }
}
