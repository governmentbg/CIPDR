using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Core.Data.Models.Common;
using URegister.Core.Data.Models.Process;
using URegister.Core.Models.Previewer;

namespace URegister.Core.Contracts
{
    public interface ICommonFileService
    {
        Task ChangeFile(string fileId, byte[] newPdf, Guid? roleId);
        Task<(byte[], FileMetadata, string)> GetFileById(string id);
        Task<FileMetadata?> GetFileForSignByProcess(Guid processId);
        Task<FileInfoModel> GetFileInfo(string id);
        Task<List<FileMetadata>> GetFilesForSign(List<Guid> roles);
        Task<OutMessage> GetOutMessage(Guid id);
        Task<OutMessage?> GetOutMessageByFileId(Guid id);
        Task<Guid> StampFile(Guid fileId);
        Task<List<Guid>> UserRolesForSign();
    }
}
