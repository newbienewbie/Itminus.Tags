namespace Itminus.Tags;

public static class TagUnionExtensions
{
    #region R/W
    public static async Task ReadAsync(this TagUnion tagunion, CancellationToken ct)
    {
        await tagunion.Map(
            async tag => {
                if (tag.AccessMode() == TagAccessMode.R1W && tag.IsScaned)
                {
                    return;
                }
                if(tag.IsWriteOnly())
                {
                    return;
                }
                await tag.ReadAsync(ct);
                tag.IsScaned = true;
            },
            async cbnt =>
            {
                if(cbnt.AcessMode == TagAccessMode.R1W && cbnt.IsScaned)
                {
                    return;
                }
                if(cbnt.IsWriteOnly())
                {
                    return;
                }
                await cbnt.ReadAsync(ct);
                cbnt.IsScaned = true;
            },
            async grp =>
            {
                await grp.ReadAsync(ct);
            }
         );
    }

    public static async Task WriteAsync(this TagUnion tagunion, CancellationToken ct)
    {
        await tagunion.Map(
            async tag => {
                if (tag.IsDirty)
                {
                    await tag.WriteAsync(ct);
                }
            },
            async cbnt =>
            {
                if(cbnt.IsReadOnly())
                {
                    return;
                }

                if(cbnt.IsDirty) 
                {
                    await cbnt.WriteAsync(ct);
                }
            },
            async grp =>
            {
                await grp.WriteAsync(ct);
            }
         );
    }

    public static bool IsDirty(this TagUnion tagunion)
    {
        return tagunion.Map(
            tag => tag.IsDirty,
            cbnt => cbnt.IsDirty,
            grp => grp.IsDirty()
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