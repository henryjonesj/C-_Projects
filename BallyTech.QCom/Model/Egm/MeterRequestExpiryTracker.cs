using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Configuration;
using BallyTech.Utility.Serialization;
using BallyTech.Utility.Time;
using log4net;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class MeterRequestExpiryTracker
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (MeterRequestExpiryTracker));

        private Scheduler _Timer = null;

        private SplitTimeInterval _SplitTimeInterval = null;


        public MeterRequestExpiryTracker()
        {
            SplitTimeInterval = new SplitTimeInterval();
        }

        private EgmModel _Model = null;
        [AutoWire(Name ="EgmModel")]
        public EgmModel Model
        {
            get { return _Model; }
            set { _Model = value; }
        }

        private Schedule _Schedule = null;
        [AutoWire]
        public Schedule Schedule
        {
            get { return _Schedule; }
            set
            {
                _Schedule = value;
                _Timer = new Scheduler(Schedule);
                _Timer.TimeOutAction += MeterRequestTimerExpired;
            }
        }

        public SplitTimeInterval SplitTimeInterval
        {
            get { return _SplitTimeInterval; }
            set { _SplitTimeInterval = value; }
        }


        private bool IsResetRequired
        {
            get { return _SplitTimeInterval.IsReset; }
        }


        internal void Start()
        {            
            SplitTimeInterval.Reset();

            if (_Log.IsInfoEnabled) _Log.Info("Starting the timer");
            Run();
        }

        private void Run()
        {                           
            var nextLap = SplitTimeInterval.NextSplit;
            _Timer.Start(nextLap);
        }

        internal void Stop()
        {
            if (IsResetRequired) return;

            if (_Log.IsInfoEnabled) _Log.Info("Stopping the timer");
            _Timer.Stop();

            SplitTimeInterval.Reset();
        }

        private void MeterRequestTimerExpired()
        {
            if ((!SplitTimeInterval.IsFinalSplit))
            {
                ContinueWithNextSplit();
                return;
            }

            _Model.MeterRequestTimerExpired();
            Stop();

        }

        private void ContinueWithNextSplit()
        {
            if (_Log.IsInfoEnabled) _Log.Info("Split Interval Surpassed");
            _Model.MeterRequestSplitIntervalSurpassed();

            Run();
        }





    }
}
