using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using log4net;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class StandAloneProgressiveJackpotHandler : Device
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (StandAloneProgressiveJackpotHandler));

        private PendingJackpot _PendingJackpot = null;

        private bool AnyPendingJackpotAvailable
        {
            get { return _PendingJackpot != null; }
        }

        public void HandleProgressiveHit(HandpayType handpayType,decimal amount)
        {
            if (AnyPendingJackpotAvailable && _PendingJackpot.IsSame(handpayType)) return;

            if (AnyPendingJackpotAvailable) ProcessPendingJackpot();
                
            _PendingJackpot = new PendingJackpot() {HandpayAmount = amount, HandpayType = handpayType};

            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Pending Handpay Info: {0}", _PendingJackpot);

            Model.Observers.EgmEventRaised(EgmEvent.EgmSapAwardHit);

            Model.EgmAdapter.FetchProgressiveMeters();
            RequestMeters(MeterType.Jackpot);
            NotifyOnGameIdle();
        }
        
        internal override void OnMetersReceived(SerializableList<MeterId> meterIdsReceived)
        {
            if (!(meterIdsReceived.Contains(MeterId.SapWins, MeterId.Wins))) return;
            ProcessPendingJackpot();   
        }

        internal override void OnGameIdle()
        {
            if (_Log.IsInfoEnabled) _Log.Info("Received Game Idle");
            ProcessPendingJackpot();
        }

        internal override void ForceReset()
        {
            ProcessPendingJackpot();
            base.ForceReset();
        }

        private void ProcessPendingJackpot()
        {
            if(!AnyPendingJackpotAvailable) return;

            if (_Log.IsInfoEnabled) _Log.Info("Resetting the Pending Handpay");                
            Model.Observers.MachinePaidJackpot(_PendingJackpot.HandpayType, _PendingJackpot.HandpayAmount);
            _PendingJackpot = null;

            this.AwaitingForGameIdle = false;
            RequestedMetersReceived();
        }
    }
}
