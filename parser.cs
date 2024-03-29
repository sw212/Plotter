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
            //
            // for cases such as "4x", we must peek becuase otherwise we 
            // would consume the "x". This would be bad if there were further
            // terms that happen to form a statement that included such "x"
            //
            token = Peek(0);

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

            case Tok.VAR:
            case Tok.ASTERISK:
            case Tok.SLASH:
            {
                return Precedence.PRODUCT;
            }

            case Tok.CARET:
            {
                return Precedence.EXPONENT;
            }

            case Tok.ASSIGN:
            {
                return Precedence.ASSIGNMENT;
            }

            case Tok.LEFT_PAREN:
            {
                return Precedence.CALL;
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

            case Tok.NUMBER:
            {
                result = new NumberExpr(token.Text);
            } break;

            case Tok.LEFT_PAREN:
            {
                result = ParseExpression(Precedence.LOWEST);
                Consume(Tok.RIGHT_PAREN);
            } break;

            case Tok.FUNCTION:
            {
                result = new NameExpr(token.Text);
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
                Consume();
                Expr right = ParseExpression(Precedence.SUM);
                result = new OperatorExpr(left, token.Type, right);
            } break;

            case Tok.VAR:
            {
                Expr right = ParseExpression(Precedence.PRODUCT);
                result = new OperatorExpr(left, Tok.ASTERISK, right);
            } break;

            case Tok.ASTERISK:
            case Tok.SLASH:
            {
                Consume();
                Expr right = ParseExpression(Precedence.PRODUCT);
                result = new OperatorExpr(left, token.Type, right);
            } break;

            case Tok.CARET:
            {
                Consume();
                Expr right = ParseExpression(Precedence.EXPONENT);
                result = new OperatorExpr(left, token.Type, right);
            } break;

            case Tok.ASSIGN:
            {
                Consume();
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

            case Tok.LEFT_PAREN:
            {
                Consume();

                if (left is NameExpr nameExpr)
                {
                    List<Expr> arguments = new List<Expr>();

                    if (!Match(Tok.RIGHT_PAREN))
                    {
                        do
                        {
                            Expr arg = ParseExpression(Precedence.LOWEST);
                            arguments.Add(arg);
                        } while(Match(Tok.COMMA));

                        Consume(Tok.RIGHT_PAREN);
                    }

                    result = new CallExpr(nameExpr.Name, arguments);
                }
                else
                {
                    throw new Exception("LHS of call must be a name expression");
                }
            } break;
        }

        return result;
    }
}
