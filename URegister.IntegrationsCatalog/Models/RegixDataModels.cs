using System.Xml.Serialization;

namespace URegister.IntegrationsCatalog.Models
{
    public class RegixDataModels
    {
        [XmlRoot(ElementName = "Address", Namespace = "http://egov.bg/RegiX/AV/TR/SubdeedsCommon")]
        public class Address
        {
            [XmlElement("CountryID")] public int CountryID { get; set; }
            [XmlElement("CountryCode")] public string CountryCode { get; set; }
            [XmlElement("Country")] public string Country { get; set; }
            [XmlElement("IsForeign")] public bool IsForeign { get; set; }
            [XmlElement("DistrictID")] public int DistrictID { get; set; }
            [XmlElement("DistrictEkatte")] public string DistrictEkatte { get; set; }
            [XmlElement("District")] public string District { get; set; }
            [XmlElement("Municipalityid")] public int Municipalityid { get; set; }
            [XmlElement("MunicipalityEkatte")] public string MunicipalityEkatte { get; set; }
            [XmlElement("Municipality")] public string Municipality { get; set; }
            [XmlElement("SettlementID")] public int SettlementID { get; set; }
            [XmlElement("SettlementEKATTE")] public string SettlementEKATTE { get; set; }
            [XmlElement("Settlement")] public string Settlement { get; set; }
            [XmlElement("AreaID")] public int AreaID { get; set; }
            [XmlElement("Area")] public string Area { get; set; }
            [XmlElement("AreaEkatte")] public string AreaEkatte { get; set; }
            [XmlElement("PostCode")] public string PostCode { get; set; }
            [XmlElement("ForeignPlace")] public string ForeignPlace { get; set; }
            [XmlElement("HousingEstate")] public string HousingEstate { get; set; }
            [XmlElement("Street")] public string Street { get; set; }
            [XmlElement("StreetNumber")] public string StreetNumber { get; set; }
            [XmlElement("Block")] public string Block { get; set; }
            [XmlElement("Entrance")] public string Entrance { get; set; }
            [XmlElement("Floor")] public string Floor { get; set; }
            [XmlElement("Apartment")] public string Apartment { get; set; }
        }

        [XmlRoot(ElementName = "Contacts", Namespace = "http://egov.bg/RegiX/AV/TR/SubdeedsCommon")]
        public class Contacts
        {
            [XmlElement("Phone")] public string Phone { get; set; }
            [XmlElement("Fax")] public string Fax { get; set; }
            [XmlElement("EMail")] public string EMail { get; set; }
            [XmlElement("URL")] public string URL { get; set; }
        }

        [XmlRoot(ElementName = "Root", Namespace = "http://egov.bg/RegiX/AV/TR/SubdeedsCommon")]
        public class Root
        {
            [XmlElement("Address")] public Address Address { get; set; }
            [XmlElement("Contacts")] public Contacts Contacts { get; set; }
        }
    }
}
