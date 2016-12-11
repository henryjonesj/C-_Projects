using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;

namespace BallyTech.QCom.Model.EventsListeners
{
    [GenerateICSerializable]
    public partial class EventListenerCollectionBase
    {
        public QComModel Model { get; set; }
        protected DuplicateEventsFilter _DuplicateEventFilter = null;
        protected SerializableList<EventListenerBase> _EventListenerCollection = new SerializableList<EventListenerBase>();
        protected SerializableList<EventCodes> _UnAuthorizedAccessEventsCollection = new SerializableList<EventCodes>();

        public EventListenerCollectionBase() { }

        public EventListenerCollectionBase(QComModel model)
        {
            Model = model;
            _DuplicateEventFilter = new DuplicateEventsFilter();
         

            _EventListenerCollection.Add(new CoinAcceptorListener() { Model = model });
            _EventListenerCollection.Add(new NoteAcceptorEventListener() { Model = model });
            _EventListenerCollection.Add(new HopperEventListener() { Model = model });
            _EventListenerCollection.Add(new DoorEventListener() { Model = model });
            _EventListenerCollection.Add(new FundsTransferEventListener() { Model = model });
            _EventListenerCollection.Add(new JackpotEventListener() { Model = model });
            _EventListenerCollection.Add(new TicketEventListener() { Model = model });
            _EventListenerCollection.Add(new CabinetEventsListener() { Model = model });
            _EventListenerCollection.Add(new PrinterEventsListener() { Model = model });
            PopulateUnAuthorizedAccessEventsCollection();
           
        }

        private void PopulateUnAuthorizedAccessEventsCollection()
        {
            _UnAuthorizedAccessEventsCollection.Add(EventCodes.EgmProcessorDoorOpened);
            _UnAuthorizedAccessEventsCollection.Add(EventCodes.EgmProcessorDoorClosed);
            _UnAuthorizedAccessEventsCollection.Add(EventCodes.PowerOffProcessorDoorAccess);

        }

        public virtual void Dispatch(Event response)
        {
            if (_DuplicateEventFilter.Filter(response) == null) return;

            foreach (var eventListener in _EventListenerCollection)
            {
                response.Dispatch(eventListener);
            }
        }
    }
}
