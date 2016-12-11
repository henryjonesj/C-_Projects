using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class PendingTicketTransaction : IVoucherTransaction
    {
        public Direction Direction { get; private set; }
        public decimal Amount { get; private set; }
        public ushort SerialNumber { get; private set; }
        public decimal ValidationId { get; private set; }

        public DateTime TransactionDateTime { get; private set; }

        public TransactionStatus Status { get; private set; }


        public PendingTicketTransaction()
        {
            
        }

        public PendingTicketTransaction(ushort serialNumber,decimal amount)
        {
            this.Direction = Direction.Out;
            this.SerialNumber = serialNumber;
            this.Amount = amount;
            this.Status = TransactionStatus.None;

        }

        public PendingTicketTransaction(decimal validationId)
        {
            this.Direction = Direction.In;
            this.ValidationId = validationId;
        }


        internal void OnTicketPrintInfoReceived(decimal validationId,DateTime dateTime)
        {            
            this.TransactionDateTime = dateTime;

            if (validationId == 0) return;

            this.ValidationId = validationId;
            this.Status = TransactionStatus.Authorized;
        }


        internal void OnTicketRedeemInfoReceived(ITicketValidated ticketValidated)
        {
            this.Status = ticketValidated.Action == InsertedTicketAction.Stack
                              ? TransactionStatus.Authorized
                              : TransactionStatus.Failure;

            this.ReasonCode = ticketValidated.RejectReasonCode;
            this.Amount = ticketValidated.TicketAmount;
        }

        public TicketRedeemReasonCode ReasonCode { get; private set; }



        internal void UpdateTransactionStatus(TransactionStatus status)
        {
            this.Status = status;
        }


        public override string ToString()
        {            
            var sb = new StringBuilder();

            sb.AppendLine(string.Format("Validation Id: {0}", this.ValidationId));
            sb.AppendLine(string.Format("Ticket Amount: {0}", this.Amount));
            sb.AppendLine(string.Format("Transaction Status: {0}", this.Status));

            return sb.ToString();
        }

    }
}
