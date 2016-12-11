using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using log4net;

namespace BallyTech.QCom.Configuration
{
    public static class ConfigurationSequence
    {        
        //TODO- Refactor this method
        internal static IQComConfiguration GetNextConfigurationElement(ConfigurationRepository repository)
        {
            IQComConfiguration configuration = repository.GetConfigurationOfType<QComEgmConfiguration>();

            if (configuration == null) return null;
            if (IsConfigurationFailed(configuration)) return null;

            if (!IsConfigurationFinished(configuration))
                return configuration;
            
             var gameConfiguration =  GetNextGameConfiguration(repository);
             if (gameConfiguration != null) return gameConfiguration;

            var progressiveConfiguration = GetNextProgressiveConfiguration(repository);
            if (progressiveConfiguration != null) return progressiveConfiguration;

            configuration = repository.GetConfigurationOfType<QComParameterConfiguration>();

            return !IsConfigurationFinished(configuration) ? configuration : null;
        }

        private static IQComConfiguration GetNextProgressiveConfiguration(ConfigurationRepository repository)
        {
            var progressiveConfigurations = repository.GetConfigurationsOfType<QComProgressiveConfiguration>();

            var egmConfiguration = repository.GetConfigurationsOfType<QComEgmConfiguration>().FirstOrDefault();

            return egmConfiguration._ProtocolVersion == ProtocolVersion.V16
                ? progressiveConfigurations.FirstOrDefault(progressiveConfiguration => !IsConfigurationAttempted(progressiveConfiguration)) : null;
        }

        private static IQComConfiguration GetNextGameConfiguration(ConfigurationRepository repository)
        {
            var gameConfigurations = repository.GetConfigurationsOfType<QComGameConfiguration>();

            return gameConfigurations.FirstOrDefault(gameConfiguration => !IsConfigurationAttempted(gameConfiguration));
        }


        private static bool IsConfigurationFailed(IQComConfiguration configuration)
        {
            return configuration.ConfigurationStatus == EgmGameConfigurationStatus.Failure;
        }


        private static bool IsConfigurationFinished(IQComConfiguration configuration)
        {
            if (configuration == null) return false;

            return configuration.ConfigurationStatus == EgmGameConfigurationStatus.Success;
        }

        private static bool IsConfigurationAttempted(IQComConfiguration configuration)
        {
            if (configuration == null) return true;

            return configuration.ConfigurationStatus == EgmGameConfigurationStatus.Success ||
                   configuration.ConfigurationStatus == EgmGameConfigurationStatus.Failure;
        }
    }
}
