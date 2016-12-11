using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.Builders;

namespace BallyTech.QCom
{
    internal class PollCycle
    {
        internal ApplicationMessage Poll { get; private set; }
        internal ApplicationMessage Broadcast { get; private set; }

        internal static PollCycle CreateWith(ApplicationMessage applicationMessage)
        {
            return new PollCycle(applicationMessage);

        }

        private PollCycle(ApplicationMessage applicationMessage)
        {
            Poll = applicationMessage.IsBroadcast ? new GeneralStatusPoll() : applicationMessage;
            Broadcast = applicationMessage.IsBroadcast ? applicationMessage : DateTimeBroadcastBuilder.Build();
        }


    }
}
