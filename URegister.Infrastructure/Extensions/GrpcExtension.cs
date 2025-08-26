using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Infrastructure.Contracts;

namespace URegister.Infrastructure.Extensions
{
    public static class GrpcExtension
    {
        public static IHttpClientBuilder AddCallCredentialsGrpc(this IHttpClientBuilder builder)
        {
            return builder.AddCallCredentials((context, metadata, serviceProvider) =>
            {
                var auditInfo = (IAuditInfo?)serviceProvider.GetService(typeof(IAuditInfo));
                metadata.Add("ActivityId", auditInfo?.ActivityId ?? string.Empty);
                metadata.Add("UserId", auditInfo?.UserId?.ToString() ?? string.Empty);
                metadata.Add("AdministrationId", auditInfo?.AdministrationId.ToString() ?? string.Empty);
                metadata.Add("RegisterId", auditInfo?.RegisterId.ToString() ?? "0");
                metadata.Add("UserFullName", auditInfo?.UserFullNameBase64 ?? string.Empty);
                return Task.CompletedTask;
            });
        }
    }
}
