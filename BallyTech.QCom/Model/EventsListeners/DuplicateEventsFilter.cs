using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using log4net;

namespace BallyTech.QCom.Model.EventsListeners
{
    [GenerateICSerializable]
    public partial class DuplicateEventsFilter
    {
        private static ILog _Log = LogManager.GetLogger(typeof (DuplicateEventsFilter));

        private Event _LastReceivedEvent = null;

        internal Event Filter(Event @event)
        {
            if (@event.Equals(_LastReceivedEvent))
            {
                if (_Log.IsWarnEnabled)
                    _Log.WarnFormat("Discarding the duplicate event with code: {0}", @event.EventCode);                
                return null;
            }

            _LastReceivedEvent = @event;
            return @event;
        }

    }
}
