using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.Utility;
using BallyTech.Utility.Serialization;
using log4net;
using BallyTech.QCom.Messages;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class Cabinet : Device,ICabinet
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (Cabinet));        

        private SerializableDictionary<EgmEvent, BoundSlot<bool>> _EventMap =
           new SerializableDictionary<EgmEvent, BoundSlot<bool>>();

        private DoorStatusProcessor _DoorStatusProcessor;
      
        public Cabinet()
        {
            Initialize();

            _EventMap.Add(EgmEvent.ChangeLampOn, IsChangeLampOn);
            IsEnabled = new AndGate(IsMachineEnabled, IsSiteEnabled);
            IsAnyDoorOpen = new OrGate(IsSlotDoorOpen, IsDropDoorOpen, IsCardCageOpen, IsBellyDoorOpen, IsCashboxDoorOpen, IsNoteAcceptorDoorOpen, IsMechanicalMeterDoorOpen,
                                        IsAuxDoorOpen);
            IsSlotBusy = new OrGate(IsAnyDoorOpen, IsGameActive);

            IsMachineEnabled.Value = true;
            IsSiteEnabled.Value = true;
            _DoorStatusProcessor = new DoorStatusProcessor(this);
        }

        private void Initialize()
        {
            IsSlotDoorOpen = new BoundSlot<bool>();   
            IsDropDoorOpen = new BoundSlot<bool>();
            IsCardCageOpen = new BoundSlot<bool>();
            IsBellyDoorOpen = new BoundSlot<bool>();
            IsGameActive = new BoundSlot<bool>();
            IsMachineEnabled = new BoundSlot<bool>();
            IsSiteEnabled = new BoundSlot<bool>();
            IsCashboxDoorOpen = new BoundSlot<bool>();
            IsChangeLampOn = new BoundSlot<bool>();
            IsAuxDoorOpen = new BoundSlot<bool>();
            IsNoteAcceptorDoorOpen = new BoundSlot<bool>();
            IsMechanicalMeterDoorOpen = new BoundSlot<bool>();
        }
        
        #region ICabinet Members

        public BoundSlot<bool> IsMachineEnabled { get; private set; }
        
        public BoundSlot<bool> IsEnabled { get; private set; }

        public BoundSlot<bool> IsSiteEnabled { get; private set; }

        public decimal MaxTransferLimit
        {
            get { return 999999m ; }
        }

        public BoundSlot<bool> IsEgmEnabled { get; private set; }

        public BoundSlot<bool> IsGameActive { get; private set; }

        public BoundSlot<bool> IsSlotBusy { get; private set; }

        public BoundSlot<bool> IsAnyDoorOpen { get; private set; }

        public BoundSlot<bool> IsSlotDoorOpen { get; private set; }

        public BoundSlot<bool> IsDropDoorOpen { get; private set; }

        public BoundSlot<bool> IsCardCageOpen { get; private set; }

        public BoundSlot<bool> IsBellyDoorOpen { get; private set; }

        public BoundSlot<bool> IsCashboxDoorOpen { get; private set; }

        public BoundSlot<bool> IsChangeLampOn { get; private set; }

        public BoundSlot<bool> IsAuxDoorOpen { get; private set; }

        public BoundSlot<bool> IsNoteAcceptorDoorOpen { get; private set; }

        public BoundSlot<bool> IsMechanicalMeterDoorOpen { get; private set; }

        public bool IsIdle { get; private set; }

        public IDepositSnapshot Snapshot
        {
            get { return new DepositSnapshot(); }
        }

        public void SetEnabled(bool enabled)
        {
            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Received {0} status from host", enabled);

            IsMachineEnabled.Value = enabled;
            Model.EgmTiltHandler.AddTilt(enabled ? string.Empty : "Game Disabled");
            Model.EgmRequestHandler.SetEnabledState(enabled);            
        }

        public void SetHostCashout(bool enabled)
        {
            Model.SetCashlessMode(enabled);            
        }

        public void SetDateTime(DateTime dateTime)
        {
            
        }

        public void EnableJackpotReset()
        {
            Model.RequestForJackpotReset(Model.EgmAdapter.EgmCurrentStatus.GetJackpotType());
        }

        public void SetAutoplay(bool enable)
        {
            
        }

        public void ResetHandpay()
        {
            if (_Log.IsInfoEnabled)
                _Log.Info("Requested for remote reset of handpay");

            if (Model.EgmAdapter.IsEgmCurrentState(EgmMainLineCurrentStatus.LinkedProgressiveAwardLockup))
            {
                Model.LinkedProgressiveDevice.RemoteReset();
                return;
            }
            Model.RequestForJackpotReset(Model.EgmAdapter.EgmCurrentStatus.GetJackpotType());
        }

        public ICollection<IEGMPort> EGMPorts
        {
            get { return new SerializableList<IEGMPort>(); }
        }

        #endregion


        public void GamePlayStatusChanged(bool isStarted)
        {
            IsGameActive.Value = isStarted;
            Model.RequestMeters(MeterType.Game,MeterType.Coins);

            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Game Play State: {0}", isStarted);
        }

        public void DoorStatusChanged(SlotDoors door, bool isOpened)
        {
            _DoorStatusProcessor.Process(door, isOpened);
        }

        internal void RaiseEvent(EgmEvent egmEvent)
        {
            Model.Observers.EgmEventRaised(egmEvent);
        }

        public void Process(EgmEvent egmEvent)
        {
            RaiseEvent(egmEvent);

            if (!(_EventMap.ContainsKey(egmEvent))) return;

            var eventInfo = _EventMap[egmEvent];
            eventInfo.Value = true;
        }

        public void ProcessIdleModeStatus(bool idleStatus)
        {
            this.IsIdle = idleStatus;
        }

        public void NewGameSelected(ushort gameVersion, byte gameVariation)
        {
            if (!Model.GameLockedByGameModule.Value)
                Model.EgmAdapter.FetchProgressiveMeters();
            
            Model.EgmAdapter.SetCurrentGame(gameVersion, gameVariation);

            Model.EgmAdapter.CabinetDevice.RaiseEvent(EgmEvent.GameSelected);
        }

        public void GameVariationChanged(ushort gameVersion, byte gameVariation)
        {
            Model.EgmAdapter.CabinetDevice.RaiseEvent(EgmEvent.EGMPaytableIdChanged);        
        }

        public void GameDenominationChanged(decimal gameDenomination)
        {
            Model.CurrentGameDenomination = gameDenomination;
        }

        #region ICabinet Members


        public void LockGame(bool lockGame, string lockDisplayMessage)
        {
            Model.EgmRequestHandler.SetLockState(new LockStateInfo() { LockState = lockGame, DisplayMessage = lockDisplayMessage, IsFanFareRequired = lockGame });            
        }

        public void SetSiteEnabled(bool enabled)
        {
            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Sending Site {0} to EGM", enabled ? "Enable" : "Disable");
            IsSiteEnabled.Value = enabled;            
        }

        #endregion

        #region ICabinet Members


        public void ClearEgmFaults()
        {
            if (_Log.IsInfoEnabled)
                _Log.Info("Requested to clear EGM Faults");

            Model.ClearEgmFaults();
        }

        #endregion

        public void LockGameForHotSwitchProcedure(bool lockGame, string lockDisplayMessage)
        {
            Model.EgmRequestHandler.SetLockState(new LockStateInfo() { LockState = lockGame, DisplayMessage = lockDisplayMessage, IsFanFareRequired = false });
        }
    }
}
