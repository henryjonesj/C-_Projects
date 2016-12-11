using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Model.Egm;
using BallyTech.QCom.Messages;

namespace BallyTech.QCom.Model.EventsListeners
{
    [GenerateICSerializable]
    public partial class HopperEventListener : EventListenerBase
    {
        private HopperDevice Hopper
        {
            get { return EgmAdapter.HopperDevice; }
        }

        public override void Process(HopperFaultEvents faultEvent)
        {
            var egmEvent = HopperEventFaultMapping.GetFault(faultEvent.EventCode);
            if (!egmEvent.HasValue) return;

            Hopper.Process(egmEvent.Value);
        }

        public override void Process(HopperRefillRecordedEvent @event)
        {
            Hopper.ProcessHopperRefillEvent();
        }
    }
}
