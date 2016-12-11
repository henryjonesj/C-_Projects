using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.QCom.Model.Egm;

namespace BallyTech.QCom.Messages
{
    public partial class LinkedProgressiveAward
    {
        public override ExtendedEgmEventData GetExtendedEgmEventData()
        {
            ExtendedEgmEventData extendedData = base.GetExtendedEgmEventData();
            extendedData.GameNumber = this.GameVersionNumber;
            extendedData.PaytableId = this.GameVariationNumber.ToString();
            extendedData.ProgGroupId = this.ProgressiveGroupId;
            extendedData.LevelId = (byte)this.LinkedProgressiveLevel;
            extendedData.Amount = this.LastJackpotAmount;
            return extendedData;
        }


        internal DateTime GetHitDateTime()
        {
            return IsInvalidDateTime
                       ? new DateTime(1900, 1, 1)
                       : QComConvert.ConvertQComRawDateTimeToDateTime(this._EventDateTime);
        }
    }
}
