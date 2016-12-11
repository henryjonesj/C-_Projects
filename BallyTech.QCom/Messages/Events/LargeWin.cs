using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.QCom.Model.Egm;

namespace BallyTech.QCom.Messages
{
    public partial class LargeWin
    {
        public override ExtendedEgmEventData GetExtendedEgmEventData()
        {
            ExtendedEgmEventData extendedData = base.GetExtendedEgmEventData();
            extendedData.GameNumber = this.GameVersionNumber;
            extendedData.PaytableId = this.GameVariationNumber.ToString();
            extendedData.Amount = this.WinAmount;
            return extendedData;
        }
    }
}
