using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace BallyTech.QCom.Messages
{
    partial class GeneralStatusPoll
    {
        public override bool IsGeneralPoll { get { return true; } }
    }
}
