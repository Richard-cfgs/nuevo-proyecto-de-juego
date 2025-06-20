using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Formats.Tar;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using Godot;
using Godot.NativeInterop;

namespace PixelWallE.Core
{
	public class Lexer
	{
		private List<(string, int)> Errors;
		public static readonly Dictionary<string, string> _keywords = new Dictionary<string, string>
		{
			//Instrucciones
			{ "Spawn", "KEYWORD" },
			{ "Color", "KEYWORD" },
			{ "Size", "KEYWORD" },
			{ "DrawLine", "KEYWORD" },
			{ "DrawCircle", "KEYWORD" },
			{ "DrawRectangle", "KEYWORD" },
			{ "Fill", "KEYWORD" },

			//funciones
			{ "GetActualX", "FUNCTION" },
			{ "GetActualY", "FUNCTION" },
			{ "GetCanvasSize", "FUNCTION" },
			{ "GetColorCount", "FUNCTION" },
			{ "IsBrushColor", "FUNCTION" },
			{ "IsBrushSize", "FUNCTION" },
			{ "IsCanvasColor", "FUNCTION" },

			//Control de flujo
			{ "GoTo", "CONTROL_FLOW" },

			//Colores
			{ "Red", "COLOR" },
			{ "Blue", "COLOR" },
			{ "Green", "COLOR" },
			{ "Yellow", "COLOR" },
			{ "Orange", "COLOR" },
			{ "Purple", "COLOR" },
			{ "Black", "COLOR" },
			{ "White", "COLOR" },
			{ "Transparent", "COLOR" },
		};
		public Lexer(string input)
		{
			List<Token> tokens = new();
			Errors = new();
			int position = 0;
			int line = 1;
			var v = Tokenize(input, tokens, position, line);

			new Parser(v, Errors);
		}
		public List<Token> Tokenize(string input, List<Token> tokens, int position, int line)
		{
			while (position < input.Length)
			{
				char currentChar = input[position];

				// Ignorar espacios
				if (currentChar == ' ') { position++; continue; }

				// Contar lineas
				else if (currentChar == '\n') { line++; position++; continue; }

				// Identificar números
				if ((currentChar >= '0' && currentChar <= '9') || (currentChar == '-' && position + 1 < input.Length && tokens[^1].Type != "NUMBER" && tokens[^1].Type != "VARIABLE" && tokens[^1].Value != ")" && input[position + 1] >= '0' && input[position + 1] <= '9'))
				{
					string number = "";
					if (currentChar < '0' || currentChar > '9')
					{
						number += input[position];
						position++;
						currentChar = input[position];
					}

					while (position < input.Length && currentChar >= '0' && currentChar <= '9')
					{
						number += input[position];
						position++;
						if (position < input.Length) currentChar = input[position];
						else break;
					}
					tokens.Add(new Token("NUMBER", number, line));
					continue;
				}

				// Identificar palabras clave ,variable o funcion
				if ((currentChar >= 'A' && currentChar <= 'Z') || (currentChar >= 'a' && currentChar <= 'z'))
				{
					string word = "";
					while (position < input.Length && ((currentChar >= 'A' && currentChar <= 'Z') || (currentChar >= 'a' && currentChar <= 'z') || (currentChar >= '0' && currentChar <= '9') || currentChar == '_'))
					{
						word += input[position];
						position++;
						if (position < input.Length) currentChar = input[position];
					}
					if (_keywords.TryGetValue(word, out string tokenType)) tokens.Add(new Token(tokenType, word, line));
					else tokens.Add(new Token("VARIABLE", word, line));
					continue;
				}

				// Identificar símbolos
				if (currentChar == '(' || currentChar == ')' || currentChar == ',' || currentChar == '[' || currentChar == ']')
				{
					tokens.Add(new Token("SYMBOL", currentChar.ToString(), line));
					position++;
					continue;
				}
				// Identificar operadores aritmeticas
				if (currentChar == '+' || currentChar == '-' || currentChar == '*' || currentChar == '/' || currentChar == '%')
				{
					//identificar potencia
					if ((position + 1 < input.Length) && currentChar == '*' && input[position + 1] == '*') { tokens.Add(new Token("OPERATION", "**", line)); position++; }
					else tokens.Add(new Token("OPERATION", currentChar.ToString(), line));
					position++;
					continue;
				}
				//Identificar operaciones booleanas dobles
				if (position + 1 < input.Length)
				{
					string opera_2 = input[position].ToString() + input[position + 1].ToString();
					if (opera_2 == "==" || opera_2 == "!=" || opera_2 == ">=" || opera_2 == "<=" || opera_2 == "||" || opera_2 == "&&")
					{
						tokens.Add(new Token("BOOLOPERATION", opera_2, line));
						position += 2;
						continue;
					}
				}

				//Asignar Variables
				if (position + 1 < input.Length && currentChar == '<' && input[position + 1] == '_')
				{
					tokens.Add(new Token("ASSIGMENT", "<_", line));
					position += 2;
					continue;
				}
				//Identificar operaciones booleanas dobles simples
				if (currentChar == '<' || currentChar == '>')
				{
					tokens.Add(new Token("BOOLOPERATION", currentChar.ToString(), line));
					position++;
					continue;
				}
				// Manejar errores
				Errors.Add(($"LexError (Línea {line}): Carácter inválido '{currentChar}'", line));
				position++;
			}
			return tokens;
		}

	}
}
