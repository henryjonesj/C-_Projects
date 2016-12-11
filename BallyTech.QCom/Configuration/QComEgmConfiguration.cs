using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using BallyTech.QCom.Messages;
using log4net;
using BallyTech.QCom.Model.Builders;

namespace BallyTech.QCom.Configuration
{
    [GenerateICSerializable]
    public partial class QComEgmConfiguration : QComConfiguration<IEgmConfiguration>, IEquatable<IQComEgmConfiguration>
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(QComEgmConfiguration));

        public QComEgmConfiguration()
        {
            ConfigurationData = null;

            AwaitingForDenominationHotSwitch = false;
        }

        public QComEgmConfiguration(IEgmConfiguration configuration): base()
        {            
            this.ConfigurationData = configuration;
            this.Id = configuration.CreateId();
        }


        internal override void Update(IEgmConfiguration configuration)
        {
            if (this.ConfigurationData.AreEqual(configuration))
            {
                if (_Log.IsInfoEnabled) _Log.Info("No change in configuration");
                return;
            }

            CheckAndUpdateHotDenominationSwitchStatus(configuration.CreditDenomination);

            this.ConfigurationData = null;
            this.ConfigurationData = configuration;
            base.Update(configuration);
        }


        public override Request ConfigurationPoll
        {
            get
            {
                return _ProtocolVersion == ProtocolVersion.V16
                           ? QComConfigurationBuilder.Build(this)
                           : QComV15ConfigurationBuilder.Build(this);
            }
        }

        public bool AwaitingForDenominationHotSwitch { get; set; }

        public override Request ConfigurationRequest
        {
            get { return ConfigurationBasedRequestBuilder.Build(ConfigurationData); }
        }



        private void CheckAndUpdateHotDenominationSwitchStatus(decimal newCreditDenomination)
        {
            if(ConfigurationData.CreditDenomination == newCreditDenomination) return;

            AwaitingForDenominationHotSwitch = true;

        }

        #region IEquatable<IEgmConfiguration> Members

        public bool Equals(IQComEgmConfiguration other)
        {
            var areEqual = this.ConfigurationData.AreEqual(other);
            
            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Egm Configuration Validation Status: {0}", areEqual);

            this.ValidationStatus = areEqual ? ValidationStatus.Success : ValidationStatus.Failure;

            if (areEqual) this.UpdateConfigurationStatus(EgmGameConfigurationStatus.Success);
            else
                _Log.WarnFormat("Host Configuration {0} mismatches with the game configuration {1}", this.ConfigurationData, other);


            return areEqual;
        }

        #endregion

        
    }


 


    

}
