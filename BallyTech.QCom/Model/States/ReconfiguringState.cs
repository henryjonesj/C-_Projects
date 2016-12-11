using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class ReconfiguringState: ConfiguringState
    {
        public override void Process(MeterGroupContributionResponse meterResponse)
        {
            UpdateMeters(meterResponse.MeterGroups);

            base.Process(meterResponse);
        }
    
    }
}
