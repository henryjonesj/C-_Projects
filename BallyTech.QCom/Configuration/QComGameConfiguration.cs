using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Configuration;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using BallyTech.QCom.Messages;
using log4net;
using BallyTech.QCom.Model.Builders;

namespace BallyTech.QCom.Configuration
{
    [GenerateICSerializable]
    public partial class QComGameConfiguration : QComConfiguration<IGameConfiguration> , IEquatable<EgmGameConfigurationResponse>
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (QComGameConfiguration));

        private Request _ConfigurationPoll = null;

        public GameConfigurationEqualityComparer GameConfigurationEqualityComparer = null;

        internal bool HasSupportForVariationHotSwitching { get; set; }

        internal byte CurrentGameVariation { get; set; }

        internal int CurrentProgressiveGroupId { get; set; }

        internal bool CurrentGameStatus { get; set; }
        
        internal HotSwitchType HotSwitch { get; set; }

        public QComGameConfiguration()
        {

        }

        public QComGameConfiguration(IGameConfiguration configuration) : base()
        {            
            this.ConfigurationData = configuration;            
            this.Id = configuration.CreateId();            
        }



        internal override void Update(IGameConfiguration configuration)
        {
            var oldPayTableId = ConfigurationData.PayTableId;

            var oldGameConfiguration = ConfigurationData;

            var noChangeInConfiguration = ConfigurationData.AreEqual(configuration);

            if (noChangeInConfiguration)
            {
                if (_Log.IsInfoEnabled) _Log.Info("No Change In Configuration");
                return;
            }

            _Log.InfoFormat("Updating Game Configuration with {0}", configuration.GameNumber);

            base.Update(configuration);

            _ConfigurationPoll = IsEligibleForHotSwitching(oldGameConfiguration) ? QComConfigurationBuilder.BuildUpdatedConfiguration(this) : null;

        }
            
        

        private bool IsEligibleForHotSwitching(IGameConfiguration oldConfiguration)
        {
            if (IsGameConfigurationNotSuccessful()) return false;

            if (IsConfigurationNotChanged(oldConfiguration)) return false;

            if (IsCurrentGameVariationNotChanged() && IsCurrentProgressiveGroupIdNotChanged() && IsCurrentGameStatusNotChanged()) return false;

            SetHotSwitchType(oldConfiguration);

            return true;
        
        }

        private void SetHotSwitchType(IGameConfiguration oldConfiguration)
        {
            if (!IsVariationNotChanged(oldConfiguration.PayTableId)) HotSwitch |= HotSwitchType.GameVariation;

            if (!IsProgressiveGroupIdNotChanged(oldConfiguration)) HotSwitch |= HotSwitchType.ProgressiveGroupId;

            if (!IsGameStatusNotChanged(oldConfiguration.GameStatus)) HotSwitch |= HotSwitchType.GameStatus;        
        }


        private bool IsGameConfigurationNotSuccessful()
        {
            return CurrentGameVariation == 0 || CurrentProgressiveGroupId == 0;
        
        }

        private bool IsCurrentGameStatusNotChanged()
        {
            return (this.CurrentGameStatus == ConfigurationData.GameStatus);
        }

        private bool IsCurrentGameVariationNotChanged()
        {
            return Convert.ToByte(ConfigurationData.PayTableId) == CurrentGameVariation;

        }

        private bool IsCurrentProgressiveGroupIdNotChanged()
        {
            return ConfigurationData.ProgressiveConfiguration != null ?
                CurrentProgressiveGroupId == ConfigurationData.ProgressiveConfiguration.ProgressiveGroupId :
                CurrentProgressiveGroupId == 65535;
        }

        private bool IsConfigurationNotChanged(IGameConfiguration oldConfiguration)
        {
            return IsVariationNotChanged(oldConfiguration.PayTableId) && IsProgressiveGroupIdNotChanged(oldConfiguration) && IsGameStatusNotChanged(oldConfiguration.GameStatus);
        
        }

        private bool IsGameStatusNotChanged(bool oldGameStatus)
        {
            return ConfigurationData.GameStatus == oldGameStatus;
        }

        private bool IsVariationNotChanged(string oldPayTableId)
        {
            return Convert.ToByte(ConfigurationData.PayTableId) == Convert.ToByte(oldPayTableId);
        }

        private bool IsProgressiveGroupIdNotChanged(IGameConfiguration oldConfiguration)
        {
            return ConfigurationData.IsProgressiveGroupIdSame(oldConfiguration);
        }
       

        public override Request ConfigurationPoll
        {
            get { return _ConfigurationPoll ?? QComConfigurationBuilder.Build(this); }
        }

        public override Request ConfigurationRequest
        {
            get { return ConfigurationBasedRequestBuilder.Build(ConfigurationData); }
        }


        #region IEquatable<EgmGameConfigurationResponse> Members

        public bool Equals(EgmGameConfigurationResponse other)
        {
            GameConfigurationEqualityComparer = new GameConfigurationEqualityComparer(ConfigurationData);
            
            var areEqual = GameConfigurationEqualityComparer.Compare(other);
            
            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Egm Game Configuration Validation Status: {0}", areEqual);

            this.ValidationStatus = areEqual ? ValidationStatus.Success : ValidationStatus.Failure;
            if (areEqual) this.UpdateConfigurationStatus(EgmGameConfigurationStatus.Success);
            else
                _Log.WarnFormat("Host Configuration {0} mismatches with the game configuration {1}", this.ConfigurationData, other);

            return areEqual;


        }


        #endregion

    }
}
