using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using BallyTech.Gtm;
using System.Collections;
using BallyTech.QCom.Model.Egm;
using BallyTech.QCom.Model;
using log4net;

namespace BallyTech.QCom.Messages
{
    partial class ProgressiveMetersResponse
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(ProgressiveMetersResponse));

        internal virtual void UpdateProgressiveLevelInfo(Game game)
        {
        }

        internal virtual void SetProgressiveLevelAmount(ICollection<Game> games)
        {

        }

        protected void UpdateProgressiveLevelData(ProgressiveInfo progressiveInfo, Game game, decimal hits, decimal wins)
        {
            var progressiveType = progressiveInfo.QComProgressiveType ? GameProgressiveType.LinkedProgressive : GameProgressiveType.StandAloneProgressive;

            var progressiveLevelNumber = (byte)(progressiveInfo.ProgressiveLevelNumber + 1);

            var progressiveLevelInfo = new ProgressiveLevelInfo(progressiveLevelNumber, progressiveType).
                UpdateAmount(progressiveInfo.ConributionAmount);

            progressiveLevelInfo.UpdateMeters(hits, wins);

            game.UpdateProgressiveInfo(progressiveLevelInfo);
        }

        protected void UpdateContributionAmount(ICollection<Game> games, ProgressiveInfo levelInfo)
        {
            var progressiveLevelNumber = (byte)(levelInfo.ProgressiveLevelNumber + 1);

            var contributionInfo = new ProgressiveLevelInfo(progressiveLevelNumber,
                                                                GetProgressiveType(levelInfo.QComProgressiveType));
            contributionInfo.UpdateAmount(levelInfo.ConributionAmount);

            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Updating level {0} with level amount {1}", contributionInfo.LineId,
                                contributionInfo.LineAmount);

            games.ToList().ForEach((game) => game.UpdateCurrentProgressiveLevelAmount(contributionInfo));
        }


        public GameProgressiveType GetProgressiveType(bool IsLpType)
        {
            return IsLpType ? GameProgressiveType.LinkedProgressive : GameProgressiveType.StandAloneProgressive;
        }
    }

    partial class ProgressiveMetersV16Response
    {        
        internal const int LengthofNonRepeatedEntries = 20;

        internal override void UpdateProgressiveLevelInfo(Game game)
        {
            foreach (var levelData in ProgressiveDataList)
            {
                UpdateProgressiveLevelData(levelData.ProgressiveLevelData, game, levelData.ProgressiveHits, levelData.ProgressiveWins);
            }
        }

        public bool HasLpLevels()
        {
            return ProgressiveDataList.Any(element => element.ProgressiveLevelData.QComProgressiveType == true);
        }


        internal override void SetProgressiveLevelAmount(ICollection<Game> games)
        {
            foreach (var progressiveLevelData in ProgressiveDataList)
            {
                UpdateContributionAmount(games, progressiveLevelData.ProgressiveLevelData);
            }
        }

    }


    partial class ProgressiveMetersV15Response
    {
        internal override void UpdateProgressiveLevelInfo(Game game)
        {
            foreach (var levelData in ProgressiveDataList)
            {
                UpdateProgressiveLevelData(levelData.ProgressiveLevelData, game, 0, 0);
            }
        }

        public bool HasLpLevels()
        {
            return ProgressiveDataList.Any(element => element.ProgressiveLevelData.QComProgressiveType == true);
        }

    }
}
