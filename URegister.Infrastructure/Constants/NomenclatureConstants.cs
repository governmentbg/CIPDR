using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Infrastructure.Constants
{
    public static class NomenclatureTypes
    {
        public const string EkArea1 = "EK001";
        public const string EkArea2 = "EK002";
        public const string EkRegion = "EK003";
        public const string EkMunicipality = "EK004";
        public const string EkTownHall = "EK005";
        public const string Ekatte = "EK006";
        public const string EkRaion = "EK007";
        public const string EkStreet = "EK008";
        public const string EkKvartal = "EK010";
        public const string EkatteMunicipalityRegion = "EKATTE";
        public const string PidType = "CL0001";
    }
    public static class InternalNomenclatureTypes
    {
        public const string RegisterType = "I0001";
        public const string RegisterEntryType = "I0002";
        public const string RegisterIdentitySecurityLevel = "I0003";
        public const string PersonType = "I0004";
    }

    public static class PersonTypeValue
    {
        public const string Manager = "00001";
        public const string AuthorizedPerson = "00002";
    }
    public static class AdditionalColumnNames
    {
        public const string Nuts3 = "Nuts3";
        public const string Document = "Document";
        public const string Ekatte = "Ekatte";
        public const string Category = "Category";
        public const string Kind = "Kind";
        public const string Kmetstvo = "Kmetstvo";
        public const string TVM = "TVM";
        public const string Altitude = "Altitude";
        public const string Area1 = "Area1";
        public const string Area2 = "Area2";
        public const string CityCode = "CityCode";
    }
    public static class UicTypes
    {
        public const int EGN = 1;
        public const int LNCH = 2;
        public const int EIK = 3;
    }

    public static class RegisterConstants
    {
        public const string CodePrefix = "R";
    }
}
