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
        private ITagCbnt _group;

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

            this._group = new ModbusTcpTagCbntBuilder("Group2", "40001")
                .WithDevice(channel)
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
        }


        public virtual async Task RunAsync(CancellationToken ct)
        {
            byte i = 0;
            while (!ct.IsCancellationRequested)
            {

                try
                {
                    Console.WriteLine($"Begin--------------");
                    if (!this._group.IsEnabled)
                    {
                        await Task.Delay(500);
                        continue;
                    }
                    await this._group.Channel.EnsureConnectedAsync();
                    Console.WriteLine($"Connected");

                    await InputAsync();

                    ProcessAsync(ref i);

                    await OutputAsync();

                    Console.WriteLine($"Done--------------");
                    // await group.WriteAsync();
                }
                catch (Exception ex)
                {
                    var msg = $"{ex.Message}\r\n{ex.StackTrace}";
                    try
                    {
                        // await this.HandleErrAsync(LogLevel.Error, msg);
                    }
                    catch (Exception handleErrException)
                    {
                        this._logger.LogError("通道={ChannelName}处理报错消息出错：{ex}", this._group.Channel.ChannelName, handleErrException.Message);
                    }
                    try
                    {
                        this._group.Channel?.DisconnectAsync();
                    }
                    catch (Exception e)
                    {
                        this._logger.LogError("通道[{ChannelName}]断开连接失败: {eMsg}", this._group.Channel.ChannelName, e.Message);
                    }
                }
                finally
                {
                    await Task.Delay(_group.ScanInterval, ct);
                }
            }
        }

        private void ProcessAsync(ref byte i)
        {
            // process
            var tag = this._group["Bit1"];
            if (tag is BitTagCbntor byteTag)
            {
                byteTag.Value = i % 2 == 0;
                i++;
            }

            var tag2 = this._group["UInt2"];
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

        private async Task InputAsync()
        {
            // read
            await this._group.ReadAsync();
            var dt = DateTimeOffset.UtcNow;
            foreach (var kvp in this._group.Children)
            {
                var tag = kvp.Value;
                Console.WriteLine($"{tag.TagDescriptor.TagName}-{tag.Value}");
            }
        }


        private async Task OutputAsync()
        {
            // write
            var tags = this._group.Children.Values.Where(t => t.IsDirty);
            foreach (var tag in tags)
            {
                Console.WriteLine($"修改{tag.TagName()}={tag.Value}");
                await tag.WriteAsync();
                tag.IsDirty = false;
                await Task.Delay(1);
            }
        }


    }
}
