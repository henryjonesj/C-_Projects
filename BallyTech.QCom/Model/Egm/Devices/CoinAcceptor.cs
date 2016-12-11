using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.Utility;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class CoinAcceptor : Device,ICoinAcceptor
    {
        private SerializableDictionary<EgmEvent, BoundSlot<bool>> _EventMap =
            new SerializableDictionary<EgmEvent, BoundSlot<bool>>();

        public CoinAcceptor()
        {
            Initialize();
            _EventMap.Add(EgmEvent.CoinInTilt, new BoundSlot<bool>() { Value = IsFaultCondition });
            _EventMap.Add(EgmEvent.DiverterMalfunction, IsDiverterMalfunction);            
        }


        private void Initialize()
        {            
            IsDiverterMalfunction =new BoundSlot<bool>();
        }


        #region ICoinAcceptor Members

        public bool IsEnabled { get; private set; }

        public bool IsFaultCondition { get; private set; }

        public BoundSlot<bool> IsDiverterMalfunction { get; private set; }
        
        public void SetState(bool enableState)
        {
            
        }

        #endregion



        public void Process(EgmEvent egmEvent)
        {
            Model.Observers.EgmEventRaised(egmEvent);

            if (!(_EventMap.ContainsKey(egmEvent))) return;

            var eventInfo = _EventMap[egmEvent];
            eventInfo.Value = true;
        }
    }
}
