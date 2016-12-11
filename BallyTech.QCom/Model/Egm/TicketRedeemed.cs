using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;

namespace BallyTech.QCom.Model.Egm
{

    partial class TicketInDevice
    {

        [GenerateICSerializable]
        public partial class TicketRedeemed : ITicketRedeemed
        {

            public TicketRedeemed()
            {
            }

            public TicketRedeemed(IVoucherTransaction voucherTransaction)
            {
                this.TransferAmount = voucherTransaction.Status == TransactionStatus.Success
                                          ? voucherTransaction.Amount
                                          : 0m;

                this.Action = voucherTransaction.Status == TransactionStatus.Success
                                  ? InsertedTicketAction.Stack
                                  : InsertedTicketAction.Reject;

                this.ValidationNumber = voucherTransaction.ValidationId;

            }



            #region ITicketRedeemed Members

            public decimal TransferAmount { get; private set; }

            public InsertedTicketAction Action { get; private set; }

            public CreditType? CreditType
            {
                get { return Gtm.CreditType.Cashable; }
            }

            #endregion

            #region ITicket Members

            public decimal ValidationNumber { get; private set; }

            #endregion
        }


        [GenerateICSerializable]
        public partial class TicketInserted : ITicketInserted
        {

            public TicketInserted()
            {
            }

            public TicketInserted(decimal validationNumber)
            {
                ValidationNumber = validationNumber;
            }


            #region ITicketInserted Members

            public decimal TicketAmount
            {
                get { return 0m; }
            }

            #endregion

            #region ITicket Members

            public decimal ValidationNumber { get; private set; }

            #endregion
        }

    }
}
