using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace BallyTech.QCom.Messages
{
    partial class EctToEgmPoll
    {
        internal bool IsUtilizedtoSetCashlessMode
        {
            get { return (this.EAmount == 0 && this.OCAmount == 0); }
        }

        internal bool IsDelieved { get; private set; } 

        internal void Delivered()
        {
            this.IsDelieved = true;
        }



    }
}
