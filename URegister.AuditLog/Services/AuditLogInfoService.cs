using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using URegister.AuditLog.Contracts;
using URegister.AuditLog.Data;
using URegister.Infrastructure.Extensions;
using URegister.NumberGenerator.Data;

namespace URegister.AuditLog.Services
{
    /// <summary>
    /// Услуга за одит лог
    /// </summary>
    /// <param name="repo">Достъп до БД</param>
    /// <param name="logger">Лог на грешките</param>
    public class AuditLogInfoService(
        IAuditLogRepository repo,
        ILogger<AuditLogInfoService> logger) : IAuditLogInfoService
    {
        public async Task AddAuditLogAndEntities(AuditEntitiesMessage request)
        {
            IPAddress ipAddress = IPAddress.None;
            IPAddress.TryParse(request.Audit.IpAddress, out ipAddress);
            var auditId = Guid.Parse(request.AuditId);
            if (request.Audit.IsInitialized()) {
                var audit = new Audit
                {
                    Id = auditId,
                    Action = request.Audit.Action,
                    ActivityId = request.Audit.ActivityId,
                    ActivityFromId = request.Audit.ActivityFromId,
                    ActivityTags = request.Audit.ActivityTags,
                    Controller = request.Audit.Controller,
                    Created = DateTime.UtcNow,
                    IpAddress = ipAddress ?? IPAddress.None,
                    Method = request.Audit.Method,
                    Parameters = request.Audit.Parameters,
                    PostData = request.Audit.PostData,
                    ProjectName = request.Audit.ProjectName,
                    UserId = string.IsNullOrEmpty(request.Audit.UserId) ? null : Guid.Parse(request.Audit.UserId),
                    TenantId = Guid.Parse(request.Audit.AdministrationId),
                    RegisterId = request.Audit.RegisterId > 0 ? request.Audit.RegisterId : null,
                    UserFullName = request.Audit.UserFullName,
                };
                await repo.AddAsync(audit);
            }
            foreach (var reqEntity in request.AuditEntities)
            {
                var entity = new AuditEntity
                {
                    Id = Guid.NewGuid(),
                    AuditId = auditId,
                    AffectedColumns = reqEntity.AffectedColumns,
                    NewValues = reqEntity.NewValues,
                    TableName = reqEntity.TableName,
                    OldValues = reqEntity.OldValues,
                    PrimaryKey = reqEntity.PrimaryKey,
                    Type = reqEntity.Type,
                };
                await repo.AddAsync(entity);
            }
            await repo.SaveChangesAsync();
        }

        /// <summary>
        /// Връща списък със записи в системния журнал
        /// </summary>
        /// <param name = "request" ></param >
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<(List<AuditMessage>, int)> GetAuditLogRecordsList(DatatableRequestWithAuditLogFilter request)
        {
            var query = repo.AllReadonly<Audit>()
                                 .IgnoreQueryFilters()
                                 .TagWith(nameof(GetAuditLogRecordsList));

            if (request.Filter != null)
            {
                if (request.Filter.DateFrom != null)
                {
                    var dateFrom = request.Filter.DateFrom.ToDateTime();
                    query = query.Where(x => x.Created >= dateFrom);
                }

                if (request.Filter.DateTo != null)
                {
                    var dateTo = request.Filter.DateTo.ToDateTime().AddDays(1);
                    query = query.Where(x => x.Created <= dateTo);
                }

                if (!string.IsNullOrEmpty(request.Filter.Method))
                {
                    query = query.Where(x => EF.Functions.ILike(x.Method, request.Filter.Method));                
                }

                //if (!string.IsNullOrEmpty(request.Filter.IpAddress) && IPAddress.TryParse(request.Filter.IpAddress, out var parsedIp))
                //{
                //    query = query.Where(x => x.IpAddress == parsedIp);
                //}

                if (!string.IsNullOrEmpty(request.Filter.IpAddress) && IPAddress.TryParse(request.Filter.IpAddress, out var parsedIp))
                {
                    // Handle raw IPv4, IPv4-mapped IPv6, and native IPv6
                    var ipv4Address = parsedIp.MapToIPv4();
                    var ipv4MappedAddress = IPAddress.Parse($"::ffff:{ipv4Address}");
                    query = query.Where(x => x.IpAddress.Equals(parsedIp) || // Native IPv6 or original input
                                            x.IpAddress.Equals(ipv4Address) || // Raw IPv4
                                            x.IpAddress.Equals(ipv4MappedAddress)); // IPv4-mapped IPv6
                }

                if (!string.IsNullOrEmpty(request.Filter.UserName))
                {
                    var searchTerms = request.Filter.UserName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(term => $"%{term}%")
                                        .ToArray();
                    query = query.Where(x => searchTerms.All(pattern => EF.Functions.ILike(x.UserFullName, pattern)));
                }                
            }

            var list = new List<AuditMessage>();
            var countAll = 0;
          
            (query, countAll) = await request.Request.GetFilteredData(query);
            var data = query.Select(x => new AuditMessage
            {
                Id = x.Id.ToString(),
                UserId = x.UserId.ToString(),
                ProjectName = x.ProjectName,
                Controller = x.Controller,
                Action = x.Action,
                Method = x.Method,
                IpAddress = x.IpAddress != null ? x.IpAddress.ToString() : string.Empty,
                CreatedDate = x.Created.ToUniversalTime().ToTimestamp(),
                Parameters = x.Parameters,
                UserFullName = x.UserFullName
            }).ToList();

            data.ForEach(x =>
            {
                if (!string.IsNullOrEmpty(x.IpAddress) && IPAddress.TryParse(x.IpAddress, out var ip))
                {
                    x.IpAddress = ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4().ToString() : x.IpAddress;
                }
            });

            return (data, countAll);
        }


        /// <summary>
        /// Връща списък със стари и нови стойности на запис в системния журнал
        /// </summary>
        /// <param name="auditId"></param>
        /// <returns></returns>
        public async Task<List<AuditEntityMessage>> GetAuditEntityValues(string auditId)
        {
            var parsedAuditId = Guid.Parse(auditId);

            var auditEntities = await repo.AllReadonly<AuditEntity>()
                                .IgnoreQueryFilters()
                                .Where(ae => ae.AuditId == parsedAuditId)
                                .TagWith(nameof(GetAuditEntityValues))
                                .ToListAsync();

            string FormatJson(string json)
            {
                if (string.IsNullOrEmpty(json)) return null;
                try
                {
                    // Parse JSON and handle nested strings
                    var parsed = JsonSerializer.Deserialize<object>(json);
                    parsed = ParseNestedJson(parsed);
                    // Format as pretty JSON
                    return JsonSerializer.Serialize(parsed, new JsonSerializerOptions { WriteIndented = true });
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Error processing JSON: {ex.Message}");
                    return null;
                }
            }

            string FormatPrimaryKey(string json)
            {
                if (string.IsNullOrEmpty(json)) return null;
                try
                {
                    var parsed = JsonSerializer.Deserialize<object>(json);
                    return JsonSerializer.Serialize(parsed); // Single-line JSON, no indentation
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Error parsing PrimaryKey JSON: {ex.Message}");
                    return null;
                }
            }

            object ParseNestedJson(object value)
            {
                if (value is JsonElement element)
                {
                    if (element.ValueKind == JsonValueKind.String && IsJsonString(element.GetString()))
                    {
                        try
                        {
                            return ParseNestedJson(JsonSerializer.Deserialize<object>(element.GetString()));
                        }
                        catch (JsonException) { return value; }
                    }
                    if (element.ValueKind == JsonValueKind.Object)
                    {
                        var dict = element.Deserialize<Dictionary<string, object>>();
                        foreach (var key in dict.Keys.ToList())
                            dict[key] = ParseNestedJson(dict[key]);
                        return dict;
                    }
                    if (element.ValueKind == JsonValueKind.Array)
                    {
                        var list = element.Deserialize<List<object>>();
                        for (int i = 0; i < list.Count; i++)
                            list[i] = ParseNestedJson(list[i]);
                        return list;
                    }
                }
                else if (value is Dictionary<string, object> dict)
                {
                    foreach (var key in dict.Keys.ToList())
                        dict[key] = ParseNestedJson(dict[key]);
                    return dict;
                }
                else if (value is List<object> list)
                {
                    for (int i = 0; i < list.Count; i++)
                        list[i] = ParseNestedJson(list[i]);
                    return list;
                }
                return value;
            }

            bool IsJsonString(string value)
            {
                if (string.IsNullOrWhiteSpace(value)) return false;
                value = value.Trim();
                return (value.StartsWith("{") && value.EndsWith("}")) || (value.StartsWith("[") && value.EndsWith("]"));
            }

            var result = auditEntities.Select(ae => new AuditEntityMessage
            {               
                PrimaryKey = FormatPrimaryKey(ae.PrimaryKey),
                OldValues = FormatJson(ae.OldValues),
                NewValues = FormatJson(ae.NewValues)
            }).ToList();

            return result;
        }

        }
}
