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
    public partial class Device
    {
        public EgmModel Model { get; set; }

        private static readonly ILog _Log = LogManager.GetLogger(typeof(Device));

        internal bool AwaitingForMeters { get; private set; }

        internal protected bool AwaitingForRomSignatureVerification { get; protected set; }

        internal protected bool AwaitingForGameIdle { get; protected set; }

        internal virtual bool IsAnyTransferInProgress 
        {
            get { return false; }
        }


        internal virtual void ClearFault()
        {
            
        }

        internal virtual void OnMetersReceived(SerializableList<MeterId> meterIdsReceived)
        {
            
        }

        internal virtual void OnGameIdle()
        {
            
        }

        internal virtual void MeterRequestTimerExpired()
        {
            
        }

        internal virtual void MeterRequestSplitIntervalSurpassed()
        {
            
        }

        internal void NotifyRomSignatureVerificationInitiated()
        {
            _Log.Info("Stopping the Meter Request Expiry Timer as Rom Signature was initiated");

            this.AwaitingForRomSignatureVerification = true;
            
            Model.MeterRequestExpiryTimer.Stop();
        }

        internal virtual void NotifyRomSignatureVerificationComplete()
        {
            _Log.Info("Resuming the Meter Request Expiry Timer as Rom Signature is Complete");

           Model.MeterRequestExpiryTimer.Start();

           this.AwaitingForRomSignatureVerification = false;
        }

        internal virtual void NotifyRomSignatureVerificationFailure()
        {

        }

        internal void RequestMeters(params MeterType[] meterTypes)
        {
            this.AwaitingForMeters = true;
            Model.RequestMeters(meterTypes);
            Model.MeterRequestExpiryTimer.Start();
        }

        internal void RequestedMetersReceived()
        {
            this.AwaitingForMeters = false;
            if (Model.AwaitingForMeters) return;

            Model.MeterRequestExpiryTimer.Stop();
        }

        protected void AwaitingForUnsolicitedMeters()
        {
            this.AwaitingForMeters = true;
            Model.MeterRequestExpiryTimer.Start();
        }

        protected void NotifyOnGameIdle()
        {
            Model.EgmAdapter.NotifyOnGameIdle();
            this.AwaitingForGameIdle = true;            
        }

        internal virtual void ForceReset()
        {
            this.AwaitingForGameIdle = false;
            RequestedMetersReceived();
        }

        internal virtual void OnLinkStatusChanged(LinkStatus linkStatus)
        {
            
        }

    }
}
