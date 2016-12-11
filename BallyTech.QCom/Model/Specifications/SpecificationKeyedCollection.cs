using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;

namespace BallyTech.QCom.Model.Specifications
{
    [GenerateICSerializable]
    public partial class SpecificationKeyedCollection : SerializableKeyedCollection<FunctionCodes, QComResponseSpecification>
    {
        protected override FunctionCodes GetKeyForItem(QComResponseSpecification item)
        {
            return item.FunctionCode;
        }
    }
}
