using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Time;

namespace BallyTech.QCom.Messages
{
    partial class CurrentDateTimeBroadcast
    {
        private long _LengthPosition = 0;

        private void UpdateLength(System.IO.BinaryWriter output)
        {
            var currentPosition = output.BaseStream.Position;
            output.BaseStream.Position = _LengthPosition;
            output.Write((byte)(currentPosition - (_LengthPosition + 1)));
            output.BaseStream.Position = currentPosition;
        }

        private void SaveLengthPosition(System.IO.BinaryWriter output)
        {
            _LengthPosition = output.BaseStream.Position;
        }


        public override Message AppendDataLinkLayerWithPollAddress(byte pollAdrress)
        {
            return new Message()
                       {
                           Header = new DataLinkLayer(){ Address  = 0xFF},
                           ApplicationData = this
                       };
        }


        internal override void UpdateBroadcastHeaderData(bool isSiteEnabled) 
        {
            this.SystemDateTime = TimeProvider.UtcNow.ToLocalTime();
            this.GlobalFlag = GlobalFlagStatus.ClockDisplayFlag;

            if (isSiteEnabled) this.GlobalFlag |= GlobalFlagStatus.SiteEnableFlag;

        }


    }
}
