using EDelivery.Integration.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Contracts
{
    public interface IFileService
    {
        Task<BlobDO?> UploadFile(string fileName, byte[] fileData);
        Task<byte[]> DownLoadFile(string url);
    }
}
