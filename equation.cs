public static class Equation
{
    public static string Compile(Expr equation)
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

            // get uv deltas for neighbouring pixels
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
            string result = $"({LHS} {Lexer.CharFromTok(operatorExpr.Operator)} {RHS})";
            return result;
        }

        else
        {
            return "";
        }
    }
}