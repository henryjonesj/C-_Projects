using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class PendingJackpot : IHandpayPending
    {
        #region IHandpayPending Members

        public decimal HandpayAmount { get; set; }

        public HandpayType HandpayType { get; set; }

        public decimal PartialPayAmount { get; set; }

        #endregion

        internal bool IsSame(HandpayType handpayType)
        {
            return this.HandpayType == handpayType;
        }


        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Handpay Type: {0}", this.HandpayType);
            sb.AppendFormat("Handpay Amount: {0}", this.HandpayAmount);

            return sb.ToString();
        }
    }
}
