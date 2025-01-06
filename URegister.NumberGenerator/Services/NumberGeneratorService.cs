using Microsoft.EntityFrameworkCore;
using URegister.Infrastructure.Data.Common;
using URegister.NumberGenerator.Contracts;
using URegister.NumberGenerator.Data;
using URegister.NumberGenerator.Data.Models;

namespace URegister.NumberGenerator.Services
{
    /// <summary>
    /// Услуга за генериране на номера
    /// </summary>
    /// <param name="repo">Достъп до БД</param>
    /// <param name="logger">Лог на грешките</param>
    /// <param name="config">Конфигурация</param>
    public class NumberGeneratorService(
        INumberGeneratorRepository repo,
        ILogger<NumberGeneratorService> logger,
        IConfiguration config) : INumberGeneratorService
    {
        /// <summary>
        /// Генерира номер
        /// </summary>
        /// <param name="register">Код на регистъра</param>
        /// <param name="initialDocumentId">Идентификатор на инициращият документ</param>
        /// <returns>Номер</returns>
        public async Task<long> GenerateNumber(string register, Guid initialDocumentId)
        {
            int ebk = config.GetValue<int>("Number:EBK");
            long number = await GetNumber(ebk);

            await AddToNumberArchive(number, register, initialDocumentId);

            return number;
        }

        /// <summary>
        /// Генерира номер за външна система
        /// </summary>
        /// <param name="ebk">ЕБК номер на администрацията</param>
        /// <param name="systemName">Име на системата</param>
        /// <param name="initialDocumentNumber">Номер на инициращ документ</param>
        /// <returns></returns>
        public async Task<long> GenerateNumberForExternalSystem(int ebk, string systemName, string initialDocumentNumber)
        {
            long number = await GetNumber(ebk);

            await AddToExternalNumberArchive(number, systemName, initialDocumentNumber);

            return number;
        }

        private async Task AddToExternalNumberArchive(long number, string systemName, string initialDocumentNumber)
        {
            ExternalNumberArchive entity = new ExternalNumberArchive
            {
                Register = systemName,
                Number = number,
                InitialDocumentId = initialDocumentNumber
            };

            await repo.AddAsync(entity);
            await repo.SaveChangesAsync();
        }

        /// <summary>
        /// Генерира номер по зададените правила
        /// </summary>
        /// <param name="ebk"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private async Task<long> GetNumber(int ebk)
        {
            int sequenceLength = config.GetValue<int>("Number:SequenceLength");
            string sequenceFormat = $"D{sequenceLength}";
            int maxValue = config.GetValue<int>("Number:MaxValue");
            long number;
            string prefix = $"{DateTime.Now:yy}{ebk:D4}{DateTime.Now.DayOfYear:D3}";
            int dbPrefix = int.Parse(prefix);
            var numerator = (await repo.ExecuteProc<Numerator>(ProcedureType.GetNumeratorSequence, dbPrefix)).SingleOrDefault();

            if (numerator == null)
            {
                throw new ArgumentException("Не е възможно получаване на последователен номер от базата");
            }

            int sequenceNumber = numerator.Sequence;

            if (sequenceNumber > maxValue)
            {
                sequenceNumber = sequenceNumber - maxValue;
                prefix = $"{DateTime.Now:yy}{ebk:D4}{(DateTime.Now.DayOfYear + 500).ToString("D3")}";
            }

            number = long.Parse($"{prefix}{sequenceNumber.ToString(sequenceFormat)}");

            int controlNumber = CalculateControllNumber(number);
            number = number * 10 + controlNumber;
            
            return number;
        }

        /// <summary>
        /// Проверява дали номера е валиден
        /// </summary>
        /// <param name="number">Номер</param>
        /// <returns></returns>
        public async Task<bool> ValidateNumber(long number)
        {
            bool isValid = false;

            // 10 - дължина на префикса + контролно число
            int numberLength = 10 + config.GetValue<int>("Number:SequenceLength");

            if (number.ToString().Length == numberLength)
            {
                long tempNumber = number / 10;
                int controlNumber = CalculateControllNumber(tempNumber);

                if (tempNumber * 10 + controlNumber == number)
                {
                    isValid = await repo.AllReadonly<NumberArchive>()
                        .AnyAsync(a => a.Number == number);
                }
            }

            return isValid;
        }

        /// <summary>
        /// Добавяне на номер в архива
        /// </summary>
        /// <param name="number">Номер</param>
        /// <param name="register">Регистър</param>
        /// <param name="initialDocumentId">Идентификатор на инициращ документ</param>
        /// <returns></returns>
        private async Task AddToNumberArchive(long number, string register, Guid initialDocumentId)
        {
            var entity = new NumberArchive
            {
                Register = register,
                Number = number,
                InitialDocumentId = initialDocumentId
            };

            await repo.AddAsync(entity);
            await repo.SaveChangesAsync();
        }

        /// <summary>
        /// Изчислява контролно число
        /// </summary>
        /// <param name="number">Номер без контролна сума</param>
        /// <returns></returns>
        private int CalculateControllNumber(long number)
        {
            // 9 - дължина на префикса
            int numberLength = 9 + config.GetValue<int>("Number:SequenceLength");

            if (number.ToString().Length != numberLength)
            {
                throw new ArgumentException("Номера трябва да е 15 цифри (без контролна сума)");
            }

            long tempNumber = number;
            int sum = 0;

            do
            {
                sum += (int)(tempNumber % 1000);
                tempNumber /= 1000;
            } while (tempNumber > 0);

            return sum % 10;
        }
    }
}
