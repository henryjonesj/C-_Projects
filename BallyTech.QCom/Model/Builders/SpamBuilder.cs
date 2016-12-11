using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;

namespace BallyTech.QCom.Model.Builders
{
    public static class SpamBuilder
    {

        internal static SpecificPromotionalAdvisoryPoll Build(string message, bool isTransparencyRequired)
        {
            var spamPoll = new SpecificPromotionalAdvisoryPoll()
                               {
                                   AdvisoryMessageFlag = AdvisoryMessageFlagCharacteristics.FanfareFlag,
                                   AdvisoryLength = (byte) message.Length,
                                   AdvisoryText = message
                               };

            if (isTransparencyRequired)
                spamPoll.AdvisoryMessageFlag |= AdvisoryMessageFlagCharacteristics.ProminenceFlag;

            return spamPoll;
        }

    }
}
