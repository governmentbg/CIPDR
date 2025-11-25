using System.ComponentModel;

namespace URegister.Infrastructure.Constants
{
    /// <summary>
    /// Типовете полета на форми
    /// </summary>
    public enum SimpleFormFieldType
    {
        [Description("Текст")] Text = 1,
        [Description("Число")] Number = 2,
        [Description("Дата")] Date = 3,
        [Description("Дата с час")] DateTime = 4,
        [Description("Да/Не стойност")] Boolean = 5,
        [Description("Избор от списък")] Select = 6,
        [Description("Мулти избор от списък")] MultiSelect = 7,
        [Description("Самодопълващо се поле")] Autocomplete = 8,
        [Description("Прикачване на  файл")] File = 9,
        [Description("Многоредов текст")] TextArea = 10,
        [Description("Имейл")] Email = 11,
        [Description("Телефон")] Phone = 12,
        [Description("Въвеждане на уеб адрес")] Url = 13,
        [Description("Лице")] Person = 17,
        [Description("Компания")] Company = 18,
        [Description("Адрес")] Address = 19,
        [Description("Населено място")] City = 20,
        [Description("Данни за автомобил")] Automobile = 21,
        [Description("Документ за самоличност")] PersonalDocument = 22,
        [Description("Идентификатор на лице")] PersonIdentifier = 26,
        [Description("Идентифициране на физическо лице")] IndividualIdentifier = 31,
        [Description("Статичен текст")] StaticText = 32,
        [Description("Идентификатор за компания")] CompanyIdentifier = 33,
        [Description("Артист")] Artist = 35,
        [Description("Време")] Time = 37,
        [Description("Населено място с район")] SettlementWithRegion = 38,
        [Description("Начин на учредяване")] MethodIncorporationImmutable = 39,
        [Description("Номер в регистър")] NumberRegister = 89,
        [Description("Данни за контакт")] ContactDetails = 90,
        [Description("Заличаване")] Delete = 91,      
        [Description("Самодопълващо се с категория")] AutocompleteWithCategory = 47,
        [Description("Компания с адрес")] CompanyWithAddress = 49,
        [Description("Физическо или юридическо лице")] MPREntity = 104,
        [Description("Българска валута")] BulgarianCurrency = 105,
        [Description("Оторизиран служител")] authorizedOfficial = 99,//Умишлено с малка буква, така е в базата
        [Description("Лице с длъжност")] namePosition = 97//Умишлено с малка буква, така е в базата
    }
}
