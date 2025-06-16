using System;
using System.Collections.Generic;
using System.Data;
using Godot;
using Godot.NativeInterop;
using System.Linq;

using PixelWallE.Core;

namespace PixelWallE.Core
{
	public class Parser
	{
		private readonly List<Token> tokens;
		private List<(string, int)> Errors;
		private Dictionary<string, int> Labels;
		private int position;
		private readonly Token EOFToken = new Token("EOF", "", -1);

		public Parser(List<Token> tokens, List<(string, int)> ErrorsLex)
		{
			this.tokens = tokens;
			Errors = new();
			Labels = new();
			position = 0;
			var statements = Parse();

			PrintCombinedErrors(ErrorsLex, Errors);

			if (ErrorsLex.Count == 0 && Errors.Count == 0) new Interprete(statements, Labels);
		}




		//INICIO DE METODOS PARA IMPRIMIR




		public void PrintCombinedErrors(List<(string Message, int Line)> lexerErrors,
						  List<(string Message, int Line)> parserErrors)
		{
			GD.PrintRich("[color=#ff5555]=== ERRORES ===[/color]");

			var shownMessages = new HashSet<string>();
			var printedLines = new HashSet<int>();

			// Función para limpiar mensajes como "(2) - ..."
			string CleanMessage(string message)
			{
				if (message.StartsWith("(") && char.IsDigit(message[1]))
				{
					int endIdx = message.IndexOf(')');
					if (endIdx > 0)
					{
						message = message.Substring(endIdx + 1).Trim();
						if (message.StartsWith("-"))
							message = message.Substring(1).Trim();
					}
				}
				return message;
			}

			// Unir y ordenar errores por línea
			var allErrors = lexerErrors.Select(e => ("Lexer", e.Message, e.Line))
				.Concat(parserErrors.Select(e => ("Parser", e.Message, e.Line)))
				.Select(e => (Source: e.Item1, Message: CleanMessage(e.Message), Line: e.Line))
				.Distinct() // Eliminar errores duplicados exactos
				.OrderBy(e => e.Line)
				.ThenBy(e => e.Source) // Opcional: primero Lexer o Parser si están en la misma línea
				.ToList();

			int currentLine = -1;

			foreach (var error in allErrors)
			{
				if (error.Line != currentLine)
				{
					GD.PrintRich($"[color=#ffff55]Línea {error.Line}:[/color]");
					currentLine = error.Line;
				}

				string uniqueKey = $"{error.Source}:{error.Line}:{error.Message}";
				if (!shownMessages.Contains(uniqueKey))
				{
					GD.Print($"- [{error.Source}] {error.Message}");
					shownMessages.Add(uniqueKey);
				}
			}

			if (allErrors.Count == 0)
			{
				GD.PrintRich("[color=#55ff55]No se encontraron errores.[/color]");
			}
		}










		//FIN DE METODOS PARA IMPRIMIR






		public List<Instructions.Statement> Parse()
		{
			List<Instructions.Statement> statements = new();

			Token current = Peek();
			if (current.Type == "EOF")
			{
				Errors.Add(("Error: No hay tokens para analizar", 1));
				return statements;
			}

			if (current.Value == "Spawn")
				statements.Add(ParseStatement(isFirstStatement: true));
			else
				Errors.Add(($"Línea {current.Line}: el código siempre debe comenzar con Spawn", current.Line));

			while (!IsAtEnd())
			{
				Token bEOFre = Peek();
				if (bEOFre.Type == "EOF") break;

				Instructions.Statement stmt = ParseStatement();
				Token after = Peek();

				if (stmt != null)
				{
					//sino no llegue al final de la linea revisar si hay dos instrucciones para dar el error.
					if (after.Type != "EOF" && bEOFre.Line == after.Line)
					{
						while (!IsAtEnd() && Peek().Line == bEOFre.Line)
						{
							Token token = Advance();
							if (token.Type == "KEYWORD" || token.Type == "ASSIGMENT" || token.Type == "CONTROL_FLOW")
							{
								Errors.Add(($"Línea {bEOFre.Line}: solo puede haber una instrucción por línea.", bEOFre.Line));
								break;
							}
						}
					}
					statements.Add(stmt);
				}

				while (!IsAtEnd() && Peek().Line == bEOFre.Line)
					Advance();
			}
			return statements;
		}

		private Instructions.Statement ParseStatement(bool isFirstStatement = false)
		{
			Token current = Peek();
			if (current.Type == "EOF")
			{
				Errors.Add(("Fin de archivo inesperado", tokens[^1].Line));
				return null;
			}

			if (current.Type == "KEYWORD")
			{
				if (current.Value != "Spawn" || isFirstStatement == true)
					return ParseInstruction();
				else
				{
					Errors.Add(($"Línea {current.Line}: solo se puede usar Spawn una vez al principio del codigo", current.Line));
					return null;
				}
			}
			else if (current.Type == "VARIABLE" && Peek(1).Type == "ASSIGMENT")
			{
				return ParseAssigment();
			}
			else if (current.Type == "CONTROL_FLOW" && current.Value == "GoTo")
			{
				return ParseGoTo();
			}
			else if (current.Type == "VARIABLE")
			{
				return ParseLabel();
			}
			Errors.Add(($"Línea {current.Line}: no se reconoce la instrucción (Token inesperado: {current.Type} '{current.Value}')", current.Line));
			return null;
		}

		private Instructions.Statement ParseInstruction()
		{
			Token keyword = Advance();
			int line = keyword.Line;

			if (!Dictionarys.InstructionSignatures.TryGetValue(keyword.Value, out int count))
			{
				Errors.Add(($"Línea {line}: instrucción desconocida '{keyword.Value}'", line));
				return null;
			}

			Consume("SYMBOL", line, "(");

			List<Expressions.Expression> args = new();

			if (!Check("SYMBOL", ")"))
			{
				args.Add(ParseExpression(keyword.Value, line));
				while (Check("SYMBOL", ","))
				{
					Advance();
					var currentExpression = ParseExpression(keyword.Value, line);
					if (currentExpression != null) args.Add(currentExpression);
				}
			}

			Consume("SYMBOL", line, ")");

			if (args.Count != count)
			{
				Errors.Add(($"Línea {line}: '{keyword.Value}' espera {count} argumentos, pero recibió {count - args.Count} inválidos.", line));
				return null;
			}

			switch (keyword.Value)
			{
				case "Spawn":
					return new Instructions.SpawnCommand
					{
						X = args[0],
						Y = args[1],
						Line = line
					};

				case "Color":
					if (args[0] is Expressions.ValidColor color)
					{
						return new Instructions.ColorCommand
						{
							Color = color.Color,
							Line = line
						};
					}
					else
					{
						Errors.Add(($"Línea {line}: 'Color' espera un nombre de color , pero se recibió {args[0]?.GetType().Name}.", line));
						return null;
					}

				case "Size":
					return new Instructions.SizeCommand
					{
						Size = args[0],
						Line = line
					};

				case "DrawLine":
					return new Instructions.DrawLineCommand
					{
						DirX = args[0],
						DirY = args[1],
						Distance = args[2],
						Line = line
					};

				case "DrawCircle":
					return new Instructions.DrawCircleCommand
					{
						DirX = args[0],
						DirY = args[1],
						Radius = args[2],
						Line = line
					};

				case "DrawRectangle":
					return new Instructions.DrawRectangleCommand
					{
						DirX = args[0],
						DirY = args[1],
						Distance = args[2],
						Width = args[3],
						Height = args[4],
						Line = line
					};

				case "Fill":
					return new Instructions.FillCommand
					{
						Line = line
					};

				default:
					Errors.Add(($"Línea {line}: instrucción '{keyword.Value}' no está implementada.", line));
					return null;
			}
		}

		private Expressions.Expression ParseFunctionCall(Token functionNameToken)
		{
			if (!Check("SYMBOL", "("))
			{
				Errors.Add(($"Línea {functionNameToken.Line}: Se esperaba '(' después de función", functionNameToken.Line));
				return null;
			}
			Advance();

			List<Expressions.Expression> arguments = new();

			if (!Check("SYMBOL", ")"))
			{
				arguments.Add(ParseExpression(functionNameToken.Value, functionNameToken.Line));
				while (Check("SYMBOL", ","))
				{
					Advance();
					var currentExpression = ParseExpression(functionNameToken.Value, functionNameToken.Line);
					if (currentExpression != null) arguments.Add(currentExpression);
				}
			}

			if (!Check("SYMBOL", ")"))
			{
				Errors.Add(($"Línea {functionNameToken.Line}: Se esperaba ')'", functionNameToken.Line));
				return null;
			}
			Advance();

			if (Dictionarys.FunctionSignatures.TryGetValue(functionNameToken.Value, out int count))
			{
				if (arguments.Count != count)
				{
					Errors.Add(($"Línea {functionNameToken.Line}: la función '{functionNameToken.Value}' espera {count} argumentos, pero recibió {count - arguments.Count} argumentos inválidos.", functionNameToken.Line));
					return null;
				}
			}
			return new Expressions.FunctionCall
			{
				FunctionName = functionNameToken.Value,
				Arguments = arguments,
				Line = functionNameToken.Line
			};
		}

		private Instructions.Statement ParseAssigment()
		{
			Token varriableName = Advance();
			int line = varriableName.Line;
			Consume("ASSIGMENT", line, "<_");
			return new Instructions.Assignment
			{
				VariableName = varriableName.Value,
				Value = ParseExpression(varriableName.Value, line),
				Line = line
			};
		}

		private Instructions.Statement ParseGoTo()
		{
			Token goToToken = Advance();
			int line = goToToken.Line;

			Consume("SYMBOL", line, "[");

			Token labelToken = Consume("VARIABLE", line);

			if (labelToken.Type == "EOF") return null;

			Consume("SYMBOL", line, "]");

			Consume("SYMBOL", line, "(");

			Expressions.Expression condition = ParseAnd(goToToken.Value, line);

			//buscar al menos un operador booleano
			if (condition != null && !ContainsAnyBooleanOperator(condition))
			{
				Errors.Add(($"Línea {line}: La condición debe contener al menos un operador booleano (==, !=, >, <, >=, <=, &&, ||)", line));
				return null;
			}

			Consume("SYMBOL", line, ")");

			return new Instructions.GoToCommand
			{
				Label = labelToken.Value,
				Condition = condition,
				Line = line
			};
		}

		private Instructions.Statement ParseLabel()
		{
			Token nameToken = Advance();
			int line = nameToken.Line;

			// Verificar si la etiqueta ya existe
			if (Labels.ContainsKey(nameToken.Value))
			{
				Errors.Add(($"Línea {line}: La etiqueta '{nameToken.Value}' ya está definida (primera definición en línea {Labels[nameToken.Value]})", line));
				return null;
			}

			// Agregar la etiqueta al diccionario con su número de línea
			Labels.Add(nameToken.Value, line);

			return new Instructions.LabelDeclaration
			{
				Name = nameToken.Value,
				Line = line
			};
		}




		private Expressions.Expression ParseAnd(string Name, int line)
		{
			Expressions.Expression left = ParseOr(Name, line);
			if (left == null) return null;

			while (Check("OPERATION", "&&"))
			{
				string op = Advance().Value;
				Expressions.Expression right = ParseOr(Name, line);
				if (right == null) return null;
				left = new Expressions.BinaryExpression
				{
					Operator = op,
					Left = left,
					Right = right
				};
			}

			return left;
		}

		private Expressions.Expression ParseOr(string Name, int line)
		{
			Expressions.Expression left = ParseComparison(Name, line);
			if (left == null) return null;

			while (Check("OPERATION", "||"))
			{
				string op = Advance().Value;
				Expressions.Expression right = ParseComparison(Name, line);
				if (right == null) return null;
				left = new Expressions.BinaryExpression
				{
					Operator = op,
					Left = left,
					Right = right
				};
			}

			return left;
		}

		private Expressions.Expression ParseComparison(string Name, int line)
		{
			Expressions.Expression left = ParseExpression(Name, line);
			if (left == null) return null;

			if (Check("BOOLOPERATION"))
			{
				string op = Advance().Value;
				Expressions.Expression right = ParseExpression(Name, line);
				if (right == null) return null;
				return new Expressions.BinaryExpression
				{
					Operator = op,
					Left = left,
					Right = right
				};
			}
			return left;
		}




		private Expressions.Expression ParseExpression(string Name, int line)
		{
			Expressions.Expression leftSide = ParseTerm(Name, line);
			if (leftSide == null) return null;
			while (IsAdditionOrSubtractionOperator())
			{
				string operatorSymbol = Advance().Value;
				Expressions.Expression rightSide = ParseTerm(Name, line);
				if (rightSide == null) return null;
				leftSide = new Expressions.BinaryExpression
				{
					Operator = operatorSymbol,
					Left = leftSide,
					Right = rightSide
				};
			}
			return leftSide;
		}

		private Expressions.Expression ParseTerm(string Name, int line)
		{
			Expressions.Expression leftSide = ParseExponent(Name, line);
			if (leftSide == null) return null;
			while (IsMultiplicationOrDivisionOperator())
			{
				string operatorSymbol = Advance().Value;
				Expressions.Expression rightSide = ParseExponent(Name, line);
				if (rightSide == null) return null;
				leftSide = new Expressions.BinaryExpression
				{
					Operator = operatorSymbol,
					Left = leftSide,
					Right = rightSide
				};
			}
			return leftSide;
		}

		private Expressions.Expression ParseExponent(string Name, int line)
		{
			Expressions.Expression baseExpression = ParsePrimary(Name, line);
			if (baseExpression == null) return null;
			while (IsPowerOperator())
			{
				string operatorSymbol = Advance().Value;
				Expressions.Expression exponent = ParseExponent(Name, line);
				if (exponent == null) return null;
				baseExpression = new Expressions.BinaryExpression
				{
					Operator = operatorSymbol,
					Left = baseExpression,
					Right = exponent
				};
			}
			return baseExpression;
		}

		private Expressions.Expression ParsePrimary(string Name, int line)
		{
			Token token = Advance();
			if (token.Type == "EOF")
			{
				Errors.Add(($"Fin de archivo inesperado en '{Name}'", tokens[^1].Line));
				return null;
			}

			if (token.Line != line)
			{
				Errors.Add(($"Linea {line}: se espera que toda la operación esté en la misma línea pero se encontro {token.Value} en la línea {token.Line}.", line));
				position--;//regresar el token que consumi que es de la otra linea
				return null;
			}

			if (token.Type == "NUMBER")
			{
				return new Expressions.NumberLiteral
				{
					Value = int.Parse(token.Value),
					Line = token.Line
				};
			}
			if (token.Type == "FUNCTION")
			{
				return ParseFunctionCall(token);
			}
			if (token.Type == "VARIABLE")
			{
				return new Expressions.VariableReference
				{
					Name = token.Value,
					Line = token.Line
				};
			}

			if (token.Type == "SYMBOL" && token.Value == "(")
			{
				Expressions.Expression inner = ParseExpression(Name, line);
				Token closingParen = Consume("SYMBOL", token.Line, ")");
				if (closingParen.Type == "EOF") return null;
				return inner;
			}

			if (token.Type == "COLOR")
			{
				return new Expressions.ValidColor
				{
					Color = token.Value,
					Line = token.Line
				};
			}
			Errors.Add(($"Línea {token.Line}: {Name} tiene como argumento una expresión no válida '{token.Value}'", token.Line));
			return null;
		}





		private bool IsAdditionOrSubtractionOperator()
		{
			Token current = Peek();
			return current.Type != "EOF" && current.Type == "OPERATION" && (current.Value == "+" || current.Value == "-");
		}

		private bool IsMultiplicationOrDivisionOperator()
		{
			Token current = Peek();
			return current.Type != "EOF" && current.Type == "OPERATION" && (current.Value == "*" || current.Value == "/" || current.Value == "%");
		}

		private bool IsPowerOperator()
		{
			Token current = Peek();
			return current.Type != "EOF" && current.Type == "OPERATION" && current.Value == "**";
		}

		private Token Peek(int offset = 0)
		{
			if (position + offset >= tokens.Count) return EOFToken;
			return tokens[position + offset];
		}

		private Token Advance()
		{
			if (IsAtEnd()) return EOFToken;
			return tokens[position++];
		}

		private bool Check(string expectedType, string expectedValue = null)
		{
			Token token = Peek();
			if (token.Type == "EOF") return false;

			bool typeMatches = token.Type == expectedType;
			bool valueMatches = expectedValue == null || token.Value == expectedValue;

			return typeMatches && valueMatches;
		}
		private Token Consume(string expectedType, int expectedLine, string expectedValue = null)
		{
			Token token = Peek(); // Solo mirar, no avanzar todavía
			if (token.Type == "EOF")
			{
				Errors.Add(($"Se esperaba {expectedType} pero se alcanzó el fin de archivo", tokens[^1].Line));
				return token;
			}

			if (token.Type != expectedType || (expectedValue != null && token.Value != expectedValue) || token.Line != expectedLine)
			{
				Errors.Add(($"Línea {expectedLine}: Se esperaba '{expectedValue}' ({expectedType}) en la linea {expectedLine}, pero se encontró '{token.Value}' ({token.Type}) en la linea {token.Line}", expectedLine));
				return EOFToken;
			}
			return Advance(); // Solo avanzar si todo está bien
		}

		private bool IsAtEnd()
		{
			return position >= tokens.Count;
		}

		//Metodo auxiliar de GoTo para saber si hay una operacion booleana en al condicion
		private bool ContainsAnyBooleanOperator(Expressions.Expression expr)
		{
			if (expr is Expressions.BinaryExpression binExpr)
			{
				// Lista de todos los operadores booleanos
				var booleanOps = new HashSet<string> { "==", "!=", ">", "<", ">=", "<=", "&&", "||" };

				// Verificar si el operador actual es booleano
				if (booleanOps.Contains(binExpr.Operator))
					return true;

				// Verificar recursivamente en los operandos
				return ContainsAnyBooleanOperator(binExpr.Left) || ContainsAnyBooleanOperator(binExpr.Right);
			}
			return false;
		}
	}
}
