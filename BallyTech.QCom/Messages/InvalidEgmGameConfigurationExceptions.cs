using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Messages
{
    public partial class InvalidConfigurationException : Exception
    {
        internal string reason;

        public InvalidConfigurationException(string reason) { this.reason = reason; }

    }


    public partial class InvalidEgmConfigurationException : InvalidConfigurationException
    {
        public InvalidEgmConfigurationException(string reason) : base(reason) { }
    }


    public partial class InvalidGameConfigurationException : InvalidConfigurationException
    {
        public InvalidGameConfigurationException(string reason) : base(reason) { }
    }

    public partial class InvalidProgressiveConfigurationException : InvalidConfigurationException
    {
        public InvalidProgressiveConfigurationException(string reason) : base(reason) { }
    }
}
