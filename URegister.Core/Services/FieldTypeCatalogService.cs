using URegister.Common;
using URegister.ObjectsCatalog;

namespace URegister.Core.Services
{
    public static class FieldTypeCatalogService
    {
        private static  List<CatalogFieldType> instance = null;

        /// <summary>
        /// Връша списък с информация за всички типове полета
        /// </summary>
        /// <param name="objectCatalogGrpcClient">Object catalog клиент</param>
        /// <returns>Списък с типове полета или null при проблем</returns>
        public static async Task<IEnumerable<CatalogFieldType>> GetAllFieldType(
            ObjectsCatalogGrpc.ObjectsCatalogGrpcClient objectCatalogGrpcClient)
        {
            if (instance == null)
            {
                CatalogFieldsListReply allFieldTypesReply =
                    await objectCatalogGrpcClient.GetFieldsListAsync(new Google.Protobuf.WellKnownTypes.Empty());

                if (allFieldTypesReply.Status.Code != ResultCodes.Ok)
                {
                    //SetErrorMessage("Проблем при зареждане на типовете полета");
                    //Logger.LogError($"Проблем при зареждане на типовете полета в {nameof(Index)}. {allFieldTypesReply.Status.Message}");
                    //return View(viewModel);
                }
                else
                {
                    instance = allFieldTypesReply.FieldTypes.ToList();
                }
            }

            return instance;
        }

        /// <summary>
        /// Заличава кеширания списък от типове на полета
        /// </summary>
        public static void ResetFieldTypeList()
        {
            instance = null;
        }
    }
}
