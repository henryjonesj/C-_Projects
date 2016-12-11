using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;

namespace BallyTech.QCom.Model.Egm
{
    public enum TransactionStatus
    {
        None,
        Success,
        Failure,
        Authorized
    }

    public interface IVoucherTransaction
    {
        Direction Direction { get; }
        decimal Amount { get; }
        ushort SerialNumber { get; }
        decimal ValidationId { get; }
        DateTime TransactionDateTime { get; }
        TransactionStatus Status { get; }
        TicketRedeemReasonCode ReasonCode { get; }
    }


    public static class VoucherTransactionExtension
    {
        
        public static bool IsAuthorized(this IVoucherTransaction ticketTransaction)
        {
            return ticketTransaction.Status == TransactionStatus.Authorized;
        }

    }

}
