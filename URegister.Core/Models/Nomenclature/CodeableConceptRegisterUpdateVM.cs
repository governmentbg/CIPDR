namespace URegister.Core.Models.Nomenclature
{
    public class CodeableConceptRegisterUpdateVM
    {
        /// <summary>
        /// Регистър
        /// </summary>
        public int RegisterId { get; set; }

        /// <summary>
        /// Тип
        /// </summary>
        public string Type { get; set; } = null!;

        /// <summary>
        /// Код
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Валиден
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Филтър на дататабле
        /// </summary>
        public string? Filter { get; set; }
    }
}
