using Itminus.Tags.Projects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itminus.Tags.Web;

internal class S7TagRuntime_Rx
{
    private readonly ITagsProjectRunner _runner;

    public S7TagRuntime_Rx(ITagsProjectRunner runner)
    {
        this._runner = runner;
    }

    #region
    public TagsProject? TagsProject { get; private set; }
    #endregion


    public IObservable<Unit> LaunchObservable()
    {
        var projfolder = "D:\\proj\\揽月\\Itminus.Tags\\samples\\tags.proj1";
        this.TagsProject = new TagsProject(projfolder);
        return this._runner.RunAsObservable(this.TagsProject!);
    }

}
