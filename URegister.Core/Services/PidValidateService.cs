using System.Text.RegularExpressions;

namespace MHRegistries.Core.Services
{
    /// <summary>
    /// Валидира различни входни параметри
    /// </summary>
    public static class PidValidateService
    {
        /// <summary>
        /// Валидира персонален идентификатор
        /// </summary>
        /// <param name="personalId">Персонален идентификатор</param>
        /// <param name="pidCode">Тип на идентификатора</param>
        /// <returns>Резултат от проверката</returns>
        public static bool ValidatePersonalId(string personalId, int pidCode)
        {
            if (string.IsNullOrWhiteSpace(personalId))
                return false;

            switch (pidCode)
            {
                case 1: //ЕГН
                    return CheckEGN(personalId);
                case 2: //ЛНЧ
                    return CheckLNCH(personalId);
                //case RegisterNomenclatures.IdentifierTypeLongConstants.EIK:
                //    return CheckEIK(personalId);
                case 3: //Социален номер - за чужди граждани
                    return true;//TODO
                case 4: //Номер на паспорт
                    return true;//TODO
                case 5: //Друг идентификатор
                    return true;//TODO
                case 6: //Новородено
                    return true;//TODO

                default:
                    // Няма как да валидирам, следователно пропускам напред
                    return true;
            }
        }

        private static bool CheckEIK(string EIK)
        {
            if ((EIK.Length != 9) && (EIK.Length != 13)) return false;

            if (CheckSum9EIK(EIK)?.ToString() == EIK.Substring(8, 1))
            {
                if (EIK.Length == 9)
                {
                    return true;
                }
                else
                {
                    return CheckSum13EIK(EIK)?.ToString() == EIK.Substring(12, 1);
                }
            }
            else
                return false;
        }

        private static int? CheckSum9EIK(string EIK)
        {
            int sum = 0, a = 0, chkSum = 0;
            for (int i = 0; i < 8; i++)
            {
                if (!int.TryParse(EIK.Substring(i, 1), out a)) return null;
                sum += a * (i + 1);
            }
            chkSum = sum % 11;
            if (chkSum == 10)
            {

                sum = 0;
                a = 0;
                chkSum = 0;
                for (int i = 0; i < 8; i++)
                {
                    if (!int.TryParse(EIK.Substring(i, 1), out a)) return null;
                    sum += a * (i + 3);
                }
                chkSum = sum % 11;
                if (chkSum == 10) chkSum = 0;
            }
            return chkSum;
        }
        private static int? CheckSum13EIK(string EIK)
        {
            int sum = 0, a = 0, chkSum = 0;
            for (int i = 8; i < 12; i++)
            {
                if (!int.TryParse(EIK.Substring(i, 1), out a)) return null;
                switch (i)
                {
                    case 8:
                        sum = a * 2;
                        continue;
                    case 9:
                        sum += a * 7;
                        continue;
                    case 10:
                        sum += a * 3;
                        continue;
                    case 11:
                        sum += a * 5;
                        continue;
                }
            }
            chkSum = sum % 11;
            if (chkSum == 10)
            {
                for (int i = 8; i < 12; i++)
                {
                    if (!int.TryParse(EIK.Substring(i, 1), out a)) return null;
                    switch (i)
                    {
                        case 8:
                            sum = a * 4;
                            continue;
                        case 9:
                            sum += a * 9;
                            continue;
                        case 10:
                            sum += a * 5;
                            continue;
                        case 11:
                            sum += a * 7;
                            continue;
                    }
                }
                chkSum = sum % 11;
                if (chkSum == 10) chkSum = 0;
            }
            return chkSum;
        }

        /// <summary>
        /// Връща датата на раждане от ЕГН без да го валидира предварително
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static DateTime GetBirthdateFromEGN(string pid)
        {
            int year = int.Parse(pid.Substring(0, 2));
            int month = int.Parse(pid.Substring(2, 2));
            int day = int.Parse(pid.Substring(4, 2));

            // Определяне на века и корекция на месеца
            if (month >= 1 && month <= 12)
            {
                year += 1900;
            }
            else if (month >= 21 && month <= 32)
            {
                month -= 20;
                year += 1800;
            }
            else if (month >= 41 && month <= 52)
            {
                month -= 40;
                year += 2000;
            }
            else
            {
                throw new ArgumentException("Невалидно ЕГН");
            }

            return new DateTime(year, month, day);
        }

        private static bool CheckLNCH(string personalId)
        {
            Regex rgx = new Regex(@"^\d{10}$");

            if (string.IsNullOrEmpty(personalId) ||
                !rgx.IsMatch(personalId))
            {
                return false;
            }

            if (!(personalId.StartsWith("100") || personalId.StartsWith("0000") || personalId.StartsWith("7000") ||
                  personalId.StartsWith("700")))
                return false;

            int lastNumber = 0;
            int sum = 0;
            int[] multipliers = new int[] { 21, 19, 17, 13, 11, 9, 7, 3, 1 };

            for (int i = 0; i < personalId.Length - 1; i++)
            {
                lastNumber = int.Parse(personalId[i].ToString());
                sum += lastNumber * multipliers[i];
            }

            lastNumber = int.Parse(personalId[personalId.Length - 1].ToString());

            return sum % 10 == lastNumber;
        }

        private static bool CheckEGN(string personalId)
        {
            int[] egnWeights = new int[] { 2, 4, 8, 5, 10, 9, 7, 3, 6 };
            Regex rgx = new Regex(@"^\d{10}$");

            if (string.IsNullOrEmpty(personalId) ||
                !rgx.IsMatch(personalId))
            {
                return false;
            }

            (int year, int month, int day) = ParseEGNDate(personalId);

            if (!CheckDate(year, month, day))
            {
                return false;
            }

            int checksum = int.Parse(personalId.Substring(9, 1));
            int egnSum = 0;

            for (int i = 0; i < 9; i++)
            {
                egnSum += int.Parse(personalId.Substring(i, 1)) * egnWeights[i];
            }

            int validChecksum = egnSum % 11;

            if (validChecksum == 10)
            {
                validChecksum = 0;
            }

            return checksum == validChecksum;
        }

        private static bool CheckDate(int year, int month, int day)
        {
            bool result = true;

            try
            {
                var date = new DateTime(year, month, day);
            }
            catch (ArgumentOutOfRangeException)
            {
                result = false;
            }

            return result;
        }

        private static (int year, int month, int day) ParseEGNDate(string egn)
        {
            int year = int.Parse(egn.Substring(0, 2));
            int month = int.Parse(egn.Substring(2, 2));
            int day = int.Parse(egn.Substring(4, 2));

            if (month > 40)
            {
                year += 2000;
                month -= 40;
            }
            else if (month > 20)
            {
                year += 1800;
                month -= 20;
            }
            else
            {
                year += 1900;
            }

            return (year, month, day);
        }
    }
}
