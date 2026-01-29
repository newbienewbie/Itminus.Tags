using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags.ConsoleSample;

internal class HandleSnap : LogicetBase
{
    private ITagCbnt _cbnt1;
    private ITagCbnt _cbnt2;
    private ITagCbntor _reqTag;
    private ITagCbntor _ackTag;
    private ITagCbntor _mat;
    private ITagCbntor _prog;

    public HandleSnap(IList<ITagChannel> channels, ITagGrp tags) : base(channels, tags)
    {
        this._cbnt1 = this.Tags.SelectCbnt("g1/拍照请求");
        this._reqTag = _cbnt1.SelectTag("拍照-请求-标志");
        this._mat = _cbnt1["拍照-请求-料号"];
        this._prog = _cbnt1["拍照-请求-程序号"];

        this._cbnt2 = this.Tags.SelectCbnt("g1/拍照响应");
        this._ackTag = _cbnt2.SelectTag("拍照-响应-标志");
    }

    public override int Order => 1;


    public override bool MatchEntry(ITagGrp entry)
    {
        return true;
    }

    public override Task ProcessAsync(ITagGrp entry, ITagChannel thisChannel)
    {
        var hasReq = this._reqTag.GetTagValue<bool>();
        var hasAck = _ackTag.GetTagValue<bool>();

        if (hasReq && !hasAck)
        {
            var matcode = this._mat.GetTagValue<byte>();
            var progNo = this._prog.GetTagValue<short>();
            Console.WriteLine($"拍照响应：料号={matcode}，程序号={progNo}");
            _ackTag.Value = true;
        }

        if (!hasReq && hasAck)
        {
            _ackTag.Value = false;
            Console.WriteLine($"清除拍照响应信号");
        }

        return Task.CompletedTask;
    }
}
