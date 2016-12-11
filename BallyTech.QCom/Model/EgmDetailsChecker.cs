using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Model;
using BallyTech.Utility.Time;
using BallyTech.Utility.Configuration;
using log4net;

namespace BallyTech.QCom.Model
{
    public enum DetailsEntryStatus
    {
        OnEnter,
        OnResponseReceived,
        OnDetailsEntry,
        OnDataLinkStatusChanged,
        None
    }

    [GenerateICSerializable]
    public partial class EgmDetailsChecker
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(EgmDetailsChecker));
        private bool _IsDataLinkStatusChanged = false;
        private bool _AreEgmDetailsPresent = false;

        private TimeSpan _DetailsTimer = TimeSpan.FromSeconds(10);

        private Scheduler _DetailsEntryTimer;
        private QComModel _Model;

        public EgmDetailsChecker() { }

        public EgmDetailsChecker(QComModel model)
        {
            _Model = model;
            _DetailsEntryTimer = new Scheduler(_Model.Schedule);
            _DetailsEntryTimer.TimeOutAction += DetailsEntryRequired;
        }

        private void DetailsEntryRequired()
        {
            _Log.Debug("On DetailsEntryRequired timer run");
            _DetailsEntryTimer.Stop();
            if (IsConfigurationMissing())
            {
                _Model.IsConfigurationRequired = true;
                _Model.EgmConfigurationStatus = ConfigurationStatus.EntryRequired;
                return;
            }
            _Model.IsConfigurationRequired = false;
            _AreEgmDetailsPresent = true;
        }

        public void CheckForEgmDetails(DetailsEntryStatus status)
        {
            switch(status)
            {
                case DetailsEntryStatus.OnEnter:
                    _Log.Debug("On enter");
                    if (IsConfigurationMissing())
                    {
                        _DetailsEntryTimer.Start(_DetailsTimer);
                        return;
                    }
                    _Log.Info("_AreEgmDetailsPresent set to true at on enter");
                    _AreEgmDetailsPresent = true;
                    _DetailsEntryTimer.Stop();
                    break;
                case DetailsEntryStatus.OnResponseReceived:
                    _Log.Debug("On response received");
                    _IsDataLinkStatusChanged = true;
                    if (!IsConfigurationMissing())
                    {
                        _Log.Debug("_AreEgmDetailsPresent set to true at response received");
                        _AreEgmDetailsPresent = true;
                        _DetailsEntryTimer.Stop();
                    }                    
                    break;
                case DetailsEntryStatus.OnDetailsEntry:
                    _Log.Debug("On details entry");
                    if (!IsConfigurationMissing()) 
                        _AreEgmDetailsPresent = true;                    
                    break;
                case DetailsEntryStatus.OnDataLinkStatusChanged:
                    _Log.Debug("On data link status changed");
                    _IsDataLinkStatusChanged = true;                    
                    break;
            }
            CheckForStateChange();
        }

        private void CheckForStateChange()
        {
            if (_IsDataLinkStatusChanged && _AreEgmDetailsPresent)
            {
                _DetailsEntryTimer.Stop();
                _Model.InitializationComplete();
            }
        }

        private bool IsConfigurationMissing()
        {
            if (!_Model.State.IsConfigurationMissing()) 
                return false;

            _Model.CheckAndRestoreEgmDetails();

            return _Model.State.IsConfigurationMissing();
        }
    }
}
