namespace Itminus.Tags;

public static class TagUnionExtensions
{
    #region R/W
    public static async Task ReadAsync(this TagUnion tagunion)
    {
        await tagunion.Map(
            async tag => {
                if (tag.AccessMode() == TagAccessMode.R1W && tag.IsScaned)
                {
                    return;
                }
                await tag.ReadAsync();
                tag.IsScaned = true;
            },
            async cbnt =>
            {
                if(cbnt.AcessMode == TagAccessMode.R1W && cbnt.IsScaned)
                {
                    return;
                }
                await cbnt.ReadAsync();
                cbnt.IsScaned = true;
            },
            async grp =>
            {
                await grp.ReadAsync();
            }
         );
    }

    public static async Task WriteAsync(this TagUnion tagunion)
    {
        await tagunion.Map(
            async tag => {
                if (tag.IsDirty)
                {
                    await tag.WriteAsync();
                }
            },
            async cbnt =>
            {
                if(cbnt.IsDirty) 
                {
                    await cbnt.WriteAsync();
                }
            },
            async grp =>
            {
                await grp.WriteAsync();
            }
         );
    }
    #endregion



    #region
    public static ITag AsTag(this TagUnion tagunion) => tagunion.Map(
        tagunit => tagunit,
        tagcbnt => throw new Exception($"{tagcbnt.Name} is a {nameof(ITagCbnt)} intead of a {nameof(ITag)}"),
        taggrp => throw new Exception($"{taggrp.Name} is a {nameof(ITagGrp)} intead of a {nameof(ITag)}")
        );

    public static ITagCbnt AsTagCbnt(this TagUnion tagunion) => tagunion.Map(
        tagunit => throw new Exception($"{tagunit.TagName()} is a {nameof(ITag)} intead of a {nameof(ITagCbnt)}"),
        tagcbnt => tagcbnt,
        taggrp => throw new Exception($"{taggrp.Name} is a {nameof(ITagGrp)} intead of a {nameof(ITagCbnt)}")
    );

    public static ITagGrp AsTagGrp(this TagUnion tagunion) => tagunion.Map(
        tagunit => throw new Exception($"{tagunit.TagName()} is a {nameof(ITag)} intead of a {nameof(ITagGrp)}"),
        tagcbnt => throw new Exception($"{tagcbnt.Name} is a {nameof(ITagCbnt)} intead of a {nameof(ITagGrp)}"),
        taggrp => taggrp
    );
    #endregion

    #region IsXyzFlag
    public static bool IsTagUnit(this TagUnion tagunion) => tagunion.Map(
        tag => true,
        tagcbnt => false,
        taggrp => false
        );
    public static bool IsTagCbnt(this TagUnion tagunion) => tagunion.Map(
        tag => false,
        tagcbnt => true,
        taggrp => false
    );
    public static bool IsTagGrp(this TagUnion tagunion) => tagunion.Map(
        tag => false,
        tagcbnt => false,
        taggrp => true
    );
    #endregion
}