using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.QCom.Model.Egm;
using BallyTech.Utility.Time;
using System.IO;

namespace BallyTech.QCom.Messages
{
    partial class Event : IEquatable<Event>
    {
        public bool IsInvalidDateTime { get; set; }
        
        
        public DateTime GetDateTime()
        {
           if (IsInvalidDateTime) return TimeProvider.UtcNow.ToLocalTime();

           return QComConvert.ConvertQComRawDateTimeToDateTime(this._EventDateTime);
     
        }

        public virtual ExtendedEgmEventData GetExtendedEgmEventData()
        {
            return new ExtendedEgmEventData()
                    {
                        IsEventRaisedByEgm = true,
                        EventCode = (uint)this.EventCode,
                        SequenceNumber = this.EventSequenceNumber,
                        EventSize = this.EventSize,
                        DateTime = this.GetDateTime()
                    };
        }

        #region IEquatable<Event> Members

        public bool Equals(Event other)
        {
            if (other == null) return false;

            return this.EventSequenceNumber == other.EventSequenceNumber &&
                   this.EventDateTime.Equals(other.EventDateTime) &&
                   this.EventCode == other.EventCode;
        }

        #endregion

        public bool IsUnnumberedEvent()
        {
            return this.EventSequenceNumber == 0;
        }

        public virtual bool IsUnknown { get; private set; }

        public void ValidateEventCode(BinaryReader reader)
        {
            var savedPosition = reader.BaseStream.Position;

            var eventCode = reader.ReadUInt16();

            if (IsUnknownEvent(eventCode)) this.IsUnknown = true; 

            reader.BaseStream.Position = savedPosition;
        }

        private bool IsUnknownEvent(UInt16 eventCode)
        {
            return (!Enum.IsDefined(typeof(EventCodes), eventCode) &&
                (eventCode <= Convert.ToUInt16("0x7FFF", 16) && eventCode >= 0));
                
        }
    }
    
}
