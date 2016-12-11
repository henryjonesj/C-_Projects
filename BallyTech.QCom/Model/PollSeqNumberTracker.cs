using System;
using BallyTech.Utility.Serialization;
using BallyTech.Utility.Time;
using BallyTech.QCom.Messages;
using log4net;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class PollSeqNumberTracker
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(PollSeqNumberTracker));

        private TimeSpan _PurgeEventTimeSpan = TimeSpan.FromSeconds(5);
        private QComModel _Model;
        private Scheduler _PollSeqNumberScheduler;
        private byte _LastSentEventSeqNumber = 0;
        
        private bool _WasPSNReset = false;
        public bool WasPSNReset
        {
            get { return _WasPSNReset; }
            set { _WasPSNReset = value; }
        }

        private bool _IsPurgeAwaitingACK = false;
        public bool IsPurgeAwaitingACK 
        {
            get { return _IsPurgeAwaitingACK; }
            set { _IsPurgeAwaitingACK = value; }
        }

        public PollSeqNumberTracker() { }

        public PollSeqNumberTracker(QComModel model)
        {
            _Model = model;
            _PollSeqNumberScheduler = new Scheduler(model.Schedule);
            _PollSeqNumberScheduler.TimeOutAction += HandlePurgePSNOutOfSequence;
        }

        public void PollPurgeEvent(byte eventSeqNo)
        {           
            _Model.SendPoll( new PurgeEventsPoll() 
                                 { 
                                     PollSequenceNumber = _Model.PurgePollSequenceNumber,
                                     EventSequenceNumber = eventSeqNo    
                                 });
            _IsPurgeAwaitingACK = true;
            _LastSentEventSeqNumber = eventSeqNo;
            _PollSeqNumberScheduler.Start(_PurgeEventTimeSpan);
        }

        public void HandlePurgePSNOutOfSequence()
        {
            _PollSeqNumberScheduler.Stop();
            _IsPurgeAwaitingACK = false;
            if (_WasPSNReset)
            {
                // Send SC generated event "SC-EGM Poll Sequence Number Fail"
                _Log.Info("Disabling EGM due to Poll Sequence Number Failure");
                _Model.GameLockedByErrantBehaviour.Value = true;
                // Send SC generated event "SC-EGM Disabled By System""
                return;
            }

            // Send SC generated event "SC-EGM Event Purge Ack Response Time-Out"
            _Model.HandlePSNOutOfSequence();
            _WasPSNReset = true;
            PollPurgeEvent(_LastSentEventSeqNumber);
        }

        public void Start()
        {
            _PollSeqNumberScheduler.Start(_PurgeEventTimeSpan);
        }

        public void Stop()
        {
            _PollSeqNumberScheduler.Stop();
        }
   }
}
