using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Utility;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class GamePlayTracker
    {
        private EgmModel _Model = null;

        private Meter _GamePlays = null;
        
        public GamePlayTracker()
        {
            
        }

        public GamePlayTracker(EgmModel model)
        {
            _Model = model;
        }

        private static bool IsMeterIncremented(Meter oldMeter,Meter newMeter)
        {
            return ((newMeter - oldMeter).DangerousGetSignedValue() > 0);
        }


        internal void Track(SerializableList<MeterId> meterIds)
        {
            if (!(meterIds.Contains(MeterId.Plays))) return;

            var gamesPlayedMeter = _Model.GetMeters().GetGamesPlayed(null, null, null, null);

            if (_GamePlays == null || !IsMeterIncremented(_GamePlays, gamesPlayedMeter))
            {
                _GamePlays = gamesPlayedMeter;
                return; 
            }

            _GamePlays = gamesPlayedMeter;

            _Model.Observers.GameStarted();
            _Model.Observers.GameEnded();
            _Model.EgmAdapter.GameEnded();
        }


        internal void Reset()
        {
            _GamePlays = null;
        }
    }
}
