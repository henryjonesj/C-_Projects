using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using log4net;
using BallyTech.Gtm;
using BallyTech.QCom.Model.Specifications;
using BallyTech.QCom.Model.Handlers;
using BallyTech.QCom.Model.Egm;

namespace BallyTech.QCom.Model.EventsListeners
{
    [GenerateICSerializable]
    public partial class EventListenerCollection : EventListenerCollectionBase
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (EventListenerCollection));

        private QComResponseSpecification _EventValidationSpecification = null;
       
        private byte _ExpectedEventSequenceNumber = 0;


        public EventListenerCollection() { }

        public EventListenerCollection(QComModel model) : base (model)
        {
            
        }
        
        private bool IsValidSequenceNumber(Event response)
        {
            if (_Log.IsDebugEnabled)
                _Log.DebugFormat("Rxd Event Seq. No: {0}, Expected: {1}", response.EventSequenceNumber, _ExpectedEventSequenceNumber);

            if (response.EventCode == EventCodes.NVRAMCleared) return true;

            if (_ExpectedEventSequenceNumber == 0 || response.EventSequenceNumber == 0 || 
                            (Model.RaleHandler.State == RaleProgressState.InProgress))
                return true;

            return (_ExpectedEventSequenceNumber == response.EventSequenceNumber);
        }


        public override void Dispatch(Event response)
        {
            if (_DuplicateEventFilter.Filter(response) == null) return;

            bool isSeqNumberValid = IsValidSequenceNumber(response);

            var previousExpectedEventSeqNumber = _ExpectedEventSequenceNumber;

            if (!response.IsUnnumberedEvent() && Model.RaleHandler.State != RaleProgressState.InProgress)
                CalculateNextSequenceNumber(response.EventSequenceNumber);
            
            if (!isSeqNumberValid)
            {
                Model.Egm.ExtendedEventData = new ExtendedEgmEventData()
                {
                    ExpectedSequenceNumber = previousExpectedEventSeqNumber,
                    SequenceNumber = response.EventSequenceNumber
                };

                Model.Egm.ReportEvent(EgmEvent.UnexpectedEGMEventSequenceNumber);
                Model.RaleHandler.CheckAndRequestAllLoggedEvents();
            }

            _EventValidationSpecification = Model.SpecificationFactory.GetSpecification(FunctionCodes.Event);

            if (!response.IsUnknown && !_EventValidationSpecification.IsSatisfiedBy(response))
            {
                    Model.Egm.ExtendedEventData = new ExtendedEgmEventData()
                    {
                        EventCode = (uint)response.EventCode,
                        EventSize = response.EventSize
                    };
                    Model.Egm.ReportEvent(EgmEvent.InvalidEgmEvent);
                    return;
                
            }

            if (!_EventValidationSpecification.IsSatisfiedBy(response.EventDateTime))
            {
                response.IsInvalidDateTime = true;

                Model.Egm.ExtendedEventData = new ExtendedEgmEventData()
                {
                    InvalidDateTime = new InvalidDateTime()
                    {
                        UnRepresentableDate = response.EventDateTime.Date,
                        UnRepresentableTime = response.EventDateTime.Time
                    }
                };

                Model.Egm.ReportEvent(EgmEvent.InvalidDateTimeStamp);

            }

            _Log.InfoFormat("Adding ExtendedData for {0}",response.EventCode);
            Model.Egm.ExtendedEventData = response.GetExtendedEgmEventData();

            _Log.InfoFormat("Event's Date and Time:{0} ", response.GetDateTime());

            if (response.IsUnknown) Model.Egm.ReportEvent(EgmEvent.UnknownEvent);

            foreach (var eventListener in _EventListenerCollection)
            {
                response.Dispatch(eventListener);
            }

            if (_UnAuthorizedAccessEventsCollection.Contains(response.EventCode))
                Model.Egm.GameLockedOnUnauthorizedAccess.Value = true;
        
            Model.PollPurgeEvent(response.EventSequenceNumber);
        }

        private void CalculateNextSequenceNumber(byte sequenceNumber)
        {
            if (sequenceNumber == byte.MaxValue)
            {
                _ExpectedEventSequenceNumber = 1;
                return;
            }

            _ExpectedEventSequenceNumber = (byte)(sequenceNumber + 1);
        }
    }
}
