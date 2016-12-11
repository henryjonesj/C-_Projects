using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace BallyTech.QCom.Model
{
    public class PollSequenceNumberSupplier
    {
        internal static byte SupplyNext(byte current)
        {
            return (byte)(current == byte.MaxValue ? 0 : ++current);
        }

    }
}
