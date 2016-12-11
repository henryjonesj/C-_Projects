using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Model;
using log4net;

namespace BallyTech.QCom.Configuration
{
    [GenerateICSerializable]
    public partial class RemoteConfigurationRequestHandler : ConfigurationRequestHandlerBase
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (RemoteConfigurationRequestHandler));

        internal override void RequestConfiguration()
        {  
            var repository = Model.ConfigurationRepository;

            if(repository.AwaitingForConfiguration)
            {
                if (_Log.IsInfoEnabled) _Log.Info("Waiting for configurations from host");
                return;
            }

            if (repository.AllConfigurationsAvailable)
            {
                if (_Log.IsDebugEnabled) _Log.Debug("All configurations are available");
                return;
            }

            if (_Log.IsInfoEnabled) _Log.Info("Requesting configurations from host");

            repository.ConfigurationRequested();
            Model.Egm.RequestConfiguration();
        }
    }
}
