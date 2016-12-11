using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using log4net;
using BallyTech.Utility;
using BallyTech.QCom.Model.Egm.Devices;

namespace BallyTech.QCom.Model.Egm
{
    public enum JackpotType
    {
        None,
        LinkedProgressive,
        LargeWin,
        CancelledCredit,
        ResidualCancelledCredit
    }
    
    [GenerateICSerializable]
    public partial class JackpotDevice : Device
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(JackpotDevice));

        private PendingJackpot _HandpayPending = null;
        private bool _HaveReceivedMeters = false;
        private bool _IsJackpotReset = false;
        private Jackpot _Jackpot = new Jackpot();
        private bool _HasLinkStatusChanged = false;

        private bool AnyHandpayPending
        {
            get { return _HandpayPending != null; }
        }

        internal Jackpot GetJackpot(HandpayType handpayType)
        {
            if (handpayType == HandpayType.LargeWin)
                return (new LargeWin() { Model = this.Model });

            if (handpayType == HandpayType.CancelledCredits)
                return (new CancelCreditJackpot() { Model = this.Model });

            if (handpayType == HandpayType.ResidualCancelledCredits)
                return (new ResidualCancelCreditJackpot() { Model = this.Model });

            return _Jackpot;
        }



        public void HandlePendingHandpay(HandpayType handpayType, decimal amount)
        {
            if (AnyHandpayPending)
            {
                if (_Log.IsInfoEnabled)
                    _Log.InfoFormat("Resetting the pending handpay {0}", _HandpayPending);

                RequestedMetersReceived();
                if (!_HasLinkStatusChanged) _Jackpot.UpdateKeyOffDestination();

                HandleHandpayReset();
            }

            _Jackpot = GetJackpot(handpayType);
            _Jackpot.UpdateMeters(amount);

            _HandpayPending = new PendingJackpot()
            {
                HandpayType = _Jackpot.HandpayType,
                HandpayAmount = amount * QComCommon.MeterScaleFactor
            };

            Model.HandpayType = _Jackpot.HandpayType;
            Model.IsHandpayPending.Value = true;
            Model.Observers.HandpayPending(_HandpayPending);

            RequestMeters(MeterType.Jackpot);
        }

        public void LinkStatusChanged(LinkStatus linkStatus)
        {
            if (!AnyHandpayPending) return;

            _HasLinkStatusChanged = true;
        }

        public void HandleHandpayLockupCleared()
        {
            if (_HandpayPending == null || _IsJackpotReset) return;
            ProcessLockupCleared();
        }

        public void HandleHandpayLockupClearedFor(HandpayType handpayType)
        {
            if ((_HandpayPending == null) || !(_Jackpot.IsSameHandpayType(handpayType))) return;
            ProcessLockupCleared();
        }

        private void ProcessLockupCleared()
        {
            _Log.Info("Processing Lockup Cleared");
            if (!_IsJackpotReset) RequestMeters(MeterType.Jackpot);

            _IsJackpotReset = true;
            if (ShouldSendJackpotReset())            
                HandleHandpayReset();            
        }

        public void HandleCancelCreditCancelled()
        {
            if (_HandpayPending == null) return;

            _Log.Info("Cancelling Cancel Credit");

            Model.Observers.HandpayReset(KeyOffDestination.Credit, _HandpayPending.HandpayAmount);

        
        }

        public void HandleHandpayReset()
        {
            if (_HandpayPending == null) return;

            RequestedMetersReceived();

            _Log.Info("Sending Handpay reset");
            Model.Observers.HandpayReset(_Jackpot.KeyOffDestination, _HandpayPending.HandpayAmount);
            
            ResetJackpotVariables();
        }

        internal override void OnMetersReceived(SerializableList<MeterId> meterIdsReceived)
        {
            if (!AnyHandpayPending) return;
            
            _HaveReceivedMeters = _Jackpot.IsJackpotMeterUpdated(meterIdsReceived);
            _Log.InfoFormat("OnMetersReceived, Have received the exact meter difference = {0}", _HaveReceivedMeters);
            if (_HaveReceivedMeters) RequestedMetersReceived();

            if (ShouldSendJackpotReset()) HandleHandpayReset();            
        }

        internal override void MeterRequestTimerExpired()
        {            
            if (!AnyHandpayPending) return;

            _Log.Info("On meter expiry");
            if (_IsJackpotReset)
            {
                _Jackpot.UpdateKeyOffDestination();
                HandleHandpayReset();
                return;
            }
            _Log.Info("Requesting for Meters at MeterRequestTimerExpired");
            RequestMeters(MeterType.Jackpot);
        }

        internal override void ForceReset()
        {
            HandleHandpayReset();
            base.ForceReset();
        }

        private bool ShouldSendJackpotReset()
        {
            return (_HaveReceivedMeters && _IsJackpotReset);
        }

        private void ResetJackpotVariables()
        {
            _HandpayPending = null;
            _IsJackpotReset = false;
            _HaveReceivedMeters = false;
            _HasLinkStatusChanged = false;
            Model.IsHandpayPending.Value = false;
            _Jackpot = new Jackpot();
        }

        public void RaiseEvent(EgmEvent egmEvent)
        {
            Model.Observers.EgmEventRaised(egmEvent);
        }
    }
}
