using Itminus.Tags.ModbusTcp;
using Itminus.Tags.ZLan;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags
{
    internal class ZLanTagRuntime
    {
        private readonly LoggerFactory _loggerFactory;
        private ILogger<S7TagRuntime> _logger;
        private IList<ITagChannel> _channels = new List<ITagChannel>();
        private ITagCbnt _group1;
        private ITagCbnt _outs;

        public ZLanTagRuntime(LoggerFactory loggerFactory)
        {
            this._loggerFactory = loggerFactory;
            this._logger = _loggerFactory.CreateLogger<S7TagRuntime>();
        }


        public void Initialize()
        {
            var channel = new ModbusTcpChannel(
                "ZLan001",
                //new ModbusTcpItem() { IpAddr = "localhost", Port = 502},
                new ModbusTcpItem() { IpAddr = "192.168.1.254", Port = 502 },
                this._loggerFactory.CreateLogger<ModbusTcpChannel>()
            );
            this._channels.Add(channel);

            this._group1 = new ZLanDICbntBuilder("Group1")
                .WithDevice(channel)
                .Configure(builder =>
                {
                    var tagFactory = builder.MakeZLanTagFactory();

                    builder.AddTag(tagFactory.AddDI("DI1", DIPinAddr.DI1));
                    builder.AddTag(tagFactory.AddDI("DI2", DIPinAddr.DI2));
                    builder.AddTag(tagFactory.AddDI("DI3", DIPinAddr.DI3));
                    builder.AddTag(tagFactory.AddDI("DI4", DIPinAddr.DI4));
                    builder.AddTag(tagFactory.AddDI("DI5", DIPinAddr.DI5));
                    builder.AddTag(tagFactory.AddDI("DI6", DIPinAddr.DI6));
                    builder.AddTag(tagFactory.AddDI("DI7", DIPinAddr.DI7));
                    builder.AddTag(tagFactory.AddDI("DI8", DIPinAddr.DI8));
                })
                .Build()
                ;

            this._outs = new ZLanDOCbntBuilder("Group2")
                .WithDevice(channel)
                .Configure(builder =>
                {
                    var tagFactory = builder.MakeZLanTagFactory();
                    builder.AddTag(tagFactory.AddDO("DO1", DOPinAddr.DO1));
                    builder.AddTag(tagFactory.AddDO("DO2", DOPinAddr.DO2));
                    builder.AddTag(tagFactory.AddDO("DO3", DOPinAddr.DO3));
                    builder.AddTag(tagFactory.AddDO("DO4", DOPinAddr.DO4));
                    builder.AddTag(tagFactory.AddDO("DO5", DOPinAddr.DO5));
                    builder.AddTag(tagFactory.AddDO("DO6", DOPinAddr.DO6));
                    builder.AddTag(tagFactory.AddDO("DO7", DOPinAddr.DO7));
                    builder.AddTag(tagFactory.AddDO("DO8", DOPinAddr.DO8));
                })
                .Build()
                ;
        }


        public virtual async Task RunAsync(CancellationToken ct)
        {

            var nth = 1;
            while (!ct.IsCancellationRequested)
            {

                try
                {
                    Console.WriteLine($"Begin--------------");
                    if (!this._group1.IsEnabled)
                    {
                        await Task.Delay(500);
                        continue;
                    }
                    await this._group1.Channel.EnsureConnectedAsync();
                    Console.WriteLine($"Connected");

                    await InputAsync();


                    ProcessAsync(ref nth);

                    await OutputAsync();
                    //await this.OutputOneByOneAsync();
                    Console.WriteLine($"Done--------------");
                    // await group.WriteAsync();
                }
                catch (Exception ex)
                {
                    var msg = $"{ex.Message}\r\n{ex.StackTrace}";
                    try
                    {
                        Console.WriteLine(msg);
                        // 停住...等待人确认
                        Console.ReadLine();
                        // await this.HandleErrAsync(LogLevel.Error, msg);
                    }
                    catch (Exception handleErrException)
                    {
                        this._logger.LogError("通道={ChannelName}处理报错消息出错：{ex}", this._group1.Channel.ChannelName, handleErrException.Message);
                    }
                    try
                    {
                        this._group1.Channel?.DisconnectAsync();
                    }
                    catch (Exception e)
                    {
                        this._logger.LogError("通道[{ChannelName}]断开连接失败: {eMsg}", this._group1.Channel.ChannelName, e.Message);
                    }
                }
                finally
                {
                    await Task.Delay(500, ct);
                }
            }
        }

        private void ProcessAsync(ref int nth)
        {

            // process
            var outtags = new List<ITag>()
            {
                 this._outs["DO1"] ,
                 this._outs["DO2"] ,
                 this._outs["DO3"] ,
                 this._outs["DO4"],
                 this._outs["DO5"] ,
                 this._outs["DO6"] ,
                 this._outs["DO7"] ,
                 this._outs["DO8"] ,
            };

            for(var i = 1; i <= outtags.Count; i ++)
            {
                var tag = outtags[i-1];
                tag.Value = nth == i ? true : false;
            }

            if (nth < 8)
            {
                nth++;
            }
            else if (nth >= 8)
            {
                nth = 1;
            }
        }

        private async Task InputAsync()
        {
            // read
            await this._group1.ReadAsync();
            var dt = DateTimeOffset.UtcNow;
            foreach (var kvp in this._group1.Children)
            {
                var tag = kvp.Value;
                Console.WriteLine($"{tag.TagDescriptor.TagName}-{tag.Value}");
            }
        }


        private async Task OutputAsync()
        {
            if (this._outs.IsDirty)
            {
                await _outs.WriteAsync();
                this._outs.IsDirty = false;
            }

        }

        private async Task OutputOneByOneAsync()
        {
            foreach (var kvp in this._outs.Children)
            {
                var tag = kvp.Value;
                if (tag.IsDirty)
                {
                    await tag.WriteAsync();
                    tag.IsDirty = false;
                }
            }
        }
    }
}
