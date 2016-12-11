using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace BallyTech.QCom
{
    public interface IPort
    {
        void Open();
        void Close();
        void Send(byte[] data);        
        byte[] Receive(int length);
        event Action DataReceived;
        void ClearReceiveBuffer();
    }
}
