public enum Tok
{
    LEFT_PAREN,
    RIGHT_PAREN,
    ASSIGN,
    PLUS,
    MINUS,
    ASTERISK,
    SLASH,
    VAR,
    NUMBER,
    EOF,
    TOK_COUNT,
}

public enum Precedence
{
    LOWEST,
    ASSIGNMENT,
    SUM,
    PRODUCT,
    EXPONENT,
    PREFIX,
    POSTFIX,
    CALL,
}


public class Token
{
    public Tok Type;
    public string Text;

    public Token(Tok type, string text)
    {
        Type = type;
        Text = text;
    }
}

public class Lexer
{
    public int Index;
    public string Text;
    public Dictionary<char, Tok> Punctuators = new Dictionary<char, Tok>();

    public static string StringFromTok(Tok tok)
    {
        switch(tok)
        {
            case Tok.LEFT_PAREN:  { return "LEFT_PAREN";  }
            case Tok.RIGHT_PAREN: { return "RIGHT_PAREN"; }
            case Tok.ASSIGN:      { return "ASSIGN";      }
            case Tok.PLUS:        { return "PLUS";        }
            case Tok.MINUS:       { return "MINUS";       }
            case Tok.ASTERISK:    { return "ASTERISK";    }
            case Tok.SLASH:       { return "SLASH";       }
            case Tok.VAR:         { return "VAR";         }
            case Tok.NUMBER:      { return "NUMBER";      }
            case Tok.EOF:         { return "EOF";         }
            case Tok.TOK_COUNT:   { return "TOK_COUNT";   }
            default:              { return "UNKNOWN";     }
        }
    }

    public static char? CharFromTok(Tok tok)
    {
        switch(tok)
        {
            case Tok.LEFT_PAREN:  { return '(';  }
            case Tok.RIGHT_PAREN: { return ')';  }
            case Tok.ASSIGN:      { return '=';  }
            case Tok.PLUS:        { return '+';  }
            case Tok.MINUS:       { return '-';  }
            case Tok.ASTERISK:    { return '*';  }
            case Tok.SLASH:       { return '/';  }
            default:              { return null; }
        }
    }

    public Lexer(string text)
    {
        Text = text;

        for (int i = 0; i < (int)Tok.TOK_COUNT; i++)
        {
            char? punctuator = CharFromTok((Tok)i);

            if (punctuator.HasValue)
            {
                Punctuators.Add(punctuator.Value, (Tok)i);
            }
        }
    }

    public Token Next()
    {
        Token result = new Token(Tok.EOF, "");

        while(Index < Text.Length)
        {
            char c = Text[Index++];

            if (Punctuators.ContainsKey(c))
            {
                result = new Token(Punctuators[c], $"{c}");
                break;
            }
            else if (Char.IsLetter(c))
            {
                int start = Index - 1;

                while(Index < Text.Length)
                {
                    if (!Char.IsLetter(Text[Index]))
                    {
                        break;
                    }
                    else
                    {
                        Index++;
                    }
                }

                string name = Text.Substring(start, Index - start);
                result = new Token(Tok.VAR, name);
                break;
            }
            else if (Char.IsAsciiDigit(c) || c == '.')
            {
                //
                // assumes valid number e.g. single decimal point
                //

                int start = Index - 1;

                while(Index < Text.Length)
                {
                    if (!Char.IsAsciiDigit(Text[Index]) && c != '.')
                    {
                        break;
                    }
                    else
                    {
                        Index++;
                    }
                }

                string name = Text.Substring(start, Index - start);
                result = new Token(Tok.NUMBER, name);
                break;
            }
        }

        return result;
    }
}