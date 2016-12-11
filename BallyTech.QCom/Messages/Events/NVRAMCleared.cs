using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.QCom.Model.Egm;

namespace BallyTech.QCom.Messages
{
    public partial class NVRAMCleared
    {
        public override ExtendedEgmEventData GetExtendedEgmEventData()
        {
            ExtendedEgmEventData extendedData = base.GetExtendedEgmEventData();
            extendedData.GameSerialNumber = this.EgmSerialNumber.ToString();
            extendedData.ManufacturerId = this.EgmManufacturerId.ToString();
            return extendedData;
        }
    }
}
