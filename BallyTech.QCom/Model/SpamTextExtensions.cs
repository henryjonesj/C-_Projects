using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model
{
    public static class SpamTextExtensions
    {
        public static IEnumerable<string> SplitBasedOnLength(this string message, int messageLength)
        {
            var messageList = new SerializableList<string>();

            var index = 0;
            var length = 0;
            for (; index < 2; index++)
            {
                length = Math.Min(messageLength, (message.Length - length));
                messageList.Add(message.Substring(index * messageLength, length));
            }

            return messageList;
        }
    }
}
