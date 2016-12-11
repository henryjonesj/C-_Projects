using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class PollQueue : SerializableList<Request>
    {
        private Request _LastSentPoll = null;

        public Request NextPoll
        {
            get
            {
                _LastSentPoll = this.Count > 0 ? this[0] : null;
                return _LastSentPoll;
            }
        }

        public void RemoveCurrentPoll()
        {
            if (this.Count > 0 && _LastSentPoll != null) this.Remove(_LastSentPoll);    
        }

        public Request LastSentPoll { get { return _LastSentPoll; } }
    }
}
