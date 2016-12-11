using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model
{
    public static class StringExtensions
    {
        public static bool IsNonZero(this string decimalString)
        {
            try
            {
                return Decimal.Parse(decimalString) > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public static bool IsNumeric(this string numberString)
        {
            if (string.IsNullOrEmpty(numberString)) return false;

            var isnumber = new Regex("^[0-9]*$");
            return isnumber.IsMatch(numberString);
        }

        public static bool IsByte(this string byteString)
        {
            if (string.IsNullOrEmpty(byteString)) return false;

            var isByte = new Regex("^[0-9a-fA-F]$");
            return isByte.IsMatch(byteString);

        }

        public static bool IsByteBcd(this string bytebcdString)
        {
            if (string.IsNullOrEmpty(bytebcdString)) return false;

            var isByteBcd = new Regex("^[0-9]{1,2}$");
            return isByteBcd.IsMatch(bytebcdString);
        }



        internal static string AdjustLength(this string value, int length)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Substring(0, (value.Length > length) ? length : value.Length);
        }


    }
}
