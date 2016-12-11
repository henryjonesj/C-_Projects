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
    public partial class SpecificationFactory
    {
        private SpecificationKeyedCollection _SpecificationRepository = new SpecificationKeyedCollection();
        public SpecificationKeyedCollection SpecificationRepository
        {
            get { return _SpecificationRepository; }
        }

        public QComResponseSpecification GetSpecification(FunctionCodes code)
        {
            return _SpecificationRepository.Contains(code) ? _SpecificationRepository[code] : new NullSpecification();
        }
    }
}
