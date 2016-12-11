using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Model;
using System.IO;
using BallyTech.Utility.IO;

namespace BallyTech.QCom.Messages
{
    partial class ApplicationMessage
    {
        public byte TotalLength { get; private set; }
       
        public bool IsBroadcast
        {
            get { return this.MessageType == FunctionCodes.Broadcast; }
        }

        private ProtocolVersion _ProtocolVersion = ProtocolVersion.Unknown;
        public ProtocolVersion ProtocolVersion 
        {
            get { return _ProtocolVersion != ProtocolVersion.Unknown ? _ProtocolVersion : Message.ProtocolVersion; }
            set { _ProtocolVersion = value;}
        }

        internal IMessageSender Sender { get; set; }
       
        public void UpdateProtocolVersion(ProtocolVersion protocolVersion)
        {            
            this.ProtocolVersion = protocolVersion;
        }

        internal virtual void UpdateBroadcastHeaderData(bool isSiteEnabled) 
        {
          
        }

        internal virtual void SetTotalLength(byte length)
        {
            TotalLength = length;
        }

        public virtual Message AppendDataLinkLayerWithPollAddress(byte pollAddress)
        {
            return new Message()
                       {
                           Header = new DataLinkLayer(){ Address = pollAddress},
                           ApplicationData = this
                       };
        }


        private void ValidateFunctionCode(BinaryReader input)
        {            
            var functionCodeByte = input.ReadByte();

             input.BaseStream.Position -= 1;

             if (!functionCodeByte.IsValidFunctionCode())
                 WriteInvalidFunctionCode(input);
        }

        private void WriteInvalidFunctionCode(BinaryReader reader)
        {
            var currentPosition = reader.BaseStream.Position;

            var writer = new BinaryWriter(reader.BaseStream);
            writer.BaseStream.Position = 3L;
            writer.Write((byte)FunctionCodes.InvalidFunctionCode);
            
            reader.BaseStream.Position = currentPosition;
        }

        public virtual bool IsGeneralPoll { get { return false; } }

        public virtual bool CanAcceptResponse(ApplicationMessage message) 
        {
            return true;
        } 

        internal bool IsOfType<T>()
        {
            return this is T;
        }

        internal T As<T>() where T: ApplicationMessage
        {
            return this as T;
        }

    }
    
}
