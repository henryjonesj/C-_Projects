using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BallyTech.QCom.Messages
{
    public class EncodedIO
    {
        public static DateTime ReadDateTimeAsQComTimeDate(System.IO.BinaryReader input, int length, int precision)
        {
            int year = 1970, month = 1, day = 1, hour = 0, minute = 0, second = 0;
            second = (int)ReadDecimalAsBCD(input, 1, 0);
            minute = (int)ReadDecimalAsBCD(input, 1, 0);
            hour = (int)ReadDecimalAsBCD(input, 1, 0);
            day = (int)ReadDecimalAsBCD(input, 1, 0);
            month = (int)ReadDecimalAsBCD(input, 1, 0);
            year = (int)ReadDecimalAsBCD(input, 1, 0) + 2000;
            if (year <= 0) year = 1; if (month <= 0) month = 1; if (day <= 0) day = 1;
            return new DateTime(year, month, day, hour, minute, second);
        }

        public static void WriteDateTimeAsQComTimeDate(System.IO.BinaryWriter output, DateTime dateTime, int length, int precision)
        {
            if (length >= 6) WriteDecimalAsBCD(output, dateTime.Second, 1, 0);
            if (length >= 5) WriteDecimalAsBCD(output, dateTime.Minute, 1, 0);
            if (length >= 4) WriteDecimalAsBCD(output, dateTime.Hour, 1, 0);
            if (length >= 3) WriteDecimalAsBCD(output, dateTime.Day, 1, 0);
            if (length >= 2) WriteDecimalAsBCD(output, dateTime.Month, 1, 0);            
            if (length >= 1) WriteDecimalAsBCD(output, (dateTime.Year % 2000), 1, 0);            
        }

        public static decimal ReadDecimalAsBinaryBE(BinaryReader input, int length, int precision)
        {
            byte[] data = input.ReadBytes(length);
            decimal result = 0m;
            for (int i = 0; i < length; i++) result = (result * 256) + data[i];
            return result * new decimal(1, 0, 0, false, (byte)precision);
        }

        public static void WriteDecimalAsBinaryBE(BinaryWriter output, decimal value, int length, int precision)
        {
            value = Decimal.Truncate(value / new decimal(1, 0, 0, false, (byte)precision));
            byte[] data = new byte[length];
            for (int i = length - 1; i >= 0; i--)
            {
                data[i] = (byte)(value % 256);
                value = Decimal.Truncate(value / 256);
            }
            output.Write(data);
        }

        public static decimal ReadDecimalAsBCD(BinaryReader input, int length, int precision)
        {
            return ReadDecimalAsBCD(input.ReadBytes(length), length, precision);
        }

        public static decimal ReadDecimalAsBCD(byte[] input, int length, int precision)
        {
            decimal result = 0m;
            byte[] data = input;
            for (int i = 0; i < length; i++) result = (result * 100m) + (data[i] >> 4) * 10 + (data[i] & 0x0F);
            return result * new decimal(1, 0, 0, false, (byte)precision);
        }

        public static void WriteDecimalAsBCD(BinaryWriter output, decimal value, int length, int precision)
        {
            byte[] data = new byte[length];
            decimal temp = Decimal.Truncate(value / new decimal(1, 0, 0, false, (byte)precision));
            if (temp < 0) temp = 0;
            decimal limit = 1m / new decimal(1, 0, 0, false, (byte)(2 * length)) - 1;
            if (temp > limit) temp = limit;

            for (int i = length - 1; i >= 0; i--)
            {
                int lastTwoDigits = (int)(temp % 100m);
                temp = Decimal.Floor(temp / 100);
                data[i] = (byte)(((lastTwoDigits / 10) << 4) | (lastTwoDigits % 10));
            }

            if (temp != 0m) throw new ArgumentOutOfRangeException("value", String.Format("value too big for {0}BCD field", length));
            output.Write(data);
        }

        public static void WriteDecimalAsBCDWithLSBFirst(BinaryWriter output, decimal value, int length, int precision)
        {
            byte[] data = new byte[length];
            decimal temp = Decimal.Truncate(value / new decimal(1, 0, 0, false, (byte)precision));
            if (temp < 0) temp = 0;
            decimal limit = 1m / new decimal(1, 0, 0, false, (byte)(2 * length)) - 1;
            if (temp > limit) temp = limit;

            for (int i = 0; i <= length - 1; i++)
            {
                int lastTwoDigits = (int)(temp % 100m);
                temp = Decimal.Floor(temp / 100);
                data[i] = (byte)(((lastTwoDigits / 10) << 4) | (lastTwoDigits % 10));
            }

            if (temp != 0m) throw new ArgumentOutOfRangeException("value", String.Format("value too big for {0}BCD field", length));
            output.Write(data);
        }

        internal static decimal ReadDecimalAsBCDWithLSBFirst(BinaryReader input, int length, int precision)
        {
            decimal result = 0m;
            byte[] data = input.ReadBytes(length);
            for (int i = length -1; i >= 0; i--) result = (result * 100m) + (data[i] >> 4) * 10 + (data[i] & 0x0F);
            return result * new decimal(1, 0, 0, false, (byte)precision);
        }

        public static void WriteStringAsASCII(BinaryWriter output, string value, int length, int precision)
        {
            if (string.IsNullOrEmpty(value)) return;
            output.Write(ReadStringAsASCII(value, length, precision));
        }

        public static byte[] ReadStringAsASCII(string value, int length, int precision)
        {
            byte[] data = null;
            if (string.IsNullOrEmpty(value)) return null;
            data = Encoding.ASCII.GetBytes(value);
            if (precision > 0) data = ArrayRestrict(data, precision);
            return data;
        }

        public static T[] ArrayRestrict<T>(T[] input, int length)
        {
            if (input.Length == length) return input;

            T[] result = new T[length];
            Array.Copy(input, result, Math.Min(input.Length, length));
            return result;
        }

        public static string ReadStringAsASCII(BinaryReader input, int length, int precision)
        {
            string result = GetDisplayMessage(input, length, Encoding.ASCII);
            return result;
        }

        private static string GetDisplayMessage(BinaryReader input, int length, Encoding encodingstyle)
        {
            if (length == 0) return string.Empty;

            int index;
            byte[] data = input.ReadBytes(length);
            string result = encodingstyle.GetString(data, 0, data.Length);

            if (IsStringTerminatedWithNull(result, out index))
            {
                result = result.Substring(0, index);
            }
            return result;
        }

        private static bool IsStringTerminatedWithNull(string result, out int index)
        {
            return (-1 != (index = result.IndexOf('\0')));
        }

        public static byte ReadByteAsBCD(BinaryReader input, int length, int precision)
        {
            return (byte)ReadDecimalAsBCD(input, length, precision);
        }

        public static void WriteByteAsBCD(BinaryWriter output, byte value, int length, int precision)
        {
            WriteDecimalAsBCD(output, value, length, precision);
        }

        public static UInt16 ReadUInt16AsBCD(BinaryReader input, int length, int precision)
        {
            return (ushort)ReadDecimalAsBCD(input, length, precision);
        }

        public static int ReadInt32AsBCD(BinaryReader input,int length,int precision)
        {
            return (int)ReadDecimalAsBCD(input, length, precision);
        }

        public static void WriteInt32AsBCD(BinaryWriter output, int value, int length, int precision)
        {
            WriteDecimalAsBCD(output,value, length, precision);
        }


        public static void WriteUInt16AsBCD(BinaryWriter output, ushort value, int length, int precision)
        {
            WriteDecimalAsBCD(output, value,length, precision);
        }

        internal static decimal ReadDecimalAsBinaryLE(BinaryReader input, int length, int precision)
        {
            byte[] data = input.ReadBytes(length);
            decimal result = 0m;            
            for (int i = length - 1; i >= 0; i--) result = (result * 256) + data[i];
            return result * new decimal(1, 0, 0, false, (byte)precision);
        }

        public static void WriteDecimalAsBinaryLE(BinaryWriter output, decimal value, int length, int precision)
        {
            value = Decimal.Truncate(value / new decimal(1, 0, 0, false, (byte)precision));
            byte[] data = new byte[length];
            for (int i = 0; i < length; i++)
            {
                data[i] = (byte)(value % 256);
                value = Decimal.Truncate(value / 256);
            }
            output.Write(data);
        }

        public static ushort ReadUInt16AsBinaryBE(BinaryReader input, int length, int precision)
        {
            if (length == 0) length = 2;
            return (ushort)ReadUInt64AsBinaryBE(input, length, precision);
        }

        public static void WriteUInt16AsBinaryBE(BinaryWriter output, ushort value, int length, int precision)
        {
            if (length == 0) length = 2;
            WriteUInt64AsBinaryBE(output, value, length, precision);
        }

        public static ulong ReadUInt64AsBinaryBE(BinaryReader input, int length, int precision)
        {
            if (length == 0) length = 8;
            ulong accumulator = 0;
            for (var i = 0; i < length; i++) accumulator = (accumulator << 8) | input.ReadByte();
            return accumulator;
        }

        public static void WriteUInt64AsBinaryBE(BinaryWriter output, ulong value, int length, int precision)
        {
            if (length == 0) length = 8;            
            for (var i = length - 1; i >= 0; i--) output.Write((byte)((value >> (8 * i)) & 0xFF));
        }

        
    }
}
