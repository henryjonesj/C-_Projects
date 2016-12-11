using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using log4net;
using BallyTech.Gtm;
using BallyTech.Utility;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class GameMeterUpdateTracker
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(GameMeterUpdateTracker));

        private EgmAdapter _EgmAdapter;
        public EgmAdapter EgmAdapter
        {
            set 
            { 
                _EgmAdapter = value;
                _EgmAdapter.GameEnded += GameEnded;
                _EgmAdapter.FetchCurrentGameMeters += FetchCurrentGameMeters;
                _EgmAdapter.FetchProgressiveMeters += FetchProgressiveMeters;
                _EgmAdapter.ProgressiveMetersReceived += ProgressiveMetersReceived;
                _EgmAdapter.GameMetersReceived += GameMetersReceived;
                _EgmAdapter.RamCleared += OnRamCleared;
                _EgmAdapter.EgmInitialized += OnEgmInitialized;
                _EgmAdapter.MetersInitialized += OnMetersInitialized;
            }
        }

        private bool _ShouldPostProgressivePeriodicMeters;
        private bool _ShouldPostGameMeters;

        private decimal _WagerTriggerAmount = 100;
        public decimal WagerTriggerAmount
        {
            get { return _WagerTriggerAmount; }
            set {_WagerTriggerAmount = value; }
        }

        public bool ShouldSendGameMetersToHost { get; set; }

        private Meter _PreviousWageredAmount = Meter.Zero;

        private void OnEgmInitialized()
        {
            InitializeWagerAmountAndFetchGameMeters();
        }

        private void OnMetersInitialized()
        {
            InitializeWagerAmountAndFetchGameMeters();
        }

        private void InitializeWagerAmountAndFetchGameMeters()
        {
            if (!_EgmAdapter.ShouldQueryMeters()) return;

            PostProgressivePeriodicUpdate();
            PostPeriodic();

            var wagerAmount = _EgmAdapter.GetMeters().GetWageredAmount(null, null, null, null);
            _PreviousWageredAmount = wagerAmount == Meter.NotAvailable ? Meter.Zero : wagerAmount;
        }

        private void GameEnded()
        {
            var totalWageredAmount = _EgmAdapter.GetMeters().GetWageredAmount(null, null, null, null);
            var currentWageredAmount = totalWageredAmount - _PreviousWageredAmount;

            if (currentWageredAmount.DangerousGetUnsignedValue() < WagerTriggerAmount) return;

            _PreviousWageredAmount = totalWageredAmount;
            FetchCurrentGameMeters();            
        }

        private void OnRamCleared()
        {
            PostProgressivePeriodicUpdate();
        }

        private void FetchProgressiveMeters()
        {
            _Log.Debug("Fetching progressive meters");
            _EgmAdapter.GetGameMeters();
            _ShouldPostProgressivePeriodicMeters = true;
        }

        private void FetchCurrentGameMeters()
        {
            _Log.Debug("Fetching game level meters");
            _EgmAdapter.GetGameMeters();
            _ShouldPostProgressivePeriodicMeters = _ShouldPostGameMeters = true;
        }

        private void ProgressiveMetersReceived()
        {
            if (!_ShouldPostProgressivePeriodicMeters) return;

            _ShouldPostProgressivePeriodicMeters = false;

            PostProgressivePeriodicUpdate();
        }

        private void PostProgressivePeriodicUpdate()
        {
            _Log.DebugFormat("Posting ProgressivePeriodicUpdate event");
            _EgmAdapter.ReportEvent(EgmEvent.ProgressiveMetersReceived);
        }

        private void GameMetersReceived()
        {
            if (!_ShouldPostGameMeters) return;

            _ShouldPostGameMeters = false;

            PostPeriodic();
        }

        private void PostPeriodic()
        {
            if (!ShouldSendGameMetersToHost) return;

            _Log.DebugFormat("Posting Periodic event");
            _EgmAdapter.ReportEvent(EgmEvent.GameMetersReceived);
        }
    }
}
