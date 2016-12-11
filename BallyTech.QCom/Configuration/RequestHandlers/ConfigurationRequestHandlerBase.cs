using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Model;

namespace BallyTech.QCom.Configuration
{
    [GenerateICSerializable]
    public abstract partial class ConfigurationRequestHandlerBase
    {

        internal QComModel Model { get; set; }

        internal abstract void RequestConfiguration();
    }
}
