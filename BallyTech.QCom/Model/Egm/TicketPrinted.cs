using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class TicketPrinted : ITicketPrinted
    {
        public TicketPrinted()
        {
            
        }

        public TicketPrinted(IVoucherTransaction ticketTransaction)
        {
            this.TicketNumber = ticketTransaction.SerialNumber;
            this.ValidationId = ticketTransaction.ValidationId;
            this.DateTime = ticketTransaction.TransactionDateTime;
            this.TicketAmount = ticketTransaction.Amount;
        }


        #region ITicketPrinted Members

        public DateTime DateTime { get; private set; }        

        public bool LargeWin
        {
            get { return false; }
        }

        public decimal TicketAmount { get; private set; }

        public decimal TicketNumber { get; private set; }

        public decimal ValidationId { get; private set; }

        public decimal Expiration
        {
            get { return 0m; }
        }

        public decimal PoolID
        {
            get { return 0m; }
        }

        public CreditType? CreditType
        {
            get { return Gtm.CreditType.Cashable; }
        }

        #endregion

        #region ITicket Members

        public decimal ValidationNumber 
        { 
           get
           {
               if (ValidationId == 0) return 0;

               string validationIdText = string.Format("{0}", ValidationId);
               return Decimal.Parse(validationIdText.Substring(2));
           }
        }
            

        #endregion
    }
}
