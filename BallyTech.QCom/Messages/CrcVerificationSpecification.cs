using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.Utility.Cryptography;
using System.IO;
using BallyTech.Utility.Serialization;
using log4net;

namespace BallyTech.QCom.Messages
{
    [GenerateICSerializable]
    public partial class CrcVerificationSpecification : SpecificationBase<byte[]>
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(CrcVerificationSpecification));

        public override bool IsSatisfiedBy(byte[] buffer)
        {
            var computedCrc = Crc.UpdateCrc16LittleEndian(0, buffer, 0, buffer.Length - 2);
            var receivedCrc = (buffer[buffer.Length - 1] << 8) | buffer[buffer.Length - 2];

            if (computedCrc != receivedCrc)
            {
                if (_Log.IsWarnEnabled) _Log.WarnFormat("CRCs mismatch. Computed Crc = {0}, Received Crc = {1}", computedCrc, receivedCrc);
                return false;
            }
            return true;
        }
    }
}
