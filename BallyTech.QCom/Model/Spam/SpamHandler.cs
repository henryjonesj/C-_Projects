using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class SpamHandler
    {
        private SpamDispatcher _Dispatcher = null;
        private Scheduler _Scheduler = null;

        private static readonly TimeSpan MessageDuration = TimeSpan.FromSeconds(2);

        private QComModel _Model;

        public SpamHandler()
        {
            
        }

        public SpamHandler(QComModel model)
        {
            this._Model = model;
            _Dispatcher = new SpamDispatcher() {Model = model};
            _Scheduler = new Scheduler(model.Schedule) {TimeOutAction = OnTimerExpired};
        }


        internal void Send(string message)
        {
            Send(message, MessageDuration);
        }

        internal void Send(string message,bool isTransparencyRequired)
        {
            _Dispatcher.Send(message,isTransparencyRequired);
        }

        internal void Send(string message,TimeSpan duration)
        {
            _Dispatcher.Send(message);
            _Scheduler.Start(duration);
        }

        private void OnTimerExpired()
        {
            Clear();
            _Scheduler.Stop();
        }

        internal void Clear()
        {
            _Dispatcher.Clear();
        }

    }
}
