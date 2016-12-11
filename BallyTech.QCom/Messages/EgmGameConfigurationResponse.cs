using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using System.Collections;
using System.IO;
using log4net;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Model;

namespace BallyTech.QCom.Messages
{
    public partial class EgmGameConfigurationResponse
    {
        private const byte MinNoOfVariations = 0;
        private const byte MaxNoOfVariationsForV15 = 8;
        private const byte MaxNoOfVariationsForV16 = 16;
        internal const int LengthofNonRepeatedEntries = 14;
        private static readonly ILog _Log = LogManager.GetLogger(typeof(EgmGameConfigurationResponse));

        private SerializableDictionary<int, bool> _ProgressiveLevelMap = new SerializableDictionary<int, bool>();

        public bool IsVariationHotSwitchingEnabled
        {
            get
            {
                return (EgmGameConfigurationFlag.VariationHotSwitchingCapabilityFlag == (_EgmGameConfigurationFlag & 
                                                            EgmGameConfigurationFlag.VariationHotSwitchingCapabilityFlag));
            }
        }
        
        
        
        public bool IsGameEnabled
        {
            get
            {
                return (EgmGameConfigurationFlag.GameEnableFlag == (_EgmGameConfigurationFlag &
                                                                    EgmGameConfigurationFlag.GameEnableFlag));
            }
        }

        public bool IsLinkedProgressiveEnabled
        {
            get
            {
                return (EgmGameConfigurationFlag.LinkedProgressiveFlag == (_EgmGameConfigurationFlag &
                                                                    EgmGameConfigurationFlag.LinkedProgressiveFlag));
            }
        }

        public bool IsNumberOfVariationAvailableValid
        {
            get
            {
                if (this.ProtocolVersion == ProtocolVersion.Unknown) return true;
                return (this.NoOfVariationAvailable > MinNoOfVariations && this.NoOfVariationAvailable <= GetMaxNoOfVariations());
            }
        }

        private byte GetMaxNoOfVariations()
        {
            return (this.ProtocolVersion == ProtocolVersion.V15) ? MaxNoOfVariationsForV15 : MaxNoOfVariationsForV16;
        }

        internal GameProgressiveType GetProgressiveTypeOfLevel(int levelNumber)
        {
            if (this.NoOfProgressiveLevels  == 0) return GameProgressiveType.None;
      
            return _ProgressiveLevelMap[levelNumber]
                       ? GameProgressiveType.LinkedProgressive
                       : GameProgressiveType.StandAloneProgressive;

        }

        internal int GetNumberOfLinkedProgressiveLevels()
        {
            if (this.NoOfProgressiveLevels == 0) return 0;

            var progressiveLevels = _ProgressiveLevelMap.Take(this.NoOfProgressiveLevels);
            return progressiveLevels.Count((item) => item.Value);
        }

        internal bool HasSapLevels()
        {
            if(this.NoOfProgressiveLevels == 0) return false;

            var progressiveLevels = _ProgressiveLevelMap.Take(this.NoOfProgressiveLevels);
            return progressiveLevels.Any((item) => !item.Value);
        }


        private void UpdateProgressiveLevelMap(BinaryReader reader)
        {
            _ProgressiveLevelMap.Add(0, this.Level0LinkedProgressiveType);
            _ProgressiveLevelMap.Add(1, this.Level1LinkedProgressiveType);
            _ProgressiveLevelMap.Add(2, this.Level2LinkedProgressiveType);
            _ProgressiveLevelMap.Add(3, this.Level3LinkedProgressiveType);
            _ProgressiveLevelMap.Add(4, this.Level4LinkedProgressiveType);
            _ProgressiveLevelMap.Add(5, this.Level5LinkedProgressiveType); 
            _ProgressiveLevelMap.Add(6, this.Level6LinkedProgressiveType); 
            _ProgressiveLevelMap.Add(7, this.Level7LinkedProgressiveType);             
        }


        internal SerializableList<ProgressiveLevelInfo> GetProgressiveLevelInfo()
        {
            var progressiveLevelInfoList = new SerializableList<ProgressiveLevelInfo>();

            for (byte qcomProgressiveLevel = 0; qcomProgressiveLevel < this.NoOfProgressiveLevels; qcomProgressiveLevel++)
            {
                var progressiveType = this.GetProgressiveTypeOfLevel(qcomProgressiveLevel);
                var progressiveLevel = (byte)(qcomProgressiveLevel + 1);

                progressiveLevelInfoList.Add(new ProgressiveLevelInfo(progressiveLevel,progressiveType));
            }

            return progressiveLevelInfoList;
        }

        internal SerializableList<ProgressiveLevelInfo> GetLinkedProgressiveLevelInfo()
        {
            return
                GetProgressiveLevelInfo().FindAll(
                    (item) => item.ProgressiveType == GameProgressiveType.LinkedProgressive).ToSerializableList();
        }

    }
}
