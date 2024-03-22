using Raylib_cs;
using static Raylib_cs.Raylib;

public static class Utils
{
    unsafe public static Font MakeFont(string fontPath, int fontSize)
    {
        Font font = new Font();
        uint fontFileBytes = 0;
        byte* fontData = LoadFileData("./resources/fonts/NotoSans.ttf", ref fontFileBytes);
        
        font.BaseSize = fontSize;
        font.GlyphCount = 95;

        font.Glyphs = LoadFontData(fontData, (int)fontFileBytes, fontSize, null, 95, FontType.Default);
        Image fontAtlas = GenImageFontAtlas(font.Glyphs, &font.Recs, 95, 16, 4, 0);
        font.Texture = LoadTextureFromImage(fontAtlas);

        UnloadImage(fontAtlas);
        UnloadFileData(fontData);

        return font;
    }
}