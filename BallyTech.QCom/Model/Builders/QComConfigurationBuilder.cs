using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.Gtm;
using BallyTech.QCom.Configuration;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model.Builders
{
    public static class QComConfigurationBuilder
    {

        internal static EgmConfigurationPoll Build(QComEgmConfiguration egmConfiguration)
        {
            var configurationData = egmConfiguration.ConfigurationData;

            return new EgmConfigurationPoll()
                       {
                           SerialNumber = Decimal.Parse(configurationData.SerialNumber),
                           ManufacturerId = configurationData.ManufacturerId.GetByteOrDefault(),
                           OldCreditDenomination = configurationData.CreditDenomination,
                           CreditDenomination = configurationData.CreditDenomination,
                           OldTokenDenomination = configurationData.TokenDenomination,
                           TokenDenomination = configurationData.TokenDenomination,
                           Jurisdiction =
                               (JurisdictionCharacteristics)
                               Enum.Parse(typeof (JurisdictionCharacteristics),
                                          configurationData.Jurisdiction.ToString(), true),

                           MaxDenomination = configurationData.MaxDenomination,
                           MaxBet = configurationData.MaxBet,
                           MaxECT = configurationData.MaxElectronicCreditTransferLimit,
                           MaxLines = configurationData.MaxLines,
                           MAXSD = configurationData.MaxStandardDeviation,
                           MaxRTP = configurationData.MaxTheoreticalPercent,
                           MinRTP = configurationData.MinTheoreticalPercent,
                           MaxNonProgressiveWin = configurationData.MaxNonProgressiveWinAmount,
                           MaxProgressiveWin = configurationData.MaxProgressiveWinAmount,                            
                       };
        }

        internal static EgmGameConfigurationPoll Build(QComGameConfiguration gameConfiguration)
        {
            var configurationData = gameConfiguration.ConfigurationData;

            var gameConfigurationPoll = new EgmGameConfigurationPoll()
                       {
                           GameVersionNumber = Convert.ToUInt16(configurationData.GameNumber),
                           GameVariationNumber = configurationData.PayTableId.GetByteOrDefault(),
                           GameConfigurationFlag =
                               configurationData.GameStatus
                                   ? GameConfigurationFlagCharacteristics.GameEnableFlag
                                   : GameConfigurationFlagCharacteristics.None
                       };


            if (configurationData.ProgressiveConfiguration == null) return gameConfigurationPoll;

            gameConfigurationPoll.UpdateProgressiveInformation(configurationData);
            return gameConfigurationPoll;
        }


        internal static EgmGameConfigurationChangePoll BuildUpdatedConfiguration(QComGameConfiguration configuration)
        {
            var configurationData = configuration.ConfigurationData;
            ;

            var gameChangeConfiguration = new EgmGameConfigurationChangePoll()
                       {
                           GameVersionNumber = Convert.ToUInt16(configurationData.GameNumber),
                           GameVariationNumber = configurationData.PayTableId.GetByteOrDefault(),
                           GameFlag =
                               configurationData.GameStatus
                                   ? GameFlagCharacteristics.GameEnableFlag
                                   : GameFlagCharacteristics.Reserved                           

                       };


            var progressiveConfiguration = configurationData.ProgressiveConfiguration;

            gameChangeConfiguration.LinkedProgressiveGroupId = progressiveConfiguration == null ? (ushort)0xFFFF : Convert.ToUInt16(progressiveConfiguration.ProgressiveGroupId);

            return gameChangeConfiguration;
        }

        private static void UpdateProgressiveInformation(this EgmGameConfigurationPoll gameConfigurationPoll, IGameConfiguration configurationData)
        {
            var progressiveData = configurationData.ProgressiveConfiguration;

            if (progressiveData.ProgressiveGroupId > 0)
                gameConfigurationPoll.LinkedProgressiveGroupId = Convert.ToUInt16(progressiveData.ProgressiveGroupId);

            if (progressiveData.ProgressiveLevelConfigurations == null) return;

            var progressiveConfigurations = progressiveData.ProgressiveLevelConfigurations.ToSerializableList();
            
            gameConfigurationPoll.NoOfProgressiveLevels = Convert.ToByte(progressiveConfigurations.Count);

            var progressiveConfig = progressiveConfigurations.OrderBy(item => item.ProgressiveLevelNumber);

            progressiveConfig.ForEach((item) =>
                gameConfigurationPoll.ProgressiveLevelsInformation.Add(
                new ProgressiveLevelData()
                {
                    ContributionAmount = item.ContributionAmount,
                    ProgressiveFlag = item.ProgressiveType == GameProgressiveType.LinkedProgressive ?
                    ProgressiveFlagCharacteristics.LinkedProgressive :
                    ProgressiveFlagCharacteristics.StandAloneProgressive
                }));
        }



        internal static EgmParametersPoll Build(QComParameterConfiguration parameterConfiguration)
        {
            var configurationData = parameterConfiguration.ConfigurationData;

            EgmParametersPoll parametersPoll =  new EgmParametersPoll()
                                       {
                                           LargeWinLockUpThreshold = configurationData.LargeWinThresholdAmount,
                                           NpWinPayoutThreshold = configurationData.NonProgressiveWinPayoutThreshold,
                                           SapWinPayoutThreshold = configurationData.ProgressiveWinPayoutThreshold,
                                           CreditInLockOut = configurationData.CreditInLockoutValue,
                                           PowerSaveTimeOut = configurationData.PowerSaveTimeOut,
                                           DoubleUpMaxLimit= (byte)configurationData.MaxDoubleUpAttempts,
                                           DoubleUpLimit = configurationData.MaxDoubleUpLimit,
                                           OperatorId = configurationData.SystemID,
                                           TimeZoneAdjustment = configurationData.TimeZoneAdjustment,
                                           PlayerInfoDisplayId = configurationData.PIDVersion,
                                           EndDayTime = QComConvert.GetEndOfDayTimeMinutes(configurationData.EndOfDayTime)
                                       };
            
            if(configurationData.EGMFeatures.IsReservedFeatureEnabled)
                parametersPoll.ParameterFlag |= ParameterFlagCharacteristics.ReserveFeatureEnable;
            if (configurationData.EGMFeatures.IsAutoplayFeatureEnabled)
                parametersPoll.ParameterFlag |= ParameterFlagCharacteristics.Autoplay;
            if (configurationData.EGMFeatures.IsCreditInLockoutFeatureEnabled)
                parametersPoll.ParameterFlag |= ParameterFlagCharacteristics.CRLIMITmode;

            return parametersPoll;
        }

        internal static NoteAcceptorFlagCharacteristics UpdateNoteAcceptorFlagCharacteristics(IEnumerable<BillDenomination> SupportedDenominations)
        {
            NoteAcceptorFlagCharacteristics BillDenominations = NoteAcceptorFlagCharacteristics.None;

            if (SupportedDenominations.Contains(BillDenomination.Bill5))
                BillDenominations |= NoteAcceptorFlagCharacteristics.Five;

            if (SupportedDenominations.Contains(BillDenomination.Bill10))
                BillDenominations |= NoteAcceptorFlagCharacteristics.Ten;

            if (SupportedDenominations.Contains(BillDenomination.Bill20))
                BillDenominations |= NoteAcceptorFlagCharacteristics.Twenty;

            if (SupportedDenominations.Contains(BillDenomination.Bill50))
                BillDenominations |= NoteAcceptorFlagCharacteristics.Fifty;

            if (SupportedDenominations.Contains(BillDenomination.Bill100))
                BillDenominations |= NoteAcceptorFlagCharacteristics.Hundred;
		
		 return BillDenominations;
		}

        internal static ProgressiveConfigurationPoll Build(QComProgressiveConfiguration progressiveConfiguration)
        {
            var configurationData = progressiveConfiguration.ConfigurationData;

            var progressiveConfigurations = new SerializableList<ProgressiveConfiguration>();
            progressiveConfigurations.AddRange(
                configurationData.ProgressiveLevelConfigurations.Select(
                    progressiveLevelConfiguration => CreateProgressiveConfiguration(progressiveLevelConfiguration)));

            progressiveConfigurations = progressiveConfigurations.OrderBy(item => item.ProgressiveLevelId).ToSerializableList();

            return new ProgressiveConfigurationPoll()
                       {
                           GameVersionNumber = (ushort) configurationData.GameNumber,
                           NoOfReatedEntries = (byte) progressiveConfigurations.Count,
                           ProgressiveConfigurations = progressiveConfigurations
                       };

        }


        private static ProgressiveConfiguration CreateProgressiveConfiguration(IProgressiveLevelConfiguration progressiveLevelConfiguration)
        {
            var progressiveLevelId = (byte)(progressiveLevelConfiguration.ProgressiveLevelNumber - 1);

           
            return new ProgressiveConfiguration()
                       {
                           StartupAmount = progressiveLevelConfiguration.StartupAmount,
                           AuxRtp = progressiveLevelConfiguration.AuxPayback,
                           CeilingAmount = progressiveLevelConfiguration.CeilingAmount,
                           JackpotLevelPercent = progressiveLevelConfiguration.Increment,
                           ProgressiveLevelId = progressiveLevelId
                       };
        }


        internal static NoteAcceptorMaintenancePoll Build(IDenominationConfiguration DenominationConfiguration)
        {

            var noteAcceptorDenominations = UpdateNoteAcceptorFlagCharacteristics(DenominationConfiguration.SupportedDenominations);
           

            return new NoteAcceptorMaintenancePoll()
                    {
                        NoteAcceptorMsbFlag=noteAcceptorDenominations
                    };
        }


    }
}
