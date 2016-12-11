using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Model.Egm;
using BallyTech.Gtm;

namespace BallyTech.QCom.Model.EventsListeners
{
    [GenerateICSerializable]
    public partial class TicketEventListener : EventListenerBase       
    {
        private TicketOutDevice TicketOutDevice
        {
            get { return EgmAdapter.TicketOutDevice; }
        }

        public override void Process(CashTicketPrinted @event)
        {
            Model.Egm.CabinetDevice.RaiseEvent(EgmEvent.CashTicketPrinted);
        }

        public override void Process(CashTicketOutRequest @event)
        {
            TicketOutDevice.HandleTicketPrintRequest(@event.TicketSerialNumber, @event.TicketAmount * QComCommon.MeterScaleFactor);
            Model.Egm.CabinetDevice.RaiseEvent(EgmEvent.CashTicketOutRequest);
        }

        public override void Process(CashTicketOutPrintFailure @event)
        {
            TicketOutDevice.TicketPrintResultReceived(false);
            Model.Egm.CabinetDevice.RaiseEvent(EgmEvent.CashTicketOutPrintFailure);
        }

        public override void Process(CashTicketOutPrintSuccessful @event)
        {
            TicketOutDevice.TicketPrintResultReceived(true);
            Model.Egm.CabinetDevice.RaiseEvent(EgmEvent.CashTicketOutPrintSuccessful);
        }

        public override void Process(CashTicketInRequest @event)
        {
            EgmAdapter.TicketInDevice.HandleTicketInserted(@event.TicketAuthorisationNumber);
            Model.Egm.CabinetDevice.RaiseEvent(EgmEvent.CashTicketInRequest);
        }

        


        
    }
}
