using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Core.Data;
using URegister.Infrastructure.Model.AuditLog;
using URegister.Core.Models.Process;
using URegister.Infrastructure.Data.AuditLog;
using URegister.Infrastructure.Extensions;
using URegister.Users;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static URegister.Users.AppUserManager;

namespace URegister.Core.Services
{
    public class AuditLogService : BaseService, IAuditLogService
    {
        private readonly AppUserManagerClient appUserManagerClient;
        public AuditLogService(IApplicationRepository repo,
            ILogger<BaseService> logger,
            AppUserManagerClient appUserManagerClient) : base(repo, logger)
        {
            this.appUserManagerClient = appUserManagerClient;
        }

        /// <summary>
        /// Връща списък със записи в системния журнал
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetAuditLogRecordsList(IDataTablesRequest request, AuditLogFilterVM filter)
        {
            var queryWhere = Repo.AllReadonly<Audit>()
                                 .IgnoreQueryFilters();

            if (filter.DateFrom != null)
            {
                queryWhere = queryWhere.Where(x => x.Created >= filter.DateFrom.Value.ToUniversalTime());
            }
            if (filter.DateTo != null)
            {
                queryWhere = queryWhere.Where(x => x.Created < filter.DateTo.Value.ToUniversalTime().AddDays(1));
            }
            if (!string.IsNullOrEmpty(filter.ActionType))
            {
                queryWhere = queryWhere.Where(x => EF.Functions.ILike(x.Method, filter.ActionType));
            }
            //if (!string.IsNullOrEmpty(filter.IpAddress) && IPAddress.TryParse(filter.IpAddress, out var parsedIp))
            //{
            //    queryWhere = queryWhere.Where(x => x.IpAddress == parsedIp);
            //}
            if (!string.IsNullOrEmpty(filter.IpAddress) && IPAddress.TryParse(filter.IpAddress, out var parsedIp))
            {
                // Handle raw IPv4, IPv4-mapped IPv6, and native IPv6
                var ipv4Address = parsedIp.MapToIPv4();
                var ipv4MappedAddress = IPAddress.Parse($"::ffff:{ipv4Address}");
                queryWhere = queryWhere.Where(x => x.IpAddress.Equals(parsedIp) || // Native IPv6 or original input
                                                  x.IpAddress.Equals(ipv4Address) || // Raw IPv4
                                                  x.IpAddress.Equals(ipv4MappedAddress)); // IPv4-mapped IPv6
            }
            if (!string.IsNullOrEmpty(filter.UserName))
            {               
                var searchTerms = filter.UserName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(term => $"%{term}%")
                                    .ToArray();
                queryWhere = queryWhere.Where(x => searchTerms.All(pattern => EF.Functions.ILike(x.UserFullName, pattern)));              
            }

            var query = queryWhere.Select(x => new AuditLogListItemVM
            {
                Id = x.Id,
                UserId = x.UserId,
                UserFullName = x.UserFullName,
                AssemblyName = x.AssemblyName,
                Controller = x.Controller,
                Action = x.Action,
                ActionType = x.Method,
                IpAddress = x.IpAddress,
                CreatedDate = x.Created,
                Parameters = x.Parameters
            })
            .TagWith(nameof(GetAuditLogRecordsList));
            
            var countAll = 0;
            (query, countAll) = request.GetResponseData(query);
          
            return request.GetResponseJson(query, countAll);
        }       

        public async Task<IActionResult> GetAuditEntityValues(Guid auditId)
        {
            var auditEntities = await Repo.AllReadonly<AuditEntity>()
                                .IgnoreQueryFilters()
                                .Where(ae => ae.AuditId == auditId)
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

            var result = auditEntities.Select(ae => new
            {
                ae.Id,
                ae.AuditId,
                PrimaryKey = FormatPrimaryKey(ae.PrimaryKey),
                OldValues = FormatJson(ae.OldValues),
                NewValues = FormatJson(ae.NewValues)
            }).ToList();

            return new JsonResult(result);
        }      
    }
}