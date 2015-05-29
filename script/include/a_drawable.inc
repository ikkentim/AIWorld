#if defined _drawable_included
    #endinput
#endif
#define _drawable_included

native Drawable:CreateDrawableText2D(Float:x, Float:y, color, const font[],
    const text[]);
native Drawable:CreateDrawableText3D(Float:x, Float:y, Float:z, color,
    const font[], const text[]);
native Drawable:CreateDrawableLine(Float:x1, Float:y1, Float:z1, Float:x2,
    Float:y2, Float:z2, color1, color2);
native Drawable:CreateDrawableLineCylinder(Float:x, Float:y, Float:z, Float:hx,
    Float:hy, Float:hz, Float:length, Float:radius, color1, color2);
native Drawable:CreateDrawableLineCone(Float:x, Float:y, Float:z, Float:hx,
    Float:hy, Float:hz, Float:length, Float:radius, color1, color2);
native Drawable:CreateDrawableLineSphere(Float:x, Float:y, Float:z, Float:radius,
    color1, color2);

native SetDrawablePosition(Drawable:drawableid, Float:x, Float:y,
    Float:z = 0.0);
native SetDrawablePosition2(Drawable:drawableid, Float:x, Float:y,
    Float:z = 0.0);
native SetDrawableScale(Drawable:drawableid, Float:x, Float:y);
native SetDrawableText(Drawable:drawableid, const value[]);
native SetDrawableColor(Drawable:drawableid, value);
native SetDrawableColor2(Drawable:drawableid, value);
native SetDrawableFont(Drawable:drawableid, const value[]);
native SetDrawableRadius(Drawable:drawableid, Float:value);
native SetDrawableLength(Drawable:drawableid, Float:value);

native ShowDrawable(Drawable:drawableid);
native HideDrawable(Drawable:drawableid);
native DestroyDrawable(Drawable:drawableid);
