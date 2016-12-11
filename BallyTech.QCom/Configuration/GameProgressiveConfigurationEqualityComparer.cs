using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;

namespace BallyTech.QCom.Configuration
{
    public static class GameProgressiveConfigurationEqualityComparer
    {

        public static bool AreEqual(this IGameProgressiveConfiguration oldConfig,IGameProgressiveConfiguration newConfig)
        {

            var areEqual = oldConfig.GameNumber == newConfig.GameNumber &&
                           oldConfig.GameStatus == newConfig.GameStatus &&
                           oldConfig.PayTableId == newConfig.PayTableId &&
                           oldConfig.ProgressiveGroupId == newConfig.ProgressiveGroupId;

            if (areEqual)
                return AreProgressiveLevelConfigurationsEqual(oldConfig.ProgressiveLevelConfigurations,
                                                              newConfig.ProgressiveLevelConfigurations);

            return false;
        }


        private static bool AreProgressiveLevelConfigurationsEqual(IEnumerable<IProgressiveLevelConfiguration> oldConfigurations, IEnumerable<IProgressiveLevelConfiguration> newConfigurations)
        {
            if (oldConfigurations.Count() != newConfigurations.Count()) return false;

            return !(from oldConfig in oldConfigurations
                     let newConfig = newConfigurations.FirstOrDefault(item => item.ProgressiveLevelNumber == oldConfig.ProgressiveLevelNumber)
                     where !DoesProgressiveConfigurationMatch(oldConfig, newConfig)
                     select oldConfig).Any();
        }



        private static bool DoesProgressiveConfigurationMatch(IProgressiveLevelConfiguration oldConfiguration,IProgressiveLevelConfiguration newConfiguration)
        {
            return oldConfiguration.AuxPayback == newConfiguration.AuxPayback &&
                   oldConfiguration.CeilingAmount == newConfiguration.CeilingAmount &&
                   oldConfiguration.ContributionAmount == newConfiguration.ContributionAmount &&
                   oldConfiguration.Increment == newConfiguration.Increment &&
                   oldConfiguration.ProgressiveLevelNumber == newConfiguration.ProgressiveLevelNumber &&
                   oldConfiguration.ProgressiveType == newConfiguration.ProgressiveType &&
                   oldConfiguration.StartupAmount == newConfiguration.StartupAmount;
        }

    }
}
