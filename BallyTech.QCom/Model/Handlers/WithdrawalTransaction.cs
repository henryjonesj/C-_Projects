using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using BallyTech.QCom.Messages;
using log4net;

namespace BallyTech.QCom.Model.Handlers
{
    [GenerateICSerializable]
    public partial class WithdrawalTransaction : QComTransaction
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(WithdrawalTransaction));

        private QComModel _Model = null;        

        public WithdrawalTransaction() { }

        public WithdrawalTransaction(ElectronicCreditTransferHandler handler)
        {
            Parent = handler;
            _Model = Parent.Model;
        }

        internal virtual decimal MaxTransferLimit
        {
            get { return 99999999.99m; }
        }

        private decimal ClipMaxAmountForPartialTransfer(bool isPartialTransfer, decimal amount)
        {
            return (isPartialTransfer && amount > MaxTransferLimit) ? MaxTransferLimit : amount;
        }

        decimal GetAmount(IFundsTransferAuthorization authorization)
        {
            bool allowPartialTransfer = false;
            bool isAutoDepositTransfer = false;

            var initiatingRequest = authorization.InitiatingRequest;

            if(initiatingRequest != null)
            {
                allowPartialTransfer = initiatingRequest.AllowPartialTransfer;
                isAutoDepositTransfer = initiatingRequest.IsAutoDepositTransfer();
            }


            return ClipMaxAmountForPartialTransfer(allowPartialTransfer,
                                                   (isAutoDepositTransfer &&
                                                    authorization.Cashable != 0)
                                                       ? Model.Egm.GetMeters().GetCreditAmount(CreditType.Cashable).
                                                             DangerousGetUnsignedValue()
                                                       : authorization.GetTransferAmount(_Model.Egm.AllowMixedCreditFundTransfer));
        }

        internal override void Execute(IFundsTransferAuthorization authorization)
        {
            var ectToEgmPoll = new EctToEgmPoll();
            ectToEgmPoll.EctToEgmPollFlag = new EctToEgmPollFlags() 
            { 
                CashlessMode = _Model.Egm.IsCashlessModeSupported,
                EctSourceId = authorization.InitiatingRequest.ToSourceId()
            };

            ectToEgmPoll.EAmount = ectToEgmPoll.OCAmount = decimal.Round(GetAmount(authorization) / QComCommon.MeterScaleFactor, 0);

            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Initiating Withdrawal transaction for amount {0} with Sequence number {1}",
                                authorization.GetTotalAmount(), _Model.ECTPollSequenceNumber);

            _Model.EctToEgmPollDispatcher.Dispatch(ectToEgmPoll);
        }
    }
}
