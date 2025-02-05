using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags;

internal class HandleSnap : LogicetBase
{
    private ITagCbnt _cbnt;
    private ITagCbntor _reqTag;
    private ITagCbntor _ackTag;
    private ITagCbntor _mat;
    private ITagCbntor _prog;

    public HandleSnap(IList<ITagChannel> channels, ITagGrp tags) : base(channels, tags)
    {
        this._cbnt = this.Tags.SelectCbnt("Group1");
        this._reqTag = _cbnt.SelectTag("拍照-请求-标志");
        this._ackTag = _cbnt.SelectTag("拍照-响应-标志");

        this._mat = _cbnt["拍照-请求-料号"];
        this._prog = _cbnt["拍照-请求-程序号"];
    }

    public override IDisposable Attach()
    {
        var reqObs = _reqTag.Watch();
        var dispose = reqObs
            .Synchronize()
            .Subscribe(
                ev =>
                {
                    var hasReq = ev.EventArgs.NewValue is null ? false : (bool)ev.EventArgs.NewValue;
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
                }
            );

        return dispose;
    }
}
