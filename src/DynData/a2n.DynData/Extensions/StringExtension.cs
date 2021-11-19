using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Linq
{
    public static class StringExtension
    {
        public static string ToHumanReadable(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            var output = "";
            var len = value.Length;
            var c = "";

            for (var i = 0; i < len; i++)
            {
                c = value[i].ToString();

                if (i == 0)
                {
                    output += c.ToUpper();
                }
                else if (c != c.ToLower() && c == c.ToUpper())
                {
                    output += " " + c;
                }
                else if (c == "-" || c == "_")
                {
                    output += " ";
                }
                else
                {
                    output += c;
                }
            }

            return output;
        }

        public static string ToCamelCase(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            if (value.Length == 1)
                return value.ToLower();
            return string.Format("{0}{1}", Char.ToLowerInvariant(value[0]), value.Substring(1));
        }
    }
}
