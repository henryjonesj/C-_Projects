using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Utility.Time;
using BallyTech.Utility.Configuration;
using BallyTech.Gtm;
using log4net;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class MeterRequestScheduler : ILinkObserver
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(MeterRequestScheduler));
        
        private Scheduler _Timer = null;

        [AutoWire(Name = "EgmModel")]
        public EgmModel Model { get; set; }

        private Schedule _Schedule = null;
        [AutoWire]
        public Schedule Schedule
        {
            get { return _Schedule; }
            set
            {
                _Schedule = value;
                _Timer = new Scheduler(Schedule);
                _Timer.TimeOutAction += OnMeterRequestSchedulerTimedout;
            }
        }


        private ILink _GameLink = null;
        public ILink GameLink
        {
            get{ return _GameLink;}
            set
            {
                if (_GameLink != null) 
                    _GameLink.RemoveObserver(this);
                _GameLink = value;
                if (_GameLink != null) 
                    _GameLink.AddObserver(this);
            }
        }


        public TimeSpan MeterRequestTimer { get; set; }

        public MeterRequestScheduler()
        {
            MeterRequestTimer = TimeSpan.FromSeconds(600);
        }

        private void Start()
        {
            _Timer.Start(MeterRequestTimer);
        }

        private void OnMeterRequestSchedulerTimedout()
        {
            _Log.Info("Meter Request Scheduler Timed Out");

            if (!Model.ShouldQueryMeters())
            {
                _Log.Info("Not Requesting Meters as Validations are not yet complete!");
                return;
            }

            Model.RequestAllMeters();
            Model.EgmAdapter.GetGameMeters();

			Start();
        }


        private void Stop()
        {

            _Timer.Stop();
        }


        #region ILinkObserver Members

        public void LinkStatusChanged()
        {
            Action action = (Model.LinkStatus == LinkStatus.Connected) ? (Action)Start : (Action)Stop;
            action.Invoke();
        }

        #endregion
    }
}
