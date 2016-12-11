using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Configuration
{
    public interface IGameProgressiveConfiguration : IProgressiveConfiguration
    {
        int GameNumber { get; }
        string PayTableId { get; }
        bool GameStatus { get; }
    }

    [GenerateICSerializable]
    public partial class GameProgressiveConfiguration : IGameProgressiveConfiguration
    {

        public GameProgressiveConfiguration()
        {
        }

        public GameProgressiveConfiguration(IGameConfiguration gameConfiguration)
        {
            GameNumber = gameConfiguration.GameNumber;
            GameStatus = gameConfiguration.GameStatus;
            PayTableId = gameConfiguration.PayTableId;

            var progressiveConfigurtaion = gameConfiguration.ProgressiveConfiguration;
            ProgressiveGroupId = progressiveConfigurtaion.ProgressiveGroupId;
            ProgressiveLevelConfigurations = progressiveConfigurtaion.ProgressiveLevelConfigurations;
        }


        #region IGameProgressiveConfiguration Members

        public int GameNumber { get; private set; }

        public string PayTableId { get; private set; }

        public bool GameStatus { get; private set; }

        #endregion

        #region IProgressiveConfiguration Members

        public decimal ProgressiveGroupId { get; private set; }

        public IEnumerable<IProgressiveLevelConfiguration> ProgressiveLevelConfigurations { get; private set; }

        #endregion
    }

}
