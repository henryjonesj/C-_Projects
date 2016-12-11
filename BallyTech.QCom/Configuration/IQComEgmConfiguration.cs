using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;

namespace BallyTech.QCom.Configuration
{
    public interface IQComEgmConfiguration: IEgmConfiguration
    {
        uint TotalNumberOfGames { get; }
    
    }
}
