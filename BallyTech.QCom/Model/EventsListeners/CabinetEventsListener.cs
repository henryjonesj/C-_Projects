using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Model.Egm;
using BallyTech.QCom.Messages;
using BallyTech.Gtm;
using BallyTech.QCom.Model.Specifications;

namespace BallyTech.QCom.Model.EventsListeners
{
    [GenerateICSerializable]
    public partial class CabinetEventsListener : EventListenerBase
    {       
        private Cabinet Cabinet
        {
            get { return EgmAdapter.CabinetDevice; }
        }

        public override void Process(Event Event)
        {
            var egmEvent = CabinetEventMapping.GetFault(Event.EventCode);
            if (!egmEvent.HasValue) return;

            Cabinet.Process(egmEvent.Value);
        }

        public override void Process(NewGameSelected Event)
        {
            Cabinet.NewGameSelected(Event.GameVersionNumber, Event.GameVariationNumber);
        }

        public override void Process(GameVariationEnabled Event)
        {
            Cabinet.GameVariationChanged(Event.GameVersionNumber, Event.GameVariationNumber);
        }

        public override void Process(DenominationChanged Event)
        {
            Cabinet.GameDenominationChanged(Event.GameDenomination);
            Process(Event as Event);
        }

        public override void Process(NVRAMCleared @event)
        {
            QComResponseSpecification _SerialNumSpecification = Model.SpecificationFactory.GetSpecification(FunctionCodes.EgmConfigurationResponse);

            var extendedData = new ExtendedEgmEventData()
            {
                ManufacturerId = @event.EgmManufacturerId.ToString(),
                GameSerialNumber = @event.EgmSerialNumber.ToString()
            };

            if (!_SerialNumSpecification.IsSatisfiedBy(@event.EgmSerialNumber, @event.EgmManufacturerId))
            {               
                Model.Egm.ExtendedEventData = extendedData;

                Model.Egm.ReportEvent(EgmEvent.InvalidSerialNumber);
            }

            Model.Egm.ExtendedEventData = @event.GetExtendedEgmEventData();

            Model.Egm.CabinetDevice.Process(CabinetEventMapping.GetFault(@event.EventCode).Value);

        }
    }
}
