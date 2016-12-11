using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Configuration
{
    [GenerateICSerializable]
    public partial class QComConfigurationId : IEquatable<QComConfigurationId>
    {
        internal FunctionCodes ConfigurationType { get; private set; }
        private int _GameNumber;

        public QComConfigurationId()
        {
            
        }

        public QComConfigurationId(FunctionCodes functionCodes, int gameNumber)
        {
            this.ConfigurationType = functionCodes;
            this._GameNumber = gameNumber;
        }


        public static QComConfigurationId CreateIdWith(FunctionCodes functionCodes,int gameNumber)
        {
            return new QComConfigurationId(functionCodes,gameNumber);
        }
        


        #region IEquatable<QComConfigurationKey> Members

        public bool Equals(QComConfigurationId other)
        {
            return (this.ConfigurationType == other.ConfigurationType && this._GameNumber == other._GameNumber);
        }

        #endregion


        public override string ToString()
        {
            return string.Format("{0}({1})", ConfigurationType, _GameNumber);
        }

    }
}
