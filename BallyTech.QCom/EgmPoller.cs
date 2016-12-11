using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.Utility.Collections;
using BallyTech.Utility.IButton;
using log4net;
using BallyTech.Utility.IO;

namespace BallyTech.QCom
{
    public class EgmPoller
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(EgmPoller));

        private CrcVerificationSpecification _CrcVerificationSpecification = new CrcVerificationSpecification();        
        private TimeSpan _MaxResponseTimeout = TimeSpan.FromMilliseconds(30);

        private DataReceiver _DataReceiver = null;

        private IPort _Port;
        public IPort Port
        {
            get { return _Port; }
            set { _Port = value; }
        }

        
        public TimeSpan MaxResponseTimeout
        {
            get { return _MaxResponseTimeout; }
            set { _MaxResponseTimeout = value; }
        }

        internal void Intialize(byte pollAddress)
        {
            _Port.Open();
            _DataReceiver = new DataReceiver(_Port)
                                {
                                    MaxResponseTimeout = this.MaxResponseTimeout,
                                    AddressVerficationRequired = true,
                                    ExpectedPollAddress = pollAddress
                                };
        }

        internal void Dispose()
        {
            _Port.Close();
        }


        internal void SendMessage(Message message)
        {
            _Port.ClearReceiveBuffer();    

            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    message.Publish(writer);
                    stream.Position = 0L;
                    byte[] data = new byte[stream.Length];

                    Array.Copy(stream.GetBuffer(), 0, data, 0, (int)stream.Length);

                    if (_Log.IsInfoEnabled)
                        _Log.InfoFormat("QCom Tx: {0}", ArrayUtil.HexDump(data, 0, (int)stream.Length));
                    _Port.Send(data);
                }
            }
        }


        internal Message ReceiveMessage()
        {
            var receivedData = _DataReceiver.ReceiveData();

            if (receivedData == null) return null;

            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("QCom Rx: {0}", ArrayUtil.HexDump(receivedData, 0, receivedData.Length));

            try
            {
                if (!_CrcVerificationSpecification.IsSatisfiedBy(receivedData)) return null;

                using (var stream = new MemoryStream(receivedData))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        stream.Position = 0L;
                        return Message.Parse(reader);                        
                    }
                }
            }           
            catch (Exception ex)
            {
                if (_Log.IsErrorEnabled) _Log.Error("Parsing failed due to", ex);
                return null;
            }

        }
    }
}
