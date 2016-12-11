using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Model.Egm.Devices.FundsTransfer;
using BallyTech.Utility;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using log4net;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.Egm.Devices;
using BallyTech.QCom.Model.Handlers;
using BallyTech.Gtm.Core;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class EgmAdapter
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(EgmAdapter));
        private EgmModel _Model = null;
        private DeviceCollection _Devices = new DeviceCollection();
        public GameCollection Games { get; private set; }
        private MeterRepository _MeterRespository = null;
        private RamClearDetector _RamClearDetector = null;

        private GamePlayTracker _GamePlayTracker = null;

        internal MeterRequestHandler MeterRequestHandler { get; private set; }
        
        public Action<IEgmConfiguration, ICollection<IGameConfiguration>> OnConfigurationReceived = delegate { };
        public Action OnSoftwareVerificationAuthenticationCompleted = delegate { };
        
        private decimal _TicketPrintLimit = 2000m;
        private bool _IsRomSignatureVerificationEnabled = false;

        public EgmMainLineCurrentStatus EgmCurrentStatus { get; set; }
        public bool IsEgmCurrentState(EgmMainLineCurrentStatus state)
        {
            return EgmCurrentStatus == state;
        }

        private int _CurrentGameNumber = -1;

        internal string GameProtocolVersion
        {
            get { return _Model.GameVersion; }
        }

        internal decimal TokenDenomination 
        { 
            get { return _Model.TokenDenomination; } 
        }

        internal decimal CreditDenomination
        {
            get { return _Model.CurrentGameDenomination; }
        }
        
        public Action<ISoftwareAuthenticationInfo> OnInitiateRomSignatureVerification = delegate { };
        
        public Action NotifyOnGameIdle = delegate { };

        internal bool IsGameFaulty { get; private set; }
        public bool IsSharedProgressiveComponentEnabled { get; private set; }

        public void GameFaultStatusChanged(bool fault)
        {
            IsGameFaulty = fault;
        }

        public void SharedProgressiveComponentEnabled()
        {
            this.IsSharedProgressiveComponentEnabled = true;
        }

        public EgmAdapter()
        {
            SetDefaultLimits();
            MeterRequestHandler = new MeterRequestHandler();
            MeterRequestHandler.OnMetersReceived -= NotifyMetersReceived;
            MeterRequestHandler.OnMetersReceived += NotifyMetersReceived;
            GameLockedOnUnauthorizedAccess = new NullLockRequestor();
        }

        public IExtendedEgmEventData ExtendedEventData { get; set; }

        internal EctFromEgmMonitor EctFromEgmMonitor { get; set; }


        internal void Initialize(EgmModel model)
        {
            _Model = model;
            InitializeDevices();
            EgmCurrentStatus = EgmMainLineCurrentStatus.IdleMode;
            _MeterRespository = new MeterRepository();
            Games = new GameCollection();
            _GamePlayTracker = new GamePlayTracker(model);
            _RamClearDetector = new RamClearDetector(this);
            _Model.EgmLockHandler.AddInput(GameLockedOnUnauthorizedAccess);

            SetMeterInitializationBasedOnRomSignatureVerificationState();
            EctFromEgmMonitor = new EctFromEgmMonitor() { EgmAdapter = this };
        }

        internal bool ShouldQueryMeters()
        {
            return _Model.ShouldQueryMeters();
        }

        private void SetMeterInitializationBasedOnRomSignatureVerificationState()
        {
            if(!_IsRomSignatureVerificationEnabled) return;

            _Model.ShouldNotifyMeterInitialization = true;
        }

        internal void InitializeRequestHandler(IEgmRequestHandler requestHandler)
        {
            _Model.EgmRequestHandler = requestHandler;
        }

        public void NotifyMetersReceived()
        {

            _Log.Info("@ NotifyMetersReceived");

            _Model.RequestedMetersReceived();
            
        }

        private void SetDefaultLimits()
        {
            HopperCollectLimit = 50;
            MaxFundsTransferLimit = 10000m;
        }

        public void SetCasinoId(string casinoId)
        {
            _Model.CasinoId = casinoId;
        }

        public decimal TicketPrintLimit
        {
            get { return _TicketPrintLimit; }
            set { _TicketPrintLimit = value; }
        }

        public decimal HopperCollectLimit { get; set; }
        public decimal MaxFundsTransferLimit { get; set; }

        public decimal HopperRefillAmount { get; set; }

        public LockRequestor GameLockedOnUnauthorizedAccess { get; set; }
       
       
        private decimal _MaxCreditLimit = 9999.99m;
        public decimal MaxCreditLimit
        {
            get { return _MaxCreditLimit; }
            set { _MaxCreditLimit = value; }
        }

        public bool IsVoucherEnabled
        {
            get { return Voucher.IsEnabled; }
        }

        public bool IsCashlessModeSupported
        {
            get { return _Model.AllowCashlessMode; }
        }

        public bool IsRomSignatureVerificationEnabled
        {
            get { return _IsRomSignatureVerificationEnabled; }
            set { _IsRomSignatureVerificationEnabled = value; }
        }

        public bool IsEgmInitialized
        {
            get { return _Model.IsInitialized; }
        }

        public Action FetchCurrentGameMeters = delegate { };

        public Action FetchProgressiveMeters = delegate { };

        public Action RamCleared = delegate { };

        public Action ProgressiveMetersReceived = delegate { };

        public Action GameMetersReceived = delegate { };

        public Action GameEnded = delegate { };

        public Action EgmInitialized = delegate { };

        public Action MetersInitialized = delegate { };

        private GameMeterUpdateTracker _GameMeterUpdateTracker;
        public GameMeterUpdateTracker GameMeterUpdateTracker
        {
            set 
            {
                _GameMeterUpdateTracker = value;
                _GameMeterUpdateTracker.EgmAdapter = this;
            }
        }

        public MeterRequestScheduler MeterRequestScheduler { get; set; }
        
        public void SetDenomination(decimal creditDenomination, decimal tokenDenomination)
        {
            _RamClearDetector.ValidDataReceived();
            
            _Model.CurrentGameDenomination = creditDenomination;
            _Model.TokenDenomination = tokenDenomination;
        }

        public decimal AccountingDenomination
        {
            get { return _Model.AccountingDenomination; }
        }

        public bool IsValid
        {
            get { return _Model.IsValid; }
        }
        
        public ICollection<IProgressiveLine> LinkedMysteryLines
        {
            get { return MysteryInformationDisplay.LinkedMysteryLines; }
        }

        public int GameCount
        {
            get { return Games.GroupBy(game => game.VersionNumber)
                              .Select(game => game.First())
                              .ToList()
                              .Count; }
        }

        internal MeterRepository MeterRepository
        {
            get { return _MeterRespository; }
        }


        public void LinkStatusChanged(LinkStatus linkStatus)
        {
            JackpotDevice.LinkStatusChanged(linkStatus);
            _Model.LinkStatusChanged(linkStatus);
            SoftwareAuthenticationDevice.OnGameLinkStatusChanged();
            MeterRequestHandler.LinkStatusChanged(linkStatus);
        }

        public void UpdateMeters(SerializableDictionary<MeterId,Meter> meters)
        {
            _Model.ResetMeters();
            _MeterRespository.UpdateMeters(meters);

            //Have received meters from the game. Hence Egm would have come out from Ram Clear State.
            if (_MeterRespository.ValidMetersAvailable) _RamClearDetector.ValidDataReceived();

            var meterIdsReceived = meters.Keys.ToSerializableList();

            Devices.NotifyMetersReceived(meterIdsReceived);
            _Model.SendPendingEvents();

            _GamePlayTracker.Track(meterIdsReceived);
        }

        public void OnRamCleared()
        {
            RamCleared();
            _RamClearDetector.EgmResetReceived();            
        }

        internal void ResetCachedMeters()
        {
            if (_Log.IsInfoEnabled) _Log.Info("Clearing cached meters");
                
            _Model.ResetMeters();
            _MeterRespository.Reset();
        }

        internal void ResetAll()
        {
            if (_Log.IsWarnEnabled) _Log.Warn("Resetting everything!!!");

            Games.Clear();
            Devices.Reset();
            _GamePlayTracker.Reset();
            _CurrentGameNumber = -1;
            _Model.ResetEgmModel();
            IsSharedProgressiveComponentEnabled = false;
        }

        internal void SetMachineState(bool state)
        {
            _Model.GameLockedByGameModule.Value = state;    

        }

        public void OnGameIdle()
        {
            this.Devices.NotifyGameIdle();
        }

        public void SetEgmInfo(EgmInfo EgmDetails)
        {
            _Log.InfoFormat("setting egm info. Serial no = {0}, game id = {1}, asset no = {2}", EgmDetails.SerialNumber, EgmDetails.ManufacturerId, EgmDetails.AssetNumber);
            _Model.SerialNumber = EgmDetails.SerialNumber.ToString();
            _Model.GameId = EgmDetails.ManufacturerId.ToString();            
            _Model.AssetNumber = EgmDetails.AssetNumber;
        }


        private void InitializeDevices()
        {
            _Devices.Add(new Cabinet() { Model = this._Model });  
            _Devices.Add(new NoteAcceptor(){Model = this._Model});
            _Devices.Add(new CoinAcceptor(){ Model = this._Model} );   
            _Devices.Add(new Printer(){ Model = _Model});
            _Devices.Add(new FundsTransferIn() { Model = _Model});
            _Devices.Add(new FundsTransferOut() { Model = _Model});
            _Devices.Add(new JackpotDevice() { Model = _Model} );
            _Devices.Add(new StandAloneProgressiveJackpotHandler() {Model = _Model});
            _Devices.Add(new TicketOutDevice() { Model = _Model});
            _Devices.Add(new TicketInDevice() {Model = _Model});
            _Devices.Add(new HopperDevice() { Model = _Model });
            _Devices.Add(new SoftwareAuthenticationDevice() { Model = this._Model });
            _Devices.Add(new MysteryInformationDisplay() { Model = this._Model });

            Voucher = new Voucher() {Model = _Model};

        }

        public IEgmMeters GetMeters()
        {
            return _Model.GetMeters();
        }


        internal DeviceCollection Devices
        {
            get { return _Devices; }
        }


        internal Cabinet CabinetDevice
        {
            get { return _Devices.GetDevice<Cabinet>(); }
        }

        internal MysteryInformationDisplay MysteryInformationDisplay
        {
            get { return _Devices.GetDevice<MysteryInformationDisplay>(); }
        }

        internal SoftwareAuthenticationDevice SoftwareAuthenticationDevice
        {
            get { return _Devices.GetDevice<SoftwareAuthenticationDevice>(); }
        }

        internal NoteAcceptor NoteAcceptorDevice
        {
            get { return _Devices.GetDevice<NoteAcceptor>(); }
        }

        internal CoinAcceptor CoinAcceptorDevice
        {
            get { return _Devices.GetDevice<CoinAcceptor>(); }
        }

        internal Printer Printer
        {
            get { return _Devices.GetDevice<Printer>(); }
        }

        internal HopperDevice HopperDevice
        {
            get { return _Devices.GetDevice<HopperDevice>(); }
        }

        public JackpotDevice JackpotDevice
        {
            get { return _Devices.GetDevice<JackpotDevice>(); }
        }

        public StandAloneProgressiveJackpotHandler SapHandler
        {
            get { return _Devices.GetDevice<StandAloneProgressiveJackpotHandler>(); }
        }

        public TicketOutDevice TicketOutDevice
        {
            get { return _Devices.GetDevice<TicketOutDevice>(); }
        }

        public TicketInDevice TicketInDevice
        {
            get { return _Devices.GetDevice<TicketInDevice>(); }
        }

        internal Voucher Voucher { get; private set; }

        public LinkedProgressiveDevice LinkedProgressiveDevice
        {
            get { return _Devices.GetDevice<LinkedProgressiveDevice>(); }
        }

        public SerializableList<SharedLinkedProgressiveLine> SharedProgressiveLines
        {
            get { return _Model.SharedLinkedProgressiveLines; }
        }

        internal Game CurrentGame
        {
            get
            {
                if (!(Games.IsAvailable)) 
                    return Game.GetDefaultWithVersion(this.GameProtocolVersion);

                if (!(Games.IsMultiGame)) return Games.FirstOrDefault();
                    
                Game currentGame = null;
                Games.TryGetValue(_CurrentGameNumber, out currentGame);

                return currentGame ?? Games.FirstOrDefault();
            }   
        }

        public bool AllowMixedCreditFundTransfer { get; set; }              

        public bool AwaitingForMeters
        {
            get { return _Devices.AnyDeviceAwaitingForMeters; }
        }

        public bool AwaitingForGameIdle
        {
            get { return _Devices.AnyDeviceAwaitingForGameIdle; }
        }

        public void SetEgmGameProperties(ushort gameVersion, byte gameVariation, int configuredGameCount)
        {
            SetCurrentGame(gameVersion, gameVariation);
            this.Games.MaxGameCount = configuredGameCount;
        }       
       
        public void SetCurrentGame(ushort gameVersion, byte gameVariation)
        {
            if (gameVersion == 0 || gameVariation == 0 || this._CurrentGameNumber==gameVersion)
                return;

            AddGame(gameVersion, gameVariation);
            _Log.InfoFormat("_CurrentGameNumber updated. previous value = {0}, current value = {1}", _CurrentGameNumber, gameVersion);
            this._CurrentGameNumber = gameVersion;

                                   
        }

        internal void CreateLinkedMysteryLines()
        {
            MysteryInformationDisplay.CreateLinkedMysteryLines();
        }

        internal void AddGame(ushort gameVersion, byte gameVariation)
        {
            Game game = Games.Contains(gameVersion) ? Games[gameVersion] :    
                        new Game(gameVersion, gameVariation, UInt16.MaxValue.ToString()) { Enabled = true };

            game.UpdateGameVariationsInfo(new GameVariationInfo(gameVariation, UInt16.MinValue * QComCommon.MeterScaleFactor));
            UpdateGame(game);
        }

        public void SetGameProtocolVersion(string gameProtocolVersion)
        {
            _Model.GameVersion = gameProtocolVersion;
        }
       
        internal void UpdateGame(Game game)
        {
            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Updating game with game number {0}", game.GameNumber);

            game.GameVersion = this.GameProtocolVersion;
            Games.Update(game);
        }

        public void UpdateGameMeters(ushort version, byte variation, SerializableDictionary<MeterId, Meter> meters)
        {
            Games[version].UpdateGameMeters(meters, variation);
        }

        public void UpdateGameMeter(MeterId meterId, Meter meter, ushort version, byte variation)
        {
            Games[version].UpdateMeter(meterId, meter, variation);
        }

        public SerializableDictionary<MeterId, Meter> GetGameMeters(ushort version, byte variation)
        {
            return Games[version].GetMeters(variation);
        }

        public void RequestFullDeposit(decimal amount)
        {
            var transferDevice = _Devices.OfType<FundsTransferOut>().SingleOrDefault();

            transferDevice.RequestForFullDeposit(amount);
        }

        internal bool IsAmountWithinMaxTransferLimit(decimal amount)
        {
            return MaxFundsTransferLimit > amount;
        }

        private bool IsHandpayTransfer(TransferDestination destination)
        {
            return destination == TransferDestination.Handpay;
        }

        internal void TransferFunds(IFundsTransferAuthorization authorization)
        { 
          if(IsHandpayTransfer(authorization.Destination))
          {
              MysteryInformationDisplay.InitiateHandpay(authorization);
              return;
          }

            var transferDevice =
                _Devices.OfType<FundsTransferBase>().SingleOrDefault(dev => dev.SupportsTransfer(authorization));

            transferDevice.InitiateTransfer(authorization);
        }

        internal FundsTransferOut TransferOutDevice
        {
            get { return _Devices.OfType<FundsTransferOut>().SingleOrDefault(); }
        }

        internal FundsTransferIn TransferInDevice
        {
            get { return _Devices.OfType<FundsTransferIn>().SingleOrDefault(); }
        }

        internal bool IsEctToEgmInProgress { get; set; }
        internal bool IsEctFromEgmInProgress { get; set; }

        public bool IsRamclearAlreadyDetected
        {
            get { return _RamClearDetector.HasRamClearAlreadyDetected; }
        }

        public void OnLockupFailed()
        {            
            TransferOutDevice.OnLockupFailed();
        }

        public void OnLockupCleared()
        {
            TransferOutDevice.OnLockupCleared();            
        }

        internal void CancelCurrentTransfer()
        {
            _Devices.OfType<FundsTransferBase>().ForEach((dev) => dev.CancelCurrentTransfer());
        }

        public void GetGameMeters()
        {
            if (!IsCurrentGameSet()) return;

            _Model.RequestGameMeters(CurrentGame.VersionNumber, CurrentGame.CurrentGameVariation);
        }

        private bool IsCurrentGameSet()
        {
            return _CurrentGameNumber != -1;
        }

        internal void SetCurrentGame(ushort gameNumber)
        {
            if (!IsCurrentGameSet() && gameNumber != 0) _CurrentGameNumber = gameNumber;
        }

        public void HandleConfigurationReceived(IEgmConfiguration egmConfiguration, ICollection<IGameConfiguration> gameConfigurations)
        {
            OnConfigurationReceived(egmConfiguration, gameConfigurations);

            if (egmConfiguration == null) return;

            this.HopperCollectLimit = egmConfiguration.HopperLimit;
            this.HopperRefillAmount = egmConfiguration.HopperRefillAmount;
            this.MaxFundsTransferLimit = egmConfiguration.MaxElectronicCreditTransferLimit;
            var maxAutoPayLimit = egmConfiguration.MaxAutoPayLimit/MeterService.MeterScaleFactor;            
            foreach (var key in LinkedProgressiveDevice.AutoPayLimit.Keys.ToList())
            {
                LinkedProgressiveDevice.AutoPayLimit[key] = maxAutoPayLimit.ToString();
            }
        }

        public void SetCashlessModeIfNecessary()
        {
            if (!_Model.AllowCashlessMode) return;

            if (_Log.IsInfoEnabled) _Log.Info("Setting cashless mode");
            _Model.SetCashlessMode(true);
        }

        public void RequestConfiguration()
        {
            _Model.ConfigurationRequested();
        }

        public void ReportMismatchedConfiguration(EgmEvent egmEvent,string errorReason)
        {
             ResetExtendedEventData();

             _Model.InvalidConfigurationReported(errorReason, egmEvent);

            _Model.GameLockedForInvalidConfiguration.Value = true;
        
        }

        public void ReportNoteAcceptorStatus(string NADS,SerializableList<BillDenomination> billDenomination)
        {
            _Model.NoteAcceptorStatusReported(NADS, billDenomination);
        }

        public void ReportEvent(EgmEvent egmEvent)
        {
            _Model.Observers.EgmEventRaised(egmEvent);
        
        }

        public void ReportErrorEvent(EgmErrorCodes errorCode)
        {
            _Model.EgmErrorNotifier.Notify(errorCode);
            
        }

        public void ValidateSignature(byte[] signature)
        {
            SoftwareAuthenticationDevice.Process(signature);
        }

        public ExtendedEgmEventData ConstructUnreasonableMeterData(MeterCodes meterCode, Meter oldValue, Meter newValue)
        {
            var extendedData = new ExtendedEgmEventData()
            {
                MeterId = (byte)meterCode,
                MeterIncrement = Math.Abs((newValue - oldValue).DangerousGetSignedValue()),
                Amount = newValue.DangerousGetUnsignedValue()

            };

            return extendedData;
        }

        public void ResetExtendedEventData()
        {
            ExtendedEventData = null;
        }

        internal void SetDepositLockState(bool lockState)
        {
            _Model.GameLockedForAutoDeposit.Value = lockState;
        }

        internal void ReportMismatchedConfiguration(IEgmConfiguration egmConfiguration)
        {
            _Model.GameLockedForInvalidConfiguration.Value = true;
           _Model.OnMismatchedConfiguration(egmConfiguration);
           
        }

        internal void ReportMismatchedConfiguration(IGameConfiguration gameConfiguration)
        {
            _Model.GameLockedForInvalidConfiguration.Value = true;
            _Model.OnMismatchedConfiguration(gameConfiguration);
        }

        public void ReceivedValidFundTransferPSN()
        {
            _Model.EgmRequestHandler.ResetPSN(ResetStatus.Success);
        }


        public void ReceivedInValidFundTransferPsn()
        {
            _Model.EgmRequestHandler.ResetPSN(ResetStatus.Attempt);
        }

        public void CreateSharedProgressiveLines(IList<ProgressiveLevelInfo> linkedprogressiveLevelInfo)
        {
            _Model.CreateSharedProgressiveLines(linkedprogressiveLevelInfo);
        }


    }
}
