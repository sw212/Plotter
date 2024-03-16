
enum Tok
{
    LEFT_PAREN,
    RIGHT_PAREN,
    COMMA,
    ASSIGN,
    PLUS,
    MINUS,
    ASTERISK,
    SLASH,
    CARET,
    TILDE,
    BANG,
    QUESTION,
    COLON,
    NAME,
    EOF,
    TOK_COUNT,
}

enum Precedence
{
    ASSIGNMENT = 1,
    CONDITIONAL,
    SUM,
    PRODUCT,
    EXPONENT,
    PREFIX,
    POSTFIX,
    CALL,
}

function isLetter(char: string)
{
    const c = char.charCodeAt(0);
    return (c >= 97 && c <= 122) || (c >= 65 && c <= 90);
}

function charFromTok(tok: Tok)
{
    switch(tok)
    {
        case Tok.LEFT_PAREN:  { return '(' }
        case Tok.RIGHT_PAREN: { return ')' }
        case Tok.COMMA:       { return ',' }
        case Tok.ASSIGN:      { return '=' }
        case Tok.PLUS:        { return '+' }
        case Tok.MINUS:       { return '-' }
        case Tok.ASTERISK:    { return '*' }
        case Tok.SLASH:       { return '/' }
        case Tok.CARET:       { return '^' }
        case Tok.TILDE:       { return '~' }
        case Tok.BANG:        { return '!' }
        case Tok.QUESTION:    { return '?' }
        case Tok.COLON:       { return ':' }
        default:              { return null}
    }
}

function stringFromTok(tok: Tok)
{
    switch(tok)
    {
        case Tok.LEFT_PAREN:  { return 'LEFT_PAREN'  }
        case Tok.RIGHT_PAREN: { return 'RIGHT_PAREN' }
        case Tok.COMMA:       { return 'COMMA'       }
        case Tok.ASSIGN:      { return 'ASSIGN'      }
        case Tok.PLUS:        { return 'PLUS'        }
        case Tok.MINUS:       { return 'MINUS'       }
        case Tok.ASTERISK:    { return 'ASTERISK'    }
        case Tok.SLASH:       { return 'SLASH'       }
        case Tok.CARET:       { return 'CARET'       }
        case Tok.TILDE:       { return 'TILDE'       }
        case Tok.BANG:        { return 'BANG'        }
        case Tok.QUESTION:    { return 'QUESTION'    }
        case Tok.COLON:       { return 'COLON'       }
        case Tok.NAME:        { return 'NAME'        }
        case Tok.EOF:         { return 'EOF'         }
    }   
}

class Token
{
    type: Tok;
    text: string;

    constructor(token: Tok, text: string)
    {
        this.type = token;
        this.text = text;
    }
}

class Lexer
{
    index: number;
    text: string;
    punctuators: Map<String, Tok>;

    constructor(text)
    {
        this.index = 0;
        this.text = text;

        for (let i = 0; i < Tok.TOK_COUNT; i++)
        {
            const punctuator = charFromTok(i);
            if (punctuator)
            {
                this.punctuators.set(punctuator, i);
            }
        }
    }

    next()
    {
        let result = new Token(Tok.EOF, "");

        while(this.index < this.text.length)
        {
            const char = this.text[this.index++];

            if (this.punctuators.has(char))
            {
                result = new Token(this.punctuators.get(char)!, char);
                break;
            }
            else if (isLetter(char))
            {
                const start = this.index - 1;

                while (this.index < this.text.length)
                {
                    if (!isLetter(this.text[this.index]))
                    {
                        break;
                    }
                    else
                    {
                        this.index++;
                    }
                }

                const name = this.text.slice(start, this.index);
                result = new Token(Tok.NAME, name);
                break;
            }
        }
        
        return result;
    }
}

class Parser
{
    tokenAt: number;
    tokens: Array<Token>;
    seenTokens: Array<Token>;

    constructor(tokens: Array<Token>)
    {
        this.tokenAt = 0;
        this.tokens = tokens;
        this.seenTokens = [];
    }

    getPrecedence()
    {
        const tok = this.peek(0).type;

        switch(tok)
        {
            case Tok.PLUS:
            {
                return Precedence.SUM;
            }
        }

        return 0;
    }

    next()
    {
        if (this.tokenAt < this.tokens.length)
        {
            return this.tokens[this.tokenAt++];
        }
        else
        {
            return new Token(Tok.EOF, "");
        }
    }

    peek(distance: number)
    {
        //
        // unsure about what distance is here?
        //

        while (distance >= this.seenTokens.length)
        {
            const token = this.next();
            this.seenTokens.push(token);
        }

        const result = this.seenTokens[distance];
        return result;
    }

    consume(expected?: Tok): Token
    {
        const token = this.peek(0);

        if (expected && token.type != expected)
        {
            throw `[ ERROR ] expected ${stringFromTok(expected)}, found ${stringFromTok(token.type)}`;
        }

        const result = this.seenTokens.shift()!;
        return result;
    }

    match(expected: Tok)
    {
        const token = this.peek(0);

        if (token.type != expected)
        {
            return false;
        }
        else
        {
            this.consume();
            return true;
        }
    }

    parseExpression(precedence: number = 0)
    {
        let token = this.consume();

        let left = parsePrefix(this, token);

        while(precedence < this.getPrecedence())
        {
            token = this.consume();
            
            left = parseInfix(this, left, token);
        }

        return left;
    }
}

function parsePrefix(parser: Parser, token: Token): Expr
{
    switch (token.type)
    {
        case Tok.NAME:
        {
            const result = new NameExpr(token.text);
            return result;
        }
    }

    return new Expr();
}

function parseInfix(parser: Parser, left: Expr, token: Token)
{
    switch (token.type)
    {
        case Tok.PLUS:
        {
            const right = parser.parseExpression(Precedence.SUM);
            const result = new OperatorExpr(left, token.type, right);
            return result;
        }
    }

    return new Expr();
}

class Expr
{

}

class NameExpr extends Expr
{
    name: string;

    constructor(name: string)
    {
        super();
        this.name = name;
    }
}

class OperatorExpr extends Expr
{
    left: Expr;
    operator: Tok;
    right: Expr;

    constructor(left: Expr, operator: Tok, right: Expr)
    {
        super();
        this.left = left;
        this.operator = operator;
        this.right = right;
    }
}