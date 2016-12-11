using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.Egm;

namespace BallyTech.QCom.Model.MessageProcessors
{
    [GenerateICSerializable]
    public partial class MessageProcessor : ApplicationMessageListener
    {
        private QComModel _Model;
        public QComModel Model
        {
            get { return _Model; }
            set { _Model = value; }
        }

        protected DeviceCollection Devices
        {
            get { return Model.Egm.Devices; }
        }   
    }
}
