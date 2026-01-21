// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IO.SignTools.Contracts;
using System.Threading.Tasks;
using URegister.Infrastructure.Contracts;

namespace URegister.Core.Services
{
    public class TempFileHandler : ITempFileHandler
    {
        protected readonly IObjectStoreService objectStoreService;

        public TempFileHandler(IObjectStoreService _objectStoreService)
        {
            objectStoreService = _objectStoreService;
        }

        public async Task DeleteFile(string filename)
        {
            await objectStoreService.DeleteObject(filename);
        }

        public async Task<byte[]> ReadFile(string filename)
        {
            (var data, _) = await objectStoreService.GetObject(filename);

            return data;
        }

        public async Task SaveFile(string filename, byte[] data)
        {
            await objectStoreService.SaveObject(filename, data, filename, "application/pdf", null);
        }
    }
}
