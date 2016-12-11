using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using log4net;
using BallyTech.Gtm;
using BallyTech.Gtm.Core;
using BallyTech.QCom.Messages;
using BallyTech.Utility;
using BallyTech.Utility.Transactions;
using BallyTech.QCom.Model.Egm;
using BallyTech.QCom.Model.EventsListeners;
using BallyTech.Utility.Time;
using BallyTech.QCom.Model.Meters;
using BallyTech.QCom.Metadata;
using BallyTech.QCom.Model.MessageProcessors;
using BallyTech.Utility.Configuration;
using BallyTech.QCom.Model.Builders;
using BallyTech.QCom.Configuration;
using BallyTech.QCom.Model.Handlers;
using BallyTech.Utility.Management;
using BallyTech.QCom.Model.Specifications;

namespace BallyTech.QCom.Model
{
    public enum ConfigurationStatus
    {
        None,
        EntryRequired,
        InProgress,
        Failed,
        TimedOut
    }

    [GenerateICSerializable]
    [Deterministic]
    public partial class QComModel : IQComModel
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(QComModel));

        private PollQueue _PollQueue = null;
        private EgmAdapter _Egm = null;
        public EgmInfo EgmDetails { get; private set; }
            
        private bool _IsListeningMode = false;

        private bool _RequestGameConfigurationViaEgmConfigurationRequestPoll = false;
        private bool _ShouldSendSeekEgmBroadcastPoll = false;

        internal Action OnGameIdle = delegate { };

        private bool _EgmDetailsEntryRequired = false;
        
        internal decimal EgmGameCount { get; set; }
        internal byte ECTPollSequenceNumber { get; set; }
        
        internal ProtocolVersion ProtocolVersion { get; private set; }
        internal SpamHandler SpamHandler { get; private set; }

        internal RaleHandler RaleHandler { get; private set; }
        internal PlayOutsideLicensedHoursHandler PlayOutsideLicensedHoursHandler { get; private set; }
        internal ProgramHashHandler ProgramHashHandler { get; private set; }

        internal ConfigurationRepository ConfigurationRepository { get; private set; }
        internal ConfigurationRequestHandlerBase ConfigurationRequestHandler { get; private set; }
        public ConfigurationRequestTimeOutHandler ConfigurationRequestTimeOutHandler { get; set; }

        internal EctToEgmPollDispatcher EctToEgmPollDispatcher { get; private set; }

        internal GameMeterRequestor GameMeterRequestor { get; private set; }
        public EctToEgmTimeoutDetector EctToEgmTimeoutDetector { get; set; }
    
        public string SystemLockupText { get; set; }

        private PlatformInfomationProvider _PlatformInformationProvider = null;

        private byte _PollAddress = 0x01;
        public LockRequestor _EgmLockedByHotSwitchProcedure = null;

        private bool _ShouldEnableFanfareForExternalJackpot = true;
        public bool ShouldEnableFanfareForExternalJackpot 
        {
            get { return _ShouldEnableFanfareForExternalJackpot; }
            set { _ShouldEnableFanfareForExternalJackpot = value; } 
        }

        private ConfigurationStatus _EgmConfigurationStatus = ConfigurationStatus.None;
        public ConfigurationStatus EgmConfigurationStatus
        {
            get { return _EgmConfigurationStatus; }
            set { _EgmConfigurationStatus = value; }
        }

        private SpecificationFactory _SpecificationFactory = new SpecificationFactory();
        public SpecificationFactory SpecificationFactory
        {
            get { return _SpecificationFactory; }
            set { _SpecificationFactory = value; }
        }

        public AdditionalConfiguration AdditionalConfiguration { get; set; }
        public SystemLockUpHandler SystemLockUpHandler { get; set; }

        public bool IsSiteEnabled
        {
            get { return Egm.CabinetDevice.IsSiteEnabled; }
        }

        public bool IsRemoteConfigurationEnabled { get; set; }

        public EgmAdapter Egm
        {
            get { return _Egm; }
            set
            {
                _Egm = value;
                ConfigurationRepository = new ConfigurationRepository(AdditionalConfiguration);
                ConfigurationRepository.CurrentEgmConfiguration.Initialize(value);
                _Egm.NotifyOnGameIdle += new Action(() => IdleModeCounter.Reset());
                MeterTracker.InitializeCentForCentValidators();
                
            }
        }

        public byte PollAddress
        {
            get { return _PollAddress; }
            set { _PollAddress = value; }
        }

        private bool _IsRamReset = true;
        public bool IsRamReset
        {
            get { return _IsRamReset; }
            set { _IsRamReset = value; }
        }

        public void Display(string message,TimeSpan duration)
        {
            
        }

        public bool ShouldDisableGameOnDeposit { get; set; }

        private Schedule _Schedule;
        [AutoWire]
        public Schedule Schedule
        {
            get { return _Schedule; }
            set 
            { 
                _Schedule = value;
                SpamHandler = new SpamHandler(this);
                PurgeEventsHandler.InitializeScheduler();
                _PlatformInformationProvider = new PlatformInfomationProvider(this);
                _PollAddressConfigScheduler = new Scheduler(_Schedule);
                _PollAddressConfigScheduler.TimeOutAction += OnPollAddressConfigSchedulerTimedout;
                EctToEgmTimeoutDetector = new EctToEgmTimeoutDetector(this);                
            }
        }

        private Scheduler _PollAddressConfigScheduler = null;
        public Scheduler PollAddressConfigScheduler
        {
            get { return _PollAddressConfigScheduler; }
        }

        private State _State;
        public State State
        {
            get { return _State; }
            set
            {
                if (_Log.IsInfoEnabled)
                    _Log.InfoFormat("<StateTransition\r\n     Module='QCom'\r\n     From='{0}'\r\n     To='{1}'\r\n/>", 
                        (object)_State ?? "uninitialized", value);

                if (_State != null)                
                    _State.Exit();   

                _State = value;
                _State.Model = this;
                LinkStatusChanged();
                _State.Enter();

                _Egm.LinkStatusChanged(_State.LinkStatus);
            }
        }

        private void LinkStatusChanged()
        {
            RaleHandler.LinkStatusChanged(_State.LinkStatus);
            PurgeEventsHandler.LinkStatusChanged(_State.LinkStatus);
            GameMeterRequestor.LinkedStatusChanged(_State.LinkStatus);
            ConfigurationRequestTimeOutHandler.LinkedStatusChanged(_State.LinkStatus);
        }

        public ILink HostLink { get; set; }
        
        public bool IsListeningMode
        {
            get { return _IsListeningMode; }
            set
            {
                _IsListeningMode = value;
                if (!_IsListeningMode)
                    Initialize();
                SetInitialState();
            }
        }

        public bool RequestGameConfigurationViaEgmConfigurationRequestPoll
        {
            get { return _RequestGameConfigurationViaEgmConfigurationRequestPoll; }
            set { _RequestGameConfigurationViaEgmConfigurationRequestPoll = value; }
        }

        public bool ShouldSendSeekEgmBroadcastPoll
        {
            get { return _ShouldSendSeekEgmBroadcastPoll; }
            set { _ShouldSendSeekEgmBroadcastPoll = value; }
        }

        private void Initialize()
        {                        
            Egm.OnConfigurationReceived += HandleConfiguration;		 		    
            Egm.OnInitiateRomSignatureVerification = InitiateRomSignatureVerification;
            Egm.InitializeRequestHandler(new QComEgmRequestHandler() {Model = this});
            _EventListenerCollection = new EventListenerCollection(this);
        }

        private void HandleConfiguration(IEgmConfiguration egmConfiguration, ICollection<IGameConfiguration> gameConfigurations)
        {
            if(_Log.IsInfoEnabled)
                _Log.Info("Receibved Egm configuration from host");

           State.OnConfigurationReceived(egmConfiguration,gameConfigurations);
        }

        
        public bool IsEgmDetailsEntryRequired
        {
            get { return _EgmDetailsEntryRequired; }
            set { _EgmDetailsEntryRequired = value; }
        }

        public bool IsConfigurationRequired { get; set; }

        private byte _PurgePollSequenceNumber = 0;
        public byte PurgePollSequenceNumber
        {
            get { return _PurgePollSequenceNumber; }
            set { _PurgePollSequenceNumber = value; }
        }

        public ProcessorCollection MessageProcessorCollection { get; private set; }

        internal PurgeEventsAckResponseProcessor PurgeEventsHandler
        {
            get { return MessageProcessorCollection._ProcessorCollection.OfType<PurgeEventsAckResponseProcessor>().FirstOrDefault(); }
        }

        private EventListenerCollectionBase _EventListenerCollection;
        public EventListenerCollectionBase EventListenerCollection
        {
            get { return _EventListenerCollection; }
        }

        private LPBroadcastScheduler _LPBroadcastScheduler;
        public LPBroadcastScheduler LPBroadcastScheduler
        {
            get { return _LPBroadcastScheduler; }
            set { _LPBroadcastScheduler = value; }
        }

        private MysteryBroadcastScheduler _MysteryBroadcastScheduler;
        public MysteryBroadcastScheduler MysteryBroadcastScheduler
        {
            get { return _MysteryBroadcastScheduler; }
            set { _MysteryBroadcastScheduler = value; }
        }        

        private MeterTracker _MeterTracker;
        public MeterTracker MeterTracker
        {
            get { return _MeterTracker; }
        }

        public event Action GameInPlay = delegate { };

        private bool _IsGameInPlay = false;
        public bool IsGameInPlay
        {
            get { return _IsGameInPlay; }
            set 
            {
                if (_IsGameInPlay != value)
                {
                    _IsGameInPlay = value;
                    GameInPlay();
                }
            }
        }

        private EgmLockHandler _EgmLockHandler;
        public EgmLockHandler EgmLockHandler
        {
            get { return _EgmLockHandler; }
            set { _EgmLockHandler = value; }
        }

        private LockRequestor _GameLockedByRALEProcedure;
        public LockRequestor GameLockedByRALEProcedure
        {
            get { return _GameLockedByRALEProcedure; }
            set
            {
                if (value == null) return;

                _GameLockedByRALEProcedure = value;
                _EgmLockHandler.AddInput(_GameLockedByRALEProcedure);
            }
        }

       
            
        private LockRequestor _GameLockedBySystemLockUp;
        public LockRequestor GameLockedBySystemLockUp
        {
            get { return _GameLockedBySystemLockUp; }
            set
            {
                if (value == null) return;

                _GameLockedBySystemLockUp = value;
                _EgmLockHandler.AddInput(_GameLockedBySystemLockUp);
            }
        }

        public LockRequestor EgmLockedByHotSwitchProcedure
        {
            get { return _EgmLockedByHotSwitchProcedure; }
            set
            {
                if(value == null) return;

                _EgmLockedByHotSwitchProcedure = value;
                _EgmLockHandler.AddInput(_EgmLockedByHotSwitchProcedure);
            }
        }

        public bool EcTFromEgmInProgress { get; set; }

        public bool AnonymousForceClearFundsRequired { get; set; }

        public IPlatformInfo PlatformInfo
        {
            get { return _PlatformInformationProvider.PlatformInfo; }
            set { _PlatformInformationProvider.PlatformInfo = value; }
        }

        private MessageReceivedCounter<GeneralStatusResponse> _IdleModeCounter;
        public MessageReceivedCounter<GeneralStatusResponse> IdleModeCounter
        {
            get { return _IdleModeCounter; }
            set { _IdleModeCounter = value; }
        } 

        public bool ShouldQueryForGameLevelMeters {get; set;}

        public TimeSpan LinkedProgressiveBroadcastTimeout { get; set; }

        public QComModel()
        {
            EcTFromEgmInProgress = false;
            SystemLockupText = "System Lockup: Call Attendant";
            LinkedProgressiveBroadcastTimeout = TimeSpan.FromSeconds(2);
             EgmDetails = new EgmInfo();
            _PollQueue = new PollQueue();
            _EventListenerCollection = new EventListenerCollectionBase(this);
            MessageProcessorCollection = new ProcessorCollection(this);
            IdleModeCounter = new IdleModeCounter().WithCountLimit(3);
            RaleHandler = new RaleHandler() { Model = this };
            PlayOutsideLicensedHoursHandler = new PlayOutsideLicensedHoursHandler() { Model = this };
            ProgramHashHandler = new ProgramHashHandler() { Model = this };
            AdditionalConfiguration = new AdditionalConfiguration();
            EctToEgmPollDispatcher = new EctToEgmPollDispatcher(this);
            GameMeterRequestor = new GameMeterRequestor() { Model = this };
            _MeterTracker = new MeterTracker() { Model = this };
            MeterTracker.MeterValidationSkipped += OnMeterValidationSkippedWith;            
        }

        public bool IsPollQueued<T>() where T : Request
        {
            return _PollQueue.OfType<T>().Any();
        }

        internal void CheckAndRestoreEgmDetails()
        {
            if (IsConfigurationRequired)
            {
                string serialNumber = string.Empty, manufacturerId = string.Empty;
                if (RestoreEgmDetailsFromRepository(out serialNumber, out manufacturerId))
                    SaveDetailsForRepository(EgmDetails.AssetNumber, decimal.Parse(manufacturerId), decimal.Parse(serialNumber));

                return;
            }
            RestoreAssetNumberFromRepository();
        }

        public void RestoreAssetNumberFromRepository()
        {
            _PlatformInformationProvider.RestoreAssetNumberFromRepository();
        }

        public bool RestoreEgmDetailsFromRepository(out string serialNumber, out string manufacturerId)
        {
            return _PlatformInformationProvider.RestoreEgmDetailsFromRepository(out serialNumber, out manufacturerId);
        }

        internal void OnMeterValidationSkippedWith(EgmEvent egmEvent)
        {
            switch (egmEvent)
            {
                case EgmEvent.InconsistentGameMeters:
                    Egm.ReportEvent(egmEvent);
                    break;
                case EgmEvent.LifetimeMetersReset:
                    RamCleared();
                    break;
            }
        }

        internal bool RamclearAlreadyProcessed
        {
            get { return Egm.IsRamclearAlreadyDetected; }
        }

        internal void RamCleared()
        {
            if (RamclearAlreadyProcessed)
            {
                if (_Log.IsWarnEnabled) _Log.Warn("Ignoring Ram clear as already processed");
                return;
            }

            ResetEgmDetails();
            ResetPollSequenceNumbers();
            ClearConfigurationsIfNecessary();
            
            Egm.OnRamCleared();
            MeterTracker.ResetMeters();

            SetInitialState();
        }

        private void ClearConfigurationsIfNecessary()
        {
            if (!IsRemoteConfigurationEnabled) return;
            if(!ConfigurationRepository.IsBasicConfigurationsCompleted) return;

            if (_Log.IsInfoEnabled) _Log.Info("Clearing the configurations");                
            ConfigurationRepository.Clear();
        }

        private void ResetEgmDetails()
        {
            EgmDetails.ManufacturerId = 0;
            EgmDetails.SerialNumber = 0;
        }

        private void SetInitialState()
        {                
            if (_IsListeningMode)
            {
                State = new DisconnectedState();
                return;
            }
            State = new DiscoveringState();
        }


        internal void ResetPollQueue()
        {
            RemoveAllPoll();
            EctToEgmPollDispatcher.ClearAll();
            //PurgeEventScheduler.Stop();  
        }

        internal void ResetValidationStatus()
        {
            ConfigurationRepository.ResetAllValidationStatus();
        }

        internal void RequestAllMeters()
        {
            SendPoll(MeterRequestBuilder.BuildMeterGroupRequest(Egm.CabinetDevice.IsMachineEnabled,
                                                                                 Egm.CurrentGame));
        }

        internal void FetchGameLevelMetersForAllGames()
        {
            GameMeterRequestor.FetchForGameLevelMeters();
        }
 
        internal void SendPoll(Request request)
        {
            if (request == null)
            {
                _Log.Warn("Attempting to queue a null poll");
                return;
            }

            _Log.InfoFormat("Request Added : {0}",request.GetType().FullName);

            _PollQueue.Add(request);
        }

        private void RemoveAllPoll()
        {
            _PollQueue.Clear();
        }


        private bool IsProtocolVersionChangeDetected(ProtocolVersion ProtocolVersion)
        {
            return this.ProtocolVersion != ProtocolVersion.Unknown ? this.ProtocolVersion != ProtocolVersion : false;

        }

        internal void ResetPollSequenceNumbers()
        {
            PurgePollSequenceNumber = ECTPollSequenceNumber = 0x01;  
        }

        internal void OnMetersUpdated(SerializableList<MeterInfo> meters)
        {
            var egmMeters = new SerializableDictionary<MeterId, Meter>();

            foreach (var meterInfo in meters)
            {
                var meterValue = _MeterTracker.Meters.GetMeter(meterInfo.MeterCode);
                var meterId = meterInfo.MeterCode.ConverToMeterId();
                if (meterId.HasValue) 
                    egmMeters[meterId.Value] = meterValue;
                
            }

            Egm.UpdateMeters(egmMeters);            
        }

        #region IQComModel Members

        public void UpdateCasinoId(string casinoid)
        {
            _Egm.SetCasinoId(casinoid);
        }

        public void DataLinkStatusChanged(bool LinkUp)
        {
            State.DataLinkStatusChanged(LinkUp);
            
        }

        public void ResponseReceived(ApplicationMessage message)
        {                        
            _PollQueue.RemoveCurrentPoll();

            if (message == null)
            {
                if (_Log.IsInfoEnabled) _Log.Info("No message received");
                State.NoResponseReceived();
                return;
            }

            if (!ProgramHashHandler.CanProcessProgramHashResponse(message,_PollQueue.LastSentPoll)) return;

            if (_PollQueue.LastSentPoll != null && !message.CanAcceptResponse(_PollQueue.LastSentPoll))
            {       
                if (_Log.IsInfoEnabled) _Log.Info("Response skip, as Can Accept Current response is false");
                return;
            }

            SetRaleState(RaleProgressState.PollQueued);

            if (_Log.IsDebugEnabled)
                _Log.DebugFormat("Message Received: {0}", message);

            


            NotifyMessageDelivery(_PollQueue.LastSentPoll);

            if (IsProtocolVersionChangeDetected(message.ProtocolVersion))
            {
                Egm.ResetExtendedEventData();
                Egm.ReportEvent(EgmEvent.InvalidProtocolVersion);
            }

            ProtocolVersion = message.ProtocolVersion;
            State.ProcessResponse(message);
            _PollAddressConfigScheduler.Stop();

            // Changing RaleState to InProgress after processing Response.
            // This is because, as per QCom Requirements, if first response to RALE poll is an Event, 
            // it will always be a new event
            SetRaleState(RaleProgressState.InProgress);

            PostProcess(message);

            Egm.SetGameProtocolVersion(message.ProtocolVersion.GetText());
        }

        private void NotifyMessageDelivery(Request request)
        {
            if (request == null) return;
            if (request.Sender == null) return;

            request.Sender.OnMessageDelivered();
        }

        private void PostProcess(ApplicationMessage message)
        {
            Egm.MeterRequestHandler.HandleResponseOfMeterRequest(message);

            if (IsRemoteConfigurationEnabled)
                ConfigurationRepository.OnProtocolVersionReceived(ProtocolVersion);

            IdleModeCounter.Received(message);
            if (!IdleModeCounter.IsCountLimitReached) return;

            _Log.Info("Invoking Game Idle");

            OnGameIdle();
            IdleModeCounter.Reset();

            if (!Egm.AwaitingForGameIdle) return;

            _Egm.OnGameIdle();            
        }

        private void SetRaleState(RaleProgressState state)
        {
            if (_PollQueue.LastSentPoll != null && _PollQueue.LastSentPoll is RequestAllLoggedEventsPoll)
                RaleHandler.HandlePollQueued(state);
        }

        public ApplicationMessage GetNextPoll()
        {
            var currentMessage = _PollQueue.NextPoll;

            if (currentMessage == null) return null;

            if (currentMessage.IsBroadcast)
                _PollQueue.RemoveCurrentPoll();

            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Message Sent: {0}", currentMessage);

            return currentMessage;
        }

        public bool CanSendPoll
        {
            get 
            {
                return IsRemoteConfigurationEnabled ? 
                    (ConfigurationRepository.AllConfigurationsAvailable && EgmDetails.AssetNumber != 0) : true;
            }
        }

        #endregion




        public void SetEgmConfiguration(uint assetNumber, decimal manufactureId, decimal serialNumber)
        {
            IsConfigurationRequired = false;
            State.SetEgmDetails(assetNumber, manufactureId, serialNumber);
        }


        public void SetAssetNumber(uint assetNumber)
        {
            SetEgmConfiguration(assetNumber, EgmDetails.ManufacturerId, EgmDetails.SerialNumber);
        }

        internal void SaveEgmDetails(byte manufactureId, decimal serialNumber)
        {
            SaveDetailsForRepository(EgmDetails.AssetNumber, manufactureId, serialNumber);
        }

        SerializableDictionary<string, string> saveData = new SerializableDictionary<string, string>();

        public void SaveDetailsForRepository(uint assetNumber, decimal manufactureId, decimal serialNumber)
        {
            _PlatformInformationProvider.SaveDetailsInRepository(assetNumber,manufactureId,serialNumber);
        }

        public void InitializationComplete()
        {            
            State.InitializationComplete();
        }

        public bool IsEgmDetailsPresent
        {
            get { return EgmDetails.SerialNumber != 0; }
        }

        public bool IsAssetNumberPresent
        {
            get { return EgmDetails.AssetNumber != 0; }
        }

        internal void RequestAllGameMeters(MeterRequestInfo meterRequestInfo)
        {
            var gameMeterRequestPoll =  MeterRequestBuilder.RequestForAllGameMeters(meterRequestInfo.GameNumber, meterRequestInfo.PayTableId);

            ConstructAndSendMeterRequestPoll(gameMeterRequestPoll, meterRequestInfo);
        }

        private void ConstructAndSendMeterRequestPoll(EgmGeneralMaintenancePoll meterRequest, MeterRequestInfo meterRequestInfo)
        {
            if (Egm.CabinetDevice.IsMachineEnabled)
                meterRequest.MaintenanceFlagStatus |= MaintenanceFlagStatus.MachineEnableFlag;

            if (!IsRemoteConfigurationEnabled)
            {
                meterRequest.GeneralFlag |= GeneraFlagStatus.GameEnableFlag;
                SendPoll(meterRequest);
                return;
            }

            var gameConfigurationId = QComConfigurationId.CreateIdWith(FunctionCodes.EgmGameConfiguration, meterRequestInfo.GameNumber);

            var gameConfiguration =
                ConfigurationRepository.GetConfigurationsOfType<QComGameConfiguration>().FirstOrDefault(
                    (element) => element.Id.Equals(gameConfigurationId));

            if (gameConfiguration != null && gameConfiguration.ConfigurationData.GameStatus)
                meterRequest.GeneralFlag |= GeneraFlagStatus.GameEnableFlag;

            SendPoll(meterRequest);
        
        
        }

        internal void InitiateRomSignatureVerification(BallyTech.Gtm.ISoftwareAuthenticationInfo SoftwareAuthenticationInfo)
        {
            var reserverdFlag = Egm.CabinetDevice.IsMachineEnabled  ? ProgramHashCharacteristics.MachineEnableFlag : ProgramHashCharacteristics.None;

            if (_Log.IsInfoEnabled) _Log.InfoFormat("Initiate RomSignature Verification with {0}", reserverdFlag.ToString() );

            byte[] finalSeed = SoftwareAuthenticationInfo.Seed;
            if (SoftwareAuthenticationInfo.Seed.Length < 20)
                finalSeed = QComConvert.PadZerosToLeft(SoftwareAuthenticationInfo.Seed, 20);

            // 20 bytes seed should be sent in LittleEndian format to Egm
            finalSeed = finalSeed.Reverse().ToArray();

            SendPoll(new ProgramHashRequestPollMessage
            {
                Seed = finalSeed.ToSerializableList(),
                Reserved = reserverdFlag | ProgramHashCharacteristics.Seed
            });
        }

        internal void ConfigureNoteAcceptor(IDenominationConfiguration configuration)
        {
            if (_Log.IsInfoEnabled)

                _Log.InfoFormat("Configuring Bill Denominations");

            SendPoll(QComConfigurationBuilder.Build(configuration));

            if (_Log.IsInfoEnabled)

                _Log.InfoFormat("Requesting Note Acceptor Status");

            SendPoll(MessageBuilder.BuildNoteAcceptorStatusRequestMessage(Egm.CabinetDevice.IsMachineEnabled, Egm.CurrentGame));
        }

        public void QueuePSNResetPoll()
        {
            EgmConfigurationRequestPoll egmConfigRequestPoll = new EgmConfigurationRequestPoll()
                                                                    {
                                                                        StatusRequestFlag = StatusRequestFlag.ResetPSN
                                                                    };

            if (Egm.CabinetDevice.IsMachineEnabled)
                egmConfigRequestPoll.StatusRequestFlag |= StatusRequestFlag.MachineEnableFlag;

            SendPoll(egmConfigRequestPoll);
            ResetPollSequenceNumbers();
        }

        private void OnPollAddressConfigSchedulerTimedout()
        {
            SendPoll(PollAddressConfigurationBuilder.Build(EgmDetails.SerialNumber, EgmDetails.ManufacturerId).WithAddress(PollAddress));
        }


       
        public bool IsHostLinkConnected()
        {
            return HostLink.LinkStatus == LinkStatus.Connected;
        }

        internal void PollPurgeEvent(byte eventSeqNumber)
        {
            if (!IsHostLinkConnected() || (RaleHandler.State == RaleProgressState.InProgress) || PurgeEventsHandler.IsAwaitingPurgeAck || !Egm.SoftwareAuthenticationDevice.IsRomSignatureVerificationComplete)
            {
                _Log.DebugFormat("Not purging the event due to one of the following conditions. Host Link Status = {0}, Rale State = {1}, Awaiting purge ack = {2}, Is ROM SIG verification complete = {3}",
                                    IsHostLinkConnected(), RaleHandler.State, PurgeEventsHandler.IsAwaitingPurgeAck, Egm.SoftwareAuthenticationDevice.IsRomSignatureVerificationComplete);
                return;
            }
            PurgeEventsHandler.PollPurgeEvent(eventSeqNumber);
        }

        public bool UpdateLinkedProgressiveContributionAmount(GameDetails gameDetails, out Meter outMeter)
        {
            bool isMessageValid = true;
            outMeter = Meter.Zero;

            Game selectedGame = Egm.Games.FirstOrDefault(element => element.VersionNumber == gameDetails.GameVersionNumber);
            if (selectedGame != null)
            {
                Meter newMeter = new Meter(gameDetails.ProgressiveAmount, 1, (uint.MaxValue + 1m));
                if (!AreMetersValid(newMeter, selectedGame))
                {
                    isMessageValid = false;
                    outMeter = selectedGame.LinkedProgressiveContributionAmount;
                }
                selectedGame.LinkedProgressiveContributionAmount = newMeter;
            }

            return isMessageValid;
        }

        private bool AreMetersValid(Meter newMeter, Game game)
        {
            QComResponseSpecification _meterMovementSpec = SpecificationFactory.GetSpecification(FunctionCodes.MultiGameVariationMetersResponse);
            Meter currentMeter = game.LinkedProgressiveContributionAmount;

            return _meterMovementSpec.IsGameMeterValid(MeterId.Bets, currentMeter, newMeter);
        }

        public void BuildAndReportLpContributionIgnoredEvent(GameDetails gameDetails, Meter oldMeter)
        {
            var game = Egm.Games.Get(gameDetails.GameVersionNumber);

            Egm.ExtendedEventData = new ExtendedEgmEventData()
            {
                GameNumber = gameDetails.GameVersionNumber,
                PaytableId = (game != null) ? game.CurrentGameVariation.ToString() : "0",
                ProgGroupId = gameDetails.ProgressiveGroupId,
                MeterIncrement = (new Meter(gameDetails.ProgressiveAmount, 1, (uint.MaxValue + 1m)) - oldMeter).DangerousGetUnsignedValue(),
                Amount = gameDetails.ProgressiveAmount
            };
            Egm.ReportErrorEvent(EgmErrorCodes.LpContributionIgnored);
        }

        public void BuildAndReportInvalidProgressiveConfigEvent(ushort gameVersion, ushort progId)
        {
            var game = Egm.Games.Get(gameVersion);

            Egm.ExtendedEventData = new ExtendedEgmEventData()
                                    {
                                        GameNumber = gameVersion,
                                        PaytableId = (game != null) ? game.CurrentGameVariation.ToString() : "0",
                                        ProgGroupId = progId
                                    };

            Egm.ReportErrorEvent(EgmErrorCodes.InvalidProgressiveConfiguration);
        }
    }
}
