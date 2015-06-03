#include <a_drawable>

new static Drawable:alies_score;
new static Drawable:axis_score;

ScoreInit()
{
    SetGVar("score_alies", 0);
    SetGVar("score_axis", 0);

    alies_score = CreateDrawableText2D(5,5, COLOR_RED, "fonts/consolas", "Alies: 0");
    axis_score = CreateDrawableText2D(5,25, COLOR_YELLOW, "fonts/consolas", "Axis: 0");
    ShowDrawable(alies_score);
    ShowDrawable(axis_score);
}

ScoreUpdate()
{
    new alies[32], axis[32];
    strformat(alies, sizeof(alies), false, "Alies: %d", GetGVar("score_alies"));
    strformat(axis, sizeof(axis), false, "Axis:  %d", GetGVar("score_axis"));

    SetDrawableText(alies_score, alies);
    SetDrawableText(axis_score, axis);
}
