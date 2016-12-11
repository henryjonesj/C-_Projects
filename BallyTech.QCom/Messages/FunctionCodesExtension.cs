using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace BallyTech.QCom.Messages
{
    public static class FunctionCodesExtension
    {
        public static bool IsValidFunctionCode(this byte functionCode)
        {
            try
            {
                return Enum.IsDefined(typeof (FunctionCodes), functionCode);
            }
            catch (Exception)
            {
                return false;
            }

        }

    }
}
