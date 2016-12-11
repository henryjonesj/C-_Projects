using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using log4net;
using BallyTech.Gtm;

namespace BallyTech.QCom.Model.Handlers
{
    [GenerateICSerializable]
    public partial class EctToEgmPollDispatcher : IMessageSender
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(EctToEgmPollDispatcher));

        private QComModel _Model = null;
        private SerializableList<EctToEgmPoll> _EctToEgmPolls = new SerializableList<EctToEgmPoll>();

        private EctToEgmPoll _LastSentPoll = null;


        public EctToEgmPollDispatcher() { }

        public EctToEgmPollDispatcher(QComModel model)
        {
            _Model = model;
        }

        internal EctToEgmPoll GetLastSentPoll()
        {
            return _LastSentPoll;
        }

        private bool CanProcessEctPoll
        {
            get { return _Model.State.LinkStatus == LinkStatus.Connected; }
        }


        internal void Dispatch(EctToEgmPoll ectToEgmPoll)
        {
            if (!CanProcessEctPoll)
            {
                if (_Log.IsErrorEnabled) _Log.Error("Not processing the Ect to Egm poll as link status is down");
                return;
            }

            ectToEgmPoll.Sender = this;
            _EctToEgmPolls.Add(ectToEgmPoll);

            if (_LastSentPoll != null) return;
            SendNextPoll();
        }

        private void SendNextPoll()
        {
            _LastSentPoll = _EctToEgmPolls.FirstOrDefault();

            if (_LastSentPoll == null) return;

            AppendAndAdvancePollSequenceNumber(_LastSentPoll);
            _Model.SendPoll(_LastSentPoll);
            
        }

        private void NotifyEctoToEgmPollDelivered()
        {
            if(_Log.IsInfoEnabled) _Log.InfoFormat("Ect to Egm poll delivered to game with sequence number {0}", _LastSentPoll.PollSequenceNumber);

            _LastSentPoll.Delivered();

            if (IsResponseRequiredForLastSentPoll())
                _Model.EctToEgmTimeoutDetector.TransferInitiated();
        }

        private void AppendAndAdvancePollSequenceNumber(EctToEgmPoll ecttoEgm)
        {
            ecttoEgm.PollSequenceNumber = _Model.ECTPollSequenceNumber;

            if (_Model.ProtocolVersion == ProtocolVersion.V15)
                _Model.ECTPollSequenceNumber = PollSequenceNumberSupplier.SupplyNext(_Model.ECTPollSequenceNumber);
        }

        internal void OnReceivingAck()
        {
            if (_LastSentPoll != null && !_LastSentPoll.IsDelieved)
            {
                _Log.Info("Received ack before message is delivered");
                return;
            }

            ExecuteNextPoll();
        }

        private void RemoveLastSentPoll()
        {
            if (_LastSentPoll == null) return;
            _EctToEgmPolls.Remove(_LastSentPoll);

            if (_Log.IsInfoEnabled) _Log.InfoFormat("Polls awaiting to be sent: {0}", _EctToEgmPolls.Count);
        }
        

        internal void ClearAll()
        {
            if (_Log.IsWarnEnabled) _Log.Warn("Clearing all Ect to Egm polls");
            _EctToEgmPolls.Clear();

            _LastSentPoll = null;
        }

        internal void NoResponseReceived()
        {
            if (_Log.IsWarnEnabled) _Log.Warn("Removing the last sent poll as no response is received");

            ExecuteNextPoll();
        }

        private void ExecuteNextPoll()
        {
            RemoveLastSentPoll();
            SendNextPoll();
        }


        #region IMessageSender Members

        public void OnMessageDelivered()
        {
            if (IsResponseRequiredForLastSentPoll())
            {
                NotifyEctoToEgmPollDelivered();
                return;
            }

            ExecuteNextPoll();
        }

        private bool IsResponseRequiredForLastSentPoll()
        {
            return _LastSentPoll != null && (_Model.ProtocolVersion == ProtocolVersion.V16 ||
                                             !_LastSentPoll.IsUtilizedtoSetCashlessMode);
        }

        #endregion

      
    }
}
