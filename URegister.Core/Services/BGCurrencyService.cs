using System.Globalization;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Services
{
    public static class BGCurrencyService
    {
        public static decimal RegistryItemValueToValueInEuro(string value)
        {
            int currencyValue = string.IsNullOrWhiteSpace(value) ? 0 : int.Parse(value.Split(':')[0]);
            string amountValue = string.IsNullOrWhiteSpace(value) ? String.Empty : value.Split(':')[1];

            if(currencyValue == (int)Currency.BGN)
            {
                return decimal.Parse(amountValue ?? "0", CultureInfo.InvariantCulture) / ValueConstants.EURInBGN;
            }
            if (currencyValue == (int)Currency.EUR)
            {
                return decimal.Parse(amountValue ?? "0", CultureInfo.InvariantCulture);
            }

            throw new ArgumentException($"Непозната номенклатура за валута в {value}");
        }
        
        public static string EuroValueToFormFieldValue(decimal valueInEuro)
        {
            bool isBeforeEuro = DateTime.Now < ValueConstants.EuroDate;

            if (isBeforeEuro)
            {
                return (int)Currency.BGN + ":" + (valueInEuro * ValueConstants.EURInBGN).ToString("F2", CultureInfo.InvariantCulture);
            }

            return (int)Currency.EUR + ":" + valueInEuro.ToString("F2", CultureInfo.InvariantCulture);
        }

        public static string RegistryItemValueToPublicText(string value)
        {
            CultureInfo bg = new CultureInfo("bg-BG");

            bool isBeforeEuro = DateTime.Now < ValueConstants.EuroDate;
            var currencySymbol = isBeforeEuro ? "лв." : "€";

            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            int currencyValue = string.IsNullOrWhiteSpace(value) ? 0 : int.Parse(value.Split(':')[0]);
            string amountValue = string.IsNullOrWhiteSpace(value) ? String.Empty : value.Split(':')[1];
            if (string.IsNullOrWhiteSpace(amountValue))
            {
                amountValue = "0";
            }
            string displayValue;
            if (!isBeforeEuro && currencyValue == (int)Currency.BGN)
            {
                displayValue = Math
                    .Round((decimal.Parse(amountValue ?? "0", CultureInfo.InvariantCulture) / ValueConstants.EURInBGN),
                        2).ToString("N2", bg);
            }
            else
            {
                displayValue = decimal.Parse(amountValue ?? "0", CultureInfo.InvariantCulture).ToString("N2", bg);
            }
            return displayValue + ' ' + currencySymbol;
        }
    }
}
