using Itminus.Tags.S7;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags
{
    internal class S7TagRuntime
    {
        private readonly LoggerFactory _loggerFactory;
        private ILogger<S7TagRuntime> _logger;
        private IList<ITagChannel> _channels = new List<ITagChannel>();
        private ITagCbnt _cbnt;

        public S7TagRuntime(LoggerFactory loggerFactory)
        {
            this._loggerFactory = loggerFactory;
            this._logger = _loggerFactory.CreateLogger<S7TagRuntime>();
        }


        public void Initialize()
        {
            var channel = new S7TagChannel(
                "S7",
                new StdUnit.Sharp7.Options.S7PlcItem() { IpAddr = "localhost", Rack = 0, Slot = 1 },
                this._loggerFactory.CreateLogger<S7TagChannel>()
            );
            this._channels.Add(channel);

            this._cbnt = new S7TagCbntBuilder("Group1", "DB200.100.1")
                .WithDevice(channel)
                .WithInterval(0)
                .Configure(builder =>
                {
                    var tagFactory = builder.MakeS7TagFactory();

                    builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                    {
                        TagName = "拍照-请求-标志",
                        Address = "DB200.100.1",
                        TagKind = TagKinds.BIT,
                        TagSize = 1,
                    }));

                    builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                    {
                        TagName = "拍照-请求-料号",
                        Address = "DB200.102",
                        TagKind = TagKinds.BYTE,
                        TagSize = 1,
                    }));

                    builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                    {
                        TagName = "拍照-请求-程序号",
                        Address = "DB200.104",
                        TagKind = TagKinds.INT16,
                        TagSize = 2,
                    }));

                    builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                    {
                        TagName = "拍照-响应-标志",
                        Address = "DB200.400.0",
                        TagKind = TagKinds.BIT,
                        TagSize = 1,
                    }));

                    builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                    {
                        TagName = "拍照-响应-OK",
                        Address = "DB200.400.1",
                        TagKind = TagKinds.BIT,
                        TagSize = 1,
                    }));

                    builder.AddTag(tagFactory.CreateTag(new TagDescriptor()
                    {
                        TagName = "拍照-响应-NG",
                        Address = "DB200.400.2",
                        TagKind = TagKinds.BIT,
                        TagSize = 1,
                    }));
                })
                .Build()
                ;
        }


        public virtual async Task RunAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {

                try
                {
                    if (!this._cbnt.IsEnabled)
                    {
                        await Task.Delay(500);
                        continue;
                    }
                    await this._cbnt.Channel.EnsureConnectedAsync();

                    await InputAsync();

                    ProcessAsync();

                    await OutputAsync();
                }
                catch (Exception ex)
                {
                    var msg = $"{ex.Message}\r\n{ex.StackTrace}";
                    this._logger.LogError("PLC={PlcName}处理报错消息出错：{ex}", this._cbnt.Channel.ChannelName, msg);
                    try
                    {
                        this._cbnt.Channel?.DisconnectAsync();
                    }
                    catch
                    {
                      
                    }
                }
                finally
                {
                    await Task.Delay(_cbnt.ScanInterval, ct);
                }
            }
        }

        private void ProcessAsync()
        {

            var reqTag = this._cbnt["拍照-请求-标志"];
            var ackTag = this._cbnt["拍照-响应-标志"];

            var hasReq = reqTag.GetTagValue<bool>();
            var hasAck = ackTag.GetTagValue<bool>();
            if (hasReq && !hasAck)
            {
                var mat = this._cbnt["拍照-请求-料号"];
                var prog = this._cbnt["拍照-请求-程序号"];
                var matcode = mat.GetTagValue<byte>();
                var progNo = prog.GetTagValue<short>();
                Console.WriteLine($"拍照响应：料号={matcode}，程序号={progNo}");
                ackTag.Value = true;
            }

            if (!hasReq && hasAck)
            {
                ackTag.Value = false;
                Console.WriteLine($"清除拍照响应信号");
            }
        }

        private async Task InputAsync()
        {
            await this._cbnt.ReadAsync();
        }


        private async Task OutputAsync()
        {
            // write
            var tags = this._cbnt.Children.Values.Where(t => t.IsDirty).ToList();
            if(tags.Count > 3)
            {
                await this._cbnt.WriteAsync();
            }
            else
            {
                foreach (var tag in tags)
                {
                    Console.WriteLine($"修改{tag.TagName()}={tag.Value}");
                    await tag.WriteAsync();
                    await Task.Delay(0);
                }
            }

        }


    }
}
