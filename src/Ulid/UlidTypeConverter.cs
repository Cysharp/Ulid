using System.ComponentModel;
using System.Globalization;

namespace System
{
    public class UlidTypeConverter : TypeConverter
    {
        private static readonly Type StringType = typeof(string);
        private static readonly Type GuidType = typeof(Guid);
        
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == StringType || sourceType == GuidType)
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == StringType || destinationType == GuidType)
            {
                return true;
            }
            
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, 
            CultureInfo culture, object value)
        {
            switch (value)
            {
                case Guid g:
                    return new Ulid(g);
                case string stringValue:
                    return Ulid.Parse(stringValue);
            }
            
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value,
            Type destinationType)
        {
            if (value is Ulid ulid)
            {
                if (destinationType == StringType)
                {
                    return ulid.ToString();
                }
                
                if (destinationType == GuidType)
                {
                    return ulid.ToGuid();
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}