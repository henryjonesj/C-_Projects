using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.Gtm;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Configuration
{
    [GenerateICSerializable]
    public partial class GameConfigurationEqualityComparer
    {
        public GameConfigurationEqualityComparer()
        {
        }
        
        private IGameConfiguration ConfigurationData;

        public GameConfigurationEqualityComparer(IGameConfiguration hostConfiguration)
        {
            this.ConfigurationData = hostConfiguration;
        }

        public bool Compare(EgmGameConfigurationResponse response)
        {
            return ConfigurationData != null ? IsGameConfigurationValid(response) && IsProgressiveConfigurationValid(response) : false;

        }

        private bool IsGameConfigurationValid(EgmGameConfigurationResponse other)
        {
            return ConfigurationData.GameNumber == other.GameVersionNumber &&
                   Convert.ToByte(ConfigurationData.PayTableId) == other.CurrentGameVariationNumber &&
                   ConfigurationData.GameStatus == other.IsGameEnabled;
        }

        private bool IsProgressiveGroupIdValid(IProgressiveConfiguration progressiveConfiguration,EgmGameConfigurationResponse other)
        {  
           if (progressiveConfiguration.ProgressiveGroupId > 0)
                return progressiveConfiguration.ProgressiveGroupId == other.ProgressiveGroupIdOfGVN;
            else
                return other.ProgressiveGroupIdOfGVN == 0xFFFF;       
        }

        private bool IsLevelInformationValid(SerializableList<IProgressiveLevelConfiguration> levelInformation,EgmGameConfigurationResponse other)
        {
            if (!IsLevelCountEqual(levelInformation, other)) return false;

            if (other.NoOfProgressiveLevels == 0 && other.ProgressiveLevelBitMask != 255) return false;

            for (int i = 0; i < other.NoOfProgressiveLevels; i++)
            {
                var egmLevelTypeInformation = other.GetProgressiveTypeOfLevel(levelInformation[i].ProgressiveLevelNumber - 1);

                if (levelInformation[i].ProgressiveType != egmLevelTypeInformation)
                    return false;
            }

            return true;
        }

        private bool IsLevelCountEqual(SerializableList<IProgressiveLevelConfiguration> levelInformation, EgmGameConfigurationResponse other)
        {
            return levelInformation.Count == other.NoOfProgressiveLevels;
        }

        private bool IsProgressiveConfigurationValid(EgmGameConfigurationResponse other)
        {
            if (ConfigurationData.ProgressiveConfiguration == null) return other.NoOfProgressiveLevels == 0;

            var progressiveConfiguration = ConfigurationData.ProgressiveConfiguration;
            var levelInformation = progressiveConfiguration.ProgressiveLevelConfigurations.ToSerializableList();

            if (levelInformation != null && !IsLevelInformationValid(levelInformation, other)) return false;

            return IsProgressiveGroupIdValid(progressiveConfiguration, other);
        }
 
        
    }
}
