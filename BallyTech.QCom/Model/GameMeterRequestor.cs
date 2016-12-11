using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Model.Builders;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.MessageProcessors;
using log4net;
using BallyTech.Gtm;
using BallyTech.QCom.Model.Egm;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class GameMeterRequestor
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(GameMeterRequestor));

        public QComModel Model { get; set; }

        private SerializableDictionary<EgmGeneralMaintenancePoll, FunctionCodes> _EgmGeneralMaintenancePolls = new SerializableDictionary<EgmGeneralMaintenancePoll, FunctionCodes>();

        public void FetchForGameLevelMeters()
        {
            _EgmGeneralMaintenancePolls.Clear();
            Model.Egm.Games.ForEach(game => AddProgressiveMeterRequestToQueue(game));

            if (Model.ShouldQueryForGameLevelMeters)
                Model.Egm.Games.ForEach(game => _EgmGeneralMaintenancePolls.Add(MeterRequestBuilder.RequestForMultiGameMeters(Model.Egm.CabinetDevice.IsMachineEnabled, game), FunctionCodes.MultiGameVariationMetersResponse));

            SendNextPoll();
        }

        private void AddProgressiveMeterRequestToQueue(Game game)
        {
            if (!game.IsProgressiveGame()) return;

            _EgmGeneralMaintenancePolls.Add(MeterRequestBuilder.RequestForProgressiveMeters(Model.Egm.CabinetDevice.IsMachineEnabled, game), FunctionCodes.ProgressiveMetersResponse);
        }

        private void SendNextPoll()
        {
            if (_EgmGeneralMaintenancePolls.Count == 0) return;

            _Log.DebugFormat("Sending request for {0} for game version = {1}", _EgmGeneralMaintenancePolls.FirstOrDefault().Value,
                                                                                _EgmGeneralMaintenancePolls.FirstOrDefault().Key.GameVersionNumber);

            Model.SendPoll(_EgmGeneralMaintenancePolls.FirstOrDefault().Key);            
        }

        private void RemoveOldPoll()
        {
            _EgmGeneralMaintenancePolls.Remove(_EgmGeneralMaintenancePolls.FirstOrDefault().Key);
        }

        public void ProgressiveMetersResponseReceived(ushort gameVersion)
        {
            CheckAndQueueNextPoll(FunctionCodes.ProgressiveMetersResponse, gameVersion);
        }

        public void MultiGameMeterResponseReceived(ushort gameVersion)
        {
            CheckAndQueueNextPoll(FunctionCodes.MultiGameVariationMetersResponse, gameVersion);
        }

        private void CheckAndQueueNextPoll(FunctionCodes functionCode, ushort gameVersion)
        {
            if (_EgmGeneralMaintenancePolls.Count == 0) return;

            if (_EgmGeneralMaintenancePolls.FirstOrDefault().Value != functionCode) return;

            if (_EgmGeneralMaintenancePolls.FirstOrDefault().Key.GameVersionNumber != gameVersion)
                _Log.DebugFormat("Received {0} for a different game version.", functionCode);

            RemoveOldPoll();
            SendNextPoll();
        }

        public void LinkedStatusChanged(LinkStatus linkStatus)
        {
            if (linkStatus == LinkStatus.Disconnected)
                _EgmGeneralMaintenancePolls.Clear();
        }
    }
}
