// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Newtonsoft.Json;

namespace URegister.Core.Models.Previewer
{
    /// <summary>
    /// Информация за визуалния подпис
    /// </summary>
    public class PdfVisualSignatureData
    {
        /// <summary>
        /// Име на подписващия
        /// </summary>
        [JsonProperty("signerName")]
        public string SignerName { get; set; }

        /// <summary>
        /// Сертификат на подписващия
        /// </summary>
        [JsonProperty("signerCert")]
        public string SignerCert { get; set; }

        /// <summary>
        /// Причина за подписване
        /// </summary>
        [JsonProperty("reason")]
        public string Reason { get; set; }

        /// <summary>
        /// Идентификатор на файл
        /// </summary>
        [JsonProperty("fileId")]
        public string FileId { get; set; }

        /// <summary>
        /// Дали подписа е визуален
        /// </summary>
        [JsonProperty("visual")]
        public bool Visual { get; set; }

        /// <summary>
        /// Позиция на подписа в PDF
        /// </summary>
        [JsonProperty("position")]
        public SignaturePositionModel Position { get; set; } = new SignaturePositionModel();
    }
}
