namespace ReMi.Common.Utils.Repository
{
    public class ChangedField
    {
        public string Name { get; private set; }
        public object OriginalValue { get; private set; }
        public object NewValue { get; private set; }

        public ChangedField(string name, object originalValue, object newValue)
        {
            Name = name;
            OriginalValue = originalValue;
            NewValue = newValue;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
