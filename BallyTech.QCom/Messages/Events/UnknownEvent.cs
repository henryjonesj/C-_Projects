using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Model.Egm;

namespace BallyTech.QCom.Messages
{
    public partial class UnknownEvent
    {
        public override ExtendedEgmEventData GetExtendedEgmEventData()
        {
            ExtendedEgmEventData extendedData = base.GetExtendedEgmEventData();
            extendedData.GenericDataBuffer = this.GenericDataBuffer.ToArray();
            return extendedData;
        }

        public override bool IsUnknown
        {
            get
            {
                return true;
            }
        }
    }
}
