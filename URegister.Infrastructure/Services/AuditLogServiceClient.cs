using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using URegister.AuditLog;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Contracts;
using URegister.Infrastructure.Data.AuditLog;
using URegister.Infrastructure.Data.Common;
using URegister.Infrastructure.Models;

namespace URegister.Infrastructure.Services
{
    public class AuditLogServiceClient ( AuditLogGrpc.AuditLogGrpcClient auditLogClient) : IAuditLogServiceClient
    {
        public async Task SaveAuditLogGrpc(IAuditInfo auditInfo, List<AuditEntry>? auditEntries)
        {
            var request = new AuditEntitiesMessage
            {
                AuditId = auditInfo.Id.ToString(),
            };
            if (!auditInfo.IsSaved)
            {
                var audit = new AuditMessage
                {
                    Id = auditInfo.Id.ToString(),
                    Action = auditInfo.Action,
                    ActivityId = auditInfo.ActivityId,
                    ActivityFromId = auditInfo.ActivityFromId,
                    ActivityTags = auditInfo.ActivityTags,
                    Controller = auditInfo.Controller,
                    IpAddress = auditInfo.IpAddress?.ToString(),
                    AdministrationId = auditInfo.AdministrationId.ToString(),
                    RegisterId = auditInfo.RegisterId,
                    Method = auditInfo.Method,
                    Parameters = auditInfo.Parameters,
                    PostData = auditInfo.PostData,
                    ProjectName = auditInfo.ProjectName,
                    UserId = auditInfo.UserId.ToString(),
                    UserFullName = auditInfo.UserFullName,
                };
                request.Audit = audit;
            }
            if (auditEntries != null)
            {
                foreach (var auditEntry in auditEntries)
                {
                    var entry = new AuditEntityMessage
                    {
                        AffectedColumns = auditEntry.ChangedColumns.Count == 0 ? null : JsonConvert.SerializeObject(auditEntry.ChangedColumns),
                        NewValues = auditEntry.NewValues.Count == 0 ? null : JsonConvert.SerializeObject(auditEntry.NewValues),
                        OldValues = auditEntry.OldValues.Count == 0 ? null : JsonConvert.SerializeObject(auditEntry.OldValues),
                        PrimaryKey = JsonConvert.SerializeObject(auditEntry.KeyValues),
                        TableName = auditEntry.GetTableName(),
                        Type = auditEntry.AuditType.ToString(),
                    };
                    request.AuditEntities.Add(entry);
                }
            }
            var response = await auditLogClient.AddAuditLogAndEntitiesAsync(request);
        }
    }
}

