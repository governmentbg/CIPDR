using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Infrastructure.Extensions
{
    public static class DateTimeExtension
    {
        public static DateTime ConvertToUtcIfUnspecified(this DateTime dt)
        {
            if (dt.Kind == DateTimeKind.Unspecified)
            {
                DateTimeZone zone = DateTimeZoneProviders.Tzdb["Europe/Sofia"];
                var localtime = LocalDateTime.FromDateTime(dt);
                var zonedtime = localtime.InZoneLeniently(zone);
                dt = zonedtime.ToInstant().InZone(zone).ToDateTimeUtc();
            }

            return dt;
        }
        public static DateTime SetToUtcIfUnspecified(this DateTime dt)
        {
            DateTime dtResult = dt;
            if (dt.Kind == DateTimeKind.Unspecified)
            {
                dtResult = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            }

            return dtResult;
        }
        public static DateTime SetToUtc(this DateTime dt)
        {
            return  DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        }
        public static DateTime ConvertUtcToBGTime(this DateTime dt)
        {
            if (dt.Kind == DateTimeKind.Utc || dt.Kind == DateTimeKind.Local)
            {
                dt = dt.ToUniversalTime();
                var bgTimeZone = DateTimeZoneProviders.Tzdb["Europe/Sofia"];
                dt = Instant.FromDateTimeUtc(dt)
                            .InZone(bgTimeZone)
                            .ToDateTimeUnspecified();
            }

            return dt;
        }

        public static DateTime? ConvertUtcToBGTime(this DateTime? model)
        {
            if (model != null)
            {
                DateTime dt = model ?? DateTime.UtcNow;
                if (dt.Kind == DateTimeKind.Utc || dt.Kind == DateTimeKind.Local)
                {
                    dt = dt.ToUniversalTime();
                    var bgTimeZone = DateTimeZoneProviders.Tzdb["Europe/Sofia"];
                    dt = Instant.FromDateTimeUtc(dt)
                                .InZone(bgTimeZone)
                                .ToDateTimeUnspecified();
                }

                return dt;
            }
            else
                return model;
        }
    }
}
