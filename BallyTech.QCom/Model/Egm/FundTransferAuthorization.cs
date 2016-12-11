using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using BallyTech.Utility.Time;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class FundTransferAuthorization : IFundsTransferAuthorization
    {
        public FundTransferAuthorization() { }

        public FundTransferAuthorization(IFundsTransferAuthorization Authorization) 
        {
            this.InitiatingRequest = Authorization.InitiatingRequest;
            this.ResultCode = Authorization.ResultCode;
            this.TransactionId= Authorization.TransactionId;

            UpdateTransferAmount(Authorization.Cashable);
        
        }

        #region IFundsTransferAuthorization Members

        public IFundsTransferRequest InitiatingRequest { get; private set; }

        public FundsTransferAuthorizationResultCode ResultCode { get; private set; }
        

        #endregion

        #region IFundsTransferInProgress Members

        public long TransactionId { get; private set; }

        #endregion

        #region IFundsTransfer Members

        public string AccountId
        {
            get { return InitiatingRequest.AccountId; }
        }

        public string ApplicationName
        {
            get { return InitiatingRequest.ApplicationName; }
        }

        public ulong BonusId
        {
            get { return InitiatingRequest.BonusId; }
        }
        
        public decimal Cashable
        {
            get;
            private set;
        }

        public decimal NonCashable
        {
            get { return InitiatingRequest.NonCashable; }
        }

        public decimal Promotional
        {
            get { return InitiatingRequest.Promotional; }
        }

        public TransferOrigin Origin
        {
            get { return InitiatingRequest.Origin; }
        }

        public TransferDestination Destination
        {
            get { return InitiatingRequest.Destination; }
        }

        public DateTime TransactionDateTime
        {
            get { return InitiatingRequest.TransactionDateTime; }
        }

        #endregion


        internal void UpdateTransferAmount(decimal amount)
        {
            this.Cashable = amount;
        
        }
    }
}
