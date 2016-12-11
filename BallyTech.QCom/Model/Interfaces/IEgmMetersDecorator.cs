using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;

namespace BallyTech.QCom.Model.Egm
{
    public interface IEgmMetersDecorator : IEgmMeters
    {
        void QueryMeters(params MeterType[] meterTypes);        
    }

    public enum MeterType
    {
        Game,
        NoteAcceptor,
        Ticket,
        Cashless,
        Coins,
        Jackpot
    }
}
