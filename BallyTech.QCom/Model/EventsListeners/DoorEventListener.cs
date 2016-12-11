using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using BallyTech.Gtm;
using BallyTech.QCom.Model.Egm;

namespace BallyTech.QCom.Model.EventsListeners
{
    [GenerateICSerializable]
    public partial class DoorEventListener : EventListenerBase
    {
        private Cabinet CabinetDevice
        {
            get { return EgmAdapter.CabinetDevice ; }
        }

        public override void Process(DoorOpenEvents response)
        {
            CabinetDevice.DoorStatusChanged(DoorEventsMapping.GetDoorOpenEvent(response.EventCode), true);
        }

        public override void Process(DoorCloseEvents response)
        {
            CabinetDevice.DoorStatusChanged(DoorEventsMapping.GetDoorCloseEvent(response.EventCode), false);
        }
    }
}
