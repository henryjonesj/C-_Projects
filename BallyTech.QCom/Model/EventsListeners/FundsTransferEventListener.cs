using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.Egm;
using BallyTech.Gtm;
using log4net;

namespace BallyTech.QCom.Model.EventsListeners
{
    [GenerateICSerializable]
    public partial class FundsTransferEventListener : EventListenerBase
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(FundsTransferEventListener));

        private void RaiseEvent()
        {
            Model.Egm.CabinetDevice.RaiseEvent(EgmEvent.ECTFromEGMRequest);
        }

        public override void Process(ECTFromEGMNew response)
        {
            Model.EcTFromEgmInProgress = true;
            EgmAdapter.RequestFullDeposit(response.Amount * QComCommon.MeterScaleFactor);
            RaiseEvent();
            EgmAdapter.EctFromEgmMonitor.UpdateTransactionAmount(response.Amount * QComCommon.MeterScaleFactor);
        }

        public override void Process(ECTfromEGMOld response)
        {
            if (response.Amount == 0)
            {
                _Log.Info("Received an ECTFromEGM event with zero amount, hence resetting lockup.");
                Model.SendPoll(new EctLockupResetPoll() { TransferStatus = EctFromEgmStatus.TransferSuccessful });
            }
            else
            {
                Model.EcTFromEgmInProgress = true;
                EgmAdapter.RequestFullDeposit(response.Amount * QComCommon.MeterScaleFactor);
            }
            RaiseEvent();
            EgmAdapter.EctFromEgmMonitor.UpdateTransactionAmount(response.Amount * QComCommon.MeterScaleFactor);
        }
    }
}
