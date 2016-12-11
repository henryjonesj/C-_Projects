using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.QCom.Model.Egm;

namespace BallyTech.QCom.Messages
{
    public partial class ProgressiveConfigurationChanged
    {
        public override ExtendedEgmEventData GetExtendedEgmEventData()
        {
            ExtendedEgmEventData extendedData = base.GetExtendedEgmEventData();
            extendedData.GameNumber = this.GameVersionNumber;
            extendedData.PaytableId = this.GameVariationNumber.ToString();
            extendedData.RTP = (uint)this.ProgressiveRtp;
            return extendedData;
        }
    }
}
