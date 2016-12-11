using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using BallyTech.QCom.Model;
using BallyTech.Utility.Cryptography;

namespace BallyTech.QCom.Messages
{
    public partial class Message
    {
        private const long LengthPosition = 1;


        protected void ComputeChecksum(BinaryReader input)
        {
            

        }



        protected void ComputeChecksum(BinaryWriter output)
        {
            int currentPosition = (int)output.BaseStream.Position;    
            output.BaseStream.Position = 0L;
            byte[] data = new byte[output.BaseStream.Length];
            output.BaseStream.Read(data,0,data.Length);
            ushort checkSum = Crc.UpdateCrc16LittleEndian(0, data, 0, data.Length);
            this.Checksum = (ushort) ((checkSum & 0xFFU) << 8 | (checkSum & 0xFF00U) >> 8);
            output.BaseStream.Position = currentPosition;
        }

        protected void UpdateLength(BinaryWriter output)
        {
            long currentposition = output.BaseStream.Position;
            var messageLength = currentposition + 2;
            output.BaseStream.Position = LengthPosition;
            output.Write((byte)(messageLength - (LengthPosition + 1)));
            output.BaseStream.Position = currentposition;        
        }


        protected void PostParse(BinaryReader reader)
        {
            ProtocolVersion = (this.Header.ProtocolVersionFlag == 0x01 ? ProtocolVersion.V16 : ProtocolVersion.V15);                       
        }


        private void UpdateProtocolVersion(BinaryReader input)
        {
            this.ApplicationData.UpdateProtocolVersion(ProtocolVersion);
        }

        public void SetTotalLength(BinaryReader input)
        {
            this.ApplicationData.SetTotalLength(this.Header.Length);
        }

        public static ProtocolVersion ProtocolVersion
        {
            get;
            private set;
        }
    
    }
}
