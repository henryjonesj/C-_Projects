using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Threading;
using System.IO;
using BallyTech.QCom.Messages;
using log4net;
using System.Threading;
using BallyTech.QCom.Model;
using BallyTech.Utility.Collections;
using BallyTech.Utility.Diagnostics;

namespace BallyTech.QCom
{
    public class QComListeningDriver : IDisposable
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(QComListeningDriver));

        private bool _IsLinkUp = false;
        private readonly ThreadedTimer _LinkDownTimer = new ThreadedTimer("LinkDownTimer");
        private TimeSpan _ReceiveTimeOut = TimeSpan.FromMilliseconds(5000);
        private CrcVerificationSpecification _CrcVerificationSpecification = new CrcVerificationSpecification();
        private DataReceiver _DataReceiver = null;
        object _ReceiverLock = new object();

        private Stopwatch _ResponseDelayTimer = new Stopwatch();

        private bool ResponseDelayRequired = true;

        private TimeSpan _MaxResponseTimeout = TimeSpan.FromMilliseconds(30);
        public TimeSpan MaxResponseTimeout
        {
            get { return _MaxResponseTimeout; }
            set { _MaxResponseTimeout = value; }
        }

        private TimeSpan _MaxResponseDelay = TimeSpan.FromMilliseconds(100);
        public TimeSpan MaxResponseDelay
        {
            get { return _MaxResponseDelay; }
            set { _MaxResponseDelay = value; }
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

        private IPort _Port;
        public IPort Port
        {
            get { return _Port; }
            set
            {
                _Port = value;
            }
        }

        private void OpenPort()
        {
            _Port.DataReceived += DataReceived;
            _Port.Open();
            _DataReceiver = new DataReceiver(_Port) { MaxResponseTimeout = _MaxResponseTimeout };
        }

        private IQComModel _Model;
        public IQComModel Model
        {
            get { return _Model; }
            set { _Model = value; }
        }

        public QComListeningDriver()
        {

        }

        public void Initialize()
        {            
            _LinkDownTimer.Tick += new EventHandler(ResponseTimeOut);
            NotifyDataLinkStatusChange(false);

            string casinoId = GetSiteIdfromDhcp();
            if (!string.IsNullOrEmpty(casinoId))
            {
                Model.UpdateCasinoId(casinoId);
            }

            OpenPort();
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

        private bool IsDataReceivable()
        {
            if (!ResponseDelayRequired) return true;

            if (MaxResponseDelayReached())
            {
                _ResponseDelayTimer.Stop();
                ResponseDelayRequired = false;
                return true;
            }

            _ResponseDelayTimer.Start();
            return false;
        }

        private bool MaxResponseDelayReached()
        {
            return (_ResponseDelayTimer.Elapsed >= MaxResponseDelay);
        }

        private void DataReceived()
        {
            lock (_ReceiverLock)
            {
                if (!IsDataReceivable())
                {
                    _Log.Info("Data not receivable");
                    return;
                }

                var receivedData = _DataReceiver.ReceiveData();



                if (receivedData == null)
                {
                    _Log.Info("No data received");
                    return;
                }

                if (_Log.IsWarnEnabled)
                    _Log.WarnFormat("QCom Rx: {0}", ArrayUtil.HexDump(receivedData, 0, (int)receivedData.Length));

                try
                {
                    if (!_CrcVerificationSpecification.IsSatisfiedBy(receivedData)) return;

                    Message message = Message.Parse(new BinaryReader(new MemoryStream(receivedData)));
                    if (message == null) return;

                    StopResponseReceiveTimer();
                    NotifyDataLinkStatusChange(true);
                    StartResponseReceiveTimer();

                    Model.ResponseReceived(message.ApplicationData);

                    if (_Log.IsDebugEnabled) _Log.DebugFormat("Received Message : {0}", message);                                        
                }
                catch (Exception ex)
                {
                    if (_Log.IsErrorEnabled) _Log.Error("Exception while parsing the message: ", ex);
                }
            }
        }

        private void StartResponseReceiveTimer()
        {
            _LinkDownTimer.Change((int)_ReceiveTimeOut.TotalMilliseconds);
        }

        private void StopResponseReceiveTimer()
        {
            _LinkDownTimer.Change(Timeout.Infinite);
        }

        private void ResponseTimeOut(object sender, EventArgs e)
        {
            ResponseDelayRequired = true;
            NotifyDataLinkStatusChange(false);
        }

        private void NotifyDataLinkStatusChange(bool linkStatus)
        {
            if (linkStatus == _IsLinkUp) return;

            _IsLinkUp = linkStatus;
            if (_Model != null)
                _Model.DataLinkStatusChanged(_IsLinkUp);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Close(true);
            GC.SuppressFinalize(this);
        }

        private void Close(bool shouldDispose)
        {
            _Port.Close();
            NotifyDataLinkStatusChange(false);
            _LinkDownTimer.Dispose();            
        }

        #endregion
    }
}
