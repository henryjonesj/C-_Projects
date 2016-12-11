using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;

namespace BallyTech.QCom.Messages
{
    partial class EgmConfigurationResponse
    {

        public bool IsProtocolValid
        {
            get 
            {
                return (this.ProtocolVersion == GetNetworkProtocolVersion());
            }
        }

        private ProtocolVersion GetNetworkProtocolVersion()
        {
            if ((this.NetworkProtocolVersion & NetworkProtocolVersionCharacteristics.ProtocolVersionMask) == NetworkProtocolVersionCharacteristics.NewQPvEgm)
                return ProtocolVersion.V16;

            if ((this.NetworkProtocolVersion & NetworkProtocolVersionCharacteristics.ProtocolVersionMask) == NetworkProtocolVersionCharacteristics.OldQPvEgm)
                return ProtocolVersion.V15;            

            return ProtocolVersion.Unknown;
        }
    }

}
