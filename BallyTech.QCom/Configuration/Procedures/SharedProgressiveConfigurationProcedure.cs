using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Model;
using log4net;

namespace BallyTech.QCom.Configuration
{
    [GenerateICSerializable]
    public partial class SharedProgressiveConfigurationProcedure : QComConfigurationProcedure
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (SharedProgressiveConfigurationProcedure));

        public SharedProgressiveConfigurationProcedure()
        {
        }

        public SharedProgressiveConfigurationProcedure(QComModel model): base(model)
        {
            
        }

        internal override void OnConfigurationFailed(ApplicationMessage applicationMessage)
        {
            if (!IsExpectedResponse(applicationMessage)) return;

            base.OnConfigurationFailed(applicationMessage);

            UpdateSharedProgressiveStatus(EgmGameConfigurationStatus.Failure);
        }

        internal override void OnConfigurationSucceeded(ApplicationMessage applicationMessage)
        {
            if (!IsExpectedResponse(applicationMessage)) return;

            base.OnConfigurationSucceeded(applicationMessage);

            var configuration = ConfigurationSequence.GetNextConfigurationElement(_Repository);

            if (configuration == null) return;

            if (configuration.Id.ConfigurationType != FunctionCodes.ProgressiveConfiguration) return;

            _Log.InfoFormat("Setting Next Shared Progressive configuration: {0}", configuration.Id);

            UpdateSharedProgressiveStatus(EgmGameConfigurationStatus.Success);
        }

        private void UpdateSharedProgressiveStatus(EgmGameConfigurationStatus status)
        {
            _Repository.GetConfigurationsOfType<QComProgressiveConfiguration>().ForEach((config) => config.UpdateConfigurationStatus(status));
        }

        

    }
}
