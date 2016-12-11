using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.Utility.Serialization;
using log4net;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class LinkedMysteryLine: IProgressiveLine
    {
        public Action<decimal> SendHandPayPending = delegate { };
        
        private static readonly ILog _Log = LogManager.GetLogger(typeof(LinkedMysteryLine));

        public LinkedMysteryLine() { }
        
        public LinkedMysteryLine(byte levelNumber, OptionalDetails optionalDetails)
        {
            LineId = levelNumber;
            _OptionalDetails = optionalDetails;

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
        }

        public void SetAward(decimal awardValue, JackpotPaymentType paymentType)
        {   
            awardValue = Math.Round(awardValue, 2);

            SendHandPayPending(awardValue);
        }

        public ushort Sequence { get { return 0; } }

        public string TransactionId { get { return String.Empty; } }

        public GameProgressiveType ProgressiveType
        {
            get { return GameProgressiveType.LinkedProgressive; }
        }


        private OptionalDetails _OptionalDetails = new OptionalDetails();
        public IOptionalDetails OptionalDetails
        {
            get { return _OptionalDetails; }
        }
    }
}
