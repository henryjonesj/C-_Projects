using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class MessageReceivedCounter<TMessage> where TMessage : ApplicationMessage
    {
        protected uint _MessageReceivedCount = 0;
        private uint _MessageReceivedCountLimit = 0;
        
        public MessageReceivedCounter<TMessage> WithCountLimit(uint counterLimit)
        {
            _MessageReceivedCountLimit = counterLimit;
            return this;
        }

        public virtual void Received(ApplicationMessage message)
        {
            if (message.IsOfType<TMessage>())
            {
                _MessageReceivedCount++;
                return;
            }

            Reset();
        }

        internal void Reset()
        {
            _MessageReceivedCount = 0;
        }

        public bool IsCountLimitReached
        {
            get { return _MessageReceivedCount >= _MessageReceivedCountLimit; }
        }

    }
}
