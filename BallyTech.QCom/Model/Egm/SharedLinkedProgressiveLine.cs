using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using log4net;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class SharedLinkedProgressiveLine : LinkedProgressiveLine
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (SharedLinkedProgressiveLine));

        internal ILinkedProgressiveHandler Handler { get; set; }


        public override void SetAward(decimal awardValue, JackpotPaymentType paymentType)
        {
            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Received award with value {0} and payment type {0}", awardValue, paymentType);

            Handler.AcknowledgeLinkedProgressiveAward(paymentType);
        }

    }
}
