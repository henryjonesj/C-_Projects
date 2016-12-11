using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BallyTech.QCom.Messages
{
    partial class ProgramHashResponse
    {

        const int v16ProgramHashSize = 20;
        const int v15ProgramHashSize = 8;

        public byte[] ProgramHash
        {
            get;
            private set;
        }

        public void ParseProgramHash(BinaryReader input)
        {   
            int ProgramHashSize =  Message.ProtocolVersion == ProtocolVersion.V16 ? v16ProgramHashSize : v15ProgramHashSize;
            ProgramHash = new byte[ProgramHashSize];     
            input.Read(ProgramHash, 0, ProgramHashSize);
        }
    
        public override bool CanAcceptResponse(ApplicationMessage message)
        {
            return !(message is ProgramHashRequestPollMessage);
        } 

    }
}
