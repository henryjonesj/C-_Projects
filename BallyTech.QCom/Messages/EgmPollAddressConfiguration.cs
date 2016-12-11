using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace BallyTech.QCom.Messages
{
    partial class EgmPollAddressConfiguration
    {
        internal EgmPollAddressConfiguration WithAddress(byte address)
        {
            this.PollAddress = address;
            return this;
        }

    }
}
