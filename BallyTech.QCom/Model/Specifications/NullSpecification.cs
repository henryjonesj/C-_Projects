using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using BallyTech.QCom.Messages;

namespace BallyTech.QCom.Model.Specifications
{
    [GenerateICSerializable]
    public partial class NullSpecification : QComResponseSpecification
    {
        public override FunctionCodes FunctionCode
        {
            get { return FunctionCodes.None; }
        }
    }
}
