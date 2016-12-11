using System;
using BallyTech.Utility.Serialization;
using BallyTech.Utility.Time;
using BallyTech.QCom.Messages;
using log4net;
using BallyTech.Utility.Configuration;
using BallyTech.Gtm;

namespace BallyTech.QCom.Model.Handlers
{
    [GenerateICSerializable]
    public partial class SystemLockUpHandler
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(SystemLockUpHandler));

        internal Scheduler _SystemLockUpScheduler;

        private int SystemLockUpTryCount = 0;

        public Request SystemLockUpPoll { get; set; }

        private QComModel _Model = null;
        [AutoWire(Name = "QComModel")]
        public QComModel Model
        {
            get { return _Model; }
            set 
            {
                _Model = value;
            
            }
        }

        private bool _IsSystemInLockUp = false;
        public bool IsSystemInLockUp
        {
            get { return _IsSystemInLockUp; }
            set { _IsSystemInLockUp = value; }
        }

        private TimeSpan _SystemLockUpTimeout = TimeSpan.FromSeconds(5);
        public TimeSpan SystemLockUpTimeout
        {
            get { return _SystemLockUpTimeout; }
            set { _SystemLockUpTimeout = value; }
        }

        private int _MaxSystemLockUpRetryCount = 1;
        public int MaxSystemLockUpRetryCount
        {
            get { return _MaxSystemLockUpRetryCount; }
            set { _MaxSystemLockUpRetryCount = value; }
        }

        private void InitializeSystemLockUpScheduler()
        {
            _SystemLockUpScheduler = new Scheduler(Model.Schedule);
            _SystemLockUpScheduler.TimeOutAction =OnSystemLockUpTimeOut;
        }

        internal void HandleSystemLockup()
        {
            if (_SystemLockUpScheduler == null)
                InitializeSystemLockUpScheduler();    
            
            _Log.InfoFormat("Disabling EGM for System Lock Up");
            
            _Model.GameLockedBySystemLockUp.Value = true;

            _SystemLockUpScheduler.Start(SystemLockUpTimeout);

        }

        internal void OnSystemLockUpTimeOut()
        {
            if (_Log.IsInfoEnabled) _Log.ErrorFormat("System Lock Up Timed Out");
            
            if (_Model.SystemLockUpHandler.IsSystemInLockUp)
            {
                _SystemLockUpScheduler.Stop();
                if (_Log.IsInfoEnabled) _Log.ErrorFormat("System Lock Up Succeeded");
                _Model.GameLockedBySystemLockUp.Value = false;
                return;
            }

            if (!_Model.GameLockedBySystemLockUp.Value)
            {
                _SystemLockUpScheduler.Stop();
                SystemLockUpTryCount++;

                if (CheckAndIssueSystemLockUpAgain()) return;

                if (_Log.IsErrorEnabled) _Log.ErrorFormat("System Lock Up Failed");
                Model.Egm.ResetExtendedEventData();
                _Model.Egm.ReportEvent(EgmEvent.EGMLockForFundTransferFailed);
                _Model.GameLockedBySystemLockUp.Value = false;
                SystemLockUpTryCount = 0;
                return;

            }

            _Log.InfoFormat("Trying System Lock Up By enabling EGM");
            
            _Model.GameLockedBySystemLockUp.Value = false;

            if (SystemLockUpPoll != null) _Model.SendPoll(SystemLockUpPoll);

            _SystemLockUpScheduler.Start(SystemLockUpTimeout);
        }

        private bool CheckAndIssueSystemLockUpAgain()
        {
            if (SystemLockUpTryCount < MaxSystemLockUpRetryCount)
            {
                if (_Log.IsInfoEnabled) _Log.ErrorFormat("Retrying System Lock Up");
                if (SystemLockUpPoll != null) _Model.SendPoll(SystemLockUpPoll);
                HandleSystemLockup();
                return true;
            }

            return false;
        
        }

    }
    
    
}
