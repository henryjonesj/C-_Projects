using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using log4net;
using BallyTech.Gtm;
using BallyTech.QCom.Messages;

namespace BallyTech.QCom.Model.Handlers
{
    public enum RaleProgressState
    {
        Complete,
        PollQueued,
        InProgress
    }


    [GenerateICSerializable]
    public partial class RaleHandler
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(RaleHandler));
        internal QComModel Model { get; set; }

        internal bool IsPollSentForTheSession { get; set; }

        internal RaleProgressState State { get; set; }

        public RaleHandler()
        {
            IsPollSentForTheSession = false;
            State = RaleProgressState.Complete;
        }

        internal void CheckAndRequestAllLoggedEvents()
        {
            if (IsPollSentForTheSession) return;

            _Log.Info("Disabling the EGM for RALE procedure");
            Model.GameLockedByRALEProcedure.Value = true;
            Model.SpamHandler.Send("QCOM: SENDING EVENT LOG", true);

            _Log.Info("Requesting all logged events from EGM");
            Model.SendPoll(new RequestAllLoggedEventsPoll());
        }

        internal void HandlePollQueued(RaleProgressState state)
        {
            IsPollSentForTheSession = true;
            State = state;
        }

        internal void LinkStatusChanged(LinkStatus status)
        {
            if (status == LinkStatus.Connected)
                IsPollSentForTheSession = false;
        }

        internal void HandleGameConnecting()
        {
            if (Model.IsRamReset)
                CheckAndRequestAllLoggedEvents();
        }

        internal void AllLoggedEventsReceived()
        {
            _Log.Info("Received all Logged Events, enabling the EGM");
            State = RaleProgressState.Complete;
            Model.IsRamReset = false;
            Model.SpamHandler.Clear();
            Model.GameLockedByRALEProcedure.Value = false;
        }
    }
}
