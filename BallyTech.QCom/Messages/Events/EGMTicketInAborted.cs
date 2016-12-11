using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.QCom.Model.Egm;

namespace BallyTech.QCom.Messages
{
    public partial class EGMTicketInAborted
    {
        public override ExtendedEgmEventData GetExtendedEgmEventData()
        {
            ExtendedEgmEventData extendedData = base.GetExtendedEgmEventData();
            extendedData.TicketBarcode = this.TicketAuthorisationNumber;
            return extendedData;
        }
    }
}
