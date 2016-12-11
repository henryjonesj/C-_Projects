using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.Utility.Collections;
using BallyTech.Utility.Diagnostics;
using BallyTech.Utility.IButton;
using log4net;

namespace BallyTech.QCom
{
    public class DataReceiver
    {
        private static ILog _Log = LogManager.GetLogger(typeof (DataReceiver));

        private const int HeaderDataLength = 3;

        private readonly IPort _Port = null;
        public TimeSpan MaxResponseTimeout { get; set; }

        internal bool AddressVerficationRequired { get; set; }
        internal byte ExpectedPollAddress { get; set; }

        public DataReceiver(IPort port)
        {
            _Port = port;
            MaxResponseTimeout = TimeSpan.Zero;
        }

        public byte[] ReceiveData()
        {
            var receivedBuffer = new MemoryStream();

            ReadData(HeaderDataLength, receivedBuffer);
            
            if (receivedBuffer.Length == 0)
            {
                if (_Log.IsWarnEnabled) _Log.Warn("No data received");
                return null;
            }

            if (receivedBuffer.Length < 3)
            {
                receivedBuffer.Position = 0L;
                if (_Log.IsWarnEnabled)
                    _Log.WarnFormat("Received data with lesser length : {0}", ArrayUtil.HexDump(receivedBuffer.GetBuffer(), 0, (int)receivedBuffer.Length));
                return null;
            }

            var headerData = ExtractHeaderMessage(receivedBuffer);

            if(!(IsValidAddress(headerData)))
            {
                if (_Log.IsErrorEnabled) _Log.ErrorFormat("Invalid Address: {0}", headerData.Address);                    
                return null;
            }
            
            //TotalMessageLength = messageLength(1st byte position) + Address field + Length Field
            int messageLength = headerData.Length + 2;
            //Read remaining data if the whole data is not received
            bool haveReadRemainingData = ReadData(messageLength, receivedBuffer);

            if (!haveReadRemainingData)
            {
                receivedBuffer.Position = 0L;
                if (_Log.IsErrorEnabled)
                    _Log.ErrorFormat("Partial read data: {0}",
                                     ArrayUtil.HexDump(receivedBuffer.GetBuffer(), 0, (int) receivedBuffer.Length));
                return null;
            }

            receivedBuffer.Position = 0L;
            var receivedData = new byte[(int)receivedBuffer.Length];
            Array.Copy(receivedBuffer.GetBuffer(), receivedData, (int)receivedBuffer.Length);

            return receivedData;
            
        }

        private bool IsValidAddress(DataLinkLayer headerData)
        {
            if (!AddressVerficationRequired) return true;

            return headerData.Address == ExpectedPollAddress || headerData.Address == 0xFC;
        }

        private static DataLinkLayer ExtractHeaderMessage(MemoryStream memoryStream)
        {
            long currentPosition = memoryStream.Position;
            memoryStream.Position = 0L;

            var dataLinkLayer = DataLinkLayer.Parse(new BinaryReader(memoryStream));
            memoryStream.Position = currentPosition;

            return dataLinkLayer;
        }


        private bool ReadData(int messageLength, MemoryStream memoryStream)
        {
            //Stopwatch to check for max response time out
            Stopwatch _MaxResponseTimer = new Stopwatch();

            _MaxResponseTimer.Start();

            while (messageLength > memoryStream.Position)
            {
                var receivedData = _Port.Receive((messageLength - (int)memoryStream.Position));
                if (receivedData != null)
                {
                    memoryStream.Write(receivedData, 0, receivedData.Length);
                    _MaxResponseTimer.Reset();    
                    _MaxResponseTimer.Start();
                }

                //If max response timed out before reading the other bytes
                if (IsResponseTimeout(_MaxResponseTimer.Elapsed)) return false;
            }

            _MaxResponseTimer.Stop();
            return true;
        }


        private bool IsResponseTimeout(TimeSpan responseTimer)
        {
            return ((MaxResponseTimeout - responseTimer) <= TimeSpan.Zero);
        }
    }
}
