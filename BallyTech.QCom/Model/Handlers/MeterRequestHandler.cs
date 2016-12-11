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
    public enum MeterRequestSate
    {
        MetersRequested,
        MetersReceived,
        PollSent,
        None
    }
    
    
    [GenerateICSerializable]
    public partial class MeterRequestHandler: IMessageSender
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(MeterRequestHandler));

        private const uint GeneralStatusResponseCountLimit = 3;

        private MessageReceivedCounter<GeneralStatusResponse> _GeneralStatusResponseCounter =
                       new MessageReceivedCounter<GeneralStatusResponse>().WithCountLimit(GeneralStatusResponseCountLimit);

        internal Action OnMetersReceived = delegate { };

        internal MeterRequestSate State { get; private set; }

        public MeterRequestHandler()
        {
            State = MeterRequestSate.None;
        }


        private bool IsGeneralStatusCountLimitReached(ApplicationMessage response)
        {
            _GeneralStatusResponseCounter.Received(response);
           
            if (!_GeneralStatusResponseCounter.IsCountLimitReached) return false;
            
            _GeneralStatusResponseCounter.Reset();
            return true;

        }

        internal void HandleResponseOfMeterRequest(ApplicationMessage message)
        {
            if (State != MeterRequestSate.PollSent) return;

            if (!IsGeneralStatusCountLimitReached(message)) return;

            State = MeterRequestSate.MetersReceived;
            OnMetersReceived();

            ResetState();

        }

        internal void SetState(MeterRequestSate meterRequestState)
        {
            State = meterRequestState;
        }

        internal void LinkStatusChanged(LinkStatus linkStatus)
        {
            if (linkStatus == LinkStatus.Disconnected && State!=MeterRequestSate.None)
                OnMetersReceived();

            ResetState();
        }

        private void ResetState()
        {
            State = MeterRequestSate.None;
        
        }
    
        public void OnMessageDelivered()
        {
            State = State == MeterRequestSate.MetersRequested ? MeterRequestSate.PollSent : MeterRequestSate.None;
        }
    }
}
