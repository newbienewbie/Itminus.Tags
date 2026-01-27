using Itminus.Tags.ModbusTcp;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags
{
    internal class ModbusTcpTagRuntime
    {
        private readonly LoggerFactory _loggerFactory;
        private ILogger<S7TagRuntime> _logger;
        private IList<ITagChannel> _channels = new List<ITagChannel>();
        private TagGrp _root;

        public ModbusTcpTagRuntime(LoggerFactory loggerFactory)
        {
            this._loggerFactory = loggerFactory;
            this._logger = _loggerFactory.CreateLogger<S7TagRuntime>();
        }


        public void Initialize()
        {
            var channel = new ModbusTcpChannel(
                "ModbusTcp",
                new ModbusTcpItem() { IpAddr = "localhost", Port = 502},
                this._loggerFactory.CreateLogger<ModbusTcpChannel>()
            );
            this._channels.Add(channel);

            var cbnt = new ModbusTcpTagCbntBuilder("Group2", "40001")
                .WithChannel(channel)
                .WithInterval(200)
                .AddTags(new List<TagDescriptor> {

                    new TagDescriptor{
                        TagName ="Float1",
                        Address = "40001",
                        TagKind = TagKinds.FLOAT,
                        TagSize = 4,
                    },
                    new TagDescriptor{
                        TagName ="Float2",
                        Address = "40003",
                        TagKind = TagKinds.FLOAT,
                        TagSize = 4,
                    },
                    // leave 40005 empty
                    new TagDescriptor{
                        TagName ="Bit1",
                        Address = "40006.1",
                        TagKind = TagKinds.BIT,
                        TagSize = 2,
                    },
                    new TagDescriptor{
                        TagName ="Bit2",
                        Address = "40006.2",
                        TagKind = TagKinds.BIT,
                        TagSize = 2,
                    },
                    new TagDescriptor{
                        TagName ="Bit3",
                        Address = "40006.15",
                        TagKind = TagKinds.BIT,
                        TagSize = 2,
                    },
                    new TagDescriptor{
                        TagName ="UInt1",
                        Address = "40007",
                        TagKind = TagKinds.UINT16,
                        TagSize = 2,
                    },
                    // leave 40008 empty
                    new TagDescriptor{
                        TagName ="UInt2",
                        Address = "40009",
                        TagKind = TagKinds.UINT16,
                        TagSize = 2,
                    },
                    new TagDescriptor{
                        TagName ="Bit4",
                        Address = "40010.15",
                        TagKind = TagKinds.BIT,
                        TagSize = 2,
                    },

                })
                //.Configure(builder =>
                //{
                //    var tagFactory = builder.MakeModbusTcpTagFactory();

                //    builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                //    {
                //        TagName = "拍照-请求-标志",
                //        Address = "40001.0",
                //        TagKind = TagKinds.BIT,
                //        TagSize = 2,
                //    }));

                //    builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                //    {
                //        TagName = "拍照-请求-料号",
                //        Address = "40002",
                //        TagKind = TagKinds.BYTE,
                //        TagSize = 2,
                //    }));

                //    builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                //    {
                //        TagName = "拍照-请求-程序号",
                //        Address = "40003",
                //        TagKind = TagKinds.INT16,
                //        TagSize = 2,
                //    }));

                //    builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                //    {
                //        TagName = "拍照-响应-标志",
                //        Address = "40011.0",
                //        TagKind = TagKinds.BIT,
                //        TagSize = 2,
                //    }));

                //    builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                //    {
                //        TagName = "拍照-响应-OK",
                //        Address = "40011.1",
                //        TagKind = TagKinds.BIT,
                //        TagSize = 2,
                //    }));

                //    builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                //    {
                //        TagName = "拍照-响应-NG",
                //        Address = "40011.2",
                //        TagKind = TagKinds.BIT,
                //        TagSize = 2,
                //    }));
                //})
                .Build()
                ;

            this._root = new TagGrp("root", isEntry: true, channel);
        }


        public virtual async Task RunAsync(CancellationToken ct)
        {
            byte b = 1;
            var runner = new TagGrpRunner();
            runner.TurnCrashed += (grp, ch, ex) => {
                var msg = $"{ex.Message}\r\n{ex.StackTrace}";
                Console.WriteLine($"PLC={_root.Name}处理报错消息出错：{ex.Message}");
                return Task.CompletedTask;
            };

            runner.TurnProcess += (grp, ch) => {
               this.ProcessAsync(ref b);
                return Task.CompletedTask;
            };

            await runner.StartAsync(this._root, ct);
        }

        private void ProcessAsync(ref byte i)
        {
            // process
            var cbnt = this._root["Group2"];
            var tag = cbnt["Bit1"].AsTag();
            if (tag is BitTagCbntor byteTag)
            {
                byteTag.Value = i % 2 == 0;
                i++;
            }

            var tag2 = cbnt["UInt2"].AsTag();
            if (tag2 is UInt16TagCbntor uint16Tag)
            {
                tag2.Value = (UInt16)((UInt16)tag2.Value +2);
            }

            //if (this._group["拍照-响应-标志"] is CbntorBitTag ack)
            //{
            //    ack.Value = true ;
            //}

            //if (this._group["拍照-响应-OK"] is CbntorBitTag ackok && this._group["拍照-响应-NG"] is CbntorBitTag ackng)
            //{
            //    var ok =false;
            //    ackok.Value = ok;

            //    ackng.Value = !ok;
            //}


            //if (this._group["拍照-请求-程序号"] is CbntorInt16Tag proc)
            //{
            //    proc.Value = (short)( i ++);
            //}
        }

 


    }
}
