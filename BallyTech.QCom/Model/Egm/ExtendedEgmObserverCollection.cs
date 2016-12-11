using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.Utility.Collections;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class ExtendedEgmObserverCollection : ObserverCollection<IExtendedEgmObserver>
    {

    }
}
