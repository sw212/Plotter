public class Expr
{
    public virtual string Print(string s)
    {
        s += "□";
        return s;
    }
}

public class VarExpr : Expr
{
    public string Name;

    public VarExpr(string name)
    {
        Name = name;
    }

    public override string Print(string s)
    {
        s += Name;
        return s;
    }
}

public class NameExpr : Expr
{
    public string Name;

    public NameExpr(string name)
    {
        Name = name;
    }

    public override string Print(string s)
    {
        s += Name;
        return s;
    }
}

public class NumberExpr : Expr
{
    public string Value;

    public NumberExpr(string value)
    {
        Value = value;
    }

    public override string Print(string s)
    {
        s += Value;
        return s;
    }
}

public class AssignExpr : Expr
{
    public string Name;
    public Expr Right;

    public AssignExpr(string name, Expr right)
    {
        Name = name;
        Right = right;
    }

    public override string Print(string s)
    {
        s += $"({Name} = ";
        s  = Right.Print(s);
        s += ")";
        return s;
    }
}

public class PrefixExpr : Expr
{
    public Tok Operator;
    public Expr Right;

    public PrefixExpr(Tok op, Expr right)
    {
        Operator = op;
        Right = right;
    }

    public override string Print(string s)
    {
        s += $"{Lexer.CharFromTok(Operator)}";
        s  = Right.Print(s);
        s += ")";
        return s;
    }
}

public class OperatorExpr : Expr
{
    public Expr Left;
    public Tok Operator;
    public Expr Right;

    public OperatorExpr(Expr left, Tok op, Expr right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }

    public override string Print(string s)
    {
        s += "(";
        s  = Left.Print(s);
        s += $" ${Lexer.CharFromTok(Operator)} ";
        s  = Right.Print(s);
        s += ")";
        return s;
    }
}

public class CallExpr: Expr
{
    public string Name;
    public List<Expr> Arguments;

    public CallExpr(string name, List<Expr> arguments)
    {
        Name = name;
        Arguments = arguments;
    }

    public override string Print(string s)
    {
        s += $"{Name}(";
        for (int i = 0; i < Arguments.Count; i++)
        {
            var arg = Arguments[i];
            s = arg.Print(s);
            
            if (i != (Arguments.Count - 1))
            {
                s += ", ";
            }
        }
        s += ")";
        return s;
    }
}