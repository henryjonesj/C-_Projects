using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using BallyTech.Gtm.Core;
using BallyTech.QCom.Model.Egm;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class ProgressiveInfoCollection : SerializableDictionary<int,ProgressiveLevelInfo>
    {
        
        internal void Update(ProgressiveLevelInfo progressiveLevelInfo)
        {
            this[progressiveLevelInfo.LineId] = progressiveLevelInfo;
        }

        internal void UpdateContributionAmount(ProgressiveLevelInfo progressiveLevelInfo)
        {
            if (!this.ContainsKey(progressiveLevelInfo.LineId)) return;
            this[progressiveLevelInfo.LineId].UpdateAmount(progressiveLevelInfo.LineAmount);
        }
    }

    [GenerateICSerializable]
    public partial class ProgressiveLevelInfo : IProgressiveLine
    {
        public ProgressiveLevelInfo()
        {
            
        }

        public ProgressiveLevelInfo(byte levelNumber,GameProgressiveType progressiveType)
        {
            LineId = levelNumber;
            ProgressiveType = progressiveType;
        }

        internal ProgressiveLevelInfo UpdateAmount(decimal amount)
        {
            LineAmount = amount;
            return this;
        }

        internal void UpdateMeters(decimal hits, decimal wins)
        {
            _OptionalDetails.HitMeter = hits;
            _OptionalDetails.WinMeter = wins;
        }

        #region IProgressiveLine Members

        public int LineId { get; private set; }

        public decimal LineAmount { get; private set; }

        public void UpdateValue(int ProgressiveId, decimal newValue)
        {
           
        }

        public void SetAward(decimal awardValue, JackpotPaymentType paymentType)
        {
           
        }

        public ushort Sequence { get; private set; }

        public string TransactionId { get; private set; }

        #endregion

        #region IProgressiveLine Members


        public GameProgressiveType ProgressiveType { get; private set; }

        private OptionalDetails _OptionalDetails = new OptionalDetails();
        public IOptionalDetails OptionalDetails 
        {
            get { return _OptionalDetails; } 
        }

        #endregion
    }
}
