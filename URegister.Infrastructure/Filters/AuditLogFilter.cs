// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Contracts;
using URegister.Infrastructure.Data.Common;
using URegister.Infrastructure.Services;

namespace URegister.Infrastructure.Filters
{
    public class AuditLogFilter<T> : IAsyncActionFilter where T : IRepository
    {
        private string controllerName = string.Empty;
        private string actionName = string.Empty;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            ControllerActionDescriptor? controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

            if (controllerActionDescriptor != null)
            {
                controllerName = GetDisplayName(controllerActionDescriptor.ControllerTypeInfo.CustomAttributes) 
                    ?? controllerActionDescriptor.ControllerName;   
                
                actionName = GetDisplayName(controllerActionDescriptor.MethodInfo.CustomAttributes) 
                    ?? controllerActionDescriptor.ActionName;

                var auditInfo = await CreateAuditInfo(context);
                
                var auditService = context.HttpContext.RequestServices.GetService(typeof(IAuditLogServiceClient)) as IAuditLogServiceClient;
                if (auditService != null)
                {
                    await auditService.SaveAuditLogGrpc(auditInfo, null);
                } else
                {
                    var auditRepository = context.HttpContext.RequestServices.GetService(typeof(T)) as IRepository;
                    if (auditRepository != null)
                    {
                        await auditRepository.SaveRequest();
                    }
                }
            }

            var result = await next();
        }

        private string GetDisplayName(IEnumerable<CustomAttributeData> customAttributes)
        {
            string result = null;
            var displayAttribute = customAttributes
                    .FirstOrDefault(a => a.AttributeType == typeof(DisplayAttribute))
                    ?.NamedArguments.FirstOrDefault(a => a.MemberName == "Name");

            if (displayAttribute != null && displayAttribute.HasValue) 
            {
                result = displayAttribute.Value.TypedValue.Value.ToString();
            }

            return result;
        }

        private async Task<IAuditInfo> CreateAuditInfo(ActionExecutingContext context)
        {
            string ip;
            string clientId = null;
            var user = context.HttpContext.User;
            var administrationId = Guid.Empty;
            string? userName = null;
            if (user != null && user.Claims != null && user.Claims.Count() > 0)
            {
                var subClaim = user.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

                if (subClaim != null)
                {
                    clientId = subClaim.Value;
                }
                var administration = user.FindFirst(CustomClaimType.AdministrationId)?.Value;
                Guid.TryParse(administration, out administrationId);
                userName = user.FindFirst(ClaimTypes.Name)?.Value;
            }

            ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

            if (context.HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var currentip))
            {
                string tempIp = currentip;
                ip = tempIp.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault();
            }

            IPAddress.TryParse(ip, out IPAddress? ipAddress);
            Guid? userId = null;

            if (clientId != null && Guid.TryParse(clientId, out Guid tempId))
            {
                userId = tempId;
            }

            var auditInfo = (IAuditInfo?)context.HttpContext.RequestServices.GetService(typeof(IAuditInfo));

            if (auditInfo == null)
            {
                throw new InvalidOperationException("No Audit info available");
            }
            else
            {
                auditInfo.Action = actionName;
                auditInfo.ActivityId = context.HttpContext.TraceIdentifier.ToString();
                auditInfo.AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                auditInfo.Method = context.HttpContext.Request.Method;
                auditInfo.Controller = controllerName;
                auditInfo.IpAddress = ipAddress;
                auditInfo.Parameters = JsonConvert.SerializeObject(context.HttpContext.Request.Query);
                auditInfo.UserId = userId;
                auditInfo.AdministrationId = administrationId;
                auditInfo.UserFullName = userName;
                if (context.HttpContext.Request.Method != "GET")
                {
                    if (context.HttpContext.Request.HasFormContentType)
                    {
                        auditInfo.PostData = JsonConvert.SerializeObject(context.HttpContext.Request.Form);
                    }
                    else
                    {
                        var req = context.HttpContext.Request;
                        string? bodyStr = null;
                        // Allows using several time the stream in ASP.Net Core
                        req.EnableBuffering();

                        // Arguments: Stream, Encoding, detect encoding, buffer size 
                        // AND, the most important: keep stream opened
                        using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true))
                        {
                            bodyStr = await reader.ReadToEndAsync();
                        }

                        // Rewind, so the core is not lost when it looks at the body for the request
                        req.Body.Position = 0;
                        auditInfo.PostData = string.IsNullOrEmpty(bodyStr) ? null : bodyStr;
                    }
                }
            }

            return auditInfo;
        }
    }
}
