using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class SplitTimeInterval
    {
        private SerializableList<TimeSpan> _SplitIntervals = new SerializableList<TimeSpan>();
        private short _CurrentSplitIndex = -1;

        public SplitTimeInterval()
        {

        }

        public SerializableList<TimeSpan> SplitIntervals
        {
            get { return _SplitIntervals; }
            set { _SplitIntervals = value; }
        }

        internal TimeSpan NextSplit
        {
            get { return _SplitIntervals[++_CurrentSplitIndex]; }
        }

        internal bool IsFinalSplit
        {
            get { return (_SplitIntervals.Count() == (_CurrentSplitIndex + 1)); }
        }
        
        internal void Reset()
        {
            _CurrentSplitIndex = -1;
        }

        internal bool IsReset
        {
            get { return _CurrentSplitIndex == -1; }
        }

    }
}
