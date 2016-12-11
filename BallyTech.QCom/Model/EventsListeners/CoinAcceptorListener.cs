using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Model.Egm;
using BallyTech.QCom.Messages;
using BallyTech.Gtm;

namespace BallyTech.QCom.Model.EventsListeners
{
    [GenerateICSerializable]
    public partial class CoinAcceptorListener : EventListenerBase
    {
        private CoinAcceptor CoinAcceptor
        {
            get { return EgmAdapter.CoinAcceptorDevice; }
        }

        public override void Process(CoinFaultEvents Event)
        {
            var egmEvent = CoinFaultEventMapping.GetFault(Event.EventCode);
            if (!egmEvent.HasValue) return;

            CoinAcceptor.Process(egmEvent.Value);
        }
    }
}
