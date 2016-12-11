using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using log4net;
using System.IO.Ports;
using System.Threading;
using BallyTech.Utility.Collections;

namespace BallyTech.QCom
{
    public class QComPortWrapper : IPort, IDisposable
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(QComPortWrapper));


        private int _ParityDelayInMilliseconds = 2;
        public int ParityDelayInMilliseconds
        {
            get { return _ParityDelayInMilliseconds; }
            set { _ParityDelayInMilliseconds = value; }
        }


        private SerialPort _Port = new SerialPort();

        private string _PortName;
        public string PortName
        {
            get { return _PortName; }
            set { _PortName = value; }
        }

        #region IPort Members

        public event Action DataReceived = delegate { };    

        public void Open()
        {
            if (_Port.IsOpen)
            {
                if (_Log.IsWarnEnabled) _Log.Warn("Port already open.");
                return;
            }

            try
            {
                _Port.BaudRate = 19200;
                _Port.DataBits = 8;
                _Port.Parity = Parity.Space;
                _Port.PortName = _PortName;
                _Port.RtsEnable = true;
                _Port.DtrEnable = true;
                _Port.StopBits = StopBits.One;
                _Port.ReadTimeout = 2000;                              
                _Port.ErrorReceived += new SerialErrorReceivedEventHandler(OnErrorDataReceived);              
                _Port.DataReceived += new SerialDataReceivedEventHandler(OnDataReceived);

                if (_Log.IsInfoEnabled) _Log.InfoFormat("Opening port {0}", _Port.PortName);

                _Port.Open();

            }
            catch (Exception ex)
            {
                if (_Log.IsErrorEnabled) _Log.Error("Execption while opening the port", ex);
            }
        }

        void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_Log.IsInfoEnabled) _Log.Info("Receiving data");
            DataReceived();
        }



        void OnErrorDataReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            if (_Log.IsErrorEnabled) _Log.Error(e.EventType);
        }

        public void Close()
        {
            if (!_Port.IsOpen)
            {
                if (_Log.IsWarnEnabled) _Log.Warn("Port not open");
                return;
            }

            try
            {
                _Port.Close();
            }
            catch (InvalidOperationException ex)
            {
                if (_Log.IsErrorEnabled) _Log.Error("Exception while closing the port", ex);
            }
        }

        public void Send(byte[] data)
        {
            if (!_Port.IsOpen)
            {
                if (_Log.IsWarnEnabled) _Log.Warn("Port not open for write");
                return;
            }

            try
            {
                _Port.Parity = Parity.Mark;                
                _Port.Write(data,0,1);
                Thread.Sleep(ParityDelayInMilliseconds);
                _Port.Parity = Parity.Space;
                _Port.Write(data, 1, data.Length - 1);                
            }
            catch (Exception ex)
            {
                if (_Log.IsErrorEnabled) _Log.Error("Exception while writing the data to the port", ex);
            }
        }

        public byte[] Receive(int length)
        {
            if (!_Port.IsOpen)
            {
                if (_Log.IsWarnEnabled) _Log.Warn("Port not open for read");
                return null;
            }
            
            if (_Port.BytesToRead <= 0) return null;

            int bytesAvailableToRead = Math.Min(length, _Port.BytesToRead);
            byte[] receivebuffer = new byte[bytesAvailableToRead];

            _Port.Read(receivebuffer, 0, bytesAvailableToRead);
            return receivebuffer;
        }

        #endregion

        #region IPort Members


        public void ClearReceiveBuffer()
        {
            try
            {
                if (_Port.BytesToRead > 0)
                    if (_Log.IsWarnEnabled) _Log.WarnFormat("Discarding {0} bytes", _Port.BytesToRead);

                _Port.DiscardInBuffer();
            }
            catch (Exception ex)
            {
                if (_Log.IsErrorEnabled)
                    _Log.ErrorFormat("Exception thrown while clearing the in buffer: \n {0}", ex);
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Close(true);
            GC.SuppressFinalize(this);
        }

        private void Close(bool shouldDispose)
        {
            _Port.Dispose();
        }

        #endregion
    }
}
