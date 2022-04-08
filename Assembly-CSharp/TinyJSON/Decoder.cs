using System;
using System.IO;
using System.Text;

namespace TinyJSON;

public sealed class Decoder : IDisposable
{
	private enum Token
	{
		None = 0,
		OpenBrace = 1,
		CloseBrace = 2,
		OpenBracket = 3,
		CloseBracket = 4,
		Colon = 5,
		Comma = 6,
		String = 7,
		Number = 8,
		True = 9,
		False = 10,
		Null = 11
	}

	private const string whiteSpace = " \t\n\r";

	private const string wordBreak = " \t\n\r{}[],:\"";

	private StringReader json;

	private char PeekChar
	{
		get
		{
			int num = json.Peek();
			if (num != -1)
			{
				return Convert.ToChar(num);
			}
			return '\0';
		}
	}

	private char NextChar => Convert.ToChar(json.Read());

	private string NextWord
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			while (" \t\n\r{}[],:\"".IndexOf(PeekChar) == -1)
			{
				stringBuilder.Append(NextChar);
				if (json.Peek() == -1)
				{
					break;
				}
			}
			return stringBuilder.ToString();
		}
	}

	private Token NextToken
	{
		get
		{
			ConsumeWhiteSpace();
			if (json.Peek() == -1)
			{
				return Token.None;
			}
			switch (PeekChar)
			{
			case '{':
				return Token.OpenBrace;
			case '}':
				json.Read();
				return Token.CloseBrace;
			case '[':
				return Token.OpenBracket;
			case ']':
				json.Read();
				return Token.CloseBracket;
			case ',':
				json.Read();
				return Token.Comma;
			case '"':
				return Token.String;
			case ':':
				return Token.Colon;
			case '-':
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				return Token.Number;
			default:
				return NextWord switch
				{
					"false" => Token.False, 
					"true" => Token.True, 
					"null" => Token.Null, 
					_ => Token.None, 
				};
			}
		}
	}

	private Decoder(string jsonString)
	{
		json = new StringReader(jsonString);
	}

	public static Variant Decode(string jsonString)
	{
		using Decoder decoder = new Decoder(jsonString);
		return decoder.DecodeValue();
	}

	public void Dispose()
	{
		json.Dispose();
		json = null;
	}

	private ProxyObject DecodeObject()
	{
		ProxyObject proxyObject = new ProxyObject();
		json.Read();
		while (true)
		{
			switch (NextToken)
			{
			case Token.Comma:
				continue;
			case Token.None:
				return null;
			case Token.CloseBrace:
				return proxyObject;
			}
			string text = DecodeString();
			if (text == null)
			{
				return null;
			}
			if (NextToken != Token.Colon)
			{
				return null;
			}
			json.Read();
			proxyObject.Add(text, DecodeValue());
		}
	}

	private ProxyArray DecodeArray()
	{
		ProxyArray proxyArray = new ProxyArray();
		json.Read();
		bool flag = true;
		while (flag)
		{
			Token nextToken = NextToken;
			switch (nextToken)
			{
			case Token.None:
				return null;
			case Token.CloseBracket:
				flag = false;
				break;
			default:
				proxyArray.Add(DecodeByToken(nextToken));
				break;
			case Token.Comma:
				break;
			}
		}
		return proxyArray;
	}

	private Variant DecodeValue()
	{
		Token nextToken = NextToken;
		return DecodeByToken(nextToken);
	}

	private Variant DecodeByToken(Token token)
	{
		return token switch
		{
			Token.String => DecodeString(), 
			Token.Number => DecodeNumber(), 
			Token.OpenBrace => DecodeObject(), 
			Token.OpenBracket => DecodeArray(), 
			Token.True => new ProxyBoolean(value: true), 
			Token.False => new ProxyBoolean(value: false), 
			Token.Null => null, 
			_ => null, 
		};
	}

	private Variant DecodeString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		json.Read();
		bool flag = true;
		while (flag)
		{
			if (json.Peek() == -1)
			{
				flag = false;
				break;
			}
			char nextChar = NextChar;
			switch (nextChar)
			{
			case '"':
				flag = false;
				break;
			case '\\':
				if (json.Peek() == -1)
				{
					flag = false;
					break;
				}
				nextChar = NextChar;
				switch (nextChar)
				{
				case '"':
				case '/':
				case '\\':
					stringBuilder.Append(nextChar);
					break;
				case 'b':
					stringBuilder.Append('\b');
					break;
				case 'f':
					stringBuilder.Append('\f');
					break;
				case 'n':
					stringBuilder.Append('\n');
					break;
				case 'r':
					stringBuilder.Append('\r');
					break;
				case 't':
					stringBuilder.Append('\t');
					break;
				case 'u':
				{
					StringBuilder stringBuilder2 = new StringBuilder();
					for (int i = 0; i < 4; i++)
					{
						stringBuilder2.Append(NextChar);
					}
					stringBuilder.Append((char)Convert.ToInt32(stringBuilder2.ToString(), 16));
					break;
				}
				}
				break;
			default:
				stringBuilder.Append(nextChar);
				break;
			}
		}
		return new ProxyString(stringBuilder.ToString());
	}

	private Variant DecodeNumber()
	{
		return new ProxyNumber(NextWord);
	}

	private void ConsumeWhiteSpace()
	{
		while (" \t\n\r".IndexOf(PeekChar) != -1)
		{
			json.Read();
			if (json.Peek() == -1)
			{
				break;
			}
		}
	}
}
