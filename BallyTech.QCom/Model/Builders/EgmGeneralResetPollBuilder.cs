using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;

namespace BallyTech.QCom.Model.Builders
{
    public static class EgmGeneralResetPollBuilder
    {

        public static Request BuildLockupResetPoll(ProtocolVersion protocolVersion,EgmMainLineCurrentStatus status)
        {
            switch (protocolVersion)
            {
                case ProtocolVersion.V16:
                    return new EgmGeneralResetPoll()
                               {
                                   GeneralResetFlagStatus = GeneralResetFlagCharacteristics.ClearCurrentLockUpCCondition,
                                   State = status
                               };

                case ProtocolVersion.V15:
                    return new EgmGeneralResetPollV15() { GeneralResetFlagStatus = GeneralResetFlagCharacteristics.ClearCurrentLockUpCCondition };

            }

            return null;
        }

        public static Request BuildFaultsResetPoll(ProtocolVersion protocolVersion)
        {
            switch (protocolVersion)
            {
                case ProtocolVersion.V16:
                    return new EgmGeneralResetPoll()
                    {
                        GeneralResetFlagStatus = GeneralResetFlagCharacteristics.ClearCurrentFaultCondition
                    };

                case ProtocolVersion.V15:
                    return new EgmGeneralResetPollV15() 
                    { 
                        GeneralResetFlagStatus = GeneralResetFlagCharacteristics.ClearCurrentFaultCondition 
                    };
            }

            return null;
        }


    }
}
