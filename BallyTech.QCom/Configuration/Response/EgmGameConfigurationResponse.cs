using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.QCom.Configuration.Response;

namespace BallyTech.QCom.Messages
{
    partial class EgmGameConfigurationResponse : IGameConfiguration
    {
        private EgmGameProgressiveConfiguration _GameProgressiveConfiguration = null;

        #region IGameConfiguration Members

        public int GameNumber
        {
            get { return this.GameVersionNumber; }
        }

        public string PayTableId
        {
            get { return this.CurrentGameVariationNumber.ToString(); }
        }

        public bool GameStatus
        {
            get { return this.IsGameEnabled; }
        }

        public virtual IProgressiveConfiguration ProgressiveConfiguration
        {
            get { return _GameProgressiveConfiguration ?? new EgmGameProgressiveConfiguration(this); }
        }

        #endregion
    }
}
