using System;
using BallyTech.Utility.Serialization;
using BallyTech.Utility.Time;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class Scheduler : IScheduleItem
    {
        private Schedule _Schedule;
        public Action TimeOutAction = delegate { };

        public Scheduler() { }

        public Scheduler(Schedule schedule)
        {
            _Schedule = schedule;
        }

        private DateTime _NextDue = DateTime.MaxValue;
        public DateTime NextDue
        {
            get { return _NextDue; }
        }

        public void Run()
        {
            Stop();
            TimeOutAction();
        }

        public void Start(TimeSpan selectedTime)
        {
            _NextDue = selectedTime == TimeSpan.MaxValue ? DateTime.MaxValue : TimeProvider.UtcNow.Add(selectedTime);
            _Schedule.Add(this);
        }

        public bool IsRunning
        {
            get { return _NextDue != DateTime.MaxValue; }
        }

        public void Stop()
        {
            _NextDue = DateTime.MaxValue;
        }
    }
}