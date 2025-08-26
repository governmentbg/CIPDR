namespace URegister.Core.Models.Process
{
    public class ProcessListItemVM
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; } 

        /// <summary>
        /// Входящ номер
        /// </summary>
        public string? IncomingNumber { get; set; }

        /// <summary>
        /// "Стар входящ номер"
        /// </summary>
        public string? OldIncomingNumber { get; set; }

        /// <summary>
        /// Номер на вписване 
        /// </summary>
        public string? RegisterNumber { get; set; }

        /// <summary>
        /// Дата на входиране
        /// </summary>
        public DateTime IncomingDate { get; set; }

        /// <summary>
        /// Стара дата на входиране
        /// </summary>

        public DateTime? OldIncomingDate { get; set; }

        /// <summary>
        /// Услуга
        /// </summary>
        public string? ServiceName { get; set; }

        /// <summary>
        /// Стъпка
        /// </summary>
        public string? StepName { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Идентификатор стъпка 
        /// </summary>
        public int StepId { get; set; }

        /// <summary>
        /// Идентификатор услуга
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// Идентификатор статус
        /// </summary>
        public int StatusId { get; set; }

        /// <summary>
        /// Идентификатор на партида в MasterPersonIndex
        /// </summary>
        public Guid MpriId { get; set; }

        /// <summary>
        /// Идентификатор на заявител в MasterPersonIndex
        /// </summary>
        public Guid MpriApplicantId { get; set; }
        /// <summary>
        /// Партида
        /// </summary>
        public string? Partida { get; set; }

        /// <summary>
        /// Заявител
        /// </summary>
        public string? Applicant { get; set; }

        /// <summary>
        /// Има ли следваща стъпка
        /// </summary>
        public bool HasNextStep { get; set; }

        /// <summary>
        /// Има ли бутон затвори
        /// </summary>
        public bool HasClose { get; set; }

        /// <summary>
        /// Има ли бутон заличаване
        /// </summary>
        public bool HasDeletion { get; set; }

        /// <summary>
        /// Има ли бутон промяна
        /// </summary>
        public bool HasChange { get; set; }

        /// <summary>
        /// Има ли бутон указания
        /// </summary>
        public bool HasInstruction { get; set; }

        /// <summary>
        /// Има ли бутон връчвания
        /// </summary>
        public bool HasDelivery { get; set; }

        /// <summary>
        /// Следваща стъпка
        /// </summary>
        public string? NextStep { get; set; }

        /// <summary>
        /// Насочено към
        /// </summary>
        public string? FromName { get; set; }

        /// <summary>
        /// Има ли удостоверение за даунлоад
        /// </summary>
        public bool HasCertificate { get; set; }

        /// <summary>
        /// Има ли buton за освобождаване за обработка
        /// </summary>
        public bool HasDeAssignUser { get; set; }
        
        /// <summary>
        /// Номер на отказ
        /// </summary>
        public string? RejectionNumber { get; set; }

        public Guid? AssignedToUserId { get; set; }
    }
}
