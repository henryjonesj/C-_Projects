using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BallyTech.QCom.Model.Egm;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model
{
    public static class EnumExtensions
    {
        public static SerializableList<TEnum> GetAllValues<TEnum>(this TEnum @enum) where TEnum : struct 
        {
            if (!@enum.GetType().IsEnum) throw new InvalidOperationException("Not an Enum");

            var fields = @enum.GetType().GetFields(BindingFlags.Public | BindingFlags.Static);

            var enumValues = new SerializableList<TEnum>();
            enumValues.AddRange(fields.Select(memberInfo => (TEnum) Enum.Parse(typeof (TEnum), memberInfo.GetValue(null).ToString(), true)));

            return enumValues;
        }

    }
}
