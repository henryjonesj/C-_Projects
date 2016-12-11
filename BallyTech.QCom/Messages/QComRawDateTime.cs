using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace BallyTech.QCom.Messages
{
    partial class QComRawDateTime
    {
        public decimal Date
        {
            get { return QComConvert.ConvertDecimalArrayToDecimal(new decimal[] { Day, Month, Year }); }
                
        }

        public decimal Time
        {
            get {return QComConvert.ConvertDecimalArrayToDecimal(new decimal[] { Hours, Minutes, Seconds }); }
        }

        public bool Equals(QComRawDateTime otherEventDateTime)
        {
            return this.Day == otherEventDateTime.Day &&
                   this.Month == otherEventDateTime.Month &&
                   this.Year == otherEventDateTime.Year &&
                   this.Hours == otherEventDateTime.Hours &&
                   this.Minutes == otherEventDateTime.Minutes &&
                   this.Seconds == otherEventDateTime.Seconds;
        }    
    }
}
