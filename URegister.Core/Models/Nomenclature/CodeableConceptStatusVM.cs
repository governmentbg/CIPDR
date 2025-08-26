namespace URegister.Core.Models.Nomenclature
{
    public class CodeableConceptStatusVM
    {
        /// <summary>
        ///  Идентификатор
        /// </summary>
        public int Id { get; set; }


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
        public int StatusId { get; set; }
    }
}
