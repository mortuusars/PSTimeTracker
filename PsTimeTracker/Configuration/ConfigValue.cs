namespace PSTimeTracker.Configuration
{
    public class ConfigValue<T>
    {
        public string Name { get; set; }
        public T Value { get; set; }

        public ConfigValue(string name, T value)
        {
            Name = name;
            Value = value;
        }
    }
}
