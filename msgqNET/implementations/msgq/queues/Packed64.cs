namespace msgqNET.implementations.msgq.queues;

public struct Packed64(ulong value) : IEquatable<Packed64>
{
    private ulong _value = value;

    // Constructor to initialize with a 64-bit value

    // Getter for the higher 32 bits
    public int CycleCounter
    {
        get => (int)(_value >> 32);  // Shift right by 32 bits to get the higher part
        set => _value = ((ulong)value << 32) | (_value & 0xFFFFFFFF); // Set the higher part
    }

    // Getter for the lower 32 bits
    public int ReadPointer
    {
        get => (int)(_value & 0xFFFFFFFF);
        set => _value = ((ulong)CycleCounter << 32) | (ulong)(value & 0xFFFFFFFF);
    }

    // Optionally, allow setting the entire 64-bit value
    public ulong Value
    {
        get => _value;
        set => _value = value;
    }

    public bool Equals(Packed64 other) => _value == other._value;

    public override bool Equals(object? obj) => obj is Packed64 other && Equals(other);

    public override int GetHashCode() => _value.GetHashCode();

    public static bool operator ==(Packed64 left, Packed64 right) => left.Equals(right);

    public static bool operator !=(Packed64 left, Packed64 right) => !left.Equals(right);
}