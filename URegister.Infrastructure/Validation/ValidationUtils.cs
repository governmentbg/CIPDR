// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using URegister.Infrastructure.Constants;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BlueCardPortal.Infrastructure.Validation
{
    public class ValidationUtils
    {
        /// <summary>
        /// Check value is EGN
        /// </summary>
        /// <param name="EGN">string</param>
        /// <param name="InitiallyValidation"></param>
        /// <returns>string</returns>
        public static bool IsEGN(string EGN, bool InitiallyValidation = false)
        {
            if (EGN == null) return false;
            if (EGN.Length != 10) return false;
            if (EGN == "0000000000") return false;

            // само първична валидация
            if (InitiallyValidation)
            {
                decimal egn = 0;
                if (!decimal.TryParse(EGN, out egn)) return false;
                return true;
            }

            // пълна валидация
            int a = 0;
            int valEgn = 0;
            for (int i = 0; i < 10; i++)
            {
                if (!int.TryParse(EGN.Substring(i, 1), out a)) return false;
                switch (i)
                {
                    case 0:
                        valEgn += 2 * a;
                        continue;
                    case 1:
                        valEgn += 4 * a;
                        continue;
                    case 2:
                        valEgn += 8 * a;
                        continue;
                    case 3:
                        valEgn += 5 * a;
                        continue;
                    case 4:
                        valEgn += 10 * a;
                        continue;
                    case 5:
                        valEgn += 9 * a;
                        continue;
                    case 6:
                        valEgn += 7 * a;
                        continue;
                    case 7:
                        valEgn += 3 * a;
                        continue;
                    case 8:
                        valEgn += 6 * a;
                        continue;
                }
            }
            long chkSum = valEgn % 11;
            if (chkSum == 10)
                chkSum = 0;
            if (chkSum != Convert.ToInt64(EGN.Substring(9, 1))) return false;
            if ((int.Parse(EGN.Substring(8, 1)) / 2) == 0)
            {
                // girl person
                return true;
            }
            // guy person
            return true;
        }
        /// <summary>
        /// EIK CheckSum
        /// </summary>
        /// <param name="EIK">string</param>
        /// <returns>bool</returns>
        public static int? CheckSum9EIK(string EIK)
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
        public static int? CheckSum13EIK(string EIK)
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
        /// EIK validation
        /// </summary>
        /// <param name="EIK">string</param>
        /// <returns>bool</returns>
        public static bool IsEIK(string EIK)
        {
            if (EIK == null) return false;
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

        public static DateTime? GetBirthDayFromEgn(string EGN)
        {
            DateTime? result = null;
            if (IsEGN(EGN) == true)
            {
                var year = int.Parse(EGN.Substring(0, 2));
                var month = int.Parse(EGN.Substring(2, 2));
                var day = int.Parse(EGN.Substring(4, 2));
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

                try
                {
                    result = new DateTime(year, month, day);
                }
                catch (Exception)
                {
                    result = null;
                }
            }
            return result;
        }

        public static bool? IsMaleFromEGN(string egn)
        {
            bool? result = null;
            if (string.IsNullOrEmpty(egn))
            {
                return result;
            }
            try
            {
                var sexDigit = int.Parse(egn.Substring(8, 1));
                if (sexDigit % 2 == 1)
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
            catch
            {

            }

            return result;
        }

        public static bool CheckLNCH(string personalId)
        {
            Regex rgx = new Regex(@"^\d{10}$");


            if (string.IsNullOrEmpty(personalId) ||
                 !rgx.IsMatch(personalId))
            {
                return false;
            }

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
        public static bool CheckBirthDay(string identifier)
        {
            var dateResult = new DateTime(1000, 1, 1, 0, 0, 0);
            var result = DateTime.TryParseExact(identifier, FormattingConstant.NormalDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateResult);
            if (result && dateResult <= DateTime.Now.AddYears(-18) && dateResult > DateTime.Now.AddYears(-100))
                return true;
            return false;
        }

      
        /// <summary>
        /// Валидация на идентификатор
        /// </summary>
        /// <param name="UicType">Тип идентификатор от NomenclatureConstants.UicTypes</param>
        /// <param name="Uic">Идентификатор</param>
        /// <returns></returns>
        public static string ValidateUic(long UicType, string Uic)
        {
            switch (UicType)
            {
                case UicTypes.EGN:
                    return IsEGN(Uic) ? string.Empty : "Невалидно ЕГН";
                case UicTypes.LNCH:
                    return CheckLNCH(Uic) ? string.Empty : "Невалидно ЛНЧ";
                case UicTypes.EIK:
                    return IsEIK(Uic) ? string.Empty : "Невалиден ЕИК";
            }

            return string.Empty;
        }
    }
}

