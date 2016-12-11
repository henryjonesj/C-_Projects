using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Model.Egm;
using BallyTech.Gtm;

namespace BallyTech.QCom.Configuration
{
    [GenerateICSerializable]
    public partial class CurrentEgmConfiguration
    {
        public bool HasSupportForDenominationSwitching = false;

        public bool IsSharedProgressiveComponentSupported { get; private set; }

        private EgmAdapter _EgmAdapter = null;


        public void Initialize(EgmAdapter egmAdapter)
        {
            _EgmAdapter = egmAdapter;
        }
        

        public void SetSupportedEgmFeatures(EgmConfigurationResponseV16 response)
        {
            HasSupportForDenominationSwitching = response.IsDenominationHotSwitchingEnabled;

            IsSharedProgressiveComponentSupported = response.IsSharedProgressivesEnabled;

        }

        internal bool IsValidGame(int gameNumber)
        {
            if (!_EgmAdapter.IsEgmInitialized) return true;

            return _EgmAdapter.Games.Contains(gameNumber);
        }




    }
}
