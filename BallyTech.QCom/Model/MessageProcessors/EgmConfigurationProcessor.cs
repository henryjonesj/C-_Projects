using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.Egm;
using BallyTech.Utility.Serialization;
using log4net;
using BallyTech.QCom.Model.Builders;
using BallyTech.QCom.Model.Specifications;
using BallyTech.Gtm;
using BallyTech.QCom.Configuration;

namespace BallyTech.QCom.Model.MessageProcessors
{
    [GenerateICSerializable]
    public partial class EgmConfigurationProcessor : MessageProcessor
    {
        internal EgmAdapter Egm { get { return Model.Egm; } }
        private static readonly ILog _Log = LogManager.GetLogger(typeof(EgmConfigurationProcessor));
        private const ushort NonProgressiveGroupId = 0xFFFF;


        public EgmConfigurationProcessor() { }

        public EgmConfigurationProcessor(QComModel model) 
        {
            Model = model;
        }

        private static bool IsValidDenomination(decimal denomination)
        {
            return denomination > 0m;
        }

        public override void Process(EgmConfigurationResponse configurationResponse)
        {
            UpdateEgmConfiguration(configurationResponse);
        }

        public override void Process(EgmConfigurationResponseV16 configurationResponse)
        {   
            UpdateEgmConfiguration(configurationResponse);

            if (configurationResponse.IsSharedProgressivesEnabled)
                Egm.SharedProgressiveComponentEnabled();

            Model.ConfigurationRepository.CurrentEgmConfiguration.SetSupportedEgmFeatures(configurationResponse);
        }

        private void UpdateEgmConfiguration(EgmConfigurationResponse configurationResponse)
        {
            var creditDenomination = configurationResponse.CreditDenomination;
            var tokenDenomination = configurationResponse.TokenDenomination;

            if (!(IsValidDenomination(creditDenomination) || IsValidDenomination(tokenDenomination)))
            {                
                HandleInvalidDenomination();
                return;
            }

          
            Model.State.SetEgmDetails(Model.EgmDetails.AssetNumber, configurationResponse.ManufacturerId, configurationResponse.EgmSerialNumber);
            Egm.SetDenomination(creditDenomination, tokenDenomination);

            Egm.SetEgmGameProperties(configurationResponse.GameVersionNumber, 
                                     configurationResponse.GameVariationNumber, 
                                     configurationResponse.TotalNumberOfGamesAvailable);
        }

        private void HandleInvalidDenomination()
        {
            if (_Log.IsInfoEnabled) _Log.InfoFormat("Invalid denomination. Possibly Egm Ram Clear");

            Model.RamCleared();
        }

        public override void Process(EgmGameConfigurationV16 applicationMessage)
        {
            var game = UpdateGame(applicationMessage);
            
            foreach (var gameVariation in applicationMessage.GameVariations)
            {
                game.UpdateGameVariationsInfo(new GameVariationInfo(gameVariation.GameVariationNumber,
                                                                    gameVariation.PercentageReturn * QComCommon.MeterScaleFactor));
            }
        }

        public override void Process(EgmGameConfigurationV15 applicationMessage)
        {
            var game = UpdateGame(applicationMessage);
            
            foreach (var gameVariation in applicationMessage.GameVariations)
            {
                game.UpdateGameVariationsInfo(new GameVariationInfo(gameVariation.GameVariationNumber,
                                                                    gameVariation.PercentageReturn * QComCommon.MeterScaleFactor));
            }
        }

        private Game UpdateGame(EgmGameConfigurationResponse applicationMessage)
        {
            _Log.InfoFormat("Received Game Version Number {0} with enabled variation {1}", 
                             applicationMessage.GameVersionNumber, applicationMessage.CurrentGameVariationNumber);

            var game = new Game(applicationMessage.GameVersionNumber, applicationMessage.CurrentGameVariationNumber,
                               applicationMessage.ProgressiveGroupIdOfGVN.ToString())
            {
                Enabled = applicationMessage.IsGameEnabled
            };


            if (Egm.IsSharedProgressiveComponentEnabled)
                CreateSharedProgressiveLines(applicationMessage);
            else
                UpdateProgressiveLevelsForGame(applicationMessage, game);

            UpdateProgressiveLevelInfo(applicationMessage,game);
            Egm.UpdateGame(game);
            UpdateVariationHotSwitchingSupport(applicationMessage);

            return game;
        }

        private void CreateSharedProgressiveLines(EgmGameConfigurationResponse applicationMessage)
        {
            if (!IsLinkedProgressiveGroupId(applicationMessage.ProgressiveGroupIdOfGVN)) return;
            if (applicationMessage.NoOfProgressiveLevels == 0) return;

            var linkedProgressiveLevelsInfo = applicationMessage.GetLinkedProgressiveLevelInfo();

            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Egm configured for Shared Linked Progressive with {0} Levels",linkedProgressiveLevelsInfo.Count);

            Egm.CreateSharedProgressiveLines(linkedProgressiveLevelsInfo);
            Egm.LinkedProgressiveDevice.SetProgressiveLineCount(linkedProgressiveLevelsInfo.Count);
            Model.LPBroadcastScheduler = new LPBroadcastScheduler(Model);
        }

        private void UpdateProgressiveLevelsForGame(EgmGameConfigurationResponse applicationMessage, Game game)
        {
            if (IsLinkedProgressiveGroupId(applicationMessage.ProgressiveGroupIdOfGVN) && applicationMessage.NoOfProgressiveLevels > 0)
            {
                var linkedProgressiveLevelsInfo = applicationMessage.GetLinkedProgressiveLevelInfo();

                _Log.InfoFormat("Game configured for Linked Progressive with {0} Levels", linkedProgressiveLevelsInfo.Count);
                Egm.LinkedProgressiveDevice.UpdateProgressiveDevice(linkedProgressiveLevelsInfo, game);
            }
        }

        private static void UpdateProgressiveLevelInfo(EgmGameConfigurationResponse applicationMessage,Game game)
        {
            if (game == null)
            {
                if(_Log.IsWarnEnabled) _Log.Warn("Game not updated");
                return;
            }

            if (applicationMessage.NoOfProgressiveLevels == 0)
            {
                if (_Log.IsInfoEnabled) _Log.Warn("Game has no progressive levels");
                return;
            }

            applicationMessage.GetProgressiveLevelInfo().ForEach(game.UpdateProgressiveInfo);

        }

        private static bool IsLinkedProgressiveGroupId(ushort progressiveGroupId)
        {
            return (progressiveGroupId > 0 && progressiveGroupId < NonProgressiveGroupId);
        }

        private void UpdateVariationHotSwitchingSupport(EgmGameConfigurationResponse response)
        {
            var allGameConfigurations = Model.ConfigurationRepository.GetConfigurationsOfType<QComGameConfiguration>();
            if (allGameConfigurations == null) return;

            var gameConfiguration = allGameConfigurations.FirstOrDefault((element) => element.ConfigurationData.GameNumber == response.GameVersionNumber);
            if (gameConfiguration == null) return;

            gameConfiguration.CurrentGameVariation = response.CurrentGameVariationNumber;

            gameConfiguration.CurrentProgressiveGroupId = response.ProgressiveGroupIdOfGVN;

            gameConfiguration.HasSupportForVariationHotSwitching = response.IsVariationHotSwitchingEnabled;

            gameConfiguration.CurrentGameStatus = response.GameStatus;
        }

    }
}
