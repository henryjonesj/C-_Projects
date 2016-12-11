using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.Utility.Serialization;
using log4net;
using BallyTech.QCom.Model;

namespace BallyTech.QCom.Configuration
{
    [GenerateICSerializable]
    public partial class SharedProgressivePgIdSwitchingProcedure : HotSwitchProcedure
    {

        private static readonly ILog _Log = LogManager.GetLogger(typeof (SharedProgressivePgIdSwitchingProcedure));

        public SharedProgressivePgIdSwitchingProcedure()
        {
        }

        public SharedProgressivePgIdSwitchingProcedure(QComModel model): base(model)
        {
        }


        internal override void OnConfigurationFailed(ApplicationMessage applicationMessage)
        {
            if (!IsExpectedResponse(applicationMessage)) return;

            base.OnConfigurationFailed(applicationMessage);

            UpdateSharedProgressiveHotSwitchSuccessStatus(EgmGameConfigurationStatus.Failure);
        }

        internal override void OnConfigurationSucceeded(ApplicationMessage applicationMessage)
        {
            if (!IsExpectedResponse(applicationMessage)) return;
            
            base.OnConfigurationSucceeded(applicationMessage);
            UpdateNextConfigurationStatus();          
        }

        private void UpdateNextConfigurationStatus()
        {
            var configuration = ConfigurationSequence.GetNextConfigurationElement(_Repository);

            if(configuration == null) return;
            if (configuration.Id.ConfigurationType != FunctionCodes.EgmGameConfiguration) return;

            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Setting Next Game configuration: {0}", configuration.Id);

            UpdateSharedProgressiveHotSwitchSuccessStatus(EgmGameConfigurationStatus.Success);
        }

        private void UpdateSharedProgressiveHotSwitchSuccessStatus(EgmGameConfigurationStatus status)
        {
            _Repository.GetConfigurationsOfType<QComGameConfiguration>().ForEach((config) => config.UpdateConfigurationStatus(status));
        }

    }
}
