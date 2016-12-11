using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using log4net;
using BallyTech.Utility.Time;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class LinkedProgressiveLine : IProgressiveLine
    {
        public Action<JackpotPaymentType> SendLPAcknowledgement = delegate { };
        private static readonly ILog _Log = LogManager.GetLogger(typeof(LinkedProgressiveLine));        

        #region IProgressiveLine Members
        private string _transactionId = TimeProvider.UtcNow.Ticks.ToString();
        public string TransactionId
        {
            get { return _transactionId; }
        }        

        private int _LineId;
        public int LineId
        {
            set { _LineId = value; }
            get { return _LineId; }
        }

        private decimal _LineAmount;
        public decimal LineAmount
        {
            get { return _LineAmount; }
            set { _LineAmount = value; }
        }

        public void UpdateValue(int ProgressiveId, decimal newValue)
        {
            _LineAmount = newValue / 0.01m;
            LPBroadCastCounter.Reset();
        }

        public virtual void SetAward(decimal awardValue, JackpotPaymentType paymentType)
        {
            _Log.Info("received Linked progressive ack from ebs");

            SendLPAcknowledgement(paymentType);
        }

        public ushort Sequence
        {
            get { return 0; }
        }

        public GameProgressiveType ProgressiveType
        {
            get { return GameProgressiveType.LinkedProgressive; }
        }

        public IOptionalDetails OptionalDetails { get; set; }
        
        #endregion
    }
}
