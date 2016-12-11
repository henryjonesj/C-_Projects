using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using log4net;

namespace BallyTech.QCom.Configuration
{
    [GenerateICSerializable]
    public partial class AttendantConfigurationRequestHandler : ConfigurationRequestHandlerBase
    {
        internal override void RequestConfiguration()
        {
            if (!Model.State.IsConfigurationMissing()) return;

            Model.IsConfigurationRequired = true;
        }
    }
}
