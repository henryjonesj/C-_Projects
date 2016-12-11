using System;
using BallyTech.Utility.Serialization;
using BallyTech.Utility.Time;
using BallyTech.QCom.Messages;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class PurgeEventScheduler
    {
        private TimeSpan _PurgeEventTimeSpan = TimeSpan.FromSeconds(10);
        private QComModel _Model;
        private Scheduler _PurgeEventScheduler;

        public PurgeEventScheduler() { }

        public PurgeEventScheduler(QComModel model)
        {
            _Model = model;
            _PurgeEventScheduler = new Scheduler(model.Schedule);
            _PurgeEventScheduler.TimeOutAction += PollPurgeEvent;            
        }

        private void PollPurgeEvent()
        {           
            _Model.SendPoll( new PurgeEventsPoll() { PollSequenceNumber = _Model.PurgePollSequenceNumber });
            _PurgeEventScheduler.Start(_PurgeEventTimeSpan);         
        }

        public void Start()
        {
            _PurgeEventScheduler.Start(_PurgeEventTimeSpan);
        }

        public void Stop()
        {
            _PurgeEventScheduler.Stop();
        }
    }
}
