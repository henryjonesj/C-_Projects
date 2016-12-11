using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using BallyTech.Utility;
using log4net;
using System.Diagnostics.CodeAnalysis;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class Game : IEgmGame
    {
        private readonly static ILog _Log = LogManager.GetLogger(typeof(Game));

        private ICollection<IProgressiveLine> _LinkedProgressiveLines;
        public ICollection<IProgressiveLine> LinkedProgressiveLines
        {
            get { return _LinkedProgressiveLines; }
        }

        private Meter _LinkedProgressiveContributionAmount = Meter.NotAvailable;
        public Meter LinkedProgressiveContributionAmount 
        {
            get  { return _LinkedProgressiveContributionAmount;  }
            set  { _LinkedProgressiveContributionAmount = value; }
        }

        private decimal _CurrentGameDenomination = 0.01m;
        public decimal CurrentGameDenomination
        {
            get { return _CurrentGameDenomination; }
            set { _CurrentGameDenomination = value; }
        }

        public int GameNumber
        {
            get { return VersionNumber; }
        }

        public int VersionNumber { get; set; }

        private byte _CurrentGameVariation;
        public byte CurrentGameVariation
        {
            get { return _CurrentGameVariation; }
        }

        internal ProgressiveInfoCollection ProgressiveLevelInfoCollection { get; private set; }


        private GameVariationInfoCollection GameVariations = new GameVariationInfoCollection();
		
        public Game()
        {
        }

        public Game(int gameVersion,byte gameVariation,string progressiveId)
        {
            this.VersionNumber = gameVersion;
            this.ProgressiveGroupId = progressiveId;
            this._CurrentGameVariation = gameVariation;
            ProgressiveLevelInfoCollection = new ProgressiveInfoCollection();
        }

        public void Update(Game game)
        {
            VersionNumber = game.VersionNumber;
            _CurrentGameVariation = game._CurrentGameVariation;
            ProgressiveGroupId = game.ProgressiveGroupId;
            _LinkedProgressiveLines = game._LinkedProgressiveLines;
            GameVariations = game.GameVariations;
            Enabled = game.Enabled;

            if (this.ProgressiveLevelInfoCollection.Count() == 0)
                this.ProgressiveLevelInfoCollection = game.ProgressiveLevelInfoCollection;
        }

        public bool IsLinkedProgressiveGame()
        {
            return (ushort.Parse(this.ProgressiveGroupId) >= 0x0001 && ushort.Parse(this.ProgressiveGroupId) <= 0xFFFE);
        }

        internal bool IsProgressiveGame()
        {
            return ProgressiveLevelInfoCollection.Count() > 0;
        }

        [SuppressMessage("BallyTech.FxCop.Repeatability", "BR0002", Justification = "Instance creation not required. Hence making it static")]
		 internal static Game GetDefaultWithVersion(string protocolVersion)
        {
            return new Game(0, 0, string.Empty)
                    {
                        GameVersion = protocolVersion
                    } ;
        }

        #region IEgmGame Members

        public string Name
        {
            get { return GameNumber > 0 ? GameNumber.ToString() : string.Empty; }
        }

        public bool Enabled { get; internal set; }

        public decimal BasePayback
        {
            get { return CurrentGameVariationInfo != null ? CurrentGameVariationInfo.TheorticalPercentage * MeterService.MeterScaleFactor : 0m; }
        }

        public int MaximumWager
        {
            get { return 0; }
        }

        public string PaytableId
        {
            get { return _CurrentGameVariation > 0 ? _CurrentGameVariation.ToString() : string.Empty; }
        }

        public IEnumerable<decimal> Denominations
        {
            get { return new SerializableList<decimal>() { { CurrentGameDenomination }}; }
        }

        public string GameVersion  { get; set; }        

        public string ProgressiveGroupId { get; private set; }

        public string PaytableName
        {
            get { return string.Empty; }
        }

        public IEnumerable<IProgressiveLine> ProgressiveLines
        {
            get 
            {
                return ProgressiveLevelInfoCollection.Values.Cast<IProgressiveLine>(); 
            }
        }

        #endregion

        public Meter GetMeter(MeterId meterId)
        {
            if(GameVariations.Count <= 0)
                return Meter.NotAvailable;

            var meterValue = Meter.Zero;
            foreach(GameVariationInfo variationInfo in GameVariations)
            {
                if (variationInfo.Meters.Keys.Contains(meterId))
                    meterValue += variationInfo.Meters[meterId];
            }

            return meterValue;
        }

        public SerializableDictionary<MeterId, Meter> GetMeters(byte variation)
        {
            return GameVariations[variation].AreMetersAvailable ? GameVariations[variation].Meters : null;
        }

        public void UpdateMeter(MeterId meterId, Meter meter, byte variation)
        {
            GameVariations[variation].UpdateMeter(meterId, meter);
        }

        public void ResetMeters(byte variation)
        {
            GameVariations[variation].ResetMeters();
        }

        public void InitializeLinkedProgressiveLevels(SerializableList<ProgressiveLevelInfo> progressiveLevelInfo, Action<JackpotPaymentType> sendLpAck)
        {
            _LinkedProgressiveLines = new SerializableList<IProgressiveLine>();

            foreach (var progressiveLine in
                progressiveLevelInfo.Select(levelInfo => new LinkedProgressiveLine() {LineId = levelInfo.LineId}))
            {
                progressiveLine.SendLPAcknowledgement += sendLpAck;
                _LinkedProgressiveLines.Add(progressiveLine);
            }
        }

        public void UpdateGameMeters(SerializableDictionary<MeterId, Meter> meters, byte gameVariationNumber)
        {
            GameVariations[gameVariationNumber].UpdateMeters(meters);
        }

        private GameVariationInfo CurrentGameVariationInfo
        {
            get
            {
                GameVariationInfo gameVariationInfo = null;
                GameVariations.TryGetValue(_CurrentGameVariation, out gameVariationInfo);
                return gameVariationInfo;
            }
        }

        internal bool IsGameVariationAvailable(byte variationNumber)
        {
            return GameVariations.Any((gameVariation) => gameVariation.VariationNumber == variationNumber);
        }

        public void UpdateGameVariationsInfo(GameVariationInfo variationInfo)
        {
            if (IsGameVariationAvailable(variationInfo.VariationNumber)) return;

            GameVariations.Add(variationInfo);
        }

        internal void UpdateProgressiveInfo(ProgressiveLevelInfo progressiveLevelInfo)
        {
            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Updating game with level {0} and type {1}", progressiveLevelInfo.LineId,
                                progressiveLevelInfo.ProgressiveType);

            this.ProgressiveLevelInfoCollection.Update(progressiveLevelInfo);
        }

        internal void UpdateCurrentProgressiveLevelAmount(ProgressiveLevelInfo levelContributionAmount)
        {            
            this.ProgressiveLevelInfoCollection.UpdateContributionAmount(levelContributionAmount);
        }
    }
}
