using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.Gtm.Core;
using BallyTech.Utility;
using BallyTech.Utility.Serialization;
using log4net;

namespace BallyTech.QCom.Model.Egm.Devices.FundsTransfer
{
    [GenerateICSerializable]
    public abstract partial class FundsTransferBase : Device
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(FundsTransferBase));

        protected FundTransferAuthorization _PendingTransfer = null;
  

        internal override bool IsAnyTransferInProgress
        {
            get { return _PendingTransfer != null; }
        }

        protected Meter GetUpdatedMeter(Direction direction)
        {
            return Model.GetMeters().GetTransferredAmount(direction, TransferDevice.AccountTransfer, null,
                                                          CreditType.Cashable);
        }

        private bool DoesExceedMaxTransferLimit(IFundsTransferAuthorization authorization)
        {
            return !(Model.EgmAdapter.IsAmountWithinMaxTransferLimit(authorization.GetTransferAmount(Model.EgmAdapter.AllowMixedCreditFundTransfer)));
        }

        protected virtual bool CanInitiateTransfer(IFundsTransferAuthorization authorization)
        {
            if(!Model.FundsTransferHandler.IsSupported)
            {
                if (_Log.IsWarnEnabled) _Log.Warn("Fundtransfer not supported") ;

                CancelUninitiatedTransfer(authorization,
                                          FundsTransferCompletionResultCode.FundsTransferNotSupported);
                return false;
            }

            if (IsAnyTransferInProgress)
            {
                if (_Log.IsWarnEnabled) _Log.Warn("Another transfer in progress");

                CancelUninitiatedTransfer(authorization,
                                          FundsTransferCompletionResultCode.AnotherTransferInProcess);
                return false;
            }

            if (DoesExceedMaxTransferLimit(authorization))
            {
                if (_Log.IsWarnEnabled) _Log.Warn("Requested amount is greater than Max Funds Transfer Limit");
                CancelUninitiatedTransfer(authorization, FundsTransferCompletionResultCode.TransferLimitExceeded);
                return false;
            }

            if (!Model.IsGameLinkUp())
            {
                if (_Log.IsWarnEnabled) _Log.Warn("Game Link Down. Unable to initiate transfer");
                CancelUninitiatedTransfer(authorization, FundsTransferCompletionResultCode.UnspecifiedFailure);
                return false;
            }

            if (Model.SoftwareAuthentication.IsMeterExclusionRequired)
            {
                if (_Log.IsWarnEnabled) _Log.Warn("Rom Signature Verification not Complete");
                CancelUninitiatedTransfer(authorization, FundsTransferCompletionResultCode.RomSignatureInProgress);
                return false;
            
            }

            return true;
        }


        internal virtual void InitiateTransfer(IFundsTransferAuthorization transferRequest)
        {
            _PendingTransfer = new FundTransferAuthorization(transferRequest);

            if (_Log.IsInfoEnabled) _Log.Info("Routing the transfer request to the handler");
            Model.FundsTransferHandler.Initiate(transferRequest);            
        }

        internal virtual void CancelCurrentTransfer()
        {
            if(!(IsAnyTransferInProgress)) return;

            if (_Log.IsInfoEnabled) _Log.Info("Cancelling the pending transfer");
            CompleteTransfer(FundsTransferCompletionResultCode.CanceledByHost);
        }


        internal override void OnMetersReceived(SerializableList<MeterId> meterIdsReceived)
        {
            if (!IsAnyTransferInProgress) return;

            if (!HaveReceivedCashlessMeter(meterIdsReceived))
            {
                if (_Log.IsInfoEnabled) _Log.Info("Waiting for cashless meter update");
                return;
            }

            if (!IsMeterChangeDetected()) return;

            if (IsValidMeterChange())
                CompleteTransfer(FundsTransferCompletionResultCode.Success);
            else
                ProcessInvalidMeterChange();
        }

       

        protected virtual void ProcessInvalidMeterChange()
        {
            if (_Log.IsWarnEnabled) _Log.Warn("Invalid meter change. Egm malfunctioned!!!");

            CompleteTransfer(FundsTransferCompletionResultCode.TransferStatusSuccessWithUnmatchingMeterDelta);

            Model.EgmAdapter.ReportErrorEvent(EgmErrorCodes.CentForCentMeterReconcilationFailure);
        }

        protected virtual void CompleteTransfer(FundsTransferCompletionResultCode resultCode)
        {
            if(!(IsAnyTransferInProgress)) return;

            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Completing the current transfer with {0}", resultCode);

            NotifyFundsTransferCompleted(resultCode);
            _PendingTransfer = null;
            this.RequestedMetersReceived();            
        }


        private void NotifyFundsTransferCompleted(FundsTransferCompletionResultCode resultCode)
        {
            Model.Observers.FundsTransferCompleted(new CanceledFundsTransferCompletion()
                                                       {
                                                           InitiatingAuthorization = _PendingTransfer,
                                                           ResultCode = resultCode
                                                       }
                );
        }


        protected void CancelUninitiatedTransfer(IFundsTransferAuthorization authorization, FundsTransferCompletionResultCode resultCode)
        {
            Model.Observers.FundsTransferCompleted(new CanceledFundsTransferCompletion()
            {
                InitiatingAuthorization = authorization,
                ResultCode = resultCode
            });
        }

        internal override void ForceReset()
        {
            CompleteTransfer(FundsTransferCompletionResultCode.UnspecifiedFailure);
            base.ForceReset();
        }

        internal override void MeterRequestSplitIntervalSurpassed()
        {
            if (!IsAnyTransferInProgress) return;

            if (_Log.IsInfoEnabled) _Log.Info("Have not received the updated meters. Hence requesting the meters...");
            Model.RequestMeters(MeterType.Cashless);
        }

        internal override void MeterRequestTimerExpired()
        {
            if (!IsAnyTransferInProgress) return;

            if (_Log.IsWarnEnabled) _Log.Warn("Meter Request Timer Expired");

            if (_PendingTransfer.Destination == TransferDestination.WithdrawToCreditMeter)
                Model.EgmRequestHandler.ResetPSN(ResetStatus.Attempt);

            CompleteTransfer(FundsTransferCompletionResultCode.UnspecifiedFailure);
        }

        internal override void OnLinkStatusChanged(LinkStatus linkStatus)
        {
            if (!IsAnyTransferInProgress) return;

            if (_Log.IsWarnEnabled)
                _Log.WarnFormat("Game link got changed to {0}", linkStatus);

            if(linkStatus == LinkStatus.Disconnected)
                CompleteTransfer(FundsTransferCompletionResultCode.TimedOutByEgm);

        }

        protected virtual bool IsValidMeterChange()
        {
            return true;
        }


        internal abstract bool SupportsTransfer(IFundsTransferAuthorization authorization);
        protected abstract bool HaveReceivedCashlessMeter(SerializableList<MeterId> meterIds);
        protected abstract bool IsMeterChangeDetected();
    }
}
