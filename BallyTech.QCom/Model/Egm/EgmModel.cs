using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.Utility;
using BallyTech.Utility.Serialization;
using log4net;
using BallyTech.Utility.Time;
using BallyTech.Utility.Configuration;
using BallyTech.QCom.Messages;
using BallyTech.Gtm.Core;
using BallyTech.QCom.Model.Handlers;
using BallyTech.QCom.Model.Egm.Devices.FundsTransfer;
using BallyTech.QCom.Model.Builders;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class EgmModel : IEgm, ILink , IExtendedEgm
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(EgmModel));

        private EgmAdapter _EgmAdapter = null;
        private MeterService _MeterService = null;

        private EgmObserverCollection _Observers = new EgmObserverCollection();
        private LinkObserverCollection _LinkObservers = new LinkObserverCollection();
        private EgmObserverEventQueue _ObserverEventQueue = new EgmObserverEventQueue();
        private ExtendedEgmObserverCollection _EgmConfigurationObservers = new ExtendedEgmObserverCollection();
        
        private EgmErrorObserverCollection _EgmErrorObservers = new EgmErrorObserverCollection();

        private IFundsTransferHandler _FundsTransferHandler = null;
        private IVoucherStrategy _VoucherHandler = null;

        internal SerializableList<SharedLinkedProgressiveLine> SharedLinkedProgressiveLines { get; private set; }
            

        private EgmLockHandler _EgmLockHandler;
        private LockRequestor _GameLockedByGameModule;
        private LockRequestor _GameLockedForRomSignatureVerification;
        private LockRequestor _GameLockedForInvalidConfiguration;
        private LockRequestor _GameLockedForAutoDeposit = new LockRequestor();
        
        internal bool IsInitialized { get; private set; }
        internal bool ShouldNotifyMeterInitialization { get; set; }        

        private bool _IsForcefullyRequestedMeters = false;

        


        private bool _AllowCashlessMode = false;
        public bool AllowCashlessMode
        {
            get { return _AllowCashlessMode; }
            set 
            {
                _AllowCashlessMode = value;                
            }
        }

        public EgmLockHandler EgmLockHandler
        {
            get { return _EgmLockHandler; }
            set { _EgmLockHandler = value; }
        }

        public LockRequestor GameLockedByGameModule
        {
            get { return _GameLockedByGameModule; }
            set
            {

                if (value == null) return;

                _GameLockedByGameModule = value;
                _EgmLockHandler.AddInput(_GameLockedByGameModule);

            }
        }

        public LockRequestor GameLockedForAutoDeposit
        {
            get { return _GameLockedForAutoDeposit; }
            set
            {
                if (value == null) return;

                _GameLockedForAutoDeposit = value;
                _EgmLockHandler.AddInput(_GameLockedForAutoDeposit);
            }
        }

        public LockRequestor GameLockedForInvalidConfiguration
        {
            get { return _GameLockedForInvalidConfiguration; }
            set
            {
                if (value == null) return;

                _GameLockedForInvalidConfiguration = value;
                _GameLockedForInvalidConfiguration.IsAccountable = true;
                _EgmLockHandler.AddInput(_GameLockedForInvalidConfiguration);

            }
        }


  
        public LockRequestor GameLockedForRomSignatureVerification
        {
            get { return _GameLockedForRomSignatureVerification; }
            set
            {
                if (value == null) return;

                _GameLockedForRomSignatureVerification = value;
                _EgmLockHandler.AddInput(_GameLockedForRomSignatureVerification);
            }
        }

        private EgmTiltHandler _EgmTiltHandler;
        public EgmTiltHandler EgmTiltHandler
        {
            get { return _EgmTiltHandler; }
        }

        public EgmErrorNotifier EgmErrorNotifier { get; set; }
    

        private MeterRequestExpiryTracker _MeterRequestExpiryTimer = null;
        public MeterRequestExpiryTracker MeterRequestExpiryTimer
        {
            get { return _MeterRequestExpiryTimer; }
            set { _MeterRequestExpiryTimer = value; }
        }
        private Schedule _Schedule;
        [AutoWire]
        public Schedule Schedule
        {
            get { return _Schedule; }
            set { _Schedule = value; }
        }

        internal EgmObserverCollection Observers
        {
            get { return _Observers; }
        }

        internal EgmObserverEventQueue ObserverEventQueue
        {
            get { return _ObserverEventQueue; }
        }


        public EgmModel()
        {            
            _VoucherHandler = new NullVoucherStrategy();
            _FundsTransferHandler = new NullFundsTransferHandler();
            _LinkedProgressiveHandler = new NullLinkedProgressiveHandler();
            _EgmTiltHandler = new EgmTiltHandler();
            EgmRequestHandler = new NullEgmRequestHandler();
            SharedLinkedProgressiveLines = new SerializableList<SharedLinkedProgressiveLine>();
        }

        private LinkedProgressiveDevice _LinkedProgressiveDevice;
        public LinkedProgressiveDevice LinkedProgressiveDevice
        {
            get { return _LinkedProgressiveDevice; }
            set 
            { 
                _LinkedProgressiveDevice = value;
                _LinkedProgressiveDevice.Model = this;
                EgmAdapter.Devices.Add(_LinkedProgressiveDevice);
            }
        }

        public EgmAdapter EgmAdapter
        {
            get { return _EgmAdapter; }
            set
            {
                _EgmAdapter = value;
                _EgmAdapter.Initialize(this);
            }
        }

        internal void SetCashlessMode(bool state)
        {            
            AllowCashlessMode = state;

            if (_Log.IsInfoEnabled) _Log.InfoFormat("Received cashless mode as {0}", AllowCashlessMode);
            FundsTransferHandler.SetState(AllowCashlessMode);
        }

        internal void RequestMeters(params MeterType[] meterTypes)
        {
            var meterRequest = new MeterRequestInfo().GetRequestFor(meterTypes);
            EgmRequestHandler.RequestMeters(meterRequest);                 
        }

        public void ForceRequestMeters()
        {
            if (!ShouldQueryMeters())
            {
                _Log.Info("Not Requesting Meters as Validations are not yet complete!");
                _EgmConfigurationObservers.RequestedMetersReceived();
                return;
            }

            _IsForcefullyRequestedMeters = true;
            RequestAllMeters();
            _EgmAdapter.FetchCurrentGameMeters();
        }


        internal void RequestAllMeters()
        {
            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Requesting all meters");

            EgmRequestHandler.RequestAllMeters();
            EgmAdapter.MeterRequestHandler.SetState(MeterRequestSate.MetersRequested);

        }

        internal void RequestAllGroupMeters()
        {
            if (_Log.IsInfoEnabled) _Log.Info("Requesting all meters");
            RequestMeters(MeterType.Cashless, MeterType.Coins, MeterType.Game, MeterType.Jackpot, MeterType.NoteAcceptor, MeterType.Ticket);
        }

        internal void RequestGameMeters(int gameNumber, byte payTableid)
        {
            var meterRequest = new MeterRequestInfo(gameNumber, payTableid);
            EgmRequestHandler.RequestMeters(meterRequest);
        }

        internal bool ShouldQueryMeters()
        {
            return !GameLockedByGameModule.Value && !GameLockedForInvalidConfiguration.Value
                && EgmAdapter.SoftwareAuthenticationDevice.IsRomSignatureVerificationComplete;
        
        }

        internal void CreateSharedProgressiveLines(IList<ProgressiveLevelInfo> linkedProgressiveInfo)
        {
            if (SharedLinkedProgressiveLines.Count > 0) return;
            
            linkedProgressiveInfo.ForEach((item) =>
                                          SharedLinkedProgressiveLines.Add(new SharedLinkedProgressiveLine()
                                                                                {
                                                                                    LineId = item.LineId,
                                                                                    Handler = LinkedProgressiveHandler
                                                                                }));
        }


        public void ForceClearFunds()
        {
            var fundsTransferOut = EgmAdapter.Devices.OfType<FundsTransferOut>().FirstOrDefault();

            if (fundsTransferOut != null)
              fundsTransferOut.ForceCashout();

            SetCashlessMode(AllowCashlessMode);
            
        }

   
        public bool AwaitingForMeters
        {
            get { return _EgmAdapter.AwaitingForMeters; }
        }

        #region IEgm Members

        public bool IsValid { get; private set; }

        public string SerialNumber { get; internal set; }

       

        public string Name
        {
            get { return ""; }
        }

        public string GameLocation
        {
            get {return null; }
        }

        public IEgmGame CurrentGame
        {
            get { return EgmAdapter.CurrentGame; }
        }

        public uint AssetNumber { get; set; }

        private decimal _TokenDenomination = 100m;
        public decimal TokenDenomination
        {
            get { return _TokenDenomination; }
            set 
            {
                if (value <= 0) return;
                _TokenDenomination = value/QComCommon.MeterScaleFactor; 
            }
        }

        private decimal _CurrentGameDenomination = 0.01m;
        public decimal CurrentGameDenomination
        {
            get { return _CurrentGameDenomination; }
            set { _CurrentGameDenomination = value; }
        }

        private decimal _AccountingDenomination = 0.01m;
        public decimal AccountingDenomination
        {
            get { return _AccountingDenomination; }
            set { _AccountingDenomination = value; }
        }

        public string GameId { get; internal set; }

        public string PaytableId
        {
            get { return string.Empty; }
        }

        public string ExternalDeviceId
        {
            get { return string.Empty; }
        }

        public int MaxBet
        {
            get { return 100; }
        }

        public string PreviousTransactionId
        {
            get { return string.Empty; }
        }

        public decimal TotalCoinIn
        {
            get { return 0; }
        }

        public decimal TotalCoinOut
        {
            get { return 0; }
        }

        public decimal GamesPlayed
        {
            get { return 0; }
        }

              
        public IEnumerable<IEgmGame> Games
        {
            get { return EgmAdapter.Games.Cast<IEgmGame>(); }
        }

        public IEnumerable<decimal> Denominations
        {
            get { return new decimal[] { 0.01m }; }
        }

        public BoundSlot<bool> IsAnyDoorOpen
        {
            get { return Cabinet.IsAnyDoorOpen; }
        }

        public BoundSlot<bool> IsSlotDoorOpen
        {
            get { return Cabinet.IsSlotDoorOpen; }
        }

        public BoundSlot<bool> IsDropDoorOpen
        {
            get { return Cabinet.IsDropDoorOpen; }
        }

        public BoundSlot<bool> IsCardCageOpen
        {
            get { return Cabinet.IsCardCageOpen; }
        }

        public BoundSlot<bool> IsBellyDoorOpen
        {
            get { return Cabinet.IsBellyDoorOpen; }
        }

        public BoundSlot<bool> IsChangeLampOn
        {
            get { return Cabinet.IsChangeLampOn; }
        }

        public BoundSlot<bool> IsCashboxDoorOpen
        {
            get { return EgmAdapter.CabinetDevice.IsCashboxDoorOpen; }
        }

        public BoundSlot<bool> IsCashboxRemoved
        {
            get { return new BoundSlot<bool>(); }
        }

        private BoundSlot<bool> _IsHandpayPending = new BoundSlot<bool>();
        public BoundSlot<bool> IsHandpayPending
        {
            get { return _IsHandpayPending; }
            set { _IsHandpayPending = value; }
        }

        private ILinkedProgressiveHandler _LinkedProgressiveHandler;
        public ILinkedProgressiveHandler LinkedProgressiveHandler
        {
            get { return _LinkedProgressiveHandler; }
            set { _LinkedProgressiveHandler = value; }
        }

        public IFundsTransferHandler FundsTransferHandler
        {
            get { return _FundsTransferHandler; }
            set { _FundsTransferHandler = value; }
        }

        public IEgmRequestHandler EgmRequestHandler { get; set; }

        public IEgmMeters GetMeters()
        {
            return _MeterService ?? (_MeterService = new MeterService(EgmAdapter));
        }

        internal void ResetMeters()
        {
            _MeterService = null;
        }

        public bool IsLocked
        {
            get { return false; }
        }

        public IPrinter Printer
        {
            get { return EgmAdapter.Printer; }
        }

        public INoteAcceptor NoteAcceptor
        {
            get { return _EgmAdapter.NoteAcceptorDevice; }
        }

        public ICabinet Cabinet
        {
            get { return _EgmAdapter.CabinetDevice ; }
        }

        public IVoucher Voucher
        {
            get { return _EgmAdapter.Voucher; }
        }

        public ICoinAcceptor CoinAcceptor
        {
            get { return _EgmAdapter.CoinAcceptorDevice; }
        }
        public IHopper Hopper
        {
            get { return _EgmAdapter.HopperDevice; }
        }

        public IVoucherStrategy VoucherHandler
        {
            set { _VoucherHandler = value; }
            get { return _VoucherHandler; }
        }

        private PlayerInformationDisplay _PlayerInformationDisplay;
        public IPlayerInformationDisplay PlayerInformationDisplay
        {
            get
            {
                if (_PlayerInformationDisplay == null)
                    _PlayerInformationDisplay = new PlayerInformationDisplay() { EgmModel = this };
                return _PlayerInformationDisplay;
            }
        }

        public bool CanCancelCurrentTransfer
        {
            get { return true; }
        }

        public void CancelCurrentTransfer()
        {
            EgmAdapter.CancelCurrentTransfer();
        }

        public void TransferFunds(IFundsTransferAuthorization transfer)
        {
            EgmAdapter.TransferFunds(transfer);
        }

        private HandpayType? _HandpayType;
        public HandpayType? HandpayType
        {
            get { return _HandpayType; }
            set { _HandpayType = value; }
        }

        private string _CasinoId = string.Empty;
        public string CasinoId
        {
            get 
            {
                if (!string.IsNullOrEmpty(_CasinoId))                    
                    return _CasinoId;
                return "1";
        }
            set { _CasinoId = value; }
        }

        public string GameVersion { get; set; }

        public ICollection<IProgressiveLine> LinkedProgressiveLines
        {
            get { return GetLinkedProgressiveLines(); }
        }

        private ICollection<IProgressiveLine> GetLinkedProgressiveLines()
        {
            if (EgmAdapter.IsSharedProgressiveComponentEnabled)
                return SharedLinkedProgressiveLines.Cast<IProgressiveLine>().ToSerializableList();

            return (CurrentGame != null)
                       ? EgmAdapter.CurrentGame.LinkedProgressiveLines
                       : new SerializableList<IProgressiveLine>();
        }

        public ICollection<IProgressiveLine> LinkedMysteryLines
        {
            get { return EgmAdapter.LinkedMysteryLines; }
        }

        public bool IsPossibleOfflineEvent
        {
            get { return false; }
        }        

        public void UpdateEgmWithProgressiveValues(ICollection<IProgressiveLine> ProgressiveData)
        {
            //throw new NotImplementedException();
        }

        public void TransferProgressiveAward(int levelId, decimal AmountInCents)
        {
            //throw new NotImplementedException();
        }

        #endregion

        #region IObservableBy<IEgmObserver> Members

        public void AddObserver(IEgmObserver observer)
        {
            Observers.Add(observer);
        }

        public void RemoveObserver(IEgmObserver observer)
        {
            Observers.Remove(observer);
        }

        #endregion        

        public void LinkStatusChanged(LinkStatus linkStatus)
        { 
            if (LinkStatus != linkStatus)
            {
                LinkStatus = linkStatus;
                _LinkObservers.LinkStatusChanged();
            }

            EgmAdapter.Devices.NotifyGameLinkStatus(linkStatus);
            ProcessChangedLinkStatus();

            if (linkStatus == LinkStatus.Connected)            
                OnLinkConnected();            
        }

        internal void ResetEgmModel()
        {
            IsInitialized = false;
            IsValid = false;
            SharedLinkedProgressiveLines.Clear();
        }

        private void OnLinkConnected()
        {
            GameLockedForInvalidConfiguration.Value = false;

            GameLockedByGameModule.Value = IsRomSignatureVerificationPending();

            _EgmAdapter.EgmInitialized();

            var isMachineEnabled = EgmAdapter.CabinetDevice.IsMachineEnabled;
            var isSiteEnabled = EgmAdapter.CabinetDevice.IsSiteEnabled;

            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Requesting for Egm {0}, Site {1}", isMachineEnabled ? "enable" : "disable",
                                                                    isSiteEnabled ? "enable" : "disable");
            			
            _EgmAdapter.Voucher.SetTicketData();
            _FundsTransferHandler.SetState(AllowCashlessMode);

            if(EgmAdapter.NoteAcceptorDevice.DenominationConfiguration!=null)
                EgmRequestHandler.Configure(EgmAdapter.NoteAcceptorDevice.DenominationConfiguration);
           
        }

        private bool IsRomSignatureVerificationPending()
        {
            if (!EgmAdapter.IsRomSignatureVerificationEnabled) return false;

            if (EgmAdapter.SoftwareAuthenticationDevice.IsRomSignatureVerificationComplete) return false;

            return true;
        
        }


        private void ProcessChangedLinkStatus()
        {
            IsValid = (LinkStatus == LinkStatus.Connected);

            if (IsInitialized) return;

            switch (LinkStatus)
            {
                case LinkStatus.Connecting:
                    _Observers.EgmInitializing();
                    return;
                case LinkStatus.Connected:
                    IsInitialized = true;
                    _EgmAdapter.CreateLinkedMysteryLines();
                    _Observers.EgmInitialized();                    
                    break;
            }
        }

        internal void MeterRequestSplitIntervalSurpassed()
        {
            EgmAdapter.Devices.NotifyMeterRequestSplitIntervalSurpassed();
        }


        internal void MeterRequestTimerExpired()
        {
            EgmAdapter.Devices.NotifyMeterRequestTimerExpired();
        }

        public void SendPendingEvents()
        {
            if (_ObserverEventQueue.Count > 0)
                if (_Log.IsInfoEnabled) _Log.Info("Dispatching the pending events");
            _ObserverEventQueue.ForwardAll(_Observers, null);
        }

        #region ILink Members

        public LinkStatus LinkStatus
        {
            get; private set;
        }

        #endregion

        #region IObservableBy<ILinkObserver> Members

        public void AddObserver(ILinkObserver observer)
        {
            _LinkObservers.Add(observer);
        }

        public void RemoveObserver(ILinkObserver observer)
        {
            _LinkObservers.Remove(observer);
        }

        #endregion

        internal bool IsGameLinkUp()
        {
            return LinkStatus == LinkStatus.Connected;
        }

        internal void RequestForJackpotReset(JackpotType jackpotType)
        {
            EgmRequestHandler.ResetJackpot(jackpotType);
        }

        internal void ClearEgmFaults()
        {
            EgmRequestHandler.ClearEgmFaults();
        }

        #region IEgm Members


        public void ConfigureProgressive(byte groupId, IEnumerable<int> lineIds)
        {
            
        }

        #endregion        
    
        #region IEgm Members


        public void ConfigureProgressive(int groupId, IEnumerable<int> lineIds, bool remove)
        {
           ;
        }

        #endregion

        #region IConfigurableEgm Members

        public void Configure(IEgmConfiguration egmConfiguration, SerializableList<IGameConfiguration> gameConfigurations)
        {
           _EgmAdapter.HandleConfigurationReceived(egmConfiguration, gameConfigurations);
        }

        internal void ConfigurationRequested()
        {
          _EgmConfigurationObservers.RequestConfiguration();
        }

        internal void InvalidConfigurationReported(string errorReason,EgmEvent @event)
        {
             _EgmConfigurationObservers.OnInvalidConfiguration(errorReason,@event);
  
        }

	    internal void NoteAcceptorStatusReported(string NADS,SerializableList<BillDenomination> billDenomination)
        {
            _EgmConfigurationObservers.SuccessFulDenominationConfiguration(NADS,billDenomination);
        }

        internal void OnMismatchedConfiguration(IEgmConfiguration egmConfiguration)
        {
            _EgmConfigurationObservers.EgmConfigurationMismatched(egmConfiguration);
        }

        internal void RequestedMetersReceived()
        {
            if (_IsForcefullyRequestedMeters)
                _EgmConfigurationObservers.RequestedMetersReceived();

            _IsForcefullyRequestedMeters  = GameLockedByGameModule.Value = false;
            
            if (!ShouldNotifyMeterInitialization) return;

            ShouldNotifyMeterInitialization = false;
            Observers.EgmEventRaised(EgmEvent.MeterInitialized);
            EgmAdapter.MetersInitialized();
        }

        internal void OnMismatchedConfiguration(IGameConfiguration gameConfiguration)
        {
            _EgmConfigurationObservers.GameConfigurationMismatched(gameConfiguration);
        }


        #endregion

        #region IExtendedEgm Members

        private bool IsValidEgmConfiguration(IEgmConfiguration egmConfiguration)
        {
            return !(string.IsNullOrEmpty(egmConfiguration.SerialNumber) ||
                     string.IsNullOrEmpty(egmConfiguration.ManufacturerId));
        }



        public void Configure(IEgmConfiguration configuration)
        {
            var egmConfiguration = IsValidEgmConfiguration(configuration) ? configuration : null;

            _EgmAdapter.HandleConfigurationReceived(egmConfiguration,
                                                    configuration.GameConfigurations.ToSerializableList());
        }

        public void ConfigureMaxCreditLimit(ICreditLimitConfiguration configuration)
        {
           
        }

        #endregion

        #region IObservableBy<IExtendedEgmObserver> Members


        public void AddObserver(IExtendedEgmObserver observer)
        {
            _EgmConfigurationObservers.AddObserver(observer);
            
        }

        public void RemoveObserver(IExtendedEgmObserver observer)
        {
            _EgmConfigurationObservers.RemoveObserver(observer);
        }

        #endregion

        #region IEgm Members

        public ISoftwareAuthentication SoftwareAuthentication
        {
            get { return _EgmAdapter.SoftwareAuthenticationDevice; }
        }

        #endregion


        public void SetMaxCreditTransferLimit(decimal withdrawalLimit, decimal depositLimit)
        {

        }

        #region IObservableBy<IEgmErrorObserver> Members

        public void AddObserver(IEgmErrorObserver observer)
        {
            _EgmErrorObservers.AddObserver(observer);
        }

        public void RemoveObserver(IEgmErrorObserver observer)
        {
            _EgmErrorObservers.RemoveObserver(observer);
        }

        #endregion

        #region IExtendedEgm Members


        public IExtendedEgmEventData GetExtendedEventData()
        {
            return EgmAdapter.ExtendedEventData;
        }

        public void ResetExtendedEventData()
        {
            EgmAdapter.ExtendedEventData = null;
        }

        public Meter GetTotalPlayerInformationDisplayAccessedCount()
        {
            return _MeterService.GetTotalPIDAccessed();
        }

        public Meter GetLinkedProgressiveWageredAmount(int? gameVersion)
        {
            return _MeterService.GetLinkedProgressiveWageredAmount(gameVersion);
        }

        #endregion

        #region IEgm Members


        public void SetCreditLimits(ICreditLimitConfiguration creditLimitConfiguration)
        {
            _EgmAdapter.MaxCreditLimit = creditLimitConfiguration.MaxCreditLimitForFundTransfer;
        }

        #endregion
    }
}
