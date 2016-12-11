using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.Utility;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class Printer : Device, IPrinter
    {
        private SerializableDictionary<EgmEvent,BoundSlot<bool>> _EventMap = new SerializableDictionary<EgmEvent, BoundSlot<bool>>();

        public Printer()
        {
            Initialize();

            _EventMap.Add(EgmEvent.PrinterPaperLow,IsPaperLow);
            _EventMap.Add(EgmEvent.PrinterPaperOut,IsPaperOut);
            _EventMap.Add(EgmEvent.PrinterCarriageJam,IsJammed);
            _EventMap.Add(EgmEvent.PrinterCommunicationError,IsFaultCondition);
            _EventMap.Add(EgmEvent.PrinterRequiresNewRibbon, IsFaultCondition);

        }

        private void Initialize()
        {
            IsActive = new BoundSlot<bool>();
            IsJammed = new BoundSlot<bool>();
            IsPaperLow = new BoundSlot<bool>();
            IsPaperOut = new BoundSlot<bool>();
            IsNonPaperConsumable = new BoundSlot<bool>();
            IsFaultCondition = new OrGate(IsJammed, IsPaperLow, IsPaperOut);
        }


        #region IPrinter Members

        public BoundSlot<bool> IsActive { get; private set; }

        public BoundSlot<bool> IsFaultCondition { get; private set; }

        public BoundSlot<bool> IsJammed { get; private set; }

        public BoundSlot<bool> IsPaperLow { get; private set; }

        public BoundSlot<bool> IsPaperOut { get; private set; }

        public BoundSlot<bool> IsNonPaperConsumable { get; private set; }

        #endregion


        public void Process(EgmEvent egmEvent)
        {
            UpdateState(egmEvent,true);
            Model.Observers.EgmEventRaised(egmEvent);
            UpdateState(egmEvent, false);
        }

        private void UpdateState(EgmEvent egmEvent, bool state)
        {
            if (!(_EventMap.ContainsKey(egmEvent))) return;

            var eventInfo = _EventMap[egmEvent];
            eventInfo.Value = state;
        }
    }
}
