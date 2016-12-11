using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.Utility;
using BallyTech.Utility.Serialization;
using log4net;
using BallyTech.QCom.Messages;

namespace BallyTech.QCom.Model.Egm.Devices.FundsTransfer
{
    [GenerateICSerializable]
    public partial class FundsTransferIn : FundsTransferBase
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (FundsTransferIn));

        private Meter _CashlessIn = Meter.Zero;


        internal override bool SupportsTransfer(IFundsTransferAuthorization authorization)
        {
            return authorization.Destination == TransferDestination.WithdrawToCreditMeter;
        }        

        private bool IsCreditTypeNotSupported(IFundsTransferAuthorization request)
        {
            if (request == null) return false;

            if (Model.EgmAdapter.AllowMixedCreditFundTransfer) return false;

            return request.NonCashable > 0m;
        }

        protected override bool CanInitiateTransfer(IFundsTransferAuthorization authorization)
        {
             if (!base.CanInitiateTransfer(authorization)) return false;

            if (IsCreditTypeNotSupported(authorization))
            {
                if (_Log.IsWarnEnabled) _Log.Warn("Egm doesn't support points/promo transfer");
                CancelUninitiatedTransfer(authorization, FundsTransferCompletionResultCode.FundsTransferNotSupported);
                return false;
            }

            if (Model.EgmAdapter.IsGameFaulty)
            {
                if (_Log.IsWarnEnabled) _Log.Warn("Egm is in faulty mode. Hence transfer is not initated");
                CancelUninitiatedTransfer(authorization, FundsTransferCompletionResultCode.GameStateChanged);
                return false;
            }

            if (!IsWithdrawalAmountWithinMaxCreditLimit(authorization.GetTotalAmount()))
            {
                if (_Log.IsWarnEnabled) _Log.Warn("Cancelling the transaction due to MaxCredit Limit!");
                CancelUninitiatedTransfer(authorization, FundsTransferCompletionResultCode.WithdrawalAmountGreaterThanMaxCreditLimit);
                return false;
            }
            return true;
        }
        private bool IsWithdrawalAmountWithinMaxCreditLimit(decimal amount)
        {
            decimal currentCredits = Model.GetMeters().GetCreditAmount(null).DangerousGetUnsignedValue();
            return (currentCredits + amount) < Model.EgmAdapter.MaxCreditLimit;
        }

        internal override void InitiateTransfer(IFundsTransferAuthorization transferRequest)
        {
            if (!CanInitiateTransfer(transferRequest)) return;

            _CashlessIn = GetUpdatedMeter(Direction.In);

            Model.EgmAdapter.IsEctToEgmInProgress = true;
            base.InitiateTransfer(transferRequest);
            this.AwaitingForUnsolicitedMeters();
        }

        protected override void CompleteTransfer(FundsTransferCompletionResultCode resultCode)
        {
            if (!IsAnyTransferInProgress) return;

            Model.EgmAdapter.IsEctToEgmInProgress = false;

            if (resultCode == FundsTransferCompletionResultCode.Success) Model.EgmRequestHandler.ResetPSN(ResetStatus.Success);

            base.CompleteTransfer(resultCode);
        }

        protected override bool HaveReceivedCashlessMeter(SerializableList<MeterId> meterIds)
        {
            return meterIds.Contains(MeterId.CashlessIn);
        }

        protected override bool IsMeterChangeDetected()
        {
            return MeterChangedDetector.IsMeterChanged(_CashlessIn, GetUpdatedMeter(Direction.In));
        }

        protected override void ProcessInvalidMeterChange()
        {
            Model.EgmAdapter.ExtendedEventData =
            Model.EgmAdapter.ConstructUnreasonableMeterData(MeterCodes.TotalEgmCashlessCreditIn, _CashlessIn, GetUpdatedMeter(Direction.In));

            base.ProcessInvalidMeterChange();
          
        }

        protected override bool IsValidMeterChange()
        {
            var transferAmount = _PendingTransfer.GetTransferAmount(Model.EgmAdapter.AllowMixedCreditFundTransfer);

            transferAmount = decimal.Round(transferAmount, 2);

            var newCashlessIn = GetUpdatedMeter(Direction.In);

            var isValidMeterChange = MeterChangedDetector.IsExpectedMeterChange(_CashlessIn, newCashlessIn,
                                                                                transferAmount);
            if (isValidMeterChange) return true;

            if (_Log.IsWarnEnabled)
                _Log.WarnFormat("Requested amount {0} mismatches with the difference of old cashless meter {1} from new cashless meter {2}",
                    transferAmount, _CashlessIn, newCashlessIn);

            return false;
        }

        internal void HandleOutOfSequencePsn()
        {
            if (!IsAnyTransferInProgress) return;
            CompleteTransfer(FundsTransferCompletionResultCode.UnspecifiedFailure);
        
        }

		

    }
}
