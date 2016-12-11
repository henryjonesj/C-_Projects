using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model.Egm
{
    public interface ILinkedProgressiveHandler
    {
        void UpdateProgressiveLevels(Game game,SerializableList<ProgressiveLevelInfo> progressiveLevelInfo);
        void OnSuccessfulAutopay();
        void HandleLinkedProgressiveLockup();
        void AcknowledgeLinkedProgressiveAward(JackpotPaymentType paymentType);
        void ResetLockup();
    }

    [GenerateICSerializable]
    public partial class NullLinkedProgressiveHandler : ILinkedProgressiveHandler
    {
        #region ILinkedProgressiveHandler Members

        public void UpdateProgressiveLevels(Game game, SerializableList<ProgressiveLevelInfo> progressiveLevelInfo) { }

        public void OnSuccessfulAutopay() { }

        public void HandleLinkedProgressiveLockup() { }

        public void ResetLockup()
        {
        }

        #endregion


        #region ILinkedProgressiveHandler Members


        public void AcknowledgeLinkedProgressiveAward(JackpotPaymentType paymentType)
        {
            
        }

        #endregion
    }
}
