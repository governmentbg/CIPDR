using URegister.Infrastructure.Data.Common;

namespace URegister.NumberGenerator.Data
{
    /// <summary>
    /// Репозитори за генератора на номера
    /// </summary>
    public class NumberGeneratorRepository : Repository, INumberGeneratorRepository
    {
        /// <summary>
        /// Създава ново репозитори за генератора на номера
        /// </summary>
        /// <param name="context"></param>
        public NumberGeneratorRepository(NumberGeneratorDbContext context)
        {
            Context = context;
        }
    }
}
