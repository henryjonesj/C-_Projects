using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Reflection;
using BallyTech.QCom.Messages;
using BallyTech.Utility.Management;
using BallyTech.Utility.IO;

namespace BallyTech.QCom.Metadata
{
    public partial class QComMetadata
    {
        private static readonly QComMetadata _Instance;
        private static readonly string _StreamHash;

        static QComMetadata()
        {
            XmlSerializer ser = new XmlSerializer(typeof(QComMetadata));
            using (Stream s = GetMetadataStream())
			{
                _StreamHash = FileUtils.HashStream(s);
                s.Position = 0;
                using (XmlReader r = XmlReader.Create(s))
                {
                    _Instance = (QComMetadata)ser.Deserialize(r);
                    if (_Instance != null) _Instance.Initialize();
                }
            }
        }

        public static string StreamHash
        {
            get { return _StreamHash; }
        }

        public static Stream GetMetadataStream()
        {
            const string metadataFileName = "QComMetadata.xml";
            FileInfo fileinfo = PathManager.GetFileInfo("AppsConfigurationFolder", metadataFileName);

            if (fileinfo.Exists)
                return fileinfo.OpenRead();

            return Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(QComMetadata),
                                                                             metadataFileName);
        }

        [XmlIgnore]
        public static QComMetadata Instance
        {
            get
            {
                return _Instance;
            }
        }
        public void Initialize()
        {
            MeterDefinitions.Initialize();
        }
    }

    partial class MeterDefinitions
    {
        private Dictionary<MeterCodes, MeterDefinition> _CodeMap;

        [XmlIgnore]
        public Dictionary<MeterCodes, MeterDefinition> CodeMap
        {
            get { return _CodeMap; }
        }

        public int GetIncrementThresholdCabinet(MeterCodes code)
        {
            return this.CodeMap[code].IncrementThresholdCabinet;
        }

        public int GetIncrementThresholdGame(MeterCodes code)
        {
            return this.CodeMap[code].IncrementThresholdGame;
        }

        public string GetUnit(MeterCodes code)
        {
            return this.CodeMap[code].Unit;
        }

        internal void Initialize()
        {
            _CodeMap = new Dictionary<MeterCodes, MeterDefinition>();
            foreach (MeterDefinition def in MeterDefinition)
            {
                def.Initialize();
                _CodeMap.Add(def.MeterCode, def);
            }
        }
    }

    partial class MeterDefinition
    {
        private MeterCodes _Code;

        [XmlIgnore]
        public MeterCodes MeterCode
        {
            get { return _Code; }
        }

        internal void Initialize()
        {
            _Code = (MeterCodes)Enum.Parse(typeof(MeterCodes), Name, true);
        }
    }
}
