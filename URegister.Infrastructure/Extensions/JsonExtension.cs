using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace URegister.Infrastructure.Extensions
{
    /// <summary>
    /// Методи за сериализация и десериализация на JSON
    /// </summary>
    public static class JsonExtension
    {
        /// <summary>
        /// Опции по подразбиране за сериализация на JSON
        /// </summary>
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        /// <summary>
        /// Десериализация на JSON стринг към обект
        /// </summary>
        /// <typeparam name="T">Тип на обекта</typeparam>
        /// <param name="json">JSON стринг</param>
        /// <returns></returns>
        public static T? FromJson<T>(this string json) =>
            JsonSerializer.Deserialize<T>(json, _jsonOptions);

        /// <summary>
        /// Сериализация на обект към JSON стринг
        /// </summary>
        /// <typeparam name="T">Тип на обекта</typeparam>
        /// <param name="obj">Обект за сериализация</param>
        /// <returns></returns>
        public static string ToJson<T>(this T obj) =>
            JsonSerializer.Serialize(obj, _jsonOptions);

        /// <summary>
        /// Десериализация на JSON стринг към обект
        /// </summary>
        /// <typeparam name="T">Тип на обекта</typeparam>
        /// <param name="json">JSON стринг</param>
        /// <param name="jsonOptions">Опции за сериализиране</param>
        /// <returns></returns>
        public static T? FromJson<T>(this string json, JsonSerializerOptions jsonOptions) =>
            JsonSerializer.Deserialize<T>(json, jsonOptions);

        /// <summary>
        /// Сериализация на обект към JSON стринг
        /// </summary>
        /// <typeparam name="T">Тип на обекта</typeparam>
        /// <param name="obj">Обект за сериализация</param>
        /// <param name="jsonOptions">Опции за сериализиране</param>
        /// <returns></returns>
        public static string ToJson<T>(this T obj, JsonSerializerOptions jsonOptions) =>
            JsonSerializer.Serialize(obj, jsonOptions);

        /// <summary>
        /// Минимизира JSON текст, като премахва интервали и оформление
        /// </summary>
        /// <param name="json">json низ за минимизиране</param>
        /// <returns></returns>
        public static string MinifyJson(this string json)
        {
            // Parse the JSON string into a JsonDocument
            var jsonDocument = JsonDocument.Parse(json);

            // Serialize it back to a string without indented formatting (compact)
            return JsonSerializer.Serialize(jsonDocument.RootElement, new JsonSerializerOptions
            {
                WriteIndented = false
            });
        }
    }
}
