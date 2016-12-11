using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using BallyTech.Gtm;
using log4net;
using BallyTech.QCom.Model.Specifications;
using BallyTech.QCom.Model;

namespace BallyTech.QCom.Configuration
{
    [GenerateICSerializable]
    public partial class ConfigurationValidator
    {
        private ConfigurationRepository _ConfigurationRepository = null;

        private static readonly ILog _Log = LogManager.GetLogger(typeof(ConfigurationValidator));

        private QComModel _Model;
        public QComModel Model
        {
            get { return _Model; }
        }


        public ConfigurationValidator()
        {
            
        }

        public ConfigurationValidator(QComModel Model)
        {
            _Model = Model;

            _ConfigurationRepository = Model.ConfigurationRepository;

            var egmConfiguration = _ConfigurationRepository.GetConfigurationOfType<QComEgmConfiguration>();

            if (egmConfiguration == null) return;

        }

        internal bool IsValid(EgmConfigurationResponse egmConfigurationResponse)
        {
            var egmConfiguration = _ConfigurationRepository.GetConfigurationOfType<QComEgmConfiguration>();

            return egmConfiguration.Equals(egmConfigurationResponse);
        }


        internal bool IsValid(EgmGameConfigurationResponse egmGameConfigurationResponse)
        {
            var allGameConfigurations = _ConfigurationRepository.GetConfigurationsOfType<QComGameConfiguration>();
            var gameConfiguration = allGameConfigurations.FirstOrDefault((element) => element.Id.Equals(egmGameConfigurationResponse.ConfigurationId));

            return gameConfiguration != null && gameConfiguration.Equals(egmGameConfigurationResponse);
        }


        internal bool IsValid(ProgressiveConfigurationResponse progressiveConfigurationResponse)
        {
            var progressiveConfiguration = _ConfigurationRepository.GetHostProgressiveConfiguration(progressiveConfigurationResponse);

            if (progressiveConfiguration == null) return false;

            return progressiveConfiguration.Equals(progressiveConfigurationResponse);
              
        }

        internal bool AreAllValidationsCompleted
        {
            get
            {
                return
                    _ConfigurationRepository.GetAllConfigurations().All(
                        (element) => element.ValidationStatus == ValidationStatus.Success);
            }

        }



    }
}
