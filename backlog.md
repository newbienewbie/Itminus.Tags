
把ModbusTcp的小端字节序方法改成如下，在VS2026中能正常构建

```c#
    private ReadOnlySpan<byte> ToLittleEndianBytes(Memory<ushort> regs )
    {
        var buff = MemoryMarshal.Cast<ushort, byte>(regs.Span);
        if (this.IsLittleEndian)
        {
            return buff;
        }

        for (int i = 0; i < regs.Length; i++)
        {
            var value = regs.Span[i];
            BinaryPrimitives.WriteUInt16LittleEndian(buff.Slice(i * 2), value);
        }
        return buff;
    }
```
但是在dotnet cli 下构建失败。原因待查。