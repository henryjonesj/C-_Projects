using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using log4net;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.Builders;

namespace BallyTech.QCom.Model.Handlers
{
    [GenerateICSerializable]
    public partial class DepositTransaction : QComTransaction
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(DepositTransaction));

        private bool _IsWaitingForGameIdle = false;        

        private const ushort LockRetryLimit = 2;
        private ushort _LockRetryCount = 0;


        public DepositTransaction() { }

        public DepositTransaction(ElectronicCreditTransferHandler parent)
        {
            Parent = parent;
            Model = Parent.Model;
        
            Model.OnGameIdle += GameIdleReceived;           
        }


        private bool PreConditionsAreSatisfied()
        {
            if (!IsEgmCreditsAvailable())
            {
                NotifyLockupFailedAndResetTransfer();
                return false;
            }

            return true;
        }

        internal override void Initiate(IFundsTransferAuthorization authorization)
        {
            if (!PreConditionsAreSatisfied()) return;
            LockupEgmIfPreConditionsAreSatisfied();
        }


        internal override void Execute(IFundsTransferAuthorization authorization)
        {
            if (_Log.IsInfoEnabled) _Log.InfoFormat("Resetting the Ect lockup");

            //Would have disabled Egm after lockup
            ChangeMachineModeIfNecessary(true);

            Model.SendPoll(new EctLockupResetPoll() { TransferStatus = EctFromEgmStatus.TransferSuccessful });
            //Parent.RequestCashlessMeter();

            ResetTransfer();
        }

        private void LockupEgmIfPreConditionsAreSatisfied()
        {
            if (PreConditionsAreSatisfied()) 
                RequestForEctLockup();                
        }

        private void RequestForEctLockup()
        {
            if (_Log.IsDebugEnabled) _Log.Debug("Deposit initiated by host");
            Model.SendPoll(new EctFromEgmLockupRequestPoll());

            ChangeMachineModeIfNecessary(false);
            WaitForGameIdleOrEctLockup();
        }

        private void NotifyLockupFailedAndResetTransfer()
        {
            Model.Egm.OnLockupFailed();            
            ResetTransfer();

            Parent.CancelPendingTransfer(Model.AnonymousForceClearFundsRequired);
        }

        private void WaitForGameIdleOrEctLockup()
        {
            if (_Log.IsInfoEnabled) _Log.Info("Waiting for game idle or lockup");

            _IsWaitingForGameIdle = true;
            _LockRetryCount++;
            Model.IdleModeCounter.Reset();
        }

        private void ChangeMachineModeIfNecessary(bool isEnabled)
        {
            if (Model.ShouldDisableGameOnDeposit)
                Model.Egm.SetDepositLockState(!isEnabled);
        }

        private bool IsEgmCreditsAvailable()
        {
            var meters = Model.Egm.GetMeters();
            return meters.GetCreditAmount(null).DangerousGetSignedValue() > 0;
        }

        private void GameIdleReceived()
        {
            if (!(_IsWaitingForGameIdle)) return;            

            _IsWaitingForGameIdle = false;

            if (IsRetryLimitExceeded())
            {
                HandleLockupFailed();
                return;
            }

            if (_Log.IsInfoEnabled) _Log.Info("Retrying for Ect lockup");
            LockupEgmIfPreConditionsAreSatisfied();
        }

        private bool IsRetryLimitExceeded()
        {
            return _LockRetryCount >= LockRetryLimit;
        }

        private void HandleLockupFailed()
        {
            if (_Log.IsInfoEnabled) _Log.Info("Lockup failed. Hence forcing Egm out of cashless mode and sending Cashout request");

            Model.SpamHandler.Send("Cashless Transfer Failed");

            if (Model.AnonymousForceClearFundsRequired) ForceEgmToCancelledCredit();

            NotifyLockupFailedAndResetTransfer();
            Model.SpamHandler.Clear();
        }

        private void ForceEgmToCancelledCredit()
        {            
            Parent.SetState(false);

            if (Model.State.LinkStatus == LinkStatus.Disconnected) return;

            if (_Log.IsInfoEnabled) _Log.Info("Forcing Egm to cash out");                
            Model.SendPoll(new CashOutRequestPoll());
        }

        private void ResetTransfer()
        {
            if (_Log.IsInfoEnabled)
                _Log.Info("Resetting the current transfer");

            _LockRetryCount = 0;
            _IsWaitingForGameIdle = false;            
            
            Model.OnGameIdle -= GameIdleReceived;
        }


        private bool CanCancelTransfer(bool isForced)
        {
            return isForced || Model.AnonymousForceClearFundsRequired;
        }


        internal override void Cancel(bool isForced)
        {
            if (!CanCancelTransfer(isForced))
            {
                if (_Log.IsWarnEnabled)
                    _Log.WarnFormat(
                        "Cannot cancel the transfer as Force Cancel: {0}, Anonymous Clear Funds Required: {1}", isForced,
                        Model.AnonymousForceClearFundsRequired);

                return;
            }

            CancelTransfer();
        }

        private void CancelTransfer()
        {
            if (!Model.EcTFromEgmInProgress)
                ForceEgmToCancelledCredit();
            else
            {
                if (_Log.IsInfoEnabled) _Log.Info("Resetting the Ect lockup");
                Model.SendPoll(new EctLockupResetPoll());
            }

            ResetTransfer();
        }
    }
}
