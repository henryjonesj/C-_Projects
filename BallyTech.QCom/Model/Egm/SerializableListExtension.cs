using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model.Egm
{
    public static class SerializableListExtension
    {
        public static bool Contains<T>(this SerializableList<T> list,params T[] items) 
        {
            return items.Any(list.Contains);
        }
    }
}
