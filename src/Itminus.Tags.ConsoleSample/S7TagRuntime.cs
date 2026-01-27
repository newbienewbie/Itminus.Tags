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
        private ITagGrp _root;

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

            var cbnt = new S7TagCbntBuilder("Group1", "DB200.100.1")
                .WithChannel(channel)
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
            var root = new TagGrp("root", isEntry: true, channel);
            root.AddTag(cbnt);
            this._root = root;
        }


        public virtual async Task RunAsync(CancellationToken ct)
        {
            var runner = new TagGrpRunner();
            runner.TurnCrashed += (grp, ch, ex) => {
                var msg = $"{ex.Message}\r\n{ex.StackTrace}";
                Console.WriteLine($"PLC={_root.Name}处理报错消息出错：{ex.Message}");
                return Task.CompletedTask;
            };

            runner.TurnProcess += async (grp, ch) => {
                await this.ProcessAsync();
            };

            await runner.StartAsync(this._root, ct);
        }

        private Task ProcessAsync()
        {

            var reqTag = this._root.Descendant("Group1/拍照-请求-标志").AsTag();
            var ackTag = this._root.Descendant("Group1/拍照-响应-标志").AsTag();

            var hasReq = reqTag.GetTagValue<bool>();
            var hasAck = ackTag.GetTagValue<bool>();
            if (hasReq && !hasAck)
            {
                var mat = this._root.Descendant("Group1/拍照-请求-料号").AsTag();
                var prog = this._root.Descendant("Group1/拍照-请求-程序号").AsTag();
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

            return Task.CompletedTask;
        }



    }
}
