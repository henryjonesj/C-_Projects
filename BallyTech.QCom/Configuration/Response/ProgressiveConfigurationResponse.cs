using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.QCom.Configuration.Response;
using BallyTech.QCom.Configuration;

namespace BallyTech.QCom.Messages
{
    partial class ProgressiveConfigurationResponse : IGameConfiguration
    {
        private EgmGameProgressiveConfiguration _ProgressiveConfiguration = null;

        #region IGameConfiguration Members

        public int GameNumber
        {
            get { return this.GameVersionNumber; }
        }

        public string PayTableId
        {
            get { return this.GameVariationNumber.ToString(); }
        }

        public bool GameStatus { get; private set; }


        internal IGameConfiguration Reconcile(ConfigurationRepository repository)
        {
            var configurationId = QComConfigurationId.CreateIdWith(FunctionCodes.EgmGameConfiguration, GameNumber);

            var gameConfiguration = repository.GetConfigurationsOfType<QComGameConfiguration>().
                FirstOrDefault(item => item.Id.Equals(configurationId));
            if (gameConfiguration == null) return this;

            BackFill(gameConfiguration.ConfigurationData);

            return this;
        }


        private void BackFill(IGameConfiguration configuration)
        {
            _ProgressiveConfiguration = new EgmGameProgressiveConfiguration(this);
            _ProgressiveConfiguration.SetProgressiveId(configuration.ProgressiveConfiguration.ProgressiveGroupId);
            this.GameStatus = configuration.GameStatus;            
        }



        public IProgressiveConfiguration ProgressiveConfiguration
        {
            get { return _ProgressiveConfiguration; }
        }

        #endregion
    }
}
