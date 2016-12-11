using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Model;
using BallyTech.QCom.Model.Builders;
using BallyTech.Gtm;
using log4net;
using BallyTech.QCom.Model.Spam;

namespace BallyTech.QCom.Configuration
{
    [GenerateICSerializable]
    public partial class QComConfigurationProcedure : ConfigurationProcedureBase
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (QComConfigurationProcedure));

        public QComConfigurationProcedure()
        {
        }

        public QComConfigurationProcedure(QComModel model): base(model)
        {         
        
        }

        internal override void DoConfiguration(IQComConfiguration configuration)
        {
            var nextConfigurationPoll = GetNextConfigurationPoll(configuration);

            if (nextConfigurationPoll == null) return;

            _CurrentConfiguration.UpdateConfigurationStatus(EgmGameConfigurationStatus.InProgress);
            _Model.SendPoll(nextConfigurationPoll);
        }

        protected bool IsExpectedResponse(ApplicationMessage message)
        {
            return _CurrentConfiguration != null && _CurrentConfiguration.IsExpectedSuccessfulResponse(message);
        }


        internal override void OnConfigurationSucceeded(ApplicationMessage applicationMessage)
        {
            if (!IsExpectedResponse(applicationMessage)) return;

            if (_Log.IsInfoEnabled) _Log.InfoFormat("Successfully Configured for: {0}", _CurrentConfiguration.Id);                

   
            _CurrentConfiguration.UpdateConfigurationStatus(EgmGameConfigurationStatus.Success);

            Reset();
        }


        protected void Reset()
        {
            if(_CurrentConfiguration == null) return;

            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Resetting the configuration : {0}", _CurrentConfiguration.Id);

            _CurrentConfiguration = null;
        }



        internal override void OnConfigurationFailed(ApplicationMessage applicationMessage)
        {
            if (!IsExpectedResponse(applicationMessage)) return;

            if (_Log.IsInfoEnabled) _Log.InfoFormat("Configuration Failed for: {0}", _CurrentConfiguration.Id);                

            _CurrentConfiguration.UpdateConfigurationStatus(EgmGameConfigurationStatus.Failure);
            
            Reset();
        }

        private Request GetNextConfigurationPoll(IQComConfiguration configuration)
        {
            _CurrentConfiguration = configuration;

            return _CurrentConfiguration == null ? null : _CurrentConfiguration.ConfigurationPoll;
        }

    }
}
