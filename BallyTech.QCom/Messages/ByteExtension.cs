using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace BallyTech.QCom.Messages
{
    public static class ByteExtension
    {
        public static bool IsValidFunctionCode(this byte functionCode)
        {
            try
            {
                return Enum.IsDefined(typeof(FunctionCodes), functionCode) && functionCode != (byte)FunctionCodes.None;
            }
            catch (Exception)
            {
                return false;
            }

        }


        public static byte GetByteOrDefault(this string strByte)
        {
            try
            {
                return Byte.Parse(strByte);
            }
            catch (Exception)
            {
                return default(Byte);
            }
        }


    }
}
