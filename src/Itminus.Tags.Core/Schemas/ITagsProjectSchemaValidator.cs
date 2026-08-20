using System.Xml.Linq;

namespace Itminus.Tags;

/// <summary>
/// 测点项目加载期的 schema 校验器（可选功能）。
/// 未在 DI 中注册时，加载期不执行 XSD 校验——旧的“无命名空间前缀”XML 仍然照常工作。
/// </summary>
public interface ITagsProjectSchemaValidator
{
    /// <summary>
    /// 校验项目根元素。不通过时抛出异常（如 <see cref="TagsProjectSchemaException"/>）。
    /// </summary>
    /// <param name="root">项目根元素（<c>&lt;Project&gt;</c>）</param>
    void Validate(XElement root);
}
