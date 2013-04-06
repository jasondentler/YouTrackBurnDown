namespace YouTrackBurnDown.Api
{
    public class Item
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }

    }
}