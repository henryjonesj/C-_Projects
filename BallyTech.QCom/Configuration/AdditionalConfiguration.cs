using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Configuration
{
    [GenerateICSerializable]
    public partial class AdditionalConfiguration
    {
        public bool IsCreditModeSupported { get; set; }
        public bool IsPgidHotSwitchingSupported { get; set; }

        public string VariationHotSwitchText { get; set; }
        public string PgidHotSwitchText { get; set; }
        public string GameStatusHotSwitchText { get; set; }
        public string DenominationHotSwitchSuccessText { get; set; }
        public string DenominationHotSwitchConfiguringText { get; set; }
        public string CombinedHotSwitchText { get; set; }


        public TimeSpan GameVariationLockUpTimer { get; set; }
        public TimeSpan IdleModeWaitTimer { get; set; }
        public TimeSpan DenominationHotSwitchSucessDisplayTimer { get; set; }
        public TimeSpan DenominationHotSwitchConfiguringDisplayTimer { get; set; }
        

        public AdditionalConfiguration()
        {
            VariationHotSwitchText = "QCOM: GAME VARIATION CHANGING SOON";

            PgidHotSwitchText = "QCOM: PGID CHANGING SOON";

            CombinedHotSwitchText = "QCOM: GAME CONFIGURATION CHANGING SOON";

            DenominationHotSwitchSuccessText = "QCOM: DENOMINATION CHANGED TO $";

            DenominationHotSwitchConfiguringText = "QCOM: DENOMINATION CHANGING SOON";

            GameStatusHotSwitchText = "QCOM: GAME STATUS CHANGING SOON";

            GameVariationLockUpTimer = TimeSpan.FromSeconds(20);

            IdleModeWaitTimer = TimeSpan.FromSeconds(60);

            DenominationHotSwitchSucessDisplayTimer = TimeSpan.FromSeconds(60);

            DenominationHotSwitchConfiguringDisplayTimer = TimeSpan.FromSeconds(20);

            IsPgidHotSwitchingSupported = true;
        
        }
        
       
    }
}
