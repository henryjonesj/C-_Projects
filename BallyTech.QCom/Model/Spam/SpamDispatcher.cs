using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Model.Builders;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class SpamDispatcher
    {
        public QComModel Model { get; set; }

        private const int MaxLengthForV16 = 80;
        private const int MaxLengthForV15 = 40;

        private bool _IsTransparencyRequired;

        private int MaxMessageLength
        {
            get { return Model.ProtocolVersion == ProtocolVersion.V16 ? MaxLengthForV16 : MaxLengthForV15; }
        }

        internal void Send(string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            if (!DoesExceedMaxLength(message))
                Model.SendPoll(Build(message, FunctionCodes.SpecificPromotionalAdvisoryMessageA));
            else
            {
                var messages = GetSpamPolls(message);
                messages.ForEach((spam) => Model.SendPoll(spam));
            }

            _IsTransparencyRequired = false;
        }

        internal void Send(string message,bool transparencyRequired)
        {
            if (string.IsNullOrEmpty(message)) return;

            _IsTransparencyRequired = transparencyRequired;                
            Send(message);
        }

        private SerializableList<SpecificPromotionalAdvisoryPoll> GetSpamPolls(string message)
        {
            var messages = message.SplitBasedOnLength(MaxMessageLength);

            return new SerializableList<SpecificPromotionalAdvisoryPoll>()
                       {
                           {Build(messages.ElementAt(0), FunctionCodes.SpecificPromotionalAdvisoryMessageA)},
                           {Build(messages.ElementAt(1), FunctionCodes.SpecificPromotionalAdvisoryMessageB)}
                       };
        }


        private SpecificPromotionalAdvisoryPoll Build(string message, FunctionCodes functionCode)
        {
            var canSetTransparency = _IsTransparencyRequired &&
                                     (functionCode == FunctionCodes.SpecificPromotionalAdvisoryMessageA);

            var spamPoll = SpamBuilder.Build(message, canSetTransparency);
            spamPoll.MessageType = functionCode;

            return spamPoll;

        }

        private bool DoesExceedMaxLength(string message)
        {           
            return message.Length > MaxMessageLength;
        }

        internal void Clear()
        {
            Model.SendPoll(new SpecificPromotionalAdvisoryPoll());
            Model.SendPoll(new SpecificPromotionalAdvisoryPoll() { MessageType = FunctionCodes.SpecificPromotionalAdvisoryMessageB });
        }

    }
}
