// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using URegister.Infrastructure.Constants;
using System;

namespace URegister.Core.Models.Previewer
{
    /// <summary>
    /// Информация за прегледа на файл
    /// </summary>
    public class PreviewInfoViewModel
    {
        public PreviewInfoViewModel()
        {

        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="fileId">Идентификатор на файл</param>
        /// <param name="returnUrl">URL на който да се върне при отказ</param>
        /// <param name="returnSignUrl">URL на който да се върне след подписване</param>
        public PreviewInfoViewModel(string fileId, string returnUrl, string returnSignUrl)
        {
            FileId = fileId;
            if (!string.IsNullOrEmpty(returnUrl))
                ReturnUrl = new Uri(returnUrl);
            if (!string.IsNullOrEmpty(returnSignUrl) || !string.IsNullOrEmpty(returnUrl))
                ReturnSignUrl = new Uri(returnSignUrl ?? returnUrl);
            IsSigned = true;
            FileHash = null;
            SignatureType = PreviewConstants.SignatureType.Detached;
            ShowPreview = true;
        }

        /// <summary>
        /// URL на който да се върне при отказ
        /// </summary>
        public Uri ReturnUrl { get; set; }

        /// <summary>
        /// URL на който да се върне след подписване
        /// </summary>
        public Uri ReturnSignUrl { get; set; }

        /// <summary>
        /// Файлът е подписан
        /// </summary>
        public bool IsSigned { get; set; }

        /// <summary>
        /// Хеш на файла
        /// </summary>
        public string FileHash { get; set; }

        /// <summary>
        /// Идентификатор на файл
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// Тип на файла
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Име на файла
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Подпис
        /// </summary>
        public string Signature { get; set; }

        /// <summary>
        /// Код на грешка
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Тип на подписа
        /// </summary>
        public string SignatureType { get; set; }

        /// <summary>
        /// Идентификатор на временния PDF
        /// </summary>
        public string TempPdfId { get; set; } = null;

        /// <summary>
        /// Причина за подписване
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Дали да се покаже преглед на документ
        /// </summary>
        public bool ShowPreview { get; set; } = true;
    }
}
