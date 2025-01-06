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
        /// <param name="nomenclatureGrpcClient">GRPC клиент за номенклатуро</param>
        /// <param name="registerId">Идентификатор на регистъра</param>
        /// <returns>Всички стойности ли са валидни</returns>
        public Task<bool> ValidateViewModel(FormViewModel viewModel,
            NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient, int registerId);
    }
}
