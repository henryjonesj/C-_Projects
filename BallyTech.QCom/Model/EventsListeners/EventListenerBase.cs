using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.Egm;

namespace BallyTech.QCom.Model.EventsListeners
{
    [GenerateICSerializable]
    public partial class EventListenerBase : EventListener
    {
        private QComModel _Model;
        public QComModel Model
        {
            get { return _Model; }
            set { _Model = value; }
        }
        
        protected EgmAdapter EgmAdapter
        {
            get { return Model.Egm; }
        }
    }
}
