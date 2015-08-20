namespace ReMi.BusinessEntities.Metrics
{
    public class MeasurementTime
    {
        public virtual string Name { get; set; }

        public virtual int Value { get; set; }

        public override string ToString()
        {
            return string.Format("[Value={0}, Name={1}]", Value, Name);
        }
    }
}
