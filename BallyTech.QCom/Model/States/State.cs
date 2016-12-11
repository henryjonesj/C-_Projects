using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Model.Builders;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.Egm;
using BallyTech.Utility;
using log4net;
using BallyTech.Utility.Diagnostics;
using BallyTech.QCom.Configuration;
using System.Collections;
using BallyTech.QCom.Model.Meters;
using BallyTech.QCom.Model.Specifications;
using BallyTech.QCom.Model.Handlers;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class State : ApplicationMessageListener
    {
        private readonly static ILog _Log = LogManager.GetLogger(typeof(State));
        private TimeSpan _ConnectTime = TimeSpan.FromSeconds(10);
        protected TimeSpan _PollAddressConfigDelay = TimeSpan.FromSeconds(2);

        //Purge poll starts with sequence number 0. Response will advance to 1.
        private const byte FirstPurgePollResponseSequenceNumber = 0x02;
        private const uint GeneralStatusResponseCountLimit = 2;

        protected ConfigurationValidator _ConfigurationValidator = null;

        protected MessageReceivedCounter<GeneralStatusResponse> _GeneralStatusResponseCounter = 
                        new MessageReceivedCounter<GeneralStatusResponse>().WithCountLimit(GeneralStatusResponseCountLimit);

        protected QComModel _Model;
        public QComModel Model
        {
            get { return _Model; }
            set { _Model = value; }
        }

        private QComResponseSpecification _MeterGroupContributionValidationSpecification = null;
        private QComResponseSpecification _SerialNumSpecification = null;

        public virtual void DataLinkStatusChanged(bool LinkUp)
        {

        }

        public virtual void Enter()
        { }

        public virtual void Exit()
        { }

        public override void Process(EventResponse eventResponse)
        {
            Model.EventListenerCollection.Dispatch(eventResponse.EventData);
        }

        public virtual void ProcessResponse(ApplicationMessage response)
        { 
            OnResponseReceived();
            response.Dispatch(this);            
            
            if (Model.RaleHandler.State != RaleProgressState.Complete)
            {
                _GeneralStatusResponseCounter.Received(response);
                if (_GeneralStatusResponseCounter.IsCountLimitReached)
                {
                    _GeneralStatusResponseCounter.Reset();
                    Model.RaleHandler.AllLoggedEventsReceived();
                }
            }
        }

        internal virtual void OnConfigurationReceived(IEgmConfiguration egmConfiguration, ICollection<IGameConfiguration> gameConfigurations)
        {
             UpdateConfigurations(egmConfiguration, gameConfigurations);            
        }

        protected bool UpdateConfigurations(IEgmConfiguration egmConfiguration, ICollection<IGameConfiguration> gameConfigurations)
        {
            try
            {
                Model.ConfigurationRepository.Update(egmConfiguration, gameConfigurations);
                return true;   
            }
            catch (InvalidEgmConfigurationException ex)
            {
                Model.Egm.ReportMismatchedConfiguration(EgmEvent.InvalidEGMConfiguration, ex.reason);
                return false;
            }
            catch (InvalidGameConfigurationException ex)
            {
                Model.Egm.ReportMismatchedConfiguration(EgmEvent.InvalidGameConfiguration,ex.reason);
                return false;
            }
            catch (InvalidProgressiveConfigurationException ex)
            {
                Model.Egm.ReportMismatchedConfiguration(EgmEvent.InvalidProgressiveConfiguration,ex.reason);
                return false;
            }
        }


        protected virtual void OnResponseReceived() { }

        public virtual LinkStatus LinkStatus
        {
            get { return LinkStatus.Disconnected; }
        }

        public override void Process(LPContribution meterResponse)
        { 
            if (ShouldIgnoreMeters(meterResponse)) return;

            _MeterGroupContributionValidationSpecification = Model.SpecificationFactory.GetSpecification(FunctionCodes.MeterGroupContributionResponse);
            if (!_MeterGroupContributionValidationSpecification.IsSatisfiedBy(meterResponse)) return;

            Model.Egm.SetCurrentGame(meterResponse.LastGameVersionNumber);

            Meter oldMeter = Meter.Zero;
            GameDetails gameDetails = new GameDetails()
            {
                GameVersionNumber = meterResponse.LastGameVersionNumber,
                ProgressiveAmount = meterResponse.LPContributionData.ProgressiveTurnoverMeter,
                ProgressiveGroupId = meterResponse.LPContributionData.ProgressiveGroupId
            };
            if (!Model.UpdateLinkedProgressiveContributionAmount(gameDetails, out oldMeter))
                Model.BuildAndReportLpContributionIgnoredEvent(gameDetails, oldMeter);

            Model.MeterTracker.UpdateMeters(meterResponse.MeterGroups);
            Model.OnMetersUpdated(meterResponse.MeterGroups);            
        }

        private bool ShouldIgnoreMeters(MeterGroupContributionResponse meterResponse)
        {
            if (meterResponse.MeterGroups.Count <= 0) return true;

            if (!Model.Egm.SoftwareAuthenticationDevice.IsRomSignatureVerificationComplete && Model.Egm.SoftwareAuthenticationDevice.IsMeterExclusionRequired)
            {
                if (_Log.IsWarnEnabled) _Log.WarnFormat("Ignoring meters as Rom Signature Verification is not complete");
                return true;
            }

            return false;
        
        }
        
        public override void Process(MeterGroupContributionResponse meterResponse)
        {
            if (ShouldIgnoreMeters(meterResponse)) return;

            _MeterGroupContributionValidationSpecification = Model.SpecificationFactory.GetSpecification(FunctionCodes.MeterGroupContributionResponse);
            if (!_MeterGroupContributionValidationSpecification.IsSatisfiedBy(meterResponse)) return;

            Model.Egm.SetCurrentGame(meterResponse.LastGameVersionNumber);

            Model.PlayOutsideLicensedHoursHandler.HandlePlayOutsideLicensedHours(meterResponse.MeterGroups);

            UpdateMeters(meterResponse.MeterGroups);            
        }

        protected void UpdateMeters(SerializableList<MeterInfo> meterGroups)
        {
            Model.MeterTracker.UpdateMeters(meterGroups);
            Model.EctToEgmTimeoutDetector.MetersReceived(meterGroups);
            Model.OnMetersUpdated(meterGroups);
        }


        public override void Process(MultiGameVariationMetersResponse gameMeterResponse)
        {
            if (!IsGameInfoValid(gameMeterResponse.GameVersionNumber, gameMeterResponse.GameVariationNumber))
            {
                _Log.Info("Invalid GVN or VAR. Hence ignoring the response");
                return;
            }
            gameMeterResponse.UpdateMeterGroups();
            ProcessMultiGameVariationMetersResponse(gameMeterResponse);
        }

        public override void Process(MultiGameVariationMetersResponseV16 gameMeterResponse)
        {
            if (!IsGameInfoValid(gameMeterResponse.GameVersionNumber, gameMeterResponse.GameVariationNumber))
            {
                _Log.Info("Invalid GVN or VAR. Hence ignoring the response");
                return;
            }

            gameMeterResponse.UpdateMeterGroups();
            ProcessMultiGameVariationMetersResponse(gameMeterResponse);
        }

        private bool IsGameInfoValid(ushort gameVersion, byte gameVariation)
        {
            QComResponseSpecification _MultigameVariationSpecification = Model.SpecificationFactory.GetSpecification(FunctionCodes.MultiGameVariationMetersResponse);
            return _MultigameVariationSpecification.IsGameInfoValid(gameVersion, gameVariation);
        }

        public override void Process(NoteAcceptorStatusResponse response)
        {
            Model.MessageProcessorCollection.Dispatch(response);
        }

        private void ProcessMultiGameVariationMetersResponse(MultiGameVariationMetersResponse gameMeterResponse)
        {
            Model.MessageProcessorCollection.Dispatch(gameMeterResponse);
        }
        
        protected void CheckEgmDetails()
        {
            if (Model.Egm.IsValid) return;
            _Model.CheckAndRestoreEgmDetails();

            Model.IsConfigurationRequired = IsConfigurationMissing();            
        }

        public override void Process(ApplicationMessage response)
        {
            Model.MessageProcessorCollection.Dispatch(response);
        }

        public virtual void InitializationComplete() { }

        public override void Process(EgmConfigurationResponse response) 
        {            
            ProcessEgmConfiguration(response);
        }

        private void ProcessEgmConfiguration(EgmConfigurationResponse response)
        {
            if (!response.IsProtocolValid)
            {
                _Log.Info("Ignoring response as protocols do not match");
                return;
            }

            Model.MessageProcessorCollection.Dispatch(response);
            Model.EgmGameCount = response.TotalNumberOfGamesAvailable;
            Model.SaveEgmDetails(response.ManufacturerId, response.EgmSerialNumber);
        }


        public override void Process(EgmGameConfigurationResponse applicationMessage)
        {
            if (!applicationMessage.IsNumberOfVariationAvailableValid)
            {
                _Log.Info("Ignoring the game configuration response as the number of variations received is invalid");
                return;
            }

            Model.MessageProcessorCollection.Dispatch(applicationMessage);
        }



        public override void Process(PurgeEventsPollAcknowledgementResponse applicationMessage)
        {
            Model.MessageProcessorCollection.Dispatch(applicationMessage);
        }

        public override void Process(PurgeEventsPollAcknowledgementResponseV16 applicationMessage)
        {
            Model.MessageProcessorCollection.Dispatch(applicationMessage);
        }

        protected bool HaveReceivedAllGameConfigurationResponses()
        {
            return Model.Egm.GameCount == Model.EgmGameCount;
        }


        protected bool IsValidEgmConfiguration(EgmConfigurationResponse configurationResponse)
        {
            bool isValid = IsEgmDetailsMatchesWithConfiguration(configurationResponse);

            _SerialNumSpecification = Model.SpecificationFactory.GetSpecification(FunctionCodes.EgmConfigurationResponse);

            if (!_SerialNumSpecification.IsSatisfiedBy(configurationResponse.EgmSerialNumber, configurationResponse.ManufacturerId))
            {
                Model.Egm.ExtendedEventData = new ExtendedEgmEventData()
                {
                    GameSerialNumber = configurationResponse.SerialNumber,
                    ManufacturerId = configurationResponse.ManufacturerId.ToString()
                };
                Model.Egm.ReportEvent(EgmEvent.InvalidSerialNumber);
                return false;
            }
            
            if (!Model.IsRemoteConfigurationEnabled) return isValid;

            isValid = _ConfigurationValidator.IsValid(configurationResponse);

            return isValid;
        }
 

        private bool IsEgmDetailsMatchesWithConfiguration(EgmConfigurationResponse configurationResponse)
        {
            return Model.EgmDetails.SerialNumber == configurationResponse.EgmSerialNumber &&
                   Model.EgmDetails.ManufacturerId == configurationResponse.ManufacturerId;
        }

        protected bool IsValidGameConfiguration(EgmGameConfigurationResponse configurationResponse)
        {
            if ((configurationResponse.CurrentGameVariationNumber <= 0m)) return false;

            return !Model.IsRemoteConfigurationEnabled || _ConfigurationValidator.IsValid(configurationResponse);
        }


        protected bool IsValidProgressiveConfiguration(ProgressiveConfigurationResponse response)
        {
            if (!Model.IsRemoteConfigurationEnabled) return true;

            if (response.NumberOfProgressiveLevels == 0) return false;

            response.CustomSapValidationSpecification = Model.SpecificationFactory.GetSpecification(FunctionCodes.ProgressiveConfigurationResponse);

            _Log.Info("Validating Progressive Configuration");

            return _ConfigurationValidator.IsValid(response);
        }


        public virtual void NoResponseReceived()
        {
            if (Model.IsConfigurationRequired) return;

            if (Model.IsEgmDetailsPresent)            
                AttemptToConnect();                
        }

        protected virtual void AttemptToConnect()
        {
            if (!(Model.IsEgmDetailsPresent)&& Model.ShouldSendSeekEgmBroadcastPoll)
            {
                Model.SendPoll(new SeekEgmBroadcastPoll());
                return;
            }
            if (Model.PollAddressConfigScheduler.IsRunning) return;

            if (Model.IsEgmDetailsPresent)
                Model.PollAddressConfigScheduler.Start(_PollAddressConfigDelay);
            else
                RequestConfigurationIfNecessary();
  
        }

        public override void Process(SeekEgmBroadcastResponse response)
        {
            Model.RestoreAssetNumberFromRepository();
            Model.SaveEgmDetails(response.ManufacturerId, response.EgmSerialNumber);
        }


        public bool IsConfigurationMissing()
        {
            if (Model.IsEgmDetailsEntryRequired)
                return Model.EgmDetails.AssetNumber == 0 || Model.EgmDetails.SerialNumber == 0 || Model.EgmDetails.ManufacturerId == 0;

            return Model.EgmDetails.AssetNumber == 0;
        }

        public virtual void SetEgmDetails(uint assetNumber, decimal manufactureId, decimal serialNumber)
        {
            _Log.InfoFormat("@ State. setting egm details . asset no = {0}", assetNumber);
            Model.EgmDetails.AssetNumber = assetNumber;

            Model.SaveDetailsForRepository(Model.EgmDetails.AssetNumber, manufactureId, serialNumber);

            _Model.IsConfigurationRequired = !(EgmDetailsReceived());
            Model.Egm.SetEgmInfo(Model.EgmDetails);

            RequestConfigurationIfNecessary();
        }

        protected bool EgmDetailsReceived()
        {
            return Model.IsEgmDetailsEntryRequired ?
                    (Model.EgmDetails.SerialNumber != 0 && Model.EgmDetails.ManufacturerId != 0 && Model.EgmDetails.AssetNumber != 0) 
                    : Model.EgmDetails.AssetNumber != 0;
        }

        public virtual void RequestConfigurationIfNecessary()
        {
            var configurationRequestHandler =
                ConfigurationRequestHandlerFactory.GetRequestHandler(Model);
            configurationRequestHandler.RequestConfiguration();
        }

        public override void Process(ProgramHashResponse applicationMessage)
        {
            _Model.Egm.ValidateSignature(applicationMessage.ProgramHash);         
        }

        public virtual void OnConfigurationTimeOut() { }
       
    }
}
