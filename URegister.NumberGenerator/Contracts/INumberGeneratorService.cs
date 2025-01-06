namespace URegister.NumberGenerator.Contracts
{
    /// <summary>
    /// Услуга за генериране на номера
    /// </summary>
    public interface INumberGeneratorService
    {
        /// <summary>
        /// Генерира номер
        /// </summary>
        /// <param name="register">Код на регистъра</param>
        /// <param name="initialDocumentId">Идентификатор на инициращият документ</param>
        /// <returns>Номер</returns>
        Task<long> GenerateNumber(string register, Guid initialDocumentId);

        /// <summary>
        /// Генерира номер за външна система
        /// </summary>
        /// <param name="ebk">ЕБК номер на администрацията</param>
        /// <param name="systemName">Име на системата</param>
        /// <param name="initialDocumentNumber">Номер на инициращ документ</param>
        /// <returns></returns>
        Task<long> GenerateNumberForExternalSystem(int ebk, string systemName, string initialDocumentNumber);

        /// <summary>
        /// Проверява дали номера е валиден
        /// </summary>
        /// <param name="number">Номер</param>
        /// <returns></returns>
        Task<bool> ValidateNumber(long number);
    }
}
