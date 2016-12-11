using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model;
using BallyTech.Utility.Threading;
using System.IO;
using log4net;
using BallyTech.Utility.Diagnostics;
using BallyTech.Utility.Collections;
using BallyTech.Utility.Time;
using BallyTech.QCom.Model.Builders;


namespace BallyTech.QCom
{
    public class QComDriver : ThreadedObject
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (QComDriver));

        private bool _LinkupStatus = false;        
        readonly Stopwatch _InterPollTimer = new Stopwatch();
        private int _NumberOfTimesTried = 0;
        private bool _ControlBit;        
        private PollCycle _CurrentPollCycle = null;

        private ProtocolVersion _ProtocolVersion = ProtocolVersion.Unknown;

        private TimeSpan _InterPollInterval = TimeSpan.FromMilliseconds(350);
        private TimeSpan _ResponseDelay = TimeSpan.FromMilliseconds(10);
        private TimeSpan _PollBroadcastInterval = TimeSpan.FromMilliseconds(10);
        private TimeSpan _SleepIntervalToStartPolling= TimeSpan.FromMilliseconds(1000);


        private EgmPoller _EgmPoller = null;

        private IQComModel _Model;
        public IQComModel Model
        {
            get { return _Model; }
            set { _Model = value; }            
        }

        public EgmPoller EgmPoller
        {
            get { return _EgmPoller; }
            set { _EgmPoller = value; }
        }
        
        public TimeSpan InterPollInterval
        {
            get { return _InterPollInterval; }
            set { _InterPollInterval = value; }
        }

        
        public TimeSpan ResponseDelay
        {
            get { return _ResponseDelay; }
            set { _ResponseDelay = value; }
        }

        public TimeSpan SleepIntervalToStartPolling
        {
            get { return _SleepIntervalToStartPolling; }
            set { _SleepIntervalToStartPolling = value; }
        }

        public TimeSpan PollBroadcastInterval
        {
            get { return _PollBroadcastInterval; }
            set { _PollBroadcastInterval = value; }
        }

        private string _SiteID = string.Empty;
        public string SiteID
        {
            get { return _SiteID; }
            set { _SiteID = value; }
        }

        private bool _UseVendorSpecificString = true;
        public bool UseVendorSpecificString
        {
            get { return _UseVendorSpecificString; }
            set { _UseVendorSpecificString = value; }
        }

        private string _SiteIdVendorSpecificString = "SiteID";
        public string SiteIdVendorSpecificString
        {
            get { return _SiteIdVendorSpecificString; }
            set { _SiteIdVendorSpecificString = value; }
        }


        private int _RetryLimit = 5;
        public int RetryLimit
        {
            get { return _RetryLimit; }
            set { _RetryLimit = value; }
        }

        private string GetSiteIdfromDhcp()
        {
            if (_UseVendorSpecificString)
            {
                try
                {
                    string siteidvendorInfo = SysInfo.GetAssociatedInformation(SiteIdVendorSpecificString);
                    if (string.IsNullOrEmpty(siteidvendorInfo))
                    {
                        _Log.Error("Unable to obtain SiteId from the DHCP. Check your vendor specfic Options.");
                        return string.Empty;
                    }
                    else
                    {
                        _Log.InfoFormat("CasinoId Received from DHCP :  {0} ", siteidvendorInfo);
                        return siteidvendorInfo.Trim();
                    }
                }
                catch (Exception)
                {
                    _Log.Error("Exception while to obtain CasinoId from the DHCP. Check your vendor specfic Options.");
                    return string.Empty;
                }
            }
            else
                return _SiteID;
        }

        protected override void OnStart()
        {
            _EgmPoller.Intialize(_Model.PollAddress);
            string casinoId = GetSiteIdfromDhcp();
            if (!string.IsNullOrEmpty(casinoId))
            {
                Model.UpdateCasinoId(casinoId);
            }

            NotifyLinkStatusChanged(false);
        }        

        protected override void Loop()
        {
            if (!Model.CanSendPoll)            
                Thread.Sleep(SleepIntervalToStartPolling.Milliseconds);            
            else
            {
                var pollCycle = GetNextPollCycle();

                ProcessNextPoll(pollCycle.Poll);

                Thread.Sleep((int)PollBroadcastInterval.TotalMilliseconds);
                pollCycle.Broadcast.UpdateBroadcastHeaderData(_Model.IsSiteEnabled);
                SendMessage(pollCycle.Broadcast);
            }
        }


        private bool IsLinkDown
        {
            get { return !_LinkupStatus; }
        }

        private PollCycle GetNextPollCycle()
        {
            if (_CurrentPollCycle != null) return _CurrentPollCycle;

            if (MessageBeingRetried() && _ProtocolVersion == ProtocolVersion.V15)
                return CreateGeneralPollCycle();

            var nextPoll = _Model.GetNextPoll();
            return _CurrentPollCycle = nextPoll != null ? PollCycle.CreateWith(nextPoll) : CreateGeneralPollCycle();
        }

        private PollCycle CreateGeneralPollCycle()
        {
            return _CurrentPollCycle = PollCycle.CreateWith(new GeneralStatusPoll());
        }

        private void ProcessNextPoll(ApplicationMessage currentMessage)
        {
            EnforceInterPollDelay();
            SendAndReceiveMessage(currentMessage);
        }

        protected override void OnStop()
        {
            _EgmPoller.Dispose();
        }

        private void ToggleControlBit(Message message)
        {
            if (message.ApplicationData.IsBroadcast) return;

            if (ShouldToggleControlBit())
                _ControlBit = !_ControlBit;

            message.Header.ControlBit = _ControlBit;
        }

        private bool ShouldToggleControlBit()
        {
            if (_ProtocolVersion != ProtocolVersion.V16) return true;
            return !(MessageBeingRetried());
        }

        private void SendAndReceiveMessage(ApplicationMessage message)
        {            
            SendMessage(message);            
            ResetPollTimer();
            EnforceResposeDelay();
            ReceiveMessage();
            CheckIfRetryLimitExceeded(message);
        }
        
        private void CheckIfRetryLimitExceeded(ApplicationMessage lastSentMessage)
        {
            if (_NumberOfTimesTried < RetryLimit) return;

            _NumberOfTimesTried = 0;
            ResetCurrentPollCycle();
            _Model.ResponseReceived(null);
            
            if (!lastSentMessage.IsGeneralPoll)
            {
                //Egm may not support the current poll.. Hence checking by sending the GeneralPoll
                CreateGeneralPollCycle();
                return;
            }

            if(IsLinkDown) return;
            NotifyLinkStatusChanged(false);
        }

        private bool MessageBeingRetried()
        {
            return _NumberOfTimesTried > 0;
        }

        private void EnforceInterPollDelay()
        {
            _InterPollTimer.Stop();

            var timeRemaining = _InterPollInterval - _InterPollTimer.Elapsed;
            if(timeRemaining <= TimeSpan.Zero) return;
          
            Thread.Sleep((int) timeRemaining.TotalMilliseconds);
        }

        private void ResetPollTimer()
        {
            _InterPollTimer.Reset();
            _InterPollTimer.Start();
        }

        private void SendMessage(ApplicationMessage applicationMessage)
        {
            var message = applicationMessage.AppendDataLinkLayerWithPollAddress(_Model.PollAddress);
            ToggleControlBit(message);

            _EgmPoller.SendMessage(message);                        

            if(applicationMessage.IsBroadcast) return;
            
            _NumberOfTimesTried++;
        }

        private void EnforceResposeDelay()
        {            
            Thread.Sleep((int)_ResponseDelay.TotalMilliseconds);            
        }
        
        private void ReceiveMessage()
        {
            try
            {
                var response = _EgmPoller.ReceiveMessage();

                if (response == null) return;

                _ProtocolVersion = response.ApplicationData.ProtocolVersion;
                if (!_LinkupStatus) NotifyLinkStatusChanged(true);
                _NumberOfTimesTried = 0;
                ResetCurrentPollCycle();

                Model.ResponseReceived(response.ApplicationData);
            }
            catch (Exception ex)
            {
                if (_Log.IsErrorEnabled) _Log.Error("Parsing failed due to", ex);
            }
        }

        private void ResetCurrentPollCycle()
        {
            _CurrentPollCycle = null;
        }

        private void NotifyLinkStatusChanged(bool linkStatus)
        {
            _LinkupStatus = linkStatus;
            _Model.DataLinkStatusChanged(_LinkupStatus);
        }
    }
}
