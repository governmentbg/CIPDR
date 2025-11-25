using URegister.Common;
using URegister.Core.Models.CurrentRegister;
using URegister.ObjectsCatalog;

namespace URegister.Core.Services
{
    public class FieldTypeCatalogService
    {
        /// <summary>
        /// Връща списък с информация за всички типове полета
        /// </summary>
        /// <param name="objectCatalogGrpcClient">Object catalog клиент</param>
        /// <param name="currentRegisterCode"></param>
        /// <returns>Списък с типове полета или null при проблем</returns>
        public static async Task<IEnumerable<CatalogFieldType>> GetAllFieldType(
            ObjectsCatalogGrpc.ObjectsCatalogGrpcClient objectCatalogGrpcClient, string currentRegisterCode = null)
        {
            CatalogFieldsListRequest request = new CatalogFieldsListRequest()
            {
                RegisterCode = currentRegisterCode
            };

            CatalogFieldsListReply allFieldTypesReply =
                await objectCatalogGrpcClient.GetFieldsListAsync(request);

            if (allFieldTypesReply.Status.Code != ResultCodes.Ok)
            {
                //TODO : логване
                //Logger.LogError($"Проблем при зареждане на типовете полета в {nameof(Index)}. {allFieldTypesReply.Status.Message}");
                return new List<CatalogFieldType>();
            }

            return allFieldTypesReply.FieldTypes.ToList();
        }

        /// <summary>
        /// Връща тип поле
        /// </summary>
        /// <param name="objectCatalogGrpcClient">Object catalog клиент</param>
        /// <param name="requestedFieldType">Tип поле</param>
        /// <returns>Тип поле</returns>
        public static async Task<CatalogFieldType> GetFieldType(
            ObjectsCatalogGrpc.ObjectsCatalogGrpcClient objectCatalogGrpcClient, string requestedFieldType)
        {
            CatalogFieldRequest request = new CatalogFieldRequest()
            {
                FieldType = requestedFieldType
            };

            CatalogFieldTypeReply fieldTypeReply =
                await objectCatalogGrpcClient.GetFieldTypeAsync(request);

            if (fieldTypeReply.Status.Code != ResultCodes.Ok)
            {
                //TODO : логване
                //Logger.LogError($"Проблем при зареждане на типовете полета в {nameof(Index)}. {allFieldTypesReply.Status.Message}");
                return new CatalogFieldType();
            }

            CatalogFieldType fieldType = new CatalogFieldType 
            {
                Type = fieldTypeReply.Type,
                Label = fieldTypeReply.Label,
                FieldTypeId = fieldTypeReply.Id
            };

            if (fieldTypeReply.RegisterRestrictionCodes != null)
            {
                fieldType.RegisterRestrictionCodes.AddRange(fieldTypeReply.RegisterRestrictionCodes);
            }
          
            return fieldType;
        }
    }
}
