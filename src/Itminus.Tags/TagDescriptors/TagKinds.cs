namespace Itminus.Tags;


/// <summary>
/// 测点种类，位、字节、ushort、整型、浮点等
/// </summary>
public enum TagKinds
{
    Unknown = 0,

    BIT = 100,
    BYTE = 102,
    INT16 = 103,
    UINT16 = 104,
    INT32 = 105,
    UINT32 = 106,
    FLOAT = 107,
    STR = 140,

    DI  = 200,
    DO  = 201,
}
