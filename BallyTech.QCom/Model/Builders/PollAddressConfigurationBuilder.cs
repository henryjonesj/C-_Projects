using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.Utility.Time;

namespace BallyTech.QCom.Model.Builders
{
    public static class PollAddressConfigurationBuilder
    {

        public static EgmPollAddressConfiguration Build(decimal serialNumber, byte manufacturerId)
        {
            return new EgmPollAddressConfiguration()
                       {
                           ManufacturerId = manufacturerId,
                           SerialNumber = serialNumber,
                           PollAddress = 0x01,
                           ExtendedDataSize = 0x05,
                           NoOfEgms = 0x01,
                           SystemDateTime = TimeProvider.UtcNow.ToLocalTime(),
                           GlobalFlag = GlobalFlagStatus.Default | GlobalFlagStatus.ClockDisplayFlag | GlobalFlagStatus.SiteEnableFlag
                       };
        }

    }
}
