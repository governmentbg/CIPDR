// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IO.SignTools.Contracts;
using iText.Signatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using URegister.Core.Models.Previewer;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Contracts;

namespace URegister.Components
{
    public class PreviewViewComponent : ViewComponent
    {
        private readonly IObjectStoreService objectStoreService;
        private readonly IIOSignToolsService signToolsService;
        private readonly ILogger logger;

        public PreviewViewComponent(
            IObjectStoreService _objectStoreService,
            IIOSignToolsService _signToolsService,
            ILogger<PreviewViewComponent> _logger)
        {
            objectStoreService = _objectStoreService;
            signToolsService = _signToolsService;
            logger = _logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(PreviewInfoViewModel info, string viewName = "")
        {
            if (info.SignatureType == PreviewConstants.SignatureType.Embeded 
                && info.ContentType == PreviewConstants.ContentType.PDF)
            {
                viewName = "PdfJsSigner";
            }
            else
            {
                if (info.IsSigned == false)
                {
                    var file = await objectStoreService.GetObject(info.FileId);
                    info.FileHash = signToolsService.GetHash(file.data, HashAlgorithmName.SHA256);
                }
            }

            return await Task.FromResult(View(viewName, info));
        }
    }
}
