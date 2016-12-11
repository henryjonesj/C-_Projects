using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.QCom.Model.Egm;

namespace BallyTech.QCom.Messages
{
    public partial class CashTicketPrinted
    {
        public override ExtendedEgmEventData GetExtendedEgmEventData()
        {
            ExtendedEgmEventData extendedData = base.GetExtendedEgmEventData();
            extendedData.Amount = this.Amount;
            extendedData.TicketSerialNumber = (uint)this.TicketSerialNumber;
            extendedData.TicketBarcode = this.TicketAuthenticationCode;
            return extendedData;
        }
    }
}
