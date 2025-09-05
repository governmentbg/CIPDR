using Newtonsoft.Json;

namespace URegister.Core.Models.Previewer
{
    /// <summary>
    /// Позиция на подписа в PDF
    /// </summary>
    public class SignaturePositionModel
    {
        /// <summary>
        /// Страница
        /// </summary>
        [JsonProperty("page")]
        public int Page { get; set; } = 1;

        /// <summary>
        /// Начална позиция по X
        /// </summary>
        [JsonProperty("x")]
        public float X { get; set; } = 400;

        /// <summary>
        /// Начална позиция по Y
        /// </summary>
        [JsonProperty("y")]
        public float Y { get; set; } = 750;

        /// <summary>
        /// Ширина
        /// </summary>
        [JsonProperty("width")]
        public float Width { get; set; } = 150;

        /// <summary>
        /// Височина
        /// </summary>
        [JsonProperty("height")]
        public float Height { get; set; } = 60;
    }
}
