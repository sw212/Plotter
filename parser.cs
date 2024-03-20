using System.Data;

public class Parser
{
    public List<Token> Tokens = new List<Token>();
    public Lexer Lexer;

    public Parser(Lexer lexer)
    {
        Lexer = lexer;
    }

    public Expr ParseExpression(Precedence precedence)
    {
        Token token = Consume();

        Expr left = ParsePrefix(token);

        while (precedence < GetInfixPrecedence())
        {
            token = Consume();

            left = ParseInfix(left, token);
        }

        return left;
    }

    public Token Peek(uint distance)
    {
        while (distance >= Tokens.Count)
        {
            Tokens.Add(Lexer.Next());
        }

        Token result = Tokens[(int)distance];
        return result;
    }

    public Token Consume(Tok expected)
    {
        Token token = Peek(0);

        if (token.Type != expected)
        {
            throw new Exception($"expected ${Lexer.StringFromTok(expected)}, found ${Lexer.StringFromTok(token.Type)}");
        }

        Tokens.Remove(token);
        return token;
    }

    public Token Consume()
    {
        Token token = Peek(0);
        Tokens.Remove(token);
        return token;
    }

    public bool Match(Tok expected)
    {
        Token token = Peek(0);

        if (token.Type != expected)
        {
            return false;
        }
        else
        {
            Consume();
            return true;
        }
    }

    public Precedence GetInfixPrecedence()
    {
        Tok tok = Peek(0).Type;

        switch(tok)
        {
            case Tok.PLUS:
            case Tok.MINUS:
            {
                return Precedence.SUM;
            }

            case Tok.ASTERISK:
            case Tok.SLASH:
            {
                return Precedence.PRODUCT;
            }

            case Tok.ASSIGN:
            {
                return Precedence.ASSIGNMENT;
            }

            default:
            {
                return Precedence.LOWEST;
            }
        }
    }

    public Expr ParsePrefix(Token token)
    {
        Expr result = new Expr();

        switch(token.Type)
        {
            case Tok.VAR:
            {
                result = new VarExpr(token.Text);
            } break;

            case Tok.LEFT_PAREN:
            {
                result = ParseExpression(Precedence.LOWEST);
                Consume(Tok.RIGHT_PAREN);
            } break;

            case Tok.MINUS:
            {
                Expr right = ParseExpression(Precedence.PREFIX);
                result = new PrefixExpr(token.Type, right);
            } break;
        }

        return result;
    }

    public Expr ParseInfix(Expr left, Token token)
    {
        Expr result = new Expr();

        switch(token.Type)
        {
            case Tok.PLUS:
            case Tok.MINUS:
            {
                Expr right = ParseExpression(Precedence.SUM);
                result = new OperatorExpr(left, token.Type, right);
            } break;

            case Tok.ASTERISK:
            case Tok.SLASH:
            {
                Expr right = ParseExpression(Precedence.PRODUCT);
                result = new OperatorExpr(left, token.Type, right);
            } break;

            case Tok.ASSIGN:
            {
                if (left is VarExpr l)
                {
                    Expr right = ParseExpression(Precedence.ASSIGNMENT - 1);
                    string name = l.Name;
                    result = new AssignExpr(name, right);
                }
                else
                {
                    throw new Exception("LHS of assignment must be a var expression");
                }
            } break;
        }

        return result;
    }
}

public static class Equation
{
    public static string Compile(Expr equation)
    {
        if (false)
        {}

        else if (equation is AssignExpr assignExpr)
        {
            string LHS = assignExpr.Name;
            string RHS = Compile(assignExpr.Right);
            string result = $"({LHS}) - (${RHS})";
            return result;
        }

        else if (equation is VarExpr varExpr)
        {
            return varExpr.Name;
        }

        else if (equation is PrefixExpr prefixExpr)
        {
            string RHS = Compile(prefixExpr.Right);
            string result = $"(${Lexer.CharFromTok(prefixExpr.Operator)}${RHS})";
            return result;
        }

        else if (equation is OperatorExpr operatorExpr)
        {
            string LHS = Compile(operatorExpr.Left);
            string RHS = Compile(operatorExpr.Right);
            string result = $"({LHS} ${Lexer.CharFromTok(operatorExpr.Operator)} ${RHS})";
            return result;
        }

        else
        {
            return "";
        }
    }
}