using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.QCom.Messages;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Model.Egm;
using BallyTech.Utility;
using BallyTech.Utility.Time;
using log4net;
using BallyTech.QCom.Model;
using System.IO;

namespace BallyTech.QCom.Model.Specifications
{
    [GenerateICSerializable]
    public partial class EventValidationSpecification: QComResponseSpecification
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(EventValidationSpecification));
       
        public override bool IsSatisfiedBy(Event Event)
        {
            _Log.Info("Validating Event Code");

            if ((ushort)Event.EventCode > Convert.ToUInt16("0x7FFF", 16)|| Event.EventCode < 0)
            {
                _Log.Info("Invalid Event Code");
                return false;
            }

            _Log.Info("Validating Event Size");
            
            int expectedEventSize = EventDataSizeMapping.GetEventDataSize(Event.EventCode);

            if (Event.EventSize != expectedEventSize || Event.EventSize > 16 && !IsSatisfiedBy(Event.EventCode))
            {   
                if (_Log.IsInfoEnabled) _Log.InfoFormat("Invalid Event Size");
                return false;
            }

            return true;
        }

        public override bool IsSatisfiedBy(QComRawDateTime dateTime)
        {
            DateTime EventDateTime;
            try
            {
                EventDateTime = QComConvert.ConvertQComRawDateTimeToDateTime(dateTime);
            }
            catch(ArgumentOutOfRangeException ex)
            {
                _Log.InfoFormat("Invalid date time as {0}", ex.Message);
                return false; 
            }

            return IsDateTimeWithinLimit(EventDateTime);
        }

        public bool IsDateTimeWithinLimit(DateTime item)
        {
            _Log.InfoFormat("Validating Event Date and Time:{0} ",item);
            
            var CurrentDateTime = TimeProvider.UtcNow.ToLocalTime();

            if (item > (CurrentDateTime.AddSeconds(5)))
            {
                
                if (_Log.IsInfoEnabled) _Log.InfoFormat("Seconds is invalid");
                return false;
            }

            if (item.AddMonths(1) < CurrentDateTime)
            {
                if (_Log.IsInfoEnabled) _Log.InfoFormat("Months is invalid");
                return false;
            }

            return true;
        }

        public override bool IsSatisfiedBy(EventCodes EventCode)
        {
            _Log.Info("Checking For Reserved Events");

            if(!Enum.IsDefined(typeof(EventCodes),EventCode))
            {
                if (_Log.IsInfoEnabled) _Log.InfoFormat("Reserved Event with Event Code: {0}", EventCode);
                return true;
            }

            return false;
        } 


        public override FunctionCodes FunctionCode
        {
            get { return FunctionCodes.Event; }
        }
             
    
    }
}
