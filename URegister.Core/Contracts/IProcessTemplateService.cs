using Microsoft.AspNetCore.Mvc;
using URegister.Core.Data.Models.Common;
using URegister.Core.Data.Models.Process;
using URegister.Core.Models.OpenData;

namespace URegister.Core.Contracts
{
    public interface IProcessTemplateService
    {
        Task<string> GetProcessCertificateHtml(Process process, Process processCertificate, BlanksTemplate? blankTemplate, List<RegisterItem> registerItemsCertificate, List<RegisterItem> registerItems);
        Task<string> GetProcessCertificateOnRegisterHtml(Process process, List<RegisterItem> registerItems, BlanksTemplate blanksTemplate);

        /// <summary>
        /// Връща списък с приключени заявени услуги от тип Вписване, като стойностите на сложните полета са конкатинирани стойности на подполетата им
        /// </summary>
        /// ///
        /// <param name="administrationId">Идентификатор на администрация</param>
        /// <param name="skip">Колко записа да бъдат пропуснати</param>
        /// <param name="take">Колко записа да бъдат взети</param>
        /// <param name="searchKey"></param>
        /// <param name="searchPattern"></param>
        /// <param name="toDate">До дата на вписване, включително</param>
        /// <param name="fromDate">От дата на вписване, включително</param>
        /// <param name="searchToDate">До дата за търсене по критерии от тип дата</param>
        /// <param name="searchFromDate">От дата за търсене по критерии от тип дата</param>
        /// <returns></returns>
        Task<(JsonResult?, List<Dictionary<string, object>>, List<PublicFieldTemplate>)> GetRegistrationProcessList(Guid administrationId, 
            int skip, 
            int take, 
            string searchKey, 
            string searchPattern, 
            DateTime? toDate, 
            DateTime? fromDate, 
            DateTime? searchToDate, 
            DateTime? searchFromDate);
        IEnumerable<IEnumerable<string>> ProcessForOpenData(List<Dictionary<string, object>> data, List<OpenDataColVM> cols);
    }
}
