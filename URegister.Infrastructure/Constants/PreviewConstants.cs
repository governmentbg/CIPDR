// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace URegister.Infrastructure.Constants
{
    /// <summary>
    /// Константи за прегледа на документи
    /// </summary>
    public static class PreviewConstants
    {
        /// <summary>
        /// Поддържани типове файлове
        /// </summary>
        public static class ContentType
        {
            public const string Docx = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            public const string PDF = "application/pdf";
            public const string ImageStartsWith = "image";
            public const string Text = "text/plain";
            public const string ImageTiff = "image/tiff";
            public const string ImageTif = "image/tif";
        }

        /// <summary>
        /// Типове подписи
        /// </summary>
        public static class SignatureType
        {
            public const string Embeded = "Embeded";
            public const string Detached = "Detached";
        }

        /// <summary>
        /// Максимален размер на файла
        /// </summary>
        public const int MaxFileSize = 10 * 1024 * 1024;

        /// <summary>
        /// Разрешени разширения на файловете
        /// </summary>
        public static string[] AcceptFileExts = { "docx", "rtf", "pdf", "jpg", "png", "tif", "tiff", "zip" };
    }
}
