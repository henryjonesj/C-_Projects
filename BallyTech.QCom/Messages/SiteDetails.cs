using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BallyTech.QCom.Messages
{
    partial class SiteDetails
    {
        public void UpdateLength(BinaryWriter output)
        {
            this.LLength = (byte)this.LText.Length;
            this.SLength = (byte) this.SText.Length;
        }

    }
}
