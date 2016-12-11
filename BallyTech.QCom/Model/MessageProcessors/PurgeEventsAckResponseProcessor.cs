using System;
using BallyTech.Utility.Serialization;
using BallyTech.Utility.Time;
using BallyTech.QCom.Messages;
using log4net;
using BallyTech.Gtm;
using BallyTech.QCom.Model.Egm;

namespace BallyTech.QCom.Model.MessageProcessors
{
    [GenerateICSerializable]
    public partial class PurgeEventsAckResponseProcessor : MessageProcessor, IMessageSender
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(PurgeEventsAckResponseProcessor));

        private TimeSpan _PurgeEventTimeSpan = TimeSpan.FromSeconds(5);
        public TimeSpan PurgeEventTimeSpan
        {
            get { return _PurgeEventTimeSpan; }
            set { _PurgeEventTimeSpan = value; }
        }
        
        private Scheduler _PollSeqNumberScheduler = null;

        private byte _LastSentEventSeqNumber = 0;
        
        private bool _WasPSNReset = false;

        private bool _IsAwaitingPurgeAck = false;
        public bool IsAwaitingPurgeAck 
        {
            get { return _IsAwaitingPurgeAck; }
        }

        internal void InitializeScheduler()
        {
            _PollSeqNumberScheduler = new Scheduler(Model.Schedule);
            _PollSeqNumberScheduler.TimeOutAction += HandlePurgePollAckTimeout;
        }

        public override void Process(PurgeEventsPollAcknowledgementResponse applicationMessage)
        {
            if (!_IsAwaitingPurgeAck) return;

            Model.PurgePollSequenceNumber = PollSequenceNumberSupplier.SupplyNext(Model.PurgePollSequenceNumber);
            _WasPSNReset = false;
            _IsAwaitingPurgeAck = false;
            _PollSeqNumberScheduler.Stop();
        }

        public override void Process(PurgeEventsPollAcknowledgementResponseV16 applicationMessage)
        {
            if (!_IsAwaitingPurgeAck) return;

            var expectedPurgePollSequenceNumber = PollSequenceNumberSupplier.SupplyNext(Model.PurgePollSequenceNumber);
            Model.PurgePollSequenceNumber = applicationMessage.PollSequenceNumber;
            
            if (expectedPurgePollSequenceNumber != applicationMessage.PollSequenceNumber)
            {
                BuildAndReportUnexpectedPurgePollSequenceNumberEvent(expectedPurgePollSequenceNumber);
                HandlePurgePSNOutOfSequence();
            }
            else
            {
                _IsAwaitingPurgeAck = false;
                _PollSeqNumberScheduler.Stop();
                _WasPSNReset = false;
            }
            
        }

        private void BuildAndReportUnexpectedPurgePollSequenceNumberEvent(byte expectedPurgePollSequenceNumber)
        {
            Model.Egm.ExtendedEventData = new ExtendedEgmEventData()
            {
                ExpectedSequenceNumber = expectedPurgePollSequenceNumber,
                PurgePollSequenceNumber = Model.PurgePollSequenceNumber,
            };

            Model.Egm.ReportEvent(EgmEvent.UnexpectedPurgePollSequenceNumber); //Reporting UnexpectedPurgePollSequenceNumber

            _Log.InfoFormat("Unexp Purge PSN {0}, Expected {1}", Model.PurgePollSequenceNumber,
                              expectedPurgePollSequenceNumber);
        
        }

        private bool IsUnnumberedEvent(byte eventSeqNo)
        {
            return eventSeqNo == 0;
        }

        public void PollPurgeEvent(byte eventSeqNo)
        {
            if (IsUnnumberedEvent(eventSeqNo)) return;

            Model.SendPoll(new PurgeEventsPoll()
                                 {
                                     PollSequenceNumber = Model.PurgePollSequenceNumber,
                                     EventSequenceNumber = eventSeqNo,
                                     Sender = this
                                 });
            _IsAwaitingPurgeAck = true;
            _LastSentEventSeqNumber = eventSeqNo;            
        }

        private void HandlePurgePollAckTimeout()
        {
            Model.Egm.ResetExtendedEventData();
            Model.Egm.ReportEvent(EgmEvent.NoResponseForPurgePoll);
            HandlePurgePSNOutOfSequence();
        }

        private void HandlePurgePSNOutOfSequence()
        {
            _PollSeqNumberScheduler.Stop();
            _IsAwaitingPurgeAck = false;
            if (_WasPSNReset)
            {
                Model.Egm.ResetExtendedEventData();
                Model.Egm.ReportErrorEvent(EgmErrorCodes.PollSequenceNumberFailure);
                _WasPSNReset = false;
                return;
            }

            Model.QueuePSNResetPoll();
            _WasPSNReset = true;
            PollPurgeEvent(_LastSentEventSeqNumber);
        }

        internal void LinkStatusChanged(LinkStatus status)
        {
            if (status == LinkStatus.Disconnected)
            {
                _WasPSNReset = false;
                _IsAwaitingPurgeAck = false;
                _PollSeqNumberScheduler.Stop();
            }

        }

        #region IMessageSender Members

        public void OnMessageDelivered()
        {
            _PollSeqNumberScheduler.Start(_PurgeEventTimeSpan);
            _Log.InfoFormat("On PurgeEvents poll delivery, waiting for {0} seconds", _PurgeEventTimeSpan.Seconds);
        }

        #endregion
    }
}
