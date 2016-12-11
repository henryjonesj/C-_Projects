using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using System.Reflection;
using log4net;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class DoorStatusProcessor 
    {
        DoorKeyedCollection _Doors = new DoorKeyedCollection();
        private Cabinet _Cabinet;
        private static readonly ILog _Log = LogManager.GetLogger(typeof(DoorStatusProcessor));
        
        public DoorStatusProcessor(){}

        public DoorStatusProcessor (Cabinet cabinet)
        {
            _Cabinet = cabinet;
            _Doors.Add(new Door(SlotDoors.Cabinet,EgmEvent.SlotDoorOpened,EgmEvent.SlotDoorClosed,cabinet.IsSlotDoorOpen));
            _Doors.Add(new Door(SlotDoors.Drop, EgmEvent.DropDoorOpened, EgmEvent.DropDoorClosed, cabinet.IsDropDoorOpen));
            _Doors.Add(new Door(SlotDoors.CardCage, EgmEvent.CardCageOpened, EgmEvent.CardCageClosed, cabinet.IsCardCageOpen));
            _Doors.Add(new Door(SlotDoors.Belly, EgmEvent.BellyDoorOpened, EgmEvent.BellyDoorClosed, cabinet.IsBellyDoorOpen));
            _Doors.Add(new Door(SlotDoors.Cashbox, EgmEvent.CashboxDoorOpened, EgmEvent.CashboxDoorClosed, cabinet.IsCashboxDoorOpen));
            _Doors.Add(new Door(SlotDoors.TopBox, EgmEvent.AuxillaryDoorOpened, EgmEvent.AuxillaryDoorClosed, cabinet.IsAuxDoorOpen));
            _Doors.Add(new Door(SlotDoors.NoteAcceptor, EgmEvent.NoteAcceptorDoorOpened, EgmEvent.NoteAcceptorDoorClosed, cabinet.IsNoteAcceptorDoorOpen));
            _Doors.Add(new Door(SlotDoors.MechanicalMeter, EgmEvent.MechanicalMeterDoorOpened, EgmEvent.MechanicalMeterDoorClosed, cabinet.IsMechanicalMeterDoorOpen));
        }

        internal void Process(SlotDoors doorName, bool isOpened)
        {
            Door door = null;
            _Doors.TryGetValue(doorName, out door);

            if (door == null) return;

            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Door: {0}; IsOpened: {1}", doorName, isOpened);

            door.Property.Value = isOpened;
            _Cabinet.RaiseEvent(isOpened ? door.OpenEvent : door.CloseEvent);

        }

    }
}
