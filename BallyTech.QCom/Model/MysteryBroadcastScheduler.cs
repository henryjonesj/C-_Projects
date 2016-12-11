using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.Builders;
using BallyTech.Gtm;
using log4net;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class MysteryBroadcastScheduler
    {
        private QComModel _Model;
        private Scheduler _MysteryBroadcastScheduler;

        public MysteryBroadcastScheduler() { }

        public MysteryBroadcastScheduler(QComModel model)
        {
            _Model = model;
            _MysteryBroadcastScheduler = new Scheduler(model.Schedule);
            _MysteryBroadcastScheduler.TimeOutAction += SendMysteryBroadcast;
            SendMysteryBroadcast();
        }

        private void SendMysteryBroadcast()
        {
            CheckAndSendMysteryBroadcast();
            _MysteryBroadcastScheduler.Start(_Model.LinkedProgressiveBroadcastTimeout);                    
        }

        private void CheckAndSendMysteryBroadcast()
        {
            int noOfMysteryLevels = 0;
            LinkedProgressiveJackpotCurrentAmounts MysteryBroadcast = new LinkedProgressiveJackpotCurrentAmounts();

            if (_Model.Egm.LinkedMysteryLines == null) return;

            foreach (IProgressiveLine mysteryLine in _Model.Egm.LinkedMysteryLines)
            {
                if (mysteryLine.LineId == 0) continue;
                noOfMysteryLevels++;
                MysteryBroadcast.LinkedProgressiveData.Add(new LinkedProgressiveDetails()
                {
                    LinkedProgressiveGroupId = (ushort)mysteryLine.OptionalDetails.ProgressiveGroupId,
                    LinkedProgressiveLevelId = GetLevel((mysteryLine.LineId - 1).ToString()),
                    LinkedProgressiveJackpotAmount = mysteryLine.LineAmount
                });
            }

            if (noOfMysteryLevels == 0) return;
            MysteryBroadcast.NumberOfProgressiveLevels =
               (ProgressiveLevel)(Enum.Parse(typeof(ProgressiveLevel), (noOfMysteryLevels - 1).ToString(), true)) | ProgressiveLevel.Reserved;

            _Model.SendPoll(MysteryBroadcast);    
        }

        private ProgressiveLevel GetLevel(string levelNumber)
        {
            return (ProgressiveLevel)(Enum.Parse(typeof(ProgressiveLevel), levelNumber, true));
        }
    }
}
