namespace Itminus.Tags.Projects;

public interface ITagsProjectRunner
{

    /// <summary>
    /// 运行
    /// </summary>
    /// <param name="proj"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task RunProjAsync(TagsProject proj, CancellationToken ct);
}

