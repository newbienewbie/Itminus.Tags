using Itminus.Tags.S7;
using System;
using System.IO;
using System.Text;
using Xunit;

namespace Itminus.Tags.Tests.S7Tags
{
    public class StrTagCbntorTests
    {
        [Fact]
        public void CreateStrTag_Throws_When_size_NotSpecified()
        {
            var builder = new S7TagCbntBuilder("cbnt1", "DB200.100");

            var descriptor = new TagDescriptor()
            {
                TagName = "str1",
                Address = "DB200.102",
                TagKind = BuiltinTagKinds.STR,
            };

            Assert.Throws<InvalidDataException>(() => {
                var cbnt = builder.AddTags([descriptor]).Build();
                var tag = cbnt.SelectTag("str1");
                Assert.NotNull(tag);
            });
        }

        [Fact]
        public void CreateStrTag_Throws_When_maxlen_NotSpecified()
        {
            var builder = new S7TagCbntBuilder("cbnt1", "DB200.100");

            var descriptor = new TagDescriptor()
            {
                TagName = "str1",
                Address = "DB200.102",
                TagKind = BuiltinTagKinds.STR,
            };
            descriptor.Extras["strlen"] = new System.Xml.Linq.XAttribute("strlen", 2);



            Assert.Throws<InvalidDataException>(() => {
                var cbnt = builder.AddTags([descriptor]).Build();
                var tag = cbnt.SelectTag("str1");
                Assert.NotNull(tag);
            });
        }

        [Fact]
        public void CreateStrTag_Throws_When_strlen_NotSpecified()
        {
            var builder = new S7TagCbntBuilder("cbnt1", "DB200.100");

            var descriptor = new TagDescriptor()
            {
                TagName = "str1",
                Address = "DB200.102",
                TagKind = BuiltinTagKinds.STR,
            };
            descriptor.Extras["maxlen"] = new System.Xml.Linq.XAttribute("maxlen", 20);


            Assert.Throws<InvalidDataException>(() => {
                var cbnt = builder.AddTags([descriptor]).Build();
                var tag = cbnt.SelectTag("str1");
                Assert.NotNull(tag);
            });
        }

        [Theory]
        [InlineData(10, 8)]
        [InlineData(0, 3)]
        [InlineData(4, 0)]
        public void CreateStrTag_Throws_When_maxlen_lt_strlen(int strlen, int maxlen)
        {
            var builder = new S7TagCbntBuilder("cbnt1", "DB200.100");
            var descriptor = new TagDescriptor()
            {
                TagName = "str1",
                Address = "DB200.102",
                TagKind = BuiltinTagKinds.STR,
            };
            descriptor.Extras["strlen"] = new System.Xml.Linq.XAttribute("strlen", strlen);
            descriptor.Extras["maxlen"] = new System.Xml.Linq.XAttribute("maxlen", maxlen);



            Assert.Throws<InvalidDataException>(() =>{
                var cbnt = builder.AddTags([descriptor]).Build();
                var tag = cbnt.SelectTag("str1");
                Assert.NotNull(tag);
            });
        }


        [Fact]
        public void StrTag_GetAndSet_WithExactLength()
        {
            var builder = new S7TagCbntBuilder("cbnt1", "DB200.100");

            byte maxLen = 10;
            byte strLen = 4;
            var descriptor = new TagDescriptor()
            {
                TagName = "str1",
                Address = "DB200.102",
                TagKind = BuiltinTagKinds.STR,
            };
            descriptor.Extras["strlen"] = new System.Xml.Linq.XAttribute("strlen", strLen);
            descriptor.Extras["maxlen"] = new System.Xml.Linq.XAttribute("maxlen", maxLen);

            var cbnt = builder.AddTags([descriptor]).Build();
            var tag = cbnt.SelectTag("str1");

            Assert.Equal(new byte[] { (byte)maxLen, (byte)strLen }, cbnt.Cache.Span.Slice(2, 2).ToArray());

            tag.Value = "ABCDE";
            Assert.Equal("ABCDE", tag.Value);
            Assert.Equal("ABCDE", tag.GetTagValue<string>());
            Assert.Equal(new byte[] { (byte)maxLen, (byte)5 }, cbnt.Cache.Span.Slice(2, 2).ToArray());

            var newVal1 = "WXYZ";
            tag.Value = newVal1;
            var got = tag.Value as string;
            Assert.Equal(newVal1, got);
            Assert.Equal(new byte[] { (byte)maxLen, (byte)4 }, cbnt.Cache.Span.Slice(2, 2).ToArray());
            var span = cbnt.Cache.Span.Slice(2+2, 4);
            var roundtrip = Encoding.ASCII.GetString(span);
            Assert.Equal(newVal1, roundtrip);
        }


        [Fact]
        public void StrTag_Set_Throws_When_TooLong()
        {
            var builder = new S7TagCbntBuilder("cbnt1", "DB200.100");

            byte maxLen = 10;
            byte strLen = 4;
            var descriptor = new TagDescriptor()
            {
                TagName = "str1",
                Address = "DB200.102",
                TagKind = BuiltinTagKinds.STR,
            };
            descriptor.Extras["strlen"] = new System.Xml.Linq.XAttribute("strlen", strLen);
            descriptor.Extras["maxlen"] = new System.Xml.Linq.XAttribute("maxlen", maxLen);

            var cbnt = builder.AddTags([descriptor]).Build();
            var tag = cbnt.SelectTag("str1");


            Assert.Throws<ArgumentException>(() => tag.Value = "1234567890-");
        }

        [Fact]
        public void StrTag_Set_When_MaxLen()
        {
            var builder = new S7TagCbntBuilder("cbnt1", "DB200.100");

            byte maxLen = 10;
            byte strLen = 4;
            var descriptor = new TagDescriptor()
            {
                TagName = "str1",
                Address = "DB200.102",
                TagKind = BuiltinTagKinds.STR,
            };
            descriptor.Extras["strlen"] = new System.Xml.Linq.XAttribute("strlen", strLen);
            descriptor.Extras["maxlen"] = new System.Xml.Linq.XAttribute("maxlen", maxLen);

            var cbnt = builder.AddTags([descriptor]).Build();
            var tag = cbnt.SelectTag("str1");

            tag.Value = "0123456789";
            Assert.Equal("0123456789", tag.GetTagValue<string>());
        }
    }
}
