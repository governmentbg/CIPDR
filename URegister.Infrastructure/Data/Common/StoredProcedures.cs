namespace URegister.Infrastructure.Data.Common
{
    /// <summary>
    /// Функции и съхранени процедури в базата данни
    /// </summary>
    public static class StoredProcedures
    {
        /// <summary>
        /// Име на функция за взимане на номер
        /// </summary>
        private const string GetNumeratorSequence = "get_sequence({0})";

        /// <summary>
        /// Имена на съхранени процедури
        /// </summary>
        private static readonly IReadOnlyDictionary<ProcedureType, string> procedureNames = new Dictionary<ProcedureType, string>
        {
            { ProcedureType.GetNumeratorSequence, GetNumeratorSequence }
        };

        /// <summary>
        /// Получаване на име на съхранена процедура
        /// </summary>
        /// <param name="procedure">Стойност от енумерация за тип процедура / функция</param>
        /// <returns></returns>
        public static string GetProcedureName(ProcedureType procedure)
        {
            return procedureNames[procedure];
        }
    }

    /// <summary>
    /// Типове съхранени процедури
    /// </summary>
    public enum ProcedureType
    {
        GetNumeratorSequence
    }
}
