using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.Egm;
using BallyTech.Gtm;
using BallyTech.Utility.Time;
using log4net;

namespace BallyTech.QCom.Model.Builders
{
    public static class LinkedProgressiveBroadcastBuilder
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (LinkedProgressiveBroadcastBuilder));

        public static LinkedProgressiveJackpotCurrentAmounts Build(EgmAdapter Egm)
        {
            if (Egm.LinkedProgressiveDevice == null || Egm.Games == null ) return null;

            if (Egm.IsSharedProgressiveComponentEnabled)
                return BuildForSharedProgressiveEgm(Egm);

            LinkedProgressiveJackpotCurrentAmounts LPBroadcast = new LinkedProgressiveJackpotCurrentAmounts();
            IEnumerable<IEgmGame> Games = Egm.Games.Cast<IEgmGame>();
            LPBroadcast.SystemDateTime = TimeProvider.UtcNow;
            byte noOfProgressiveLevels = Egm.LinkedProgressiveDevice.NumberOfProgressiveLevels;
            LPBroadcast.NumberOfProgressiveLevels =
               (ProgressiveLevel)(Enum.Parse(typeof(ProgressiveLevel), (noOfProgressiveLevels - 1).ToString(), true)) | ProgressiveLevel.Reserved;


            foreach(Game game in Games)
            {                
                if (!game.Enabled) continue;

                if (game.LinkedProgressiveLines == null) continue;

                var updatedProgressiveLines = game.LinkedProgressiveLines.TakeWhile((line) => line.LineAmount > 0);

                foreach (LinkedProgressiveLine line in updatedProgressiveLines)
                {
                    LPBroadcast.LinkedProgressiveData.Add(new LinkedProgressiveDetails()
                            {
                                LinkedProgressiveGroupId = ushort.Parse(game.ProgressiveGroupId),
                                LinkedProgressiveJackpotAmount = line.LineAmount,
                                LinkedProgressiveLevelId  = GetLevel((line.LineId - 1).ToString())
                            });
                }
            }


            return LPBroadcast.LinkedProgressiveData.Count == 0 ? null : LPBroadcast;
        }


        private static LinkedProgressiveJackpotCurrentAmounts BuildForSharedProgressiveEgm(EgmAdapter egm)
        {
            if (egm.CurrentGame == null) return null;

            var lpBroadcast = new LinkedProgressiveJackpotCurrentAmounts
                                  {
                                      SystemDateTime = TimeProvider.UtcNow.ToLocalTime()
                                  };

            var noOfProgressiveLevels = egm.LinkedProgressiveDevice.NumberOfProgressiveLevels;
            lpBroadcast.NumberOfProgressiveLevels =
                (ProgressiveLevel) (Enum.Parse(typeof (ProgressiveLevel), (noOfProgressiveLevels - 1).ToString(), true)) |
                ProgressiveLevel.Reserved;

            var progressiveId = egm.CurrentGame.ProgressiveGroupId;

            var updatedProgressiveLines = egm.SharedProgressiveLines;

            foreach (var line in updatedProgressiveLines)
            {
                lpBroadcast.LinkedProgressiveData.Add(new LinkedProgressiveDetails()
                {
                    LinkedProgressiveGroupId = !string.IsNullOrEmpty(progressiveId) ? UInt16.Parse(progressiveId) : (UInt16)0,
                    LinkedProgressiveJackpotAmount = line.LineAmount,
                    LinkedProgressiveLevelId = GetLevel((line.LineId - 1).ToString())
                });
            }

            return lpBroadcast;
        }


        private static ProgressiveLevel GetLevel(string levelNumber)
        {
            return ((ProgressiveLevel)(Enum.Parse(typeof(ProgressiveLevel), levelNumber, true)) | ProgressiveLevel.Reserved); 
        }
    }
}
