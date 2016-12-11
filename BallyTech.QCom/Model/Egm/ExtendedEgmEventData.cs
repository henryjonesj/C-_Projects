using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class ExtendedEgmEventData : IExtendedEgmEventData
    {
        public ExtendedEgmEventData()
        {
            IsEventRaisedByEgm = false;
            SequenceNumber = EventCode = EctPollSequenceNumber = PurgePollSequenceNumber = uint.MinValue;
            EventSize = ushort.MinValue;
            DateTime = DateTime.MinValue;
            GameNumber = ProgGroupId = ushort.MinValue;
            PaytableId = GameSerialNumber = ManufacturerId = string.Empty;
            LevelId = byte.MinValue;
            Amount = TicketBarcode = decimal.Zero;
            TicketSerialNumber = RTP = uint.MinValue;
            MeterId = byte.MinValue;
            MeterIncrement = decimal.Zero;
            ExpectedSequenceNumber = uint.MinValue;
            ErrorText = string.Empty;
            LicenseKey = decimal.Zero;
            SystemLockupUserResponse = byte.MinValue;
            Denomination = decimal.Zero;
            InvalidDateTime = null;
            Seed = null;
            Signature = null;
            GenericDataBuffer = null;
        }

        #region IExtendedEgmEventData Members

        public bool IsEventRaisedByEgm { get; set; }

        public uint SequenceNumber { get; set; }

        public uint EventCode { get; set; }

        public ushort EventSize { get; set; }

        public DateTime DateTime { get; set; }

        public ushort GameNumber { get; set; }

        public string PaytableId { get; set; }

        public ushort ProgGroupId { get; set; }

        public byte LevelId { get; set; }

        public decimal Amount { get; set; }

        public string GameSerialNumber { get; set; }

        public string ManufacturerId { get; set; }

        public uint TicketSerialNumber { get; set; }

        public uint RTP { get; set; }

        public decimal TicketBarcode { get; set; }

        public ushort MeterId { get; set; }

        public decimal MeterIncrement { get; set; }

        public uint ExpectedSequenceNumber { get; set; }

        public string ErrorText { get; set; }

        public decimal LicenseKey { get; set; }

        public byte SystemLockupUserResponse { get; set; }

        public decimal Denomination { get; set; }

        public IInvalidDateTime InvalidDateTime { get; set; }

        public uint EctPollSequenceNumber { get; set; }

        public uint PurgePollSequenceNumber { get; set; }

        public byte[] Seed { get; set; }

        public byte[] Signature { get; set; }

        public byte[] GenericDataBuffer { get; set; }

        #endregion

    }

    [GenerateICSerializable]
    public partial class InvalidDateTime : IInvalidDateTime
    {
        public decimal UnRepresentableDate { get; set; }
        public decimal UnRepresentableTime { get; set; }
    
    }
}
