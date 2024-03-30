using System.Drawing;
using System.Numerics;
using Raylib_cs;
using RL = Raylib_cs;
using static Raylib_cs.Raylib;

const int screenWidth  = 800;
const int screenHeight = 450;
const float screenAspect = (float)screenWidth / (float)screenHeight;

const int FONT_SIZE = 20;

double time = 0.0;

InitWindow(screenWidth, screenHeight, "Plotter");
RenderTexture2D target = LoadRenderTexture(GetScreenWidth(), GetScreenHeight());

Font font = Utils.MakeFont("./resources/fonts/NotoSans.ttf", FONT_SIZE);

Vector2 centreAt = new Vector2(0f, 0f);
Camera2D camera = new Camera2D(offset: Vector2.Zero, target: Vector2.Zero, rotation: 0.0f, zoom: 1.0f);

Vector2 equationCompileExtent = MeasureTextEx(font, "Compile", FONT_SIZE, 0.0f);
RL.Rectangle equationCompileRect = new RL.Rectangle(screenWidth - 10.0f - 10.0f - equationCompileExtent.X, 10.0f, equationCompileExtent.X + 10.0f, 30.0f);
RL.Rectangle equationTextRect = new RL.Rectangle(10.0f, 10.0f, 300.0f, 30.0f);

string equationText = "y = x*sin(x)^2";
Shader shader = new Shader();
Equation.CompileEquation(ref shader, equationText);

void HandleNavigateInput()
{
    if (IsMouseButtonDown(MouseButton.Left))
    {
        Vector2 delta = GetMouseDelta();
        delta.Y *= -1f;
        centreAt -= camera.Zoom * delta * (screenAspect / GetScreenHeight());
    }
}

void HandleZoomInput()
{
    float zoomModifier = 0.0f;

    float wheelFactor = 0.1f;
    float wheel = GetMouseWheelMove();
    zoomModifier -= wheel * wheelFactor;

    float arrowFactor = 0.1f;
    float arrows = IsKeyDown(KeyboardKey.Up) - IsKeyDown(KeyboardKey.Down);
    zoomModifier -= arrows * arrowFactor;

    camera.Zoom += zoomModifier;
    camera.Zoom = Math.Max(camera.Zoom, 1e-2f);
}

void HandleEquationTextKeyInput()
{
    int code = GetCharPressed();

    while (code > 0)
    {
        if ((code >= 32) && (code <= 125))
        {
            char key = (char)code;
            equationText += key;
        }

        code = GetCharPressed();
    }

    if (IsKeyPressed(KeyboardKey.Backspace) || IsKeyPressedRepeat(KeyboardKey.Backspace))
    {
        equationText = equationText.Substring(0, int.Max(0, equationText.Length - 1));
    }

    if (IsKeyDown(KeyboardKey.LeftControl) && IsKeyPressed(KeyboardKey.Enter))
    {
        Equation.CompileEquation(ref shader, equationText);
    }
}

void HandleEquationTextMouseInput()
{
    Vector2 mousePos = GetMousePosition();
    if (CheckCollisionPointRec(mousePos, equationCompileRect))
    {
        // NOTE: this is not how buttons should behave, but for now it will suffice
        if (IsMouseButtonReleased(MouseButton.Left))
        {
            Equation.CompileEquation(ref shader, equationText);
        }
    }
}

void DrawEquationRects()
{
    DrawRectangleRec(equationTextRect, RL.Color.LightGray);
    Vector2 textExtent = MeasureTextEx(font, equationText, (float)FONT_SIZE, 0.0f);
    Vector2 equationTextTL = new Vector2(equationTextRect.X + 5.0f, equationTextRect.Y + 5.0f);
    DrawTextEx(font, equationText, equationTextTL, FONT_SIZE, 0.0f, RL.Color.DarkGray);
    
    // cursor
    if ((time % 1.0) < 0.5)
    {
        Vector2 cursorPos = equationTextTL + (textExtent.X + 2) * Vector2.UnitX;
        DrawTextEx(font, "_", cursorPos, FONT_SIZE, 0.0f, RL.Color.DarkGray);
    }

    // compile button
    {
        DrawRectangleRec(equationCompileRect, RL.Color.DarkGray);
        Vector2 textPos = new Vector2(equationCompileRect.X + 5, equationCompileRect.Y + 5);
        DrawTextEx(font, "Compile", textPos, FONT_SIZE, 0.0f, RL.Color.LightGray);
    }
}

(Vector2, Vector2) GetAxisRange()
{
    Vector2 xRange = new Vector2(-1, 1);
    Vector2 yRange = new Vector2(-1, 1);

    xRange *= camera.Zoom * screenAspect;
    yRange *= camera.Zoom;

    xRange.X += centreAt.X;
    xRange.Y += centreAt.X;
    yRange.X += centreAt.Y;
    yRange.Y += centreAt.Y;

    return (xRange, yRange);
}

void DrawGrid()
{
    float s = camera.Zoom;

    float H = GetScreenHeight();
    float W = GetScreenWidth();

    // lines
    {
        double OoM = Math.Pow(10, Math.Round(Math.Log10((double)s / 10.0)));
        double scale = (2f * s) / (OoM * H);
        
        var l = (H / (2f * s)) * (centreAt.X - screenAspect * s);
        var r = (H / (2f * s)) * (centreAt.X + screenAspect * s);

        int xLoIdx = (int)Math.Ceiling(l * scale);
        int xHiIdx = (int)Math.Ceiling(r * scale);

        for (int xIdx = xLoIdx; xIdx < xHiIdx; xIdx++)
        {
            if (xIdx == 0)
            {
                continue;
            }

            float xAt = (float)(-l + (xIdx / scale));
            Vector2 from = new Vector2(xAt, 0f);
            Vector2 to   = new Vector2(xAt, H);

            float thick = 1f;
            RL.Color color = (xIdx % 10) == 0 ? RL.Color.DarkGray : RL.Color.LightGray;
            if (xIdx == 0)
            {
                thick = 2f;
                color = RL.Color.Black;
            }

            DrawLineEx(from, to, thick, color);
        }

        var b = (H / (2f * s)) * (centreAt.Y - s);
        var t = (H / (2f * s)) * (centreAt.Y + s);

        int yLoIdx = (int)Math.Ceiling(b * scale);
        int yHiIdx = (int)Math.Ceiling(t * scale);

        for (int yIdx = yLoIdx; yIdx < yHiIdx; yIdx++)
        {
            if (yIdx == 0)
            {
                continue;
            }

            float yAt = H - (float)(-b + (yIdx / scale));
            Vector2 from = new Vector2(0f, yAt);
            Vector2 to   = new Vector2(W, yAt);

            float thick = 1f;
            RL.Color color = (yIdx % 10) == 0 ? RL.Color.DarkGray : RL.Color.LightGray;
            if (yIdx == 0)
            {
                thick = 2f;
                color = RL.Color.Black;
            }

            DrawLineEx(from, to, thick, color);
        }

        if (xLoIdx <= 0 && xHiIdx >= 0)
        {
            float xAt = -l;
            Vector2 from = new Vector2(xAt, 0f);
            Vector2 to   = new Vector2(xAt, H);

            float thick = 2f;
            RL.Color color = RL.Color.Black;

            DrawLineEx(from, to, thick, color);
        }

        if (yLoIdx <= 0 && yHiIdx >= 0)
        {
            float yAt = H + b;
            Vector2 from = new Vector2(0f, yAt);
            Vector2 to   = new Vector2(W , yAt);

            float thick = 2f;
            RL.Color color = RL.Color.Black;

            DrawLineEx(from, to, thick, color);
        }
    }

    // ticks
    {
        double lg = Math.Log10((double)s);
        double OoM = Math.Pow(10, (int)lg);

        double scale = (2f * s) / (OoM * H);

        var l = (H / (2f * s)) * (centreAt.X - screenAspect * s);
        var r = (H / (2f * s)) * (centreAt.X + screenAspect * s);
        var b = (H / (2f * s)) * (centreAt.Y - s);
        var t = (H / (2f * s)) * (centreAt.Y + s);

        int xLoIdx = (int)Math.Ceiling((l - 50) * scale);
        int xHiIdx = (int)Math.Ceiling((r + 50) * scale);

        int yLoIdx = (int)Math.Ceiling((b - 50) * scale);
        int yHiIdx = (int)Math.Ceiling((t + 50) * scale);

        // x axis
        {        
            var bClamp = Math.Clamp(b, -H, -FONT_SIZE);
            var yAt = H + bClamp;

            for (int xIdx = xLoIdx; xIdx < xHiIdx; xIdx++)
            {
                if (xIdx == 0)
                {
                    continue;
                }

                float  xAt = (float)(-l + (xIdx / scale));
                float  val = (float)(xIdx / scale) * (2f * s) / H;
                Vector2 at = new Vector2(xAt, yAt);

                DrawTextEx(font, val.ToString("G2"), at, FONT_SIZE, 0f, RL.Color.DarkGray);

                if (yLoIdx <= 0 && yHiIdx >= 0)
                {
                    Vector2    tickAt = new Vector2(xAt, H + b);
                    Vector2 tickDelta = new Vector2(0, 5);
                    Vector2 tickStart = tickAt - tickDelta;
                    Vector2   tickEnd = tickAt + tickDelta;

                    DrawLineEx(tickStart, tickEnd, 3f, RL.Color.Black);
                }
            }
        }
        
        // y axis
        {
            var lClamp = Math.Clamp(-l, 0, W);
            var xAt = lClamp;

            for (int yIdx = yLoIdx; yIdx < yHiIdx; yIdx++)
            {
                if (yIdx == 0)
                {
                    continue;
                }

                float  yAt = H - (float)(-b + (yIdx / scale));
                float  val = (float)(yIdx / scale) * (2f * s) / H;
                string display = val.ToString("G2");
                Vector2 extent = MeasureTextEx(font, display, FONT_SIZE, 0f);
                Vector2 at = new Vector2(Math.Max(0f, xAt - extent.X - 2), yAt);

                DrawTextEx(font, display, at, FONT_SIZE, 0f, RL.Color.DarkGray);

                if (xLoIdx <= 0 && xHiIdx >= 0)
                {
                    Vector2    tickAt = new Vector2(-l, yAt);
                    Vector2 tickDelta = new Vector2(5, 0);
                    Vector2 tickStart = tickAt - tickDelta;
                    Vector2   tickEnd = tickAt + tickDelta;

                    DrawLineEx(tickStart, tickEnd, 3f, RL.Color.Black);
                }                
            }
        }
    }

}

SetTargetFPS(60);

while(!WindowShouldClose())
{
    time += GetFrameTime();

    HandleZoomInput();
    HandleNavigateInput();
    HandleEquationTextKeyInput();
    HandleEquationTextMouseInput();

    BeginTextureMode(target);
    {
        DrawRectangle(0, 0, GetScreenWidth(), GetScreenHeight(), RL.Color.Black);
    }
    EndTextureMode();

    BeginDrawing();
    {
        ClearBackground(RL.Color.RayWhite);
        
        // DrawAxis();
        DrawGrid();
        DrawEquationRects();

        BeginShaderMode(shader);
        {
            var (xRange, yRange) = GetAxisRange();
            Vector4 axisRange = new Vector4(xRange.X, xRange.Y, yRange.X, yRange.Y);
            int axisLoc = GetShaderLocation(shader, "axisRange");
            SetShaderValue<Vector4>(shader, axisLoc, axisRange, ShaderUniformDataType.Vec4);
        
            DrawTexture(target.Texture, 0, 0, RL.Color.White);
        }
        EndShaderMode();
    }
    EndDrawing();
}

CloseWindow();