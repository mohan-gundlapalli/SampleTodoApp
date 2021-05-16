using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApp
{
    public static class GenericHelper
    {
        public static string ToFormatted(this DateTime dateTime, string dateFormat)
        {
            return dateTime.ToString(dateFormat);
        }

        public static DateTime ToDateTime(this string dateTimeStr, string dateFormat)
        {
            if (DateTime.TryParseExact(dateTimeStr, dateFormat, 
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
            {
                return dateTime;
            }

            throw new FormatException($"The given {dateTimeStr} is not in correct format");
        }
    }
}
