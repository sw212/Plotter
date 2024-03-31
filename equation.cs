using Raylib_cs;
using static Raylib_cs.Raylib;

public static class Equation
{
    public static void CompileEquation(ref Shader shader, string equation)
    {
        Lexer   lexer = new Lexer(equation);
        Parser parser = new Parser(lexer);

        Expr equationExpr = parser.ParseExpression(Precedence.LOWEST);

        bool validEquation = CheckEquation(equationExpr);

        if (!validEquation)
        {
            Console.WriteLine("[ ERROR ] invalid shader");
            return;
        }
        else
        {
            UnloadShader(shader);
        }

        string equationShader = Equation.GenerateShader(equationExpr);
        //
        // when only providing a fragment shader, 
        // it seems we have to either read shaders from file, or use unsafe block...
        //
        unsafe
        {
            Utf8Buffer eqnBuffer = equationShader.ToUtf8Buffer();
            shader = LoadShaderFromMemory((sbyte*)0, eqnBuffer.AsPointer());
        }
    }

    public static bool CheckEquation(Expr equation)
    {
        if (false)
        {}

        else if (equation is AssignExpr assignExpr)
        {
            string LHS = assignExpr.Name;
            if (LHS != "y")
            {
                return false;
            }
            else
            {
                return CheckEquation(assignExpr.Right);
            }
        }

        else if (equation is VarExpr varExpr)
        {
            return varExpr.Name == "x";
        }

        else if (equation is NumberExpr numberExpr)
        {
            int dotCount = numberExpr.Value.AsSpan().Count('.');
            bool result = dotCount <= 1;
            return result;
        }

        else if (equation is PrefixExpr prefixExpr)
        {
            Tok op = prefixExpr.Operator;
            Tok[] allowedOps = [Tok.PLUS, Tok.MINUS];
            bool validOp = allowedOps.Any(allowedOp => op == allowedOp);
            if (!allowedOps.Any(allowedOp => op == allowedOp))
            {
                return false;
            }
            else
            {
                return (prefixExpr.Right is NumberExpr || prefixExpr.Right is VarExpr) && CheckEquation(prefixExpr.Right);
            }
        }

        else if (equation is OperatorExpr operatorExpr)
        {
            Tok op = operatorExpr.Operator;
            Tok[] allowedOps = [Tok.PLUS, Tok.MINUS, Tok.ASTERISK, Tok.SLASH, Tok.CARET];
            bool validOp = allowedOps.Any(allowedOp => op == allowedOp);
            if (!allowedOps.Any(allowedOp => op == allowedOp))
            {
                return false;
            }
            else
            {
                return CheckEquation(operatorExpr.Left) && CheckEquation(operatorExpr.Right);
            }
        }

        else if (equation is CallExpr callExpr)
        {
            // TODO: add function dependent argument count checks & function argument checks
            return true;
        }

        else
        {
            return false;
        }
    }

    public static string GenerateShader(Expr equation)
    {
        string source = 
        """
        #version 330

        uniform vec4 axisRange; // (xLo, xHi, yLo, yHi)

        in  vec2 fragTexCoord;
        in  vec4 fragColor;
        out vec4 finalColor;

        void main()
        {
            vec2 p = fragTexCoord;

            float x = axisRange.x + (axisRange.y - axisRange.x) * p.x;
            float y = axisRange.z + (axisRange.w - axisRange.z) * (1.0 - p.y);

            // get coord deltas for neighbouring pixels
            float dx = dFdx(x);
            float dy = dFdy(y);

            float z = plot(x,y);
            vec2  z_lo = vec2(plot(x - dx, y), plot(x, y - dy));
            vec2  z_hi = vec2(plot(x + dx, y), plot(x, y + dy));

            vec2 z_delta = 0.5 * (z_hi - z_lo);
            float dist = abs(z / length(z_delta));

            float alpha = clamp(2.0 - dist, 0.0, 1.0);

            finalColor = vec4(1.0, 0.0, 0.0, alpha);
        }
        """;

        string[] parts = source.Split("void main");
        parts[1] = "void main" + parts[1];

        string compiledExpr = CompileExpr(equation);
        string compiledEqn = 
        $$"""
        float plot(float x, float y)
        {
            float z = {{compiledExpr}};
            return z;
        }

        """;

        string result = String.Join(compiledEqn, parts);

        return result;
    }

    public static string CompileExpr(Expr equation)
    {
        if (false)
        {}

        else if (equation is AssignExpr assignExpr)
        {
            string LHS = assignExpr.Name;
            string RHS = CompileExpr(assignExpr.Right);
            string result = $"({LHS}) - ({RHS})";
            return result;
        }

        else if (equation is NameExpr nameExpr)
        {
            return nameExpr.Name;
        }

        else if (equation is VarExpr varExpr)
        {
            return varExpr.Name;
        }

        else if (equation is NumberExpr numberExpr)
        {
            string result = numberExpr.Value.Contains('.') ? numberExpr.Value : numberExpr.Value + ".0";
            return result;
        }

        else if (equation is PrefixExpr prefixExpr)
        {
            string RHS = CompileExpr(prefixExpr.Right);
            string result = $"({Lexer.CharFromTok(prefixExpr.Operator)}{RHS})";
            return result;
        }

        else if (equation is OperatorExpr operatorExpr)
        {
            string LHS = CompileExpr(operatorExpr.Left);
            string RHS = CompileExpr(operatorExpr.Right);
            string result;
            if (operatorExpr.Operator == Tok.CARET)
            {
                result = $"pow({LHS}, {RHS})";
            }
            else
            {
                result = $"({LHS} {Lexer.CharFromTok(operatorExpr.Operator)} {RHS})";
            }
            return result;
        }

        else if (equation is CallExpr callExpr)
        {
            List<string> compiledArguments = new List<string>();

            for (int i = 0; i < callExpr.Arguments.Count; i++)
            {
                string arg = CompileExpr(callExpr.Arguments[i]);
                compiledArguments.Add(arg);
            }

            string arguments = string.Join(", ", compiledArguments);
            string result = $"{callExpr.Name}({arguments})";
            return result;
        }

        else
        {
            return "";
        }
    }
}