using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Model;
using BallyTech.QCom.Messages;
using log4net;

namespace BallyTech.QCom.Configuration
{
    public class ConfigurationProcedureFactory
    {
        internal QComModel Model { get; set; }

        private static readonly ILog _Log = LogManager.GetLogger(typeof(ConfigurationManager));

        internal ConfigurationProcedureBase GetConfigurationProcedure(IQComConfiguration configuration)
        {
            if (configuration == null) return null;

            var poll = configuration.ConfigurationPoll;

            if (poll == null)
                return configuration.Id.ConfigurationType == FunctionCodes.ProgressiveConfiguration ? GetProcedureForNonCustomSapConfiguration(configuration) : null;

            _Log.InfoFormat("Poll Message Type:{0}", poll.MessageType);

            switch (poll.MessageType)
            {
                case FunctionCodes.EgmGameConfigurationChange:
                    return GetProcedureForHotSwitch(configuration);

                case FunctionCodes.EgmConfiguration:
                    return GetProcedureForEgmConfiguration(configuration);

                case FunctionCodes.ProgressiveConfiguration:
                    return GetProcedureForProgressiveConfiguration();

                default:
                    return new QComConfigurationProcedure(Model);
            }

        }

        private ConfigurationProcedureBase GetProcedureForEgmConfiguration(IQComConfiguration configuration)
        {
            var egmConfiguration = configuration as QComEgmConfiguration;

            return egmConfiguration.AwaitingForDenominationHotSwitch ? new DenominationSwitchProcedure(Model) : new QComConfigurationProcedure(Model);
        }

        private ConfigurationProcedureBase GetProcedureForProgressiveConfiguration()
        {
            var currentEgmConfiguration = Model.ConfigurationRepository.CurrentEgmConfiguration;

            return currentEgmConfiguration.IsSharedProgressiveComponentSupported
                       ? new SharedProgressiveConfigurationProcedure(Model)
                       : new QComConfigurationProcedure(Model);

        }

        private ConfigurationProcedureBase GetProcedureForHotSwitch(IQComConfiguration configuration)
        {
            var currentEgmConfiguration = Model.ConfigurationRepository.CurrentEgmConfiguration;

            var gameConfiguration = configuration as QComGameConfiguration;

            return currentEgmConfiguration.IsSharedProgressiveComponentSupported
                  && gameConfiguration.HotSwitch == HotSwitchType.ProgressiveGroupId
                  ? new SharedProgressivePgIdSwitchingProcedure(Model)
                  : new HotSwitchProcedure(Model);
        }

        private ConfigurationProcedureBase GetProcedureForNonCustomSapConfiguration(IQComConfiguration configuration)
        {
            configuration.UpdateConfigurationStatus(EgmGameConfigurationStatus.InProgress);

            return GetProcedureForProgressiveConfiguration();
        
        }

       
    }
}
