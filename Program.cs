using System.Drawing;
using System.Numerics;
using Raylib_cs;
using RL = Raylib_cs;
using static Raylib_cs.Raylib;
using System.ComponentModel;

const int screenWidth  = 800;
const int screenHeight = 450;
const float screenAspect = (float)screenWidth / (float)screenHeight;

Point axisX_Start = new Point(0, screenHeight/2);
Point   axisX_End = new Point(screenWidth, screenHeight/2);

Point axisY_Start = new Point(screenWidth/2, 0);
Point   axisY_End = new Point(screenWidth/2, screenHeight);

InitWindow(screenWidth, screenHeight, "Plotter");
Camera2D camera = new Camera2D(offset: Vector2.Zero, target: Vector2.Zero, rotation: 0.0f, zoom: 1.0f);

RenderTexture2D target = LoadRenderTexture(GetScreenWidth(), GetScreenHeight());

string source = "y = -4x";
Lexer   lexer = new Lexer(source);
Parser parser = new Parser(lexer);

Expr     equationExpr = parser.ParseExpression(Precedence.LOWEST);
string equationShader = Equation.Compile(equationExpr);
//
// when only providing a fragment shader, 
// it seems we have to either read shaders from file, or use unsafe block...
//
Shader shader;
unsafe
{
    Utf8Buffer eqnBuffer = equationShader.ToUtf8Buffer();
    shader = LoadShaderFromMemory((sbyte*)0, eqnBuffer.AsPointer());
}

float[] LinSpace(float start, float end, int n)
{
    float[] result = new float[n];

    for (int i = 0; i < n; i++)
    {
        float t = (float)i / (float)(n - 1);
        float value = start +  t * (end - start);
        result[i] = value;
    }

    return result;
}

Point ChartToScreen(Vector2 V)
{
    Point result = new Point();

    var (xRange, yRange) = GetAxisRange();

    float xDelta = V.X - xRange[0];
    float xFrac  = xDelta / (xRange[1] - xRange[0]);

    float yDelta = yRange[1] - V.Y;
    float yFrac  = yDelta / (yRange[1] - yRange[0]);

    result.X = (int)(xFrac * screenWidth);
    result.Y = (int)(yFrac * screenHeight);

    return result;
}

void HandleInput()
{
    float zoomModifier = 0.0f;

    float wheelFactor = 0.1f;
    float wheel = GetMouseWheelMove();
    zoomModifier -= wheel * wheelFactor;

    float arrowFactor = 0.001f;
    float arrows = IsKeyDown(KeyboardKey.Up) - IsKeyDown(KeyboardKey.Down);
    zoomModifier -= arrows * arrowFactor;

    camera.Zoom += zoomModifier;
}

(Vector2, Vector2) GetAxisRange()
{
    Vector2 xRange = new Vector2(-1, 1);
    Vector2 yRange = new Vector2(-1, 1);

    xRange *= camera.Zoom * screenAspect;
    yRange *= camera.Zoom;

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

void DrawEquation()
{
    int n = 500;
    var (xRange, yRange) = GetAxisRange();

    float[] xValues = LinSpace(xRange[0], xRange[1], n);

    float[] yValues = new float[n];
    for (int i = 0; i < n; i++)
    {
        float x = xValues[i];
        float y = (float)Math.Sin(x);
        yValues[i] = y;
    }

    Point[] screenPositions = new Point[n];
    for (int i = 0; i < n; i++)
    {
        Vector2 chartPos = new Vector2(xValues[i], yValues[i]);
        Point  screenPos = ChartToScreen(chartPos);

        screenPositions[i] = screenPos;
    }

    for (int i = 0; i < (n-1); i++)
    {
        Point start = screenPositions[i];
        Point   end = screenPositions[i+1];

        DrawLine(start.X, start.Y, end.X, end.Y, RL.Color.Black);
    }
}

SetTargetFPS(60);

while(!WindowShouldClose())
{
    HandleInput();

    BeginTextureMode(target);
    {
        DrawRectangle(0, 0, GetScreenWidth(), GetScreenHeight(), RL.Color.Black);
    }
    EndTextureMode();

    BeginDrawing();
    {
        ClearBackground(RL.Color.RayWhite);
        
        // DrawText("Equation", 10, 10, 20, RL.Color.Black);
        DrawAxis();
        // DrawEquation();

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