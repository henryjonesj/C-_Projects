using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class PaymentCompletedProgressiveJackpot: IProgressiveLinePayment
    {
        #region IProgressiveLinePayment Members

        public int ProgressiveGroupId { get; set; }

        public HandpayType LevelType { get; set; }

        public decimal Amount { get; set; }

        public JackpotPaymentType PaymentType { get; set; }

        #endregion
    }
}
