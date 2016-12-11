using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace BallyTech.QCom.Messages
{
    public interface IMessageSender
    {
        void OnMessageDelivered();
    }
}
