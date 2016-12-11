using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Model.Egm;
using BallyTech.Gtm;
using BallyTech.QCom.Messages;
using BallyTech.Utility.Configuration;
using log4net;
using BallyTech.QCom.Model.Builders;

namespace BallyTech.QCom.Model.Handlers
{
    [GenerateICSerializable]
    public partial class ElectronicCreditTransferHandler : IFundsTransferHandler
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (ElectronicCreditTransferHandler));

        private QComTransaction _PendingTransaction = null;

        private QComModel _Model = null;
        [AutoWire(Name = "QComModel")]
        public QComModel Model
        {
            get { return _Model; }
            set { _Model = value; }
        }

        #region IFundsTransferAdapter Members

        public void SetState(bool isEnabled)
        {            
            if (_Log.IsInfoEnabled) 
                _Log.InfoFormat("Setting the cashless mode to : {0}", isEnabled);

            var ectToEgmPoll = new EctToEgmPoll()
                               {
                                   EctToEgmPollFlag = new EctToEgmPollFlags()
                                                          {
                                                              CashlessMode = isEnabled,
                                                               EctSourceId = SourceIds.CashlessGaming
                                                          },
                               };

            Model.EctToEgmPollDispatcher.Dispatch(ectToEgmPoll);
        }

        public void Initiate(IFundsTransferAuthorization fundsTransferRequest)
        {            
            switch (fundsTransferRequest.Destination)
            {
                case TransferDestination.WithdrawToCreditMeter:
                    DoWithdrawalTransfer(fundsTransferRequest);
                    return;
                case TransferDestination.DepositToHost:
                    DoDepositTransfer(fundsTransferRequest);
                    return;
            }
        }

        private bool IsCancelTransferRequest(IFundsTransferAuthorization authorization)
        {
            if (authorization == null) return false;

            return (authorization.ResultCode != FundsTransferAuthorizationResultCode.Success);
        }

        internal void CancelPendingTransfer(bool isForced)
        {
            if (_PendingTransaction == null)
            {
                CancelEctLockup(isForced);
                return;
            }

            if (_Log.IsWarnEnabled) _Log.Warn("Cancelling the pending transaction");

            _PendingTransaction.Cancel(isForced);
            ResetTransaction();
                
        }


        private void CancelEctLockup(bool isForced)
        {
            if (_Log.IsWarnEnabled)
                _Log.Warn(
                    "Pending transfer is not available.Egm might have still be locked up even after committing the transaction");

            var transaction = new DepositTransaction(this);
            transaction.Cancel(isForced);
        }

        private static bool IsAWithdrawalTransfer(IFundsTransferAuthorization authorization)
        {
            if (authorization == null) return false;
            if (authorization.InitiatingRequest == null) return false;

            return authorization.Destination == TransferDestination.WithdrawToCreditMeter;
        }

        public void Commit(IFundsTransferAuthorization transferAuthorization)
        {
            if (IsAWithdrawalTransfer(transferAuthorization)) return;


            if (IsCancelTransferRequest(transferAuthorization))
            {
                CancelPendingTransfer((transferAuthorization.Destination == TransferDestination.Handpay));
                return;
            }

            CompletePendingTransfer(transferAuthorization);
        }

        private void CompletePendingTransfer(IFundsTransferAuthorization transferAuthorization)
        {
           //Deposit transaction only have commit phase...
            if (_PendingTransaction == null)
                _PendingTransaction = new DepositTransaction(this);

            _PendingTransaction.Execute(transferAuthorization);
        }

        #endregion

        private void DoWithdrawalTransfer(IFundsTransferAuthorization authorization)
        {
            _PendingTransaction = new WithdrawalTransaction(this);
            _PendingTransaction.Execute(authorization);
            ResetTransaction();
        }

        private void DoDepositTransfer(IFundsTransferAuthorization authorization)
        {
            _PendingTransaction = new DepositTransaction(this);
            _PendingTransaction.Initiate(authorization);
        }

        private void ResetTransaction()
        {
            _PendingTransaction = null;
        }

        internal void RequestCashlessMeter()
        {            
            var cabinet = _Model.Egm.CabinetDevice;

            var meterRequestPoll = MeterRequestBuilder.RequestFor(new MeterType[] { MeterType.Cashless },
                                                                                  Model.Egm.CurrentGame);

            if (cabinet.IsMachineEnabled)
                meterRequestPoll.MaintenanceFlagStatus |= MaintenanceFlagStatus.MachineEnableFlag;

            Model.SendPoll(meterRequestPoll);
        }

        #region IFundsTransferHandler Members


        public bool IsSupported
        {
            get { return true; }
        }

        #endregion
    }
}
