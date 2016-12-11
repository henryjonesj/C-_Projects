using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model.Spam
{
    [GenerateICSerializable]
    public partial class SpamText
    {

        internal string Message { get; private set; }
        internal TimeSpan Duration { get; private set; }

        public SpamText()
        {
            
        }


        public SpamText(string message,TimeSpan duration)
        {
            this.Message = message;
            this.Duration = duration;
        }

        public void Reset()
        {
            this.Message = string.Empty;
        }




    }
}
