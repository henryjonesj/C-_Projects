using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.QCom.Configuration;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Model;
using BallyTech.QCom.Model.Egm;

namespace BallyTech.QCom.Messages
{
    partial class EgmConfigurationResponse : IQComEgmConfiguration
    {
        
        #region IEgmConfiguration Members

        public string SerialNumber
        {
            get { return EgmSerialNumber.ToString(); }
        }

        string IEgmConfiguration.ManufacturerId
        {
            get { return ManufacturerId.ToString(); }
        }

        public byte Jurisdiction { get; private set; }

        public virtual decimal CreditDenomination
        {
            get { return OldCreditDenomination; }
        }

        public virtual decimal TokenDenomination
        {
            get { return OldTokenDenomination; }
        }

        private decimal _MaxDenomination = decimal.MinusOne;
        public virtual decimal MaxDenomination { get { return _MaxDenomination; } }

        public virtual decimal MinTheoreticalPercent { get; private set; }

        public virtual decimal MaxTheoreticalPercent { get; private set; }

        public virtual decimal MaxStandardDeviation { get; private set; }

        public virtual int MaxLines { get; private set; }

        public virtual decimal MaxBet { get; private set; }

        public virtual decimal MaxProgressiveWinAmount { get; private set; }

        public virtual decimal MaxNonProgressiveWinAmount { get; private set; }

        public virtual decimal MaxElectronicCreditTransferLimit { get; private set; }

        public decimal LargeWinThresholdAmount { get; private set; }

        public decimal CreditInLockoutValue { get; private set; }

        #endregion

        #region IEgmConfiguration Members


        private SerializableList<IGameConfiguration> _GameConfigurations = new SerializableList<IGameConfiguration>();
        public IEnumerable<IGameConfiguration> GameConfigurations
        {
            get { return _GameConfigurations; }
        }

       

        #endregion

        #region IEgmConfiguration Members

        public IDenominationConfiguration DenominationConfiguration
        {
            get { return null; }
        }


        public decimal HopperLimit { get; private set; }

        public decimal MaxAutoPayLimit { get; private set; }

        public decimal HopperRefillAmount { get; private set; }

        public decimal MaxDoubleUpLimit { get; private set; }

        public uint PowerSaveTimeOut { get; private set; }

        public uint MaxDoubleUpAttempts { get; private set; }

        public decimal MaxCreditLimit { get; private set; }
        #endregion



        #region IEgmConfiguration Members


        public decimal NonProgressiveWinPayoutThreshold { get; private set; }

        public decimal ProgressiveWinPayoutThreshold { get; private set; }

        #endregion

        #region IEgmConfiguration Members


        public virtual bool IsDenominationHotSwitchingEnabled
        {
            get { return false; }
        }

        public virtual bool IsSharedProgressivesEnabled
        {
            get { return false; }
        }

        #endregion

        public uint TotalNumberOfGames
        {
            get { return TotalNumberOfGamesAvailable; }
        }
		
		#region IEgmConfiguration Members


        public IEGMFeatures EGMFeatures { get; private set; }

        public byte SystemID { get; private set; }

        public decimal EndOfDayTime { get; private set; }

        public byte PIDVersion { get; private set; }

        public decimal TimeZoneAdjustment { get; private set; }

        #endregion

        internal IEgmConfiguration Reconcile(QComModel model)
        {
            var repository = model.ConfigurationRepository;

            var egmConfiguration = repository.GetConfigurationOfType<QComEgmConfiguration>();
            if (egmConfiguration == null) return this;

            BuildGames(model.Egm);

            BackFill(egmConfiguration.ConfigurationData);

            return this;
        }

        private void BuildGames(EgmAdapter egmAdapter)
        {
            var games = egmAdapter.Games;

            if(games.IsAvailable)
            {
                games.ForEach(
                    (item) =>
                    _GameConfigurations.Add(new EgmGameConfigurationResponse() {GameVersionNumber = (ushort)item.GameNumber}));
            }

            for(var index = _GameConfigurations.Count ;index < TotalNumberOfGames; index ++)
            {
                _GameConfigurations.Add(new EgmGameConfigurationResponse());
            }

        }

        internal virtual void BackFill(IEgmConfiguration configuration)
        {

            this._MaxDenomination = configuration.MaxDenomination;
            this.MinTheoreticalPercent = configuration.MinTheoreticalPercent;
            this.MaxTheoreticalPercent = configuration.MaxTheoreticalPercent;
            this.MaxStandardDeviation = configuration.MaxStandardDeviation;
            this.MaxLines = configuration.MaxLines;
            this.MaxBet = configuration.MaxBet;
            this.MaxNonProgressiveWinAmount = configuration.MaxNonProgressiveWinAmount;
            this.MaxProgressiveWinAmount = configuration.MaxProgressiveWinAmount;
            this.MaxElectronicCreditTransferLimit = configuration.MaxElectronicCreditTransferLimit;

            UpdateParameters(configuration);

        }

        protected void UpdateParameters(IEgmConfiguration configuration)
        {
            this.Jurisdiction = configuration.Jurisdiction;
            this.HopperLimit = configuration.HopperLimit;
            this.MaxAutoPayLimit = configuration.MaxAutoPayLimit;
            this.HopperRefillAmount = configuration.HopperRefillAmount;

            this.LargeWinThresholdAmount = configuration.LargeWinThresholdAmount;
            this.CreditInLockoutValue = configuration.CreditInLockoutValue;
            this.MaxDoubleUpLimit = configuration.MaxDoubleUpLimit;
            this.MaxDoubleUpAttempts = configuration.MaxDoubleUpAttempts;

            this.PowerSaveTimeOut = configuration.PowerSaveTimeOut;
            this.NonProgressiveWinPayoutThreshold = configuration.NonProgressiveWinPayoutThreshold;
            this.ProgressiveWinPayoutThreshold = configuration.ProgressiveWinPayoutThreshold;

            this.EGMFeatures = configuration.EGMFeatures;
            this.SystemID = configuration.SystemID;
            this.EndOfDayTime = configuration.EndOfDayTime;
            this.PIDVersion = configuration.PIDVersion;
            this.TimeZoneAdjustment = configuration.TimeZoneAdjustment;

        }



    }
}
