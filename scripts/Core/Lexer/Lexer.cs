using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;
using Godot.NativeInterop;

namespace PixelWallE.Core
{
    public class Lexer
    {
        private string _input; //codigo
        private int _line; //linea actual
        private int _position; //posicion actual
        private static readonly Dictionary<string, string> _keywords = new Dictionary<string, string>
        {
            // ===== COMANDOS BÁSICOS =====
            { "Spawn", "KEYWORD" },
            { "Color", "KEYWORD" },
            { "Size", "KEYWORD" },
            { "DrawLine", "KEYWORD" },
            { "DrawCircle", "KEYWORD" },
            { "DrawRectangle", "KEYWORD" },
            { "Fill", "KEYWORD" },

            // ===== FUNCIONES =====
            { "GetActualX", "FUNCTION" },
            { "GetActualY", "FUNCTION" },
            { "GetCanvasSize", "FUNCTION" },
            { "GetColorCount", "FUNCTION" },
            { "IsBrushColor", "FUNCTION" },
            { "IsBrushSize", "FUNCTION" },
            { "IsCanvasColor", "FUNCTION" },

            // ===== CONTROL DE FLUJO =====
            { "GoTo", "CONTROL_FLOW" }
        };
        public Lexer(string input)
        {
            _input = input;
            _line = 1;
            _position = 0;
        }
        public static List<Token> Tokenize(string input)
        {
            List<Token> tokens = new List<Token>();
            int position = 0;
            int line = 1;
            while (position < input.Length)
            {
                char currentChar = input[position];

                // Ignorar espacios
                if (currentChar == ' ') { position++; continue; }

                // Contar lineas
                else if (currentChar == '\n') { line++; position++; continue; }

                // Identificar números
                if (currentChar >= '0' && currentChar <= '9')
                {
                    string number = "";
                    while (position < input.Length && currentChar >= '0' && currentChar <= '9')
                    {
                        number += input[position];
                        position++;
                        currentChar = input[position];
                    }
                    tokens.Add(new Token("NUMBER", number, line));
                    continue;
                }

                // Identificar palabras clave ,variable o funcion
                if ((currentChar >= 'A' && currentChar <= 'Z') || (currentChar >= 'a' && currentChar <= 'z'))
                {
                    string word = "";
                    while (position < input.Length && ((currentChar >= 'A' && currentChar <= 'Z') || (currentChar >= 'a' && currentChar <= 'z') || (currentChar >= '0' && currentChar <= '9') || currentChar == '-' || currentChar == '_'))
                    {
                        word += input[position];
                        position++;
                        currentChar = input[position];
                    }
                    if (_keywords.TryGetValue(word, out string tokenType))
                        tokens.Add(new Token(tokenType, word, line));
                    else
                        tokens.Add(new Token("VARIABLE", word, line));

                    continue;
                }

                // Identificar símbolos
                if (currentChar == '(' || currentChar == ')' || currentChar == ',' || currentChar == '[' || currentChar == ']' || currentChar == '"')
                {
                    tokens.Add(new Token("SYMBOL", currentChar.ToString(), line));
                    position++;
                    continue;
                }
                // Identificar operadores aritmeticas
                if (currentChar == '+' || currentChar == '-' || currentChar == '*' || currentChar == '/' || currentChar == '%')
                {
                    //identificar potencia
                    if ((position + 1 < input.Length) && currentChar == '*' && input[position + 1] == '*') { tokens.Add(new Token("Operation", "**", line)); position++; }
                    else tokens.Add(new Token("Operation", currentChar.ToString(), line));
                    position++;
                    continue;
                }
                //Identificar operaciones booleanas
                if (position + 1 < input.Length)
                {
                    string opera = input[position].ToString() + input[position + 1].ToString();
                    if (opera == "==" || opera == ">=" || opera == "<=" || opera == ">" || opera == "<")
                    {
                        tokens.Add(new Token("Operation", opera, line));
                        position += 2;
                        continue;
                    }
                }

                //Asignar Variables
                if (position + 1 < input.Length && currentChar == '<' && input[position + 1] == '-')
                {
                    tokens.Add(new Token("Assignment", "<-", line));
                    position += 2;
                    continue;
                }
                // Manejar errores
                Console.WriteLine($"Error en línea {line}: Carácter inválido '{currentChar}'");
                position++;
            }

            foreach (Token n in tokens)
            {
                GD.Print($"{n.Type} {n.Value} {n.Line} \n");
            }
            return tokens;
        }

    }
}