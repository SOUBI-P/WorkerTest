public class DataItem
{
    public int Value { get; }
    public DateTime CreatedAt { get; }

    public DataItem(int value)
    {
        Value = value;
        CreatedAt = DateTime.Now;
    }
}
