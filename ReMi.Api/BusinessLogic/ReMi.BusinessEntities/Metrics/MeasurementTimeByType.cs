using System;
using ReMi.Common.Constants.Metrics;
using ReMi.Common.Utils.Enums;

namespace ReMi.BusinessEntities.Metrics
{
    public class MeasurementTimeByType : MeasurementTime
    {
        public MeasurementType Type { get; set; }

        public override string Name
        {
            get { return Type.ToDescriptionString(); }
            set
            {
                if (Enum.IsDefined(typeof(MeasurementType), value))
                {
                    var result = Enum.Parse(typeof(MeasurementType), value);
                    if (result != null)
                    {
                        Type = (MeasurementType)result;
                        return;
                    }
                }
                throw new ArgumentOutOfRangeException("value", "Should be valid entry from MeasurementType!");
            }
        }

        public override string ToString()
        {
            return string.Format("[Value={0}, Type={1}]", Value, Type);
        }
    }
}
