using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.Gtm;
using BallyTech.Utility.Serialization;
using log4net;

namespace BallyTech.QCom.Configuration
{
    [GenerateICSerializable]
    public partial class ProgressiveConfigurationEqualityComparer
    {
        public ProgressiveConfigurationEqualityComparer()
        {
        
        }
        
        private IGameProgressiveConfiguration ConfigurationData;

        private bool CanValidateCustomSapParamters = false;

        private static readonly ILog _Log = LogManager.GetLogger(typeof(ProgressiveConfigurationEqualityComparer));

        public ProgressiveConfigurationEqualityComparer(IGameProgressiveConfiguration hostConfiguration,bool CanValidateCustomSap)
        {
            this.ConfigurationData = hostConfiguration;
            CanValidateCustomSapParamters = CanValidateCustomSap;

        }
        
        public bool Compare(ProgressiveConfigurationResponse response)
        {
            return ConfigurationData != null ? IsGameConfigurationEqual(response) && AreProgressiveLevelConfigurationValid(response) : false;
        }

        private bool IsGameConfigurationEqual(ProgressiveConfigurationResponse response)
        {
            return ConfigurationData.GameNumber == response.GameVersionNumber &&
                   Convert.ToByte(ConfigurationData.PayTableId) == response.GameVariationNumber &&
                   ConfigurationData.ProgressiveLevelConfigurations.Count() == response.NumberOfProgressiveLevels;
        }

        private bool AreProgressiveLevelConfigurationValid(ProgressiveConfigurationResponse response)
        {
            var hostProgressiveConfiguration = ConfigurationData.ProgressiveLevelConfigurations.OrderBy(item => item.ProgressiveLevelNumber);
            var egmProgressiveConfiguration = response.ProgressiveConfigurationList;

            foreach (var hostconfig in hostProgressiveConfiguration)
            {
                var egmconfig = response.GetEgmProgressiveLevelConfiguration(egmProgressiveConfiguration, hostconfig);

                if (egmconfig == null)
                {
                    _Log.InfoFormat("No matching configuration was found");
                    return false;
                }

                if(!ShouldValidateConfiguration(response,hostconfig)) continue;
                if (!AreCustomSapParametersEqual(hostconfig, egmconfig)) return false;
            }

            return true;
        }

        private bool ShouldValidateConfiguration(ProgressiveConfigurationResponse response,IProgressiveLevelConfiguration configuration)
        {
            if (!CanValidateCustomSapParamters) return false;
            if (response.CustomSAPCapabilityFlag) return true;

            return (configuration.ProgressiveType == GameProgressiveType.LinkedProgressive);
        }


        private static bool AreCustomSapParametersEqual(IProgressiveLevelConfiguration hostConfig, ProgressiveConfigurationGroup egmConfig)
        {
            _Log.InfoFormat(" Validating for Egm {0},{1},{2},{3},{4}", egmConfig.ProgressiveLevelFlag,egmConfig.CeilingAmount,egmConfig.AuxPayback/10000, egmConfig.Increment/10000,egmConfig.StartupAmount);
            _Log.InfoFormat("Validating for Host {0},{1},{2},{3},{4}", hostConfig.ProgressiveLevelNumber, hostConfig.CeilingAmount,hostConfig.AuxPayback,hostConfig.Increment,hostConfig.StartupAmount);
            
            return egmConfig.CeilingAmount == hostConfig.CeilingAmount
                               && egmConfig.AuxPayback / 10000 == hostConfig.AuxPayback
                               && egmConfig.Increment / 10000 == hostConfig.Increment
                               && egmConfig.StartupAmount == hostConfig.StartupAmount;
        }

    }
}