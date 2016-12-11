using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace BallyTech.QCom.Messages
{
    public partial class BetMetersResponse
    {
        public UInt16 BetCategoryCount
        {
            get { return Convert.ToUInt16(GameBetFactorA * GameBetFactorB); }
        }
    }
}
