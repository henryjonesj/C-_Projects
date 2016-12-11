using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using BallyTech.QCom.Messages;

namespace BallyTech.QCom.Configuration.Response
{
    [GenerateICSerializable] 
    public partial class EgmGameProgressiveConfiguration : IProgressiveConfiguration
    {
        private SerializableList<ProgressiveLevelConfiguration> _LevelConfigurations =
            new SerializableList<ProgressiveLevelConfiguration>();

        public EgmGameProgressiveConfiguration()
        {
        }


        public EgmGameProgressiveConfiguration(EgmGameConfigurationResponse gameConfigurationResponse)
        {
            this.ProgressiveGroupId = gameConfigurationResponse.ProgressiveGroupIdOfGVN;
            UpdateLevelConfigurations(gameConfigurationResponse);
        }

        public EgmGameProgressiveConfiguration(ProgressiveConfigurationResponse progressiveConfiguration)
        {
            if(progressiveConfiguration.NumberOfProgressiveLevels == 0) return;

            var progressiveLevelConfigurations = progressiveConfiguration.ProgressiveConfigurationList;

            for (var levelNo = 0; levelNo < progressiveLevelConfigurations.Count; levelNo++)
            {
                var levelType = progressiveLevelConfigurations[levelNo].ProgressiveLevelFlag;

                var progressiveLevelConfiguration = new ProgressiveLevelConfiguration(levelNo + 1,
                                                    progressiveLevelConfigurations[levelNo].GetLevelType(levelType));

                progressiveLevelConfiguration.Update(progressiveLevelConfigurations[levelNo]);

                _LevelConfigurations.Add(progressiveLevelConfiguration);
            }
        }

        private void UpdateLevelConfigurations(EgmGameConfigurationResponse gameConfiguration)
        {
            if(gameConfiguration.NoOfProgressiveLevels == 0) return;

            for (var levelNo = 0; levelNo < gameConfiguration.NoOfProgressiveLevels; levelNo++)
            {
                var progressiveLevelConfiguration = new ProgressiveLevelConfiguration(levelNo + 1,
                                                                                      gameConfiguration.GetProgressiveTypeOfLevel(levelNo));
                _LevelConfigurations.Add(progressiveLevelConfiguration);
            }
        }


        internal void SetProgressiveId(decimal progressiveId)
        {
            this.ProgressiveGroupId = progressiveId;
        }


        #region IProgressiveConfiguration Members

        public decimal ProgressiveGroupId { get; private set; }

        public IEnumerable<IProgressiveLevelConfiguration> ProgressiveLevelConfigurations
        {
            get { return  _LevelConfigurations.Cast<IProgressiveLevelConfiguration>(); }
        }

        #endregion
    }
}
