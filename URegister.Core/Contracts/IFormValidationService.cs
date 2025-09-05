using Microsoft.AspNetCore.Http;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.NomenclaturesCatalog;

namespace URegister.Core.Contracts
{
    /// <summary>
    /// Сервиз за back end валидация на данните въведени във форма
    /// </summary>
    public interface IFormValidationService
    {
        /// <summary>
        /// Валидира стойностите на полетата във формата
        /// </summary>
        /// <param name="viewModel">Моделът за валидация</param>
        /// <param name="nomenclatureGrpcClient">GRPC клиент за номенклатура</param>
        /// <param name="registerId">Идентификатор на регистъра</param>
        /// <param name="processRegistrationDateUtc">Дата на създаване на заяявената услуга</param>
        /// <param name="skipRequiredTest">Да се пропусне ли проверка за задължителни полета</param>
        /// <returns>Всички стойности ли са валидни</returns>
        public Task<bool> ValidateViewModel(FormViewModel viewModel,
            NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient, int registerId,
            DateTime? processRegistrationDateUtc = null,
            bool skipRequiredTest = false);

        /// <summary>
        /// Валидира качен файл
        /// </summary>
        /// <param name="field">Полето за валидация</param>
        /// <param name="file">Файл за валидация</param>
        /// <returns></returns>
        public Task<bool> ValidateFile(FormField field, IFormFile file);

        /// <summary>
        /// Връща колекция от всички грешки при валидация на полета
        /// </summary>
        /// <param name="model">Валидираният модел</param>
        /// <returns></returns>
        public Task<Dictionary<string, string>> GetValidatedFormFieldsErrors(FormViewModel model);

        /// <summary>
        /// Проверка дали съдържанието на файл отговаря на разширението
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public Task<bool> IsFileAcceptableFormat(IFormFile file);
    }
}
