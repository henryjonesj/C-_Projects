using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Model.Egm;
using BallyTech.Utility;

namespace BallyTech.QCom.Messages
{
    public partial class MultiGameVariationMetersResponse
    {
        protected SerializableDictionary<MeterId, Meter> meters = new SerializableDictionary<MeterId, Meter>();
        public virtual void UpdateMeterGroups()
        {
            meters.Clear();
            meters.Add(MeterId.Plays, new Meter(this.GameStroke,1,(uint.MaxValue + 1m)));
            meters.Add(MeterId.Bets, new Meter(this.GameTurnOver, 1, (uint.MaxValue + 1m)));
            meters.Add(MeterId.Wins, new Meter(this.TotalGameWins, 1, (uint.MaxValue + 1m)));
            meters.Add(MeterId.LpWins, new Meter(this.TotalGameLinkedProgressiveWins, 1, (uint.MaxValue + 1m)));
        }

        public SerializableDictionary<MeterId, Meter> GetMeterGroups()
        {
            return meters;
        }
    }

    public partial class MultiGameVariationMetersResponseV16
    {
        public override void UpdateMeterGroups()
        {
            base.UpdateMeterGroups();
            meters.Add(MeterId.GamesWon, new Meter(this.TotalGameWon, 1, (uint.MaxValue + 1m)));
        }
    }
}
