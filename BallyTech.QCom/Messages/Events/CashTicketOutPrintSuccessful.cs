using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.QCom.Model.Egm;

namespace BallyTech.QCom.Messages
{
    public partial class CashTicketOutPrintSuccessful
    {
        public override ExtendedEgmEventData GetExtendedEgmEventData()
        {
            ExtendedEgmEventData extendedData = base.GetExtendedEgmEventData();
            extendedData.Amount = this.TicketAmount;
            extendedData.TicketSerialNumber = this.TicketSerialNumber;
            return extendedData;
        }
    }
}
