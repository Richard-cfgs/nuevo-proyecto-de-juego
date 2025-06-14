using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace PixelWallE.Core
{
    public class Interprete
    {
        //Simulacion del pincel y el canvas
        private int X;
        private int Y;
        private int BrushSize;
        private string BrushColor;
        private int CanvasWidth;
        private int CanvasHeight;

        private Dictionary<string, int> Variables;
        private Dictionary<string, int> Labels;

        private List<Instructions.Statement> Statements;
        private int CurrentIndex;
        public List<string> Errors;

        public Interprete(List<Instructions.Statement> statements, Dictionary<string, int> labels)
        {
            Statements = statements;
            Labels = labels;
            Variables = new();
            Errors = new();
            CurrentIndex = 0;
            X = 0;
            Y = 0;
            BrushSize = 1;
            BrushColor = "Transparent";
            CanvasWidth = 100;
            CanvasHeight = 100;
            Run();
        }

        public void Run()
        {
            while (CurrentIndex < Statements.Count)
                Execute(Statements[CurrentIndex++]);
        }



        private void Execute(Instructions.Statement stmt)
        {
            switch (stmt)
            {
                case Instructions.SpawnCommand spawn:
                    {
                        var x = Evaluate(spawn.X);
                        var y = Evaluate(spawn.Y);

                        if (x is int ix && y is int iy)
                        {
                            X = ix;
                            Y = iy;
                        }
                        else
                        {
                            Errors.Add($"Línea {spawn.Line}: 'Spawn' espera dos valores numéricos.");
                        }
                        break;
                    }

                case Instructions.ColorCommand colorCmd:
                    {
                        if (IsKnownColor(colorCmd.Color)) BrushColor = colorCmd.Color;
                        else Errors.Add($"Línea {colorCmd.Line}: 'Color' espera un color válido.");
                        break;
                    }


                case Instructions.SizeCommand sizeCmd:
                    {
                        var size = Evaluate(sizeCmd.Size);
                        if (size is int s)
                        {
                            if (s % 2 == 0) BrushSize = s - 1;
                            else BrushSize = s;
                        }
                        else
                        {
                            Errors.Add($"Línea {sizeCmd.Line}: 'Size' espera un valor numérico.");
                        }
                        break;
                    }

                case Instructions.DrawLineCommand drawLineCmd:
                    {
                        // Evaluar parámetros
                        var dirX = Evaluate(drawLineCmd.DirX);
                        var dirY = Evaluate(drawLineCmd.DirY);
                        var distance = Evaluate(drawLineCmd.Distance);

                        if (dirX is not int iDirX || dirY is not int iDirY || distance is not int iDistance)
                        {
                            Errors.Add($"Línea {drawLineCmd.Line}: 'DrawLine' espera parámetros enteros.");
                            break;
                        }

                        if (!IsValidDirection(iDirX, iDirY) || iDistance <= 0)
                        {
                            Errors.Add($"Línea {drawLineCmd.Line}: Dirección o distancia inválida.");
                            break;
                        }

                        int radius = (BrushSize - 1) / 2;

                        for (int step = 0; step < iDistance; step++)
                        {
                            Canvas.SetPixel(X, Y, BrushColor);

                            // Pintar grosor perpendicular a la dirección

                            if (iDirX == 0) // Línea vertical
                            {
                                for (int offset = 1; offset <= radius; offset++)
                                {
                                    Canvas.SetPixel(X + offset, Y, BrushColor);
                                    Canvas.SetPixel(X - offset, Y, BrushColor);
                                }
                            }
                            else if (iDirY == 0) // Línea horizontal
                            {
                                for (int offset = 1; offset <= radius; offset++)
                                {
                                    Canvas.SetPixel(X, Y + offset, BrushColor);
                                    Canvas.SetPixel(X, Y - offset, BrushColor);
                                }
                            }
                            else // Diagonal
                            {
                                for (int offset = 1; offset <= radius; offset++)
                                {
                                    // Perpendicular a la diagonal
                                    Canvas.SetPixel(X - offset * iDirY, Y + offset * iDirX, BrushColor);
                                    Canvas.SetPixel(X + offset * iDirY, Y - offset * iDirX, BrushColor);
                                }
                            }
                            X += iDirX;
                            Y += iDirY;
                        }
                        break;
                    }

                case Instructions.DrawCircleCommand drawCircleCmd:
                    {
                        // 1. Evaluar parámetros
                        var dirX = Evaluate(drawCircleCmd.DirX);
                        var dirY = Evaluate(drawCircleCmd.DirY);
                        var radius = Evaluate(drawCircleCmd.Radius);

                        // 2. Validar tipos y valores
                        if (dirX is not int iDirX || dirY is not int iDirY || radius is not int iRadius || iRadius <= 0)
                        {
                            Errors.Add($"Línea {drawCircleCmd.Line}: 'DrawCircle' espera enteros.");
                            break;
                        }

                        if (!IsValidDirection(iDirX, iDirY))
                        {
                            Errors.Add($"Línea {drawCircleCmd.Line}: direcciones inválidas");
                            break;
                        }

                        // 4. Calcular el centro del círculo (posición actual + dirección * radio)
                        int centerX = X + (iDirX * iRadius);
                        int centerY = Y + (iDirY * iRadius);

                        // 5. Grosor del pincel (asegurar que sea impar)
                        int halfThickness = (BrushSize - 1) / 2;

                        // 6. Algoritmo de Bresenham para la circunferencia
                        int x = 0;
                        int y = iRadius;
                        int d = 3 - 2 * iRadius;

                        while (x <= y)
                        {
                            // Dibujar los 8 octantes con grosor
                            for (int dy = -halfThickness; dy <= halfThickness; dy++)
                            {
                                for (int dx = -halfThickness; dx <= halfThickness; dx++)
                                {
                                    // Octante 1 (x, y) y simetrías
                                    Canvas.SetPixel(centerX + x + dx, centerY + y + dy, BrushColor);
                                    Canvas.SetPixel(centerX - x + dx, centerY + y + dy, BrushColor);
                                    Canvas.SetPixel(centerX + x + dx, centerY - y + dy, BrushColor);
                                    Canvas.SetPixel(centerX - x + dx, centerY - y + dy, BrushColor);
                                    // Octante 2 (y, x) y simetrías
                                    Canvas.SetPixel(centerX + y + dx, centerY + x + dy, BrushColor);
                                    Canvas.SetPixel(centerX - y + dx, centerY + x + dy, BrushColor);
                                    Canvas.SetPixel(centerX + y + dx, centerY - x + dy, BrushColor);
                                    Canvas.SetPixel(centerX - y + dx, centerY - x + dy, BrushColor);
                                }
                            }

                            // Actualizar parámetro de decisión
                            if (d < 0)
                                d += 4 * x + 6;
                            else
                            {
                                d += 4 * (x - y) + 10;
                                y--;
                            }
                            x++;
                        }

                        // 7. Mover Wall-E al centro del círculo (requerimiento explícito del PDF)
                        X = centerX;
                        Y = centerY;
                        break;
                    }

                case Instructions.DrawRectangleCommand drawRectangleCmd:
                    {
                        var dirX = Evaluate(drawRectangleCmd.DirX);
                        var dirY = Evaluate(drawRectangleCmd.DirY);
                        var distance = Evaluate(drawRectangleCmd.Distance);
                        var width = Evaluate(drawRectangleCmd.Width);
                        var height = Evaluate(drawRectangleCmd.Height);

                        if (dirX is not int iDirX || dirY is not int iDirY || distance is not int iDistance || width is not int iWidth || height is not int iHeight)
                        {
                            Errors.Add($"Línea {drawRectangleCmd.Line}: 'DrawRectangle' espera 5 parámetros enteros.");
                            break;
                        }

                        if (!IsValidDirection(iDirX, iDirY) || iDistance < 0 || iWidth <= 0 || iHeight <= 0)
                        {
                            Errors.Add($"Línea {drawRectangleCmd.Line}: Parámetros inválidos. Dirección (-1,0,1), distancia≥0, ancho/alto>0.");
                            break;
                        }

                        // Calcular centro del rectángulo
                        int centerX = X + (iDirX * iDistance);
                        int centerY = Y + (iDirY * iDistance);

                        // Calcular bordes del rectángulo
                        int halfWidth = iWidth / 2;
                        int halfHeight = iHeight / 2;
                        int left = centerX - halfWidth;
                        int right = centerX + halfWidth;
                        int top = centerY - halfHeight;
                        int bottom = centerY + halfHeight;

                        int halfBrush = (BrushSize - 1) / 2;

                        // Bordes horizontales (superior e inferior)
                        for (int x = left - halfBrush; x <= right + halfBrush; x++)
                        {
                            for (int b = -halfBrush; b <= halfBrush; b++)
                            {
                                Canvas.SetPixel(x, top + b, BrushColor);    // Borde superior
                                Canvas.SetPixel(x, bottom + b, BrushColor); // Borde inferior
                            }
                        }

                        // Bordes verticales (izquierdo y derecho)
                        for (int y = top - halfBrush; y <= bottom + halfBrush; y++)
                        {
                            for (int b = -halfBrush; b <= halfBrush; b++)
                            {
                                Canvas.SetPixel(left + b, y, BrushColor);    // Borde izquierdo
                                Canvas.SetPixel(right + b, y, BrushColor);   // Borde derecho
                            }
                        }

                        // Mover Wall-E al centro del rectángulo
                        X = centerX;
                        Y = centerY;
                        break;
                    }

                case Instructions.FillCommand:
                    {
                        string targetColor = Canvas.GetPixel(X, Y);
                        if (targetColor == BrushColor) break;

                        int[] dirx = { 0, 0, 1, -1 };
                        int[] diry = { -1, 1, 0, 0 };
                        var stack = new Stack<(int x, int y)>();
                        stack.Push((X, Y));
                        Canvas.SetPixel(X, Y, BrushColor);  // Pintar primero el origen

                        while (stack.Count > 0)
                        {
                            var (x, y) = stack.Pop();

                            // Explorar vecinos
                            for (int i = 0; i < 4; i++)
                            {
                                int x1 = x + dirx[i];
                                int y1 = y + diry[i];
                                if (!IsValidPosition(x1, y1)) continue;
                                if (Canvas.GetPixel(x1, y1) != targetColor) continue;
                                Canvas.SetPixel(x1, y1, BrushColor);
                                stack.Push((x1, y1));
                            }
                        }
                        break;
                    }

                case Instructions.Assignment assign:
                    {
                        var value = Evaluate(assign.Value);
                        if (value is int intValue) Variables[assign.VariableName] = intValue;
                        else Errors.Add($"Línea {assign.Line}: no se pudo evaluar la expresión de asignación.");
                        break;
                    }

                case Instructions.GoToCommand goTo:
                    {
                        var condition = Evaluate(goTo.Condition);
                        if (condition is bool b && b)
                        {
                            if (Labels.TryGetValue(goTo.Label, out int index))
                            {
                                CurrentIndex = index;
                                return;
                            }
                            else
                            {
                                Errors.Add($"Línea {goTo.Line}: etiqueta '{goTo.Label}' no encontrada.");
                            }
                        }
                        break;
                    }

                case Instructions.LabelDeclaration:
                    // Las etiquetas ya están registradas. No hacen nada en tiempo de ejecución.
                    break;

                default:
                    Errors.Add("Error interno: instrucción desconocida.");
                    break;
            }
        }





        //este metodo evalua todo tipo de expresiones para devolver su resultado

        private object Evaluate(Expressions.Expression expr)
        {
            // Caso 1: Es un número y se devolve
            if (expr is Expressions.NumberLiteral num)
            {
                return num.Value;
            }

            // Caso 2: Referencia a una variable
            if (expr is Expressions.VariableReference varRef)
            {
                //referencia a su valor
                if (Variables.TryGetValue(varRef.Name, out int value))
                    return value;
                Errors.Add($"Línea {varRef.Line}: variable '{varRef.Name}' no está definida.");
                return null;
            }

            if (expr is Expressions.BinaryExpression bin)
            {
                object left = Evaluate(bin.Left);
                object right = Evaluate(bin.Right);

                // Si ambos lados son números
                if (left is int l && right is int r)
                {
                    switch (bin.Operator)
                    {
                        case "+": return l + r;
                        case "-": return l - r;
                        case "*": return l * r;
                        case "/":
                            if (r == 0)
                            {
                                Errors.Add($"Línea {bin.Line}: división por cero.");
                                return 0;
                            }
                            return l / r;

                        case "%":
                            if (r == 0)
                            {
                                Errors.Add($"Línea {bin.Line}: módulo por cero.");
                                return 0;
                            }
                            return l % r;

                        case "**":
                            return (int)Math.Pow(l, r);

                        case "==": return l == r;
                        case "!=": return l != r;
                        case ">": return l > r;
                        case "<": return l < r;
                        case ">=": return l >= r;
                        case "<=": return l <= r;

                        default:
                            Errors.Add($"Línea {bin.Line}: operador aritmético desconocido '{bin.Operator}'.");
                            return null;
                    }
                }

                // Si ambos lados son booleanos (para && y ||)
                if (left is bool bl && right is bool br)
                {
                    switch (bin.Operator)
                    {
                        case "&&": return bl && br;
                        case "||": return bl || br;
                        default:
                            Errors.Add($"Línea {bin.Line}: operador lógico desconocido '{bin.Operator}'.");
                            return null;
                    }
                }

                // Si son strings o comparación entre tipos diferentes
                if (bin.Operator == "==") return left == right;
                if (bin.Operator == "!=") return left != right;
                Errors.Add($"Línea {bin.Line}: operación inválida entre tipos incompatibles.");
                return null;
            }


            if (expr is Expressions.FunctionCall func)
            {
                return EvaluateFunction(func);
            }

            Errors.Add($"Línea {expr.Line}: expresión desconocida.");
            return null;
        }




        //este método evalua las funciones para retornar un resultado
        private object EvaluateFunction(Expressions.FunctionCall func)
        {
            string name = func.FunctionName;
            int line = func.Line;

            List<object> args = new();

            foreach (Expressions.Expression expr in func.Arguments)
            {
                var value = Evaluate(expr);
                if (value == null)
                {
                    Errors.Add($"Línea {line}: argumento inválido para la función {name}");
                    return null;
                }
                args.Add(value);
            }

            // Aquí empieza la lógica de cada función
            switch (name)
            {
                case "GetActualX": return X;

                case "GetActualY": return Y;

                case "GetCanvasSize": return CanvasWidth;

                case "GetColorCount":
                    // 1. Validar argumentos
                    if (args[0] is not string || args[1] is not int || args[2] is not int || args[3] is not int || args[4] is not int)
                    {
                        Errors.Add($"Línea {line}: 'GetColorCount' espera (string color, int x1, int y1, int x2, int y2).");
                        return null;
                    }

                    string searchColor = (string)args[0];
                    int x1 = (int)args[1];
                    int y1 = (int)args[2];
                    int x2 = (int)args[3];
                    int y2 = (int)args[4];

                    if (!IsValidPosition(x1, y1) || !IsValidPosition(x2, y2))
                        return 0;

                    // 3. Normalizar coordenadas (para manejar cualquier orden de esquinas)
                    int startX = Math.Min(x1, x2);
                    int endX = Math.Max(x1, x2);
                    int startY = Math.Min(y1, y2);
                    int endY = Math.Max(y1, y2);

                    // 4. Contar píxeles del color buscado
                    int count = 0;
                    for (int y = startY; y <= endY; y++)
                    {
                        for (int x = startX; x <= endX; x++)
                        {
                            if (Canvas.GetPixel(x, y) == searchColor)
                                count++;
                        }
                    }
                    return count;

                case "IsBrushColor":
                    if (args[0] is not string)
                    {
                        Errors.Add($"Línea {line}: 'IsBrushColor' espera 1 argumento de tipo color.");
                        return null;
                    }
                    return BrushColor == (string)args[0];

                case "IsBrushSize":
                    if (args[0] is not int)
                    {
                        Errors.Add($"Línea {line}: 'IsBrushSize' espera 1 argumento numérico.");
                        return null;
                    }
                    return BrushSize == (int)args[0];

                case "IsCanvasColor":
                    // Verificar tipos de los argumentos
                    if (args[0] is not string || args[1] is not int || args[2] is not int)
                    {
                        Errors.Add($"Línea {line}: 'IsCanvasColor' espera (string color, int vertical, int horizontal).");
                        return null;
                    }

                    string targetColor = (string)args[0];
                    int verticalOffset = (int)args[1];
                    int horizontalOffset = (int)args[2];

                    int checkX = X + horizontalOffset;
                    int checkY = Y + verticalOffset;

                    if (!IsValidPosition(checkX, checkY))
                        return 0; // Fuera de límites

                    string pixelColor = Canvas.GetPixel(checkX, checkY);

                    return pixelColor == targetColor;

                default:
                    Errors.Add($"Línea {line}: función desconocida '{name}'");
                    return null;
            }
        }






        //metodo auxiliar, informacion booleana
        private bool IsKnownColor(string color)
        {
            if (Lexer._keywords.ContainsKey(color))
                return true;
            return false;
        }
        private bool IsValidDirection(int dirX, int dirY)
        {
            if (dirX >= -1 && dirX <= 1 && dirY >= -1 && dirY <= 1 && (dirX != 0 || dirY != 0))
                return true;
            return false;
        }
        private bool IsValidPosition(int x, int y)
        {
            if (x < CanvasWidth && x >= 0 && y < CanvasHeight && y >= 0)
                return true;
            return false;
        }
    }
}
