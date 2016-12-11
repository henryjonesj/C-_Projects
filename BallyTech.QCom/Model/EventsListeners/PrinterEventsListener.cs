using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Model.Egm;

namespace BallyTech.QCom.Model.EventsListeners
{
    [GenerateICSerializable]
    public partial class PrinterEventsListener : EventListenerBase
    {

        private Printer Printer
        {
            get { return EgmAdapter.Printer; }
        }


        public override void Process(PrinterFaultEvents @event)
        {
            var egmEvent = PrinterFaultEventsMapping.GetFault(@event.EventCode);
            if(!egmEvent.HasValue) return;

            Printer.Process(egmEvent.Value);
        }

    }
}
