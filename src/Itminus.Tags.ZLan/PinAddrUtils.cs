namespace Itminus.Tags.ZLan
{
    public static class PinAddrUtils
    {
        public static DIPinAddr ParseDI(string pinName)
        {
            return Enum.Parse<DIPinAddr>(pinName);
        }

        public static DOPinAddr ParseDO(string pinName)
        {
            return Enum.Parse<DOPinAddr>(pinName);
        }
    }
}
