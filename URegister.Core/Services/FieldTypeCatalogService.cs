using URegister.Common;
using URegister.ObjectsCatalog;

namespace URegister.Core.Services
{
    public class FieldTypeCatalogService
    {
        /// <summary>
        /// Връша списък с информация за всички типове полета
        /// </summary>
        /// <param name="objectCatalogGrpcClient">Object catalog клиент</param>
        /// <returns>Списък с типове полета или null при проблем</returns>
        public static async Task<IEnumerable<CatalogFieldType>> GetAllFieldType(
            ObjectsCatalogGrpc.ObjectsCatalogGrpcClient objectCatalogGrpcClient)
        {
         
            CatalogFieldsListReply allFieldTypesReply =
                await objectCatalogGrpcClient.GetFieldsListAsync(new Google.Protobuf.WellKnownTypes.Empty());

            if (allFieldTypesReply.Status.Code != ResultCodes.Ok)
            {
                //TODO : логване
                //Logger.LogError($"Проблем при зареждане на типовете полета в {nameof(Index)}. {allFieldTypesReply.Status.Message}");
                return new List<CatalogFieldType>();
            }

            return allFieldTypesReply.FieldTypes.ToList();
        }
    }
}
