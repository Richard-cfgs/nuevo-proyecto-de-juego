using System;
using System.Collections.Generic;
using Godot.NativeInterop;
using PixelWallE.Core;
namespace PixelWallE.Core
{
    public class Parser
    {
        private readonly List<Token> tokens;
        private List<string> Errors;
        private Dictionary<string, int> Labels;
        private int position = 0;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
            Errors = new();
            Labels = new();
        }

        public List<Instructions.Statement> Parse()
        {
            List<Instructions.Statement> statements = new();

            Token current = Peek();
            if (current.Value == "Spawn")
                statements.Add(ParseStatement(isFirstStatement: true));
            else
                Errors.Add($"Línea {current.Line}: el código siempre debe comenzar con Spawn");

            while (!IsAtEnd())
            {
                Token before = Peek(); // token inicial de la línea
                Instructions.Statement stmt = ParseStatement();
                Token after = Peek();  // lo que viene después de parsear

                if (stmt != null)
                {
                    // Verificamos si se intentó poner dos instrucciones en una línea
                    if (before.Line == after.Line)
                        Errors.Add($"Línea {before.Line}: solo puede haber una instrucción por línea.");

                    statements.Add(stmt); // solo agregamos si no es null
                }

                //avanzar a la siguiente linea por si hubo algun error en la actual
                while (!IsAtEnd() && Peek().Line == before.Line)
                    Advance();
            }
            return statements;
        }

        private Instructions.Statement ParseStatement(bool isFirstStatement = false)
        {
            Token current = Peek(); // Miro el token actual, sin consumirlo

            if (current.Type == "KEYWORD")
            {
                if (current.Value != "Spawn" || isFirstStatement == true)
                    return ParseInstruction();
                else
                {
                    Errors.Add($"Línea {current.Line}: solo se puede usar Spawn una vez al principio del codigo");
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
            Errors.Add($"Línea {current.Line}: no se reconoce la instrucción");
            return null;
        }





        //este metodo analiza todas las Instrucciones del tipo KeyWord

        private Instructions.Statement ParseInstruction()
        {
            Token keyword = Advance(); // Avanza y toma el nombre de la instrucción
            int line = keyword.Line;

            if (!Dictionarys.InstructionSignatures.TryGetValue(keyword.Value, out Instructions.InstructionSignature signature))
            {
                Errors.Add($"Línea {line}: instrucción desconocida '{keyword.Value}'");
                return null;
            }

            Consume("SYMBOL", "(", line);

            List<Expressions.Expression> args = new();

            if (!Check("SYMBOL", ")"))
            {
                args.Add(ParseExpression());
                while (Check("SYMBOL", ","))
                {
                    Advance(); // Consumimos la coma
                    args.Add(ParseExpression());
                }
            }

            Consume("SYMBOL", ")", line);

            // Validación de cantidad
            if (args.Count != signature.ExpectedCount)
            {
                Errors.Add($"Línea {line}: '{keyword.Value}' espera {signature.ExpectedCount} argumentos, pero recibió {args.Count}.");
                return null;
            }

            // Validación de tipo
            for (int i = 0; i < args.Count; i++)
            {
                string expected = signature.ExpectedTypes[i];
                if (!ArgumentType(args[i], expected))
                {
                    Errors.Add($"Línea {line}: El argumento {i + 1} de '{keyword.Value}' debe ser de tipo '{expected}'.");
                    return null;
                }
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
                    return new Instructions.ColorCommand
                    {
                        Color = ((Expressions.VariableReference)args[0]).Name,
                        Line = line
                    };

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
                    Errors.Add($"Línea {line}: instrucción '{keyword.Value}' no está implementada.");
                    return null;
            }
        }





        // Este método analiza llamadas a funciones como GetActualX(), GetColorCount(...), etc.
        private Expressions.Expression ParseFunctionCall(Token functionNameToken)
        {
            Consume("SYMBOL", "(", functionNameToken.Line);

            List<Expressions.Expression> arguments = new();

            if (!Check("SYMBOL", ")"))
            {
                arguments.Add(ParseExpression());

                while (Check("SYMBOL", ","))
                {
                    Advance(); // Consumimos la coma
                    arguments.Add(ParseExpression());
                }
            }

            Consume("SYMBOL", ")", functionNameToken.Line);

            // Validación semántica (cantidad y tipo de argumentos)
            if (Dictionarys.FunctionSignatures.TryGetValue(functionNameToken.Value, out Expressions.FunctionSignature signature))
            {
                if (arguments.Count != signature.ExpectedCount)
                {
                    Errors.Add($"Línea {functionNameToken.Line}: la función '{functionNameToken.Value}' espera {signature.ExpectedCount} argumentos, pero recibió {arguments.Count}.");
                }
                else
                {
                    for (int i = 0; i < arguments.Count; i++)
                    {
                        string expected = signature.ExpectedTypes[i];
                        if (!ArgumentType(arguments[i], expected))
                        {
                            Errors.Add($"Línea {functionNameToken.Line}: el argumento {i + 1} de '{functionNameToken.Value}' debe ser de tipo '{expected}'.");
                        }
                    }
                }
            }

            return new Expressions.FunctionCall
            {
                FunctionName = functionNameToken.Value,
                Arguments = arguments,
                Line = functionNameToken.Line
            };
        }





        //Este metodo analiza la asignacion de variables

        private Instructions.Statement ParseAssigment()
        {
            Token varriableName = Advance();
            int line = varriableName.Line;
            Consume("ASSIGMENT", "<-", line);
            return new Instructions.Assignment
            {
                VariableName = varriableName.Value,
                Value = ParseExpression(),
                Line = line
            };
        }


        //Este metodo analiza los saltos condicionales
        private Instructions.Statement ParseGoTo()
        {
            Token goToToken = Advance(); // Consumimos "GoTo"
            int line = goToToken.Line;

            // [Etiqueta]
            Consume("SYMBOL", "[", line);

            Token labelToken = Consume("VARIABLE", null, line);
            string label = labelToken?.Value;


            Consume("SYMBOL", "]", line);

            // (Condición)
            Consume("SYMBOL", "(", line);

            Expressions.Expression condition = ParseAnd();

            Consume("SYMBOL", ")", line);

            return new Instructions.GoToCommand
            {
                Label = label,
                Condition = condition,
                Line = line
            };
        }


        //este metodo analiza las etiquetas
        private Instructions.Statement ParseLabel()
        {
            Token nameToken = Advance();
            int line = nameToken.Line;

            // Verificamos si el siguiente token está en otra línea
            Token next = Peek();

            if (next.Line == line)
            {
                Errors.Add($"Línea {line}: una etiqueta debe estar sola en una línea.");
                return null;
            }
            if (Labels.TryGetValue(nameToken.Value, out int OuldNameTokenLine)) Errors.Add($"Línea {nameToken.Line}: la etiqueta {nameToken.Value} ya existe en la línea {OuldNameTokenLine}");
            else Labels.Add(nameToken.Value, line);
            return new Instructions.LabelDeclaration
            {
                Name = nameToken.Value,
                Line = line
            };
        }







        //metodos auxiliares del GoTo


        private Expressions.Expression ParseAnd()
        {
            // Empezamos parseando lo de mayor prioridad: OR
            Expressions.Expression left = ParseOr();

            // Mientras siga habiendo '&&', seguimos combinando
            while (Check("OPERATION", "&&"))
            {
                string op = Advance().Value;

                Expressions.Expression right = ParseOr(); // cada lado puede ser ORs también

                left = new Expressions.BinaryExpression
                {
                    Operator = op,
                    Left = left,
                    Right = right
                };
            }

            return left;
        }
        private Expressions.Expression ParseOr()
        {
            // Empezamos con una comparación (==, <, >, etc.)
            Expressions.Expression left = ParseComparison();

            // Mientras haya más OR, seguimos encadenando
            while (Check("OPERATION", "||"))
            {
                string op = Advance().Value;

                Expressions.Expression right = ParseComparison();

                left = new Expressions.BinaryExpression
                {
                    Operator = op,
                    Left = left,
                    Right = right
                };
            }

            return left;
        }

        private Expressions.Expression ParseComparison()
        {
            Expressions.Expression left = ParseExpression();

            if (Check("BOOLOPERATION"))
            {
                string op = Advance().Value;
                Expressions.Expression right = ParseExpression();

                return new Expressions.BinaryExpression
                {
                    Operator = op,
                    Left = left,
                    Right = right
                };
            }
            return left;
        }








        //metodos auxiliares de FuncionCall e Instruction
        private bool ArgumentType(Expressions.Expression arg, string expected)
        {
            switch (expected)
            {
                case "number":
                    return arg is Expressions.NumberLiteral;

                case "color":
                    if (arg is Expressions.VariableReference varRef)
                        return IsKnownColor(varRef.Name);
                    return false;

                case "variable":
                    return arg is Expressions.VariableReference;

                case "expression":
                    return true;

                default:
                    return false;
            }
        }
        private bool IsKnownColor(string name)
        {
            switch (name)
            {
                case "Red":
                case "Blue":
                case "Green":
                case "Yellow":
                case "Orange":
                case "Purple":
                case "Black":
                case "White":
                case "Transparent":
                    return true;
                default:
                    return false;
            }
        }





        //Add los argumentos al AST

        //Este método analiza suma y resta
        private Expressions.Expression ParseExpression()
        {
            Expressions.Expression leftSide = ParseTerm();
            while (IsAdditionOrSubtractionOperator())
            {
                string operatorSymbol = Advance().Value;
                Expressions.Expression rightSide = ParseTerm();
                leftSide = new Expressions.BinaryExpression
                {
                    Operator = operatorSymbol,
                    Left = leftSide,
                    Right = rightSide
                };
            }
            return leftSide;
        }

        // Este método analiza multiplicaciones, divisiones y módulo
        private Expressions.Expression ParseTerm()
        {
            Expressions.Expression leftSide = ParseExponent();
            while (IsMultiplicationOrDivisionOperator())
            {
                string operatorSymbol = Advance().Value;
                Expressions.Expression rightSide = ParseExponent();
                leftSide = new Expressions.BinaryExpression
                {
                    Operator = operatorSymbol,
                    Left = leftSide,
                    Right = rightSide
                };
            }
            return leftSide;
        }

        // Este método analiza potencias (**) y es asociativo a la derecha
        private Expressions.Expression ParseExponent()
        {
            Expressions.Expression baseExpression = ParsePrimary();
            while (IsPowerOperator())
            {
                string operatorSymbol = Advance().Value;
                Expressions.Expression exponent = ParseExponent();
                baseExpression = new Expressions.BinaryExpression
                {
                    Operator = operatorSymbol,
                    Left = baseExpression,
                    Right = exponent
                };
            }
            return baseExpression;
        }

        // Este método analiza los elementos más básicos de una expresión
        private Expressions.Expression ParsePrimary()
        {
            Token token = Advance();
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
                Expressions.Expression inner = ParseExpression();
                Consume("SYMBOL", ")", token.Line);
                return inner;
            }
            Errors.Add($"Línea {token.Line}: expresión no válida");
            return null;
        }







        //informacion booleana

        private bool IsComma()
        {
            Token current = Peek();
            return current.Type == "SYMBOL" && current.Value == ",";
        }
        private bool IsAdditionOrSubtractionOperator()
        {
            Token current = Peek();
            return current.Type == "OPERATION" && (current.Value == "+" || current.Value == "-");
        }
        private bool IsMultiplicationOrDivisionOperator()
        {
            Token current = Peek();
            return current.Type == "OPERATION" && (current.Value == "*" || current.Value == "/" || current.Value == "%");
        }
        private bool IsPowerOperator()
        {
            Token current = Peek();
            return current.Type == "OPERATION" && current.Value == "**";
        }





        //metodos auxiliares

        //revisa que viene despues
        private Token Peek(int offset = 0)
        {
            if (position + offset >= tokens.Count) return new Token("EOF", "", tokens[tokens.Count - 1].Line);
            return tokens[position + offset];
        }
        //toma el token, avanza el cursor y lo compara con lo deseado
        private Token Consume(string expectedType, string expectedValue = null, int? expectedLine = null)
        {
            Token token = Advance();
            if (token.Type != expectedType || (token.Value != expectedValue && expectedValue != null) || (token.Line != expectedLine && expectedLine != null))
            {
                Errors.Add($"Línea {token.Line}: Se esperaba '{expectedValue}' en la misma línea, pero se encontró '{token.Value}' en línea {token.Line}");
                return null;
            }
            return token;
        }

        //revisa que viene delante y lo compara con lo deseado
        private bool Check(string expectedType, string expectedValue = null)
        {
            if (IsAtEnd()) return false;

            Token token = Peek();

            bool typeMatches = token.Type == expectedType;
            bool valueMatches = expectedValue == null || token.Value == expectedValue;

            return typeMatches && valueMatches;
        }
        //toma el token y avanza el cursor
        private Token Advance()
        {
            if (!IsAtEnd()) position++;
            return tokens[position - 1];
        }
        //revisa que siga dentro de la lista de tokens
        private bool IsAtEnd()
        {
            return position >= tokens.Count;
        }

    }
}

