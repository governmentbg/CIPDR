using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using URegister.ObjectsCatalog.Data.Models;

namespace URegister.ObjectsCatalog.Data.Configurations
{
    /// <summary>
    /// Конфигурация на полета
    /// </summary>
    public class FieldConfiguration : IEntityTypeConfiguration<Field>
    {
        /// <summary>
        /// Метод за конфигуриране на полета
        /// </summary>
        /// <param name="builder"></param>
        public void Configure(EntityTypeBuilder<Field> builder)
        {
        }
    }
}
