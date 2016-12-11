using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.Gtm;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model.Builders
{
    public static class ExternalJackpotInformationPollBuilder
    {
        private static int MaxStringLength = 16;

        public static ExternalJackpotInformationPoll Build(ICollection<IExternalJackpotDisplayProfile> Profiles)
        {
            ExternalJackpotInformationPoll poll = new ExternalJackpotInformationPoll()
            {
                ExternalJackpotIconDisplayFlag = ExternalJackpotIconDisplayFlagCharacteristics.IconDisplayFlag1,
            };
            
            poll.ExternalJacpotLevelDetails = new SerializableList<ExternalJackpotLevelDetail>(); 
            foreach (IExternalJackpotDisplayProfile Profile in Profiles)
            {
                poll.ExternalJacpotLevelDetails.Add(new ExternalJackpotLevelDetail()
                    {
                        ExternalJackpotProgressiveGroupId = (ushort)Profile.ProgressiveGroupId,
                        LevelName = string.IsNullOrEmpty(Profile.LevelName) ? string.Empty : GetName(Profile.LevelName),
                        LevelFlag = (Profile.LevelId <= 0) ? 0 : (ExternalJackpotLevel)(Enum.Parse(typeof(ExternalJackpotLevel), (Profile.LevelId - 1).ToString(), true))
                    }
                );                
            }
            poll.ExternalJackpotFlag = GetFlagCharacteristics(Profiles.Count);
            poll.RtpPercentage = Profiles.Sum(element => element.ReturnToPlayer);
            return poll;
        }

        private static ExternalJackpotFlagCharacteristics GetFlagCharacteristics(int noOfMysteriesConfigured)
        {
            return (ExternalJackpotFlagCharacteristics)(Enum.Parse(typeof(ExternalJackpotFlagCharacteristics), noOfMysteriesConfigured.ToString(), true))
                            | ExternalJackpotFlagCharacteristics.DisplayFlag;
        }

        private static string GetName(string name)
        {
            return name.Substring(0, name.Length > (MaxStringLength - 1) ? (MaxStringLength - 1) : name.Length).PadRight(MaxStringLength, Char.MinValue);
        }
    }
}
