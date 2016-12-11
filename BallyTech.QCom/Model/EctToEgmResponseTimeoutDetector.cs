using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using log4net;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.Handlers;
using BallyTech.Gtm;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class EctToEgmTimeoutDetector
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (EctToEgmTimeoutDetector));

        private QComModel _Model = null;
        private Scheduler _Scheduler = null;
        private EctToEgmPollDispatcher _EctToEgmPollDispacter = null;

        public TimeSpan EctToEgmResponseDelay { get; set; }

        public QComModel Model
        {
            get { return _Model; }
            set
            {
                Setup(value);
            }
        }

        public EctToEgmTimeoutDetector(){}

        public EctToEgmTimeoutDetector(QComModel model)
        {
            Setup(model);
        }

        private void Setup(QComModel model)
        {
            _Model = model;
            _EctToEgmPollDispacter = _Model.EctToEgmPollDispatcher;

            EctToEgmResponseDelay = TimeSpan.FromSeconds(5);
            _Scheduler = new Scheduler(_Model.Schedule) { TimeOutAction = TimeoutDetected };
        }


        internal void TransferInitiated()
        {
            _Scheduler.Start(EctToEgmResponseDelay);
        }


        internal void AcknowledgementResponseReceived()
        {
            if (!IsResponsePendingForTheLastSentPoll) return;

            _Scheduler.Stop();
        }

        private bool IsResponsePendingForTheLastSentPoll
        {
            get { return _EctToEgmPollDispacter.GetLastSentPoll() != null; }
        }


        internal void MetersReceived(IList<MeterInfo> meters)
        {
            if(!IsResponsePendingForTheLastSentPoll) return;

            if(!IsOldVersionGame()) return;
            if(!HaveReceivedCashlessInMeter(meters)) return;

            _EctToEgmPollDispacter.OnReceivingAck();

            _Scheduler.Stop();
        }

        private static bool HaveReceivedCashlessInMeter(IEnumerable<MeterInfo> meters)
        {
            return meters.Any((item) => item.MeterCode == MeterCodes.TotalEgmCashlessCreditIn);
        }

        private bool IsOldVersionGame()
        {
            return (_Model.ProtocolVersion == ProtocolVersion.V15);
        }

        private void TimeoutDetected()
        {
            _Scheduler.Stop();

            var lastSentPoll = _EctToEgmPollDispacter.GetLastSentPoll();
            if(lastSentPoll == null) return;

            if (_Log.IsErrorEnabled) _Log.ErrorFormat("No response received for poll {0}", lastSentPoll);

            NotifyTimeoutDetected();
        }

        private void NotifyTimeoutDetected()
        {
            _Model.Egm.ReportEvent(EgmEvent.ElectronicCreditTransferToEgmTimedout);
            _EctToEgmPollDispacter.NoResponseReceived();
        }







    }
}
