using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.IO;

namespace BallyTech.QCom.Messages
{
    public class InvalidFunctionCodeException : InvalidFieldException
    {
        private readonly byte _Value;

        public InvalidFunctionCodeException()
        {
            
        }

        public InvalidFunctionCodeException(byte value)
        {
            this._Value = value;
        }

        public byte Value
        {
            get { return _Value; }
        }

    }
}
