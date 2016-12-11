using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.Utility.Serialization;
using log4net;
using BallyTech.Utility;
using BallyTech.Gtm.Core;
using BallyTech.QCom.Messages;

namespace BallyTech.QCom.Model.Egm.Devices.FundsTransfer
{
    [GenerateICSerializable]
    public partial class FundsTransferOut : FundsTransferBase
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (FundsTransferOut));

        private Meter _CashlessOut = Meter.Zero;
        private bool _IsCashoutRequested = false;

        internal override bool SupportsTransfer(IFundsTransferAuthorization authorization)
        {
            return authorization.Destination == TransferDestination.DepositToHost;
        }

        internal override void InitiateTransfer(IFundsTransferAuthorization transferRequest)
        {
            if (!(CanInitiateTransfer(transferRequest))) return;

            _PendingTransfer = new FundTransferAuthorization(transferRequest);

            _CashlessOut = GetUpdatedMeter(Direction.Out);
            Model.EgmAdapter.IsEctFromEgmInProgress = true;

            if (!(_IsCashoutRequested))
            {
                Initiate(transferRequest);
                return;
            }

            ResetLockup();

        }

        private void Initiate(IFundsTransferAuthorization transferRequest)
        {   
            if(Model.EgmAdapter.IsGameFaulty)
            {
                if (_Log.IsWarnEnabled)
                    _Log.Warn("Unable to initiate deposit because since game is in faulty condition");
                CompleteTransfer(FundsTransferCompletionResultCode.GameStateChanged);
                return;
            }

            if (_Log.IsInfoEnabled) _Log.Info("Initiating the deposit transfer");
                
            Model.FundsTransferHandler.Initiate(transferRequest);

           
        }


        protected override bool HaveReceivedCashlessMeter(SerializableList<MeterId> meterIds)
        {
            return meterIds.Contains(MeterId.CashlessOut);
        }

        protected override bool IsMeterChangeDetected()
        {
            var newCashlesOut = GetUpdatedMeter(Direction.Out);

            return MeterChangedDetector.IsMeterChanged(newCashlesOut, _CashlessOut);
        }

        protected override bool IsValidMeterChange()
        {
            var transferAmount = _PendingTransfer.GetTransferAmount(Model.EgmAdapter.AllowMixedCreditFundTransfer);

            transferAmount = decimal.Round(transferAmount, 2);

            var newCashlessOut = GetUpdatedMeter(Direction.Out);

            var isValidMeterChange = MeterChangedDetector.IsExpectedMeterChange(_CashlessOut, newCashlessOut,
                                                                                transferAmount);
            if (isValidMeterChange) return true;

            if (_Log.IsWarnEnabled)
                _Log.WarnFormat("Requested amount {0} mismatches with the difference of old cashless meter {1} from new cashless meter {2}",
                    transferAmount, _CashlessOut, newCashlessOut);

            return false;
        }

        protected override void ProcessInvalidMeterChange()
        {
            Model.EgmAdapter.ExtendedEventData =
            Model.EgmAdapter.ConstructUnreasonableMeterData(MeterCodes.TotalEgmCashlessCreditOut, _CashlessOut, GetUpdatedMeter(Direction.Out));

            base.ProcessInvalidMeterChange();

        }

        protected override void CompleteTransfer(FundsTransferCompletionResultCode resultCode)
        {
            if(!IsAnyTransferInProgress) return;

            if (resultCode != FundsTransferCompletionResultCode.Success)
                Model.EgmAdapter.IsEctFromEgmInProgress = false;

            base.CompleteTransfer(resultCode);

            ResetTransaction();
            Model.GameLockedForAutoDeposit.Value = false;

            Model.FundsTransferHandler.SetState(Model.AllowCashlessMode);
        }

        internal override void NotifyRomSignatureVerificationFailure()
        {
            if (_Log.IsWarnEnabled) _Log.Warn("Completing the transfer Rom Signature Verification Failed!");

            CompleteTransfer(FundsTransferCompletionResultCode.RomSignatureInProgress);

            this.AwaitingForRomSignatureVerification = false;
            this.RequestedMetersReceived();
        }

        private void ResetTransaction()
        {
            _IsCashoutRequested = false;                   
            this.RequestedMetersReceived();
        }

        private bool DoesExceedMaxTransferLimit(decimal amount)
        {
            return !(Model.EgmAdapter.IsAmountWithinMaxTransferLimit(amount));
        }

        private bool CanProcessCashoutRequest(decimal amount)
        {
            var cashOutAmount = amount != -1 ? amount : Model.GetMeters().GetCreditAmount(null).DangerousGetUnsignedValue();

            if(DoesExceedMaxTransferLimit(cashOutAmount))
            {
                if(_Log.IsWarnEnabled)
                    _Log.WarnFormat("Cashout amount {0} exceeds the max funds transferlimit. Hence cancelling it..",cashOutAmount);

                CancelPendingTransfer();
                return false;
            }

            return true;
        }

        public void RequestForFullDeposit(decimal amount)
        {
            if (!CanProcessCashoutRequest(amount)) return;

            _IsCashoutRequested = true;

            if (IsAnyTransferInProgress)
            {
                ResetLockup();
                return;
            }

            if(!ShouldHandleCashoutRequest(amount)) return;

            ProcessCashout(amount);           
        }


        private static bool ShouldHandleCashoutRequest(decimal amount)
        {
            return amount > 0;
        }


        private void ProcessCashout(decimal amount)
        {
            if(!Model.AllowCashlessMode)
            {
                if (_Log.IsInfoEnabled) _Log.Info("Canelling the cashout as cashless mode is not available...");
                CancelPendingTransfer();
                return;
            }

            if (amount < Model.EgmAdapter.GetMeters().GetCreditAmount(CreditType.Cashable).DangerousGetUnsignedValue())
            {
                if (_Log.IsInfoEnabled) _Log.Info("Requesting for partial deposit");
                Model.Observers.RequestForHostCashout(true, amount);
                return;
            }
            if (_Log.IsInfoEnabled) _Log.Info("Requesting for full deposit");
            Model.Observers.RequestForHostCashout(false, 0);
        }



        private void ResetLockup()
        {
            if (_Log.IsInfoEnabled) _Log.Info("Resetting the deposit lockup on the game");

            Model.FundsTransferHandler.Commit(_PendingTransfer);
            this.AwaitingForUnsolicitedMeters();
        }

        internal void OnLockupFailed()
        {
            if(!IsAnyTransferInProgress) return;

            if (_Log.IsInfoEnabled) _Log.Info("Lockup failed. Hence committing the transfer");
                
            CompleteTransfer(FundsTransferCompletionResultCode.UnspecifiedFailure);
        }

        internal void OnEctfromEgmLockUp()
        {
            if (!IsAnyTransferInProgress) return;

            if (_PendingTransfer.Cashable != 0) return;
                
            _PendingTransfer.UpdateTransferAmount(Model.EgmAdapter.EctFromEgmMonitor.PendingTransactionAmount);

        }


        internal void OnLockupCleared()
        {
            if (IsAnyTransferInProgress)
            {
                if (_Log.IsInfoEnabled)
                    _Log.Info("Not resetting the transaction since waiting for the cashless out meter update");
                return;
            }

            ResetTransaction();
        }

        internal override void MeterRequestTimerExpired()
        {
            if (!IsAnyTransferInProgress) return;

            CancelPendingTransfer();
            base.MeterRequestTimerExpired();
            
        }

        internal override void OnLinkStatusChanged(LinkStatus linkStatus)
        {
            if (!IsAnyTransferInProgress) return;

            CancelPendingTransfer();
            base.OnLinkStatusChanged(linkStatus);
        }


        private void CancelPendingTransfer()
        {
            Model.FundsTransferHandler.Commit(new DeclinedFundsTransferAuthorization()
                                                  {
                                                      ResultCode =
                                                          FundsTransferAuthorizationResultCode.UnableToAcceptTransfer,
                                                          InitiatingRequest = new FundsTransferRequest(){Destination = TransferDestination.DepositToHost}
                                                  });
        }
        

        internal override void CancelCurrentTransfer()
        {
            if (!IsAnyTransferInProgress) return;

            CancelPendingTransfer();
            base.CancelCurrentTransfer();
        }

        internal void ForceCashout()
        {
            if (_Log.IsWarnEnabled) _Log.Warn("Forcing the Egm to payout as coins/tickets/handpay");

            Model.FundsTransferHandler.Commit(new DeclinedFundsTransferAuthorization()
            {
                ResultCode =
                    FundsTransferAuthorizationResultCode.UnableToAcceptTransfer,
                    InitiatingRequest =  new FundsTransferRequest(){Destination = TransferDestination.Handpay}
            });
        }

    }
}
