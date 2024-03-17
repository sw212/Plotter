
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

function isNumber(char: string)
{
    const c = char.charCodeAt(0);
    return (c >= '0'.charCodeAt(0) && c <= '9'.charCodeAt(0));
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
    punctuators: Map<string, Tok>;

    constructor(text)
    {
        this.index = 0;
        this.text = text;
        this.punctuators = new Map<string, Tok>;

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

        const isNameChar = (char) => {
            return isLetter(char) || isNumber(char)
        }

        while(this.index < this.text.length)
        {
            const char = this.text[this.index++];

            if (this.punctuators.has(char))
            {
                result = new Token(this.punctuators.get(char)!, char);
                break;
            }
            else if (isNameChar(char))
            {
                const start = this.index - 1;

                while (this.index < this.text.length)
                {
                    if (!isNameChar(this.text[this.index]))
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

    getInfixPrecedence()
    {
        const tok = this.peek(0).type;

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

        while(precedence < this.getInfixPrecedence())
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

        case Tok.LEFT_PAREN:
        {
            const result = parser.parseExpression();
            parser.consume(Tok.RIGHT_PAREN);
            return result;
        }

        case Tok.MINUS:
        {
            const right = parser.parseExpression(Precedence.PREFIX);
            const result = new PrefixExpr(token.type, right);
            return result;
        }
    }

    return new Expr();
}

function parseInfix(parser: Parser, left: Expr, token: Token)
{
    switch (token.type)
    {
        // -----[ arithmetic ]----- //
        case Tok.PLUS:
        case Tok.MINUS:
        {
            const right = parser.parseExpression(Precedence.SUM);
            const result = new OperatorExpr(left, token.type, right);
            return result;
        }

        case Tok.ASTERISK:
        case Tok.SLASH:
        {
            const right = parser.parseExpression(Precedence.PRODUCT);
            const result = new OperatorExpr(left, token.type, right);
            return result;
        }

        // -----[ assign ]----- //
        case Tok.ASSIGN:
        {
            if (!(left instanceof NameExpr))
            {
                throw `[ ERROR ] LHS of assignment must be a name expression`;
            }

            const right = parser.parseExpression(Precedence.ASSIGNMENT - 1);
            const name = left.name;
            const result = new AssignExpr(name, right);
            return result;
        }
    }

    return new Expr();
}

class Expr
{
    print(s: string): string
    {
        s += "☐";
        return s;
    }
}

class NameExpr extends Expr
{
    name: string;

    constructor(name: string)
    {
        super();
        this.name = name;
    }

    print(s: string): string
    {
        s += this.name;
        return s;
    }
}

class AssignExpr extends Expr
{
    name: string;
    right: Expr;

    constructor(name: string, right: Expr)
    {
        super();
        this.name = name;
        this.right = right;
    }

    print(s: string): string
    {
        s += `(${this.name} = `
        s  = this.right.print(s);
        s += ")";
        return s;
    }
}

class PrefixExpr extends Expr
{
    operator: Tok;
    right: Expr;

    constructor(operator: Tok, right: Expr)
    {
        super();
        this.operator = operator;
        this.right = right;
    }

    print(s: string): string
    {
        s += `(${charFromTok(this.operator)}`;
        s  = this.right.print(s);
        s += ")";
        return s;
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

    print(s: string): string
    {
        s += "("
        s  = this.left.print(s);
        s += ` ${charFromTok(this.operator)} `;
        s  = this.right.print(s); 
        s += ")";
        return s;
    }
}

// -------[ TEST ]------- //
const assert = require("node:assert/strict");

function test(source: string, expected: string)
{
    const lexer = new Lexer(source);
    const tokens: Array<Token> = [];

    while (true)
    {
        const token = lexer.next();
        tokens.push(token);

        if (token.type === Tok.EOF)
        {
            break;
        }
    }

    const parser = new Parser(tokens);
    const result = parser.parseExpression();
    const LHS = result.print("");
    
    if (LHS !== expected)
    {
        debugger;
    }

    assert.strictEqual(LHS, expected);
}

test("1+2"  , "(1 + 2)")

test("1 + 2", "(1 + 2)")
test("a + 4", "(a + 4)")
test("a + b", "(a + b)")

test("-a + b", "((-a) + b)")
test("-a = b", "((-a) = b)")
test("-a = -b", "((-a) = (-b))")
test("(-a) = -b", "((-a) = (-b))")
test("(-a) = -b", "(((-a)) = (-b))")
