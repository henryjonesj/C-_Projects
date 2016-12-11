using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class EgmInfo
    {
        public byte ManufacturerId { get; set; }

        public decimal SerialNumber { get; set; }

        public uint AssetNumber { get; set; }
    
    }
}
