using System.Drawing;
using System.Numerics;
using Raylib_cs;
using RL = Raylib_cs;
using static Raylib_cs.Raylib;
using System.Diagnostics;

const int screenWidth  = 800;
const int screenHeight = 450;
const float screenAspect = (float)screenWidth / (float)screenHeight;

const int FONT_SIZE = 20;

double time = 0.0;

Point axisX_Start = new Point(0, screenHeight/2);
Point   axisX_End = new Point(screenWidth, screenHeight/2);

Point axisY_Start = new Point(screenWidth/2, 0);
Point   axisY_End = new Point(screenWidth/2, screenHeight);


InitWindow(screenWidth, screenHeight, "Plotter");
RenderTexture2D target = LoadRenderTexture(GetScreenWidth(), GetScreenHeight());

Font font = Utils.MakeFont("./resources/fonts/NotoSans.ttf", FONT_SIZE);

Vector2 centreAt = new Vector2(0f, 0f);
Camera2D camera = new Camera2D(offset: Vector2.Zero, target: Vector2.Zero, rotation: 0.0f, zoom: 1.0f);

Vector2 equationCompileExtent = MeasureTextEx(font, "Compile", FONT_SIZE, 0.0f);
RL.Rectangle equationCompileRect = new RL.Rectangle(screenWidth - 10.0f - 10.0f - equationCompileExtent.X, 10.0f, equationCompileExtent.X + 10.0f, 30.0f);
RL.Rectangle equationTextRect = new RL.Rectangle(10.0f, 10.0f, 300.0f, 30.0f);

string equationText = "y = -4x * x + 2";
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

    float arrowFactor = 0.01f;
    float arrows = IsKeyDown(KeyboardKey.Up) - IsKeyDown(KeyboardKey.Down);
    zoomModifier -= arrows * arrowFactor;

    camera.Zoom += zoomModifier;
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

void DrawAxis()
{
    DrawLine(axisX_Start.X, axisX_Start.Y, axisX_End.X, axisX_End.Y, RL.Color.Black);
    DrawLine(axisY_Start.X, axisY_Start.Y, axisY_End.X, axisY_End.Y, RL.Color.Black);

    var (xRange, yRange) = GetAxisRange();

    string xLo = xRange[0].ToString("N1");
    string xHi = xRange[1].ToString("N1");

    string yLo = yRange[0].ToString("N1");
    string yHi = yRange[1].ToString("N1");

    DrawText(xLo, axisX_Start.X + 5, axisX_Start.Y + 5, 12, RL.Color.Black);
    DrawText(xHi, axisX_End.X - 20, axisX_End.Y + 5, 12, RL.Color.Black);

    DrawText(yHi, axisY_Start.X - 25, axisY_Start.Y + 5, 12, RL.Color.Black);
    DrawText(yLo, axisY_End.X - 25, axisY_End.Y -15, 12, RL.Color.Black);
}

void DrawGrid()
{
    float s = camera.Zoom;

    var (xRange, yRange) = GetAxisRange();

    double OoM = Math.Pow(10.0, Math.Round(Math.Log((double)s * 32.0) / Math.Log(10.0)));
    double scale = s / OoM;
    
    var l = (GetScreenHeight() / (2f * s)) * (centreAt.X - screenAspect * s);
    var r = (GetScreenHeight() / (2f * s)) * (centreAt.X + screenAspect * s);

    int xLoIdx = (int)Math.Ceiling(l * scale);
    int xHiIdx = (int)Math.Ceiling(r * scale);

    for (int xIdx = xLoIdx; xIdx < xHiIdx; xIdx++)
    {
        float xAt = (float)(-l + (xIdx /scale));
        Vector2 from = new Vector2(xAt, 0f);
        Vector2 to   = new Vector2(xAt, GetScreenHeight());

        float thick = 1f;
        RL.Color color = (xIdx % 10) == 0 ? RL.Color.DarkGray : RL.Color.LightGray;
        if (xIdx == 0)
        {
            thick = 2f;
            color = RL.Color.Black;
        }

        DrawLineEx(from, to, thick, color);
    }

    var b = (GetScreenHeight() / (2f * s)) * (centreAt.Y - s);
    var t = (GetScreenHeight() / (2f * s)) * (centreAt.Y + s);

    int yLoIdx = (int)Math.Ceiling(b * scale);
    int yHiIdx = (int)Math.Ceiling(t * scale);

    // Console.WriteLine($"s:{s}   OoM:{OoM}   scale=OoM/s:{scale:F2}   ({xLoIdx}, {xHiIdx})    ({l:F2}, {r:F2})");

    for (int yIdx = yLoIdx; yIdx < yHiIdx; yIdx++)
    {
        float yAt = GetScreenHeight() - (float)(-b + (yIdx /scale));
        Vector2 from = new Vector2(0f, yAt);
        Vector2 to   = new Vector2(GetScreenWidth(), yAt);

        float thick = 1f;
        RL.Color color = (yIdx % 8) == 0 ? RL.Color.DarkGray : RL.Color.LightGray;
        if (yIdx == 0)
        {
            thick = 2f;
            color = RL.Color.Black;
        }

        DrawLineEx(from, to, thick, color);
    }

    if (IsKeyPressed(KeyboardKey.RightShift))
    {
        Debugger.Break();
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