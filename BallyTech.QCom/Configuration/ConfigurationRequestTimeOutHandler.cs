using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using log4net;
using BallyTech.QCom.Model;
using BallyTech.Utility.Time;
using BallyTech.Utility.Configuration;
using BallyTech.QCom.Model.Egm;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Configuration
{
    [GenerateICSerializable]
    public partial class ConfigurationRequestTimeOutHandler
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(ConfigurationRequestTimeOutHandler));

        private Scheduler _Timer = null;

        [AutoWire(Name = "QComModel")]
        public QComModel Model { get; set; }

        private Schedule _Schedule = null;
        [AutoWire]
        public Schedule Schedule
        {
            get { return _Schedule; }
            set
            {
                _Schedule = value;
                _Timer = new Scheduler(Schedule);
                _Timer.TimeOutAction += OnConfigurationRequestSchedulerTimedout;
            }
        }

        public TimeSpan ConfigurationRequestTimer { get; set; }

        public ConfigurationRequestTimeOutHandler()
        {
            ConfigurationRequestTimer = TimeSpan.FromSeconds(600);
        }

        private void Start()
        {
            _Log.Info("Configuration Request Scheduler Started");
            
            _Timer.Start(ConfigurationRequestTimer);

        }

        private void OnConfigurationRequestSchedulerTimedout()
        {
            if (IsConfigurationProcedureComplete) return;

            if (HasAnyConfigurationFailed) return;

            _Log.Info("Configuration Request Scheduler Timed Out");

            var configuration = GetTimedOutConfiguration();

            if (configuration == null) return;
            
            RaiseConfigurationTimedOutEvent(configuration);

            CompleteTimedOutConfiguration(configuration);
        
        }

        private void Stop()
        {
            _Log.Info("Configuration Request Scheduler Stopped");
            
            _Timer.Stop();
        }


        public void LinkedStatusChanged(LinkStatus linkStatus)
        {
            if (linkStatus != LinkStatus.Connecting)
                Stop();
            else
            {
                if (!_Timer.IsRunning) Start();
            }

            if (linkStatus == LinkStatus.Connected) UpdateEgmConfigurationStatus(ConfigurationStatus.None);
           
        }

        private void UpdateEgmConfigurationStatus(ConfigurationStatus status)
        {
            Model.EgmConfigurationStatus = status;
        }

        private bool AreAllValidationsCompleted
        {
            get
            {
                return
                   Model.ConfigurationRepository.GetAllConfigurations().All(
                        (element) => element.ValidationStatus == ValidationStatus.Success);
            }

        }

        private bool HasAnyConfigurationFailed
        {
            get
            {
                return Model.ConfigurationRepository.GetAllConfigurations().Any(
                    (element) => element.ValidationStatus == ValidationStatus.Failure);
            }
        }

            

        private bool IsConfigurationProcedureComplete
        {
            get { return Model.ConfigurationRepository.AreAllConfigurationsFinished && AreAllValidationsCompleted; }
        
        }

        private IQComConfiguration GetPendingConfiguration()
        {
            return Model.ConfigurationRepository.GetAllConfigurations().Where(
                             (element) => element.ConfigurationStatus != EgmGameConfigurationStatus.Success).FirstOrDefault();
        
        }

        private IQComConfiguration GetValidationPendingConfiguration()
        {
            return Model.ConfigurationRepository.GetAllConfigurations().Where(
                         (element) => element.ValidationStatus != ValidationStatus.Success).FirstOrDefault();
        }

        private IQComConfiguration GetTimedOutConfiguration()
        {
            return Model.ConfigurationRepository.IsAnyConfigurationPending ? GetPendingConfiguration() : GetValidationPendingConfiguration();
             
        }



        private void RaiseConfigurationTimedOutEvent(IQComConfiguration Configuration)
        {
            var gameConfiguration = Configuration as QComGameConfiguration;
            if (gameConfiguration != null)
            {
                BuildAndReportConfigurationTimedOutEvent(gameConfiguration.ConfigurationData.GameNumber, "Invalid Game Configuration");
                return;
            }

            var progressiveConfiguration = Configuration as QComProgressiveConfiguration;
            if (progressiveConfiguration != null)
            {
                BuildAndReportConfigurationTimedOutEvent(progressiveConfiguration.ConfigurationData.GameNumber, "Invalid Progressive Configuraiton");
                return;
            }

            BuildAndReportConfigurationTimedOutEvent(0, "Invalid Egm Configuration");

        
        }

        private void CompleteTimedOutConfiguration(IQComConfiguration Configuration)
        {
            Configuration.UpdateConfigurationStatus(EgmGameConfigurationStatus.Failure);

            Model.State.OnConfigurationTimeOut();

            if (!Model.IsConfigurationRequired) UpdateEgmConfigurationStatus(ConfigurationStatus.TimedOut);
        
        }

        private void BuildAndReportConfigurationTimedOutEvent(int gameNumber, string errorText)
        {
            Model.Egm.ExtendedEventData = new ExtendedEgmEventData()
            {
                GameNumber = (ushort)gameNumber,
                ErrorText = errorText
            };

            Model.Egm.ReportEvent(EgmEvent.GameConfigurationTimedOut);
        
        }


    }
}
