using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BallyTech.Utility;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class DoorKeyedCollection : SerializableKeyedCollection<SlotDoors,Door>
    {
        protected override SlotDoors GetKeyForItem(Door item)
        {
            return item.Name;
        }
    }


    [GenerateICSerializable]
    public partial class Door
    {
        public Door()
        {
        }

        public Door(SlotDoors door, EgmEvent doorOpenEvent, EgmEvent doorClosedEvent, BoundSlot<bool> property)
        {
            Name = door;
            OpenEvent = doorOpenEvent;
            CloseEvent = doorClosedEvent;
            Property = property;
        }

        internal SlotDoors Name { get; private set; }
        internal EgmEvent OpenEvent { get; private set; }
        internal EgmEvent CloseEvent { get; private set; }
        internal BoundSlot<bool> Property { get; private set; }

    }
}
