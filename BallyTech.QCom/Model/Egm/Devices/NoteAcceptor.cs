using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.Utility;
using BallyTech.Utility.Serialization;
using log4net;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class NoteAcceptor : Device, INoteAcceptor
    {
        private SerializableDictionary<EgmEvent, BoundSlot<bool>> _EventMap =
            new SerializableDictionary<EgmEvent, BoundSlot<bool>>();

         private static readonly ILog _Log = LogManager.GetLogger(typeof (NoteAcceptor));

        
        public NoteAcceptor()
        {
            Initialize();
            _EventMap.Add(EgmEvent.BillJam, IsJammed);
            _EventMap.Add(EgmEvent.NoteAcceptorJammed, IsJammed);
            _EventMap.Add(EgmEvent.BillAcceptorHardwareFailure, IsFaultCondition);
            _EventMap.Add(EgmEvent.CashboxFull, IsFull);
            _EventMap.Add(EgmEvent.CashboxRemoved, IsCashboxRemoved);
            _EventMap.Add(EgmEvent.DiverterMalfunction, IsFaultCondition);
        }

        private void Initialize()
        {
            IsFaultCondition = new BoundSlot<bool>();
            IsJammed = new BoundSlot<bool>();
            IsNearFull = new BoundSlot<bool>();
            IsFull = new BoundSlot<bool>();
            IsCashboxRemoved = new BoundSlot<bool>();
        }


        #region INoteAcceptor Members

        public string FirmwareID { get; private set; }
        
        public BoundSlot<bool> IsEnabled
        {
            get { return new BoundSlot<bool>() { Value = true }; } // to do
        }

        public BoundSlot<bool> IsActive
        {
            get { return new BoundSlot<bool>() { Value = false }; }
        }

        public BoundSlot<bool> IsFaultCondition { get; private set; }

        public BoundSlot<bool> IsJammed { get; private set; }

        public BoundSlot<bool> IsNearFull { get; private set; }

        public BoundSlot<bool> IsFull { get; private set; }

        public BoundSlot<bool> IsCashboxDoorOpen
        {
            get { return Model.EgmAdapter.CabinetDevice.IsCashboxDoorOpen; }
        }

        public BoundSlot<bool> IsCashboxRemoved { get; private set; }

        internal bool IsCreditInputEnabled = true;

        public void SetState(bool enableState)
        {
            EdgeDetector<bool> noteAcceptor = new EdgeDetector<bool>(IsCreditInputEnabled, enableState);

            if (noteAcceptor.Falling() || noteAcceptor.Rising())
            {
                _Log.WarnFormat("Credit Input :{0} ", enableState ? "enabled" : "disabled");
                Model.EgmRequestHandler.SetNoteAcceptorState(enableState);
            }

            IsCreditInputEnabled = enableState;
        }

        #endregion


        public void Process(EgmEvent egmEvent)
        {
            Model.Observers.EgmEventRaised(egmEvent);

            if (!(_EventMap.ContainsKey(egmEvent))) return;

            var eventInfo = _EventMap[egmEvent];
            eventInfo.Value = true;
        }


        #region INoteAcceptor Members


        public void Configure(IDenominationConfiguration configuration)
        {
            if (configuration != null)
            {
                DenominationConfiguration = configuration;
                Model.EgmRequestHandler.Configure(configuration);
            }
        }

        #endregion

        #region INoteAcceptor Members


        public IDenominationConfiguration DenominationConfiguration { get; set; }

        #endregion
    }
}
