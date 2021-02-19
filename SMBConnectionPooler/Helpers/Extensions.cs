using System;
using System.Collections.Generic;
using System.Text;

namespace SMBConnectionPooler.Helpers
{
    public static class Extensions
    {
        public static int ToInt(this object value, int defaultValue)
        {
            int intValue;

            return value is bool
                ? (bool)value ? 1 : 0
                : int.TryParse(value + "", out intValue)
                    ? intValue
                    : defaultValue;
        }

        public static int? ToInt(this object value, int? defaultValue)
        {
            int intValue;

            return value is bool
                ? (bool)value ? 1 : 0
                : int.TryParse(value + "", out intValue)
                    ? intValue
                    : defaultValue;
        }
    }
}
