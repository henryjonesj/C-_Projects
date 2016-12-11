using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class OptionalDetails : IOptionalDetails
    {
        #region IOptionalDetails Members

        public DateTime HitTime { get; set; }

        public int GameNumber { get; set; }

        public string PaytableId{ get; set; }

        public int ProgressiveGroupId{ get; set; }

        public decimal HitMeter { get; set; }

        public decimal WinMeter { get; set; }

        #endregion
    }
}
