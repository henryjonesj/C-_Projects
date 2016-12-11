using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using BallyTech.Utility;
using log4net;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class HopperDevice : Device, IHopper
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(HopperDevice));
        private SerializableDictionary<EgmEvent, BoundSlot<bool>> _HopperFaults =
            new SerializableDictionary<EgmEvent, BoundSlot<bool>>();

        public HopperDevice()
        {
            _HopperFaults.Add(EgmEvent.HopperEmpty, _IsFaultCondition);
            _HopperFaults.Add(EgmEvent.HopperJammed, _IsFaultCondition);
            _HopperFaults.Add(EgmEvent.HopperRunaway, _IsFaultCondition);
            _HopperFaults.Add(EgmEvent.HopperLow, _IsFaultCondition);
        }

        #region IHopper Members

        public bool IsEnabled
        {
            get;
            set;
        }

        private BoundSlot<bool> _IsFaultCondition = new BoundSlot<bool>();
        
        public bool IsFaultCondition
        {
            get { return _IsFaultCondition.Value; }            
        }

        public void SetState(bool enableState)
        {
            
        }

        #endregion

        internal override void ClearFault()
        {
            _IsFaultCondition.Value = false;
        }

        public void Process(EgmEvent egmEvent)
        {
            Model.Observers.EgmEventRaised(egmEvent);

            if (!(_HopperFaults.ContainsKey(egmEvent))) return;

            var eventInfo = _HopperFaults[egmEvent];
            eventInfo.Value = true;
        }

        public void ProcessHopperRefillEvent()
        {
            if (_Log.IsInfoEnabled)
                _Log.Info("Process Hopper Refill Event");
            Model.Observers.EgmEventRaised(EgmEvent.HopperRefillRecorded);
            //AwaitingForUnsolicitedMeters();
        }

        //internal override void OnMetersReceived(SerializableList<MeterId> meterIdsReceived)
        //{
        //    if (meterIdsReceived.Contains(MeterId.HopperRefill))
        //    {
        //        if (_Log.IsInfoEnabled)
        //            _Log.Info("Hopper Refill Meter Received");
        //        RequestedMetersReceived();
        //        Model.Observers.EgmEventRaised(EgmEvent.HopperRefillRecorded);
        //    }
        //}

        //internal override void MeterRequestTimerExpired()
        //{
        //    if (_Log.IsInfoEnabled)
        //        _Log.Info("Hopper Refill Meter Request Timer Expired");
        //    Model.Observers.EgmEventRaised(EgmEvent.HopperRefillRecorded);
        //}

    }
}
