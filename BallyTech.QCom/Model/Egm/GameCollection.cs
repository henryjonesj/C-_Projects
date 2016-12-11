using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class GameCollection : SerializableKeyedCollection<int, Game>
    {
        protected override int GetKeyForItem(Game item)
        {
            return item.GameNumber;
        }


        internal void Update(Game game)
        {
            if(this.Contains(game.GameNumber))
			{
				this[game.GameNumber].Update(game);
		   		return;
			}
            
            this.Add(game);
        }

        internal Game Get(int gameVersionNumber)
        {
            return this.FirstOrDefault((item) => item.VersionNumber == gameVersionNumber);
        }

        internal int MaxGameCount { get; set; }

        internal bool IsMultiGame
        {
            get { return MaxGameCount > 1 || this.Count > 1; }
        }

        internal bool IsAvailable
        {
            get { return this.Count > 0; }
        }

    }
}
