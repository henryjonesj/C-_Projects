using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using BallyTech.QCom.Messages;

namespace BallyTech.QCom.Model.EventsListeners
{
    public static class DoorEventsMapping
    {
        private static Dictionary<EventCodes, SlotDoors> _DoorOpenEventMap = new Dictionary<EventCodes, SlotDoors>();
        private static Dictionary<EventCodes, SlotDoors> _DoorCloseEventMap = new Dictionary<EventCodes, SlotDoors>();

        static DoorEventsMapping()
        {
            _DoorOpenEventMap.Add(EventCodes.EgmMainDoorOpened, SlotDoors.Cabinet);
            _DoorCloseEventMap.Add(EventCodes.EgmMainDoorClosed, SlotDoors.Cabinet);
            _DoorOpenEventMap.Add(EventCodes.EgmCashBoxDoorOpened, SlotDoors.Cashbox);
            _DoorCloseEventMap.Add(EventCodes.EgmCashBoxDoorClosed, SlotDoors.Cashbox);
            _DoorOpenEventMap.Add(EventCodes.EgmProcessorDoorOpened, SlotDoors.CardCage);
            _DoorCloseEventMap.Add(EventCodes.EgmProcessorDoorClosed, SlotDoors.CardCage);
            _DoorOpenEventMap.Add(EventCodes.EgmBellyPanelDoorOpened, SlotDoors.Belly);
            _DoorCloseEventMap.Add(EventCodes.EgmBellyPanelDoorClosed, SlotDoors.Belly);
            _DoorOpenEventMap.Add(EventCodes.EgmNoteAcceptorDoorOpened, SlotDoors.NoteAcceptor);
            _DoorCloseEventMap.Add(EventCodes.EgmNoteAcceptorDoorClosed, SlotDoors.NoteAcceptor);
            _DoorOpenEventMap.Add(EventCodes.EgmMechanicalMeterDoorOpened, SlotDoors.MechanicalMeter);
            _DoorCloseEventMap.Add(EventCodes.EgmMechanicalMeterDoorClosed, SlotDoors.MechanicalMeter);
            _DoorOpenEventMap.Add(EventCodes.EgmTopBoxDoorOpened, SlotDoors.TopBox);
            _DoorCloseEventMap.Add(EventCodes.EgmTopBoxDoorClosed, SlotDoors.TopBox);
        }

        public static SlotDoors GetDoorOpenEvent(EventCodes eventCode)
        {
            return _DoorOpenEventMap.ContainsKey(eventCode) ? _DoorOpenEventMap[eventCode] : SlotDoors.Cabinet;
        }

        public static SlotDoors GetDoorCloseEvent(EventCodes eventCode)
        {
            return _DoorCloseEventMap.ContainsKey(eventCode) ? _DoorCloseEventMap[eventCode] : SlotDoors.Cabinet;
        }
    }
}
