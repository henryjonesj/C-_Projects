using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model.Specifications
{
    [GenerateICSerializable]
    public partial class QComSpecificationFactory : SpecificationFactoryBase
    {
        //public QComSpecificationFactory()
        //{
        //    _SpecificationRepository.Add(new NoteAcceptorStatusResponseSpecification());
        //}
    }

    [GenerateICSerializable]
    public partial class QComNullSpecificationFactory : SpecificationFactoryBase
    {
        //public override T Get<T>()
        //{
        //    return new NullSpecification();
        //}
    }
}
