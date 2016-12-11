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
    public partial class NoteAcceptorEventListener : EventListenerBase
    {
        private NoteAcceptor NoteAcceptor
        {
            get { return EgmAdapter.NoteAcceptorDevice; }
        }

        public override void Process(NoteAcceptorFaultEvents response)
        {
            var egmEvent = NoteAcceptorEventFaultMapping.GetFault(response.EventCode);
            if (!egmEvent.HasValue) return;

            NoteAcceptor.Process(egmEvent.Value);
        }
    }
}
