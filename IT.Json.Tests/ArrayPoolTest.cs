using System.Buffers;

namespace IT.Json.Tests;

public class ArrayPoolTest
{
    [Test]
    public void Test()
    {
        var max = 1024 * 1024 * 1024;//1 GB
        var rented = ArrayPool<byte>.Shared.Rent(max);

        Assert.That(rented, Is.Not.Null);
        Assert.That(rented, Has.Length.EqualTo(max));

        ArrayPool<byte>.Shared.Return(rented);

        rented = ArrayPool<byte>.Shared.Rent(Array.MaxLength);

        Assert.That(rented, Is.Not.Null);
        Assert.That(rented, Has.Length.EqualTo(Array.MaxLength));

        ArrayPool<byte>.Shared.Return(rented);

        //GC.AllocateUninitializedArray
    }
}