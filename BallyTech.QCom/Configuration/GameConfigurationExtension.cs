using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model;
using BallyTech.Utility.Serialization;
using log4net;

namespace BallyTech.QCom.Configuration
{
    public static class GameConfigurationExtension
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (GameConfigurationExtension));
        
        public static QComConfigurationId CreateId(this IGameConfiguration gameConfiguration)
        {
            return new QComConfigurationId(FunctionCodes.EgmGameConfiguration, gameConfiguration.GameNumber);
        }

        public static bool IsInAccepatableFormat(this IGameConfiguration gameConfiguration)
        {
            return gameConfiguration.PayTableId.IsByteBcd();
        }

        public static bool IsProgressiveInformationValid(this IProgressiveConfiguration progressiveData)
        {
            if (progressiveData.ProgressiveLevelConfigurations == null) return true;

            var progressiveConfigurations = progressiveData.ProgressiveLevelConfigurations.ToSerializableList();

            var maxlevelConfig = progressiveConfigurations.OrderBy(item => item.ProgressiveLevelNumber).LastOrDefault();

            if (maxlevelConfig == null) return true;

            if (maxlevelConfig.ProgressiveLevelNumber != progressiveConfigurations.Count || maxlevelConfig.ProgressiveLevelNumber > 8)
                return false;

            return true;

        }

        internal static bool AreEqual(this IGameConfiguration oldConfiguration, IGameConfiguration newConfiguration)
        {
            var areEqual = oldConfiguration.GameNumber == newConfiguration.GameNumber &&
                            oldConfiguration.GameStatus == newConfiguration.GameStatus &&
                            oldConfiguration.PayTableId == newConfiguration.PayTableId;

            if (!areEqual) return false;

            return IsProgressiveConfigurationEqual(oldConfiguration.ProgressiveConfiguration,
                                                   newConfiguration.ProgressiveConfiguration);


        }


        internal static bool IsProgressiveConfigurationEqual(this IProgressiveConfiguration oldprogressiveConfig, IProgressiveConfiguration newProgressiveConfig)
        {
            if (oldprogressiveConfig == null && newProgressiveConfig == null) return true;

            if (oldprogressiveConfig == null) return false;
            if (newProgressiveConfig == null) return false;

            var areEqual = oldprogressiveConfig.ProgressiveGroupId == newProgressiveConfig.ProgressiveGroupId;
            if (!areEqual) return false;

            return IsProgressiveLevelConfigurationEqual(oldprogressiveConfig.ProgressiveLevelConfigurations,
                                                        newProgressiveConfig.ProgressiveLevelConfigurations);

        }

        internal static bool IsProgressiveGroupIdSame(this IGameConfiguration newConfiguration,IGameConfiguration oldConfiguration)
        {
            if (oldConfiguration.ProgressiveConfiguration == null || newConfiguration.ProgressiveConfiguration == null)
                return true;

            return oldConfiguration.ProgressiveConfiguration.ProgressiveGroupId == newConfiguration.ProgressiveConfiguration.ProgressiveGroupId;

        }


        private static bool IsProgressiveLevelConfigurationEqual(IEnumerable<IProgressiveLevelConfiguration> oldprogressivelevelConfigurations, IEnumerable<IProgressiveLevelConfiguration> newprogressivelevelConfigurations)
        {
            if (oldprogressivelevelConfigurations == null && newprogressivelevelConfigurations == null) return true;

            if (oldprogressivelevelConfigurations == null) return false;
            if (newprogressivelevelConfigurations == null) return false;

            if (oldprogressivelevelConfigurations.Count() != newprogressivelevelConfigurations.Count())
            {
                _Log.Info("Count mismatched in Progressive Levels");
                return false;
            }

            foreach (var oldprogressivelevelConfiguration in oldprogressivelevelConfigurations)
            {
                var configuration = oldprogressivelevelConfiguration;
                var newConfiguration =
                    newprogressivelevelConfigurations.FirstOrDefault(
                        (element) =>
                        element.ProgressiveLevelNumber == configuration.ProgressiveLevelNumber);

                if (newConfiguration == null) return false;

                var areEqual = newConfiguration.ContributionAmount == oldprogressivelevelConfiguration.ContributionAmount &&
                                newConfiguration.ProgressiveType == oldprogressivelevelConfiguration.ProgressiveType;

                if (!areEqual) return false;

            }

            return true;
        }

    }


}
