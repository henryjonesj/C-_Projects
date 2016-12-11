using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace BallyTech.QCom.Messages
{
    partial class SeekEgmBroadcastPoll
    {

        public override Message AppendDataLinkLayerWithPollAddress(byte pollAddress)
        {
            return new Message()
                       {
                           Header = new DataLinkLayer() {Address = 0xFC },
                           ApplicationData = this
                       };
        }

    }
}
