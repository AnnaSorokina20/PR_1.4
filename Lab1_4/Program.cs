using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

class Program
{
    static bool isMinimization = false; // true – шукаємо min, false – max
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        while (true)
        {
            Console.WriteLine("=== Меню ===");
            Console.WriteLine("1. Розв’язати задачу ");
            Console.WriteLine("2. Вихід");
            Console.Write("Оберіть пункт (1-2): ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    string objective = ReadObjectiveFunction();
                    List<string> constraints = ReadConstraints();
                    int n = ReadVariableCount();

                    object[,] table = BuildSimplexTable(objective, constraints, n);

                    DisplayNormalizedConstraints(table, constraints.Count, n, constraints);
                    Console.WriteLine("\nПочаткова симплекс-таблиця:");
                    DisplaySimplexTable(table);

                    ComputeOptimalSolution(table);
                    break;

                case "2":
                    Console.WriteLine("До побачення!");
                    return;

                default:
                    Console.WriteLine("Неправильний вибір. Спробуйте ще раз.\n");
                    break;
            }
        }
    }

    static string ReadObjectiveFunction()
    {
        Console.WriteLine("Введіть цільову функцію Z у форматі (x(i)… -> max/min):");
        return Console.ReadLine();
    }

    static List<string> ReadConstraints()
    {
        Console.WriteLine("Введіть обмеження (порожній рядок — щоб завершити):");
        List<string> constraints = new List<string>();
        string input;
        while (!string.IsNullOrWhiteSpace(input = Console.ReadLine()))
        {
            constraints.Add(input);
        }
        return constraints;
    }

    static int ReadVariableCount()
    {
        Console.WriteLine("Введіть кількість змінних:");
        return int.Parse(Console.ReadLine());
    }

    static object[,] BuildSimplexTable(string objectiveFunction, List<string> constraints, int variablesCount)
    {
        double[] objectiveCoefficients = ParseObjectiveFunctionCoefficients(objectiveFunction, variablesCount);
        double[,] coefficientsMatrix = new double[constraints.Count + 1, variablesCount + 1];
        string[] basisLabels = new string[constraints.Count + 1];
        basisLabels[constraints.Count] = "Z=";

        int slackVariablesCount = 0;

        for (int row = 0; row < constraints.Count; row++)
        {
            double[] constraintCoefficients = ParseConstraintCoefficients(constraints[row], variablesCount);
            for (int col = 0; col < constraintCoefficients.Length; col++)
            {
                coefficientsMatrix[row, col] = constraintCoefficients[col];
            }

            if (constraints[row].Contains("=") && !constraints[row].Contains(">=") && !constraints[row].Contains("<="))
            {
                basisLabels[row] = "0=";
            }
            else
            {
                if (constraints[row].Contains(">="))
                {
                    for (int col = 0; col < coefficientsMatrix.GetLength(1); col++)
                    {
                        coefficientsMatrix[row, col] *= -1;
                    }
                }
                slackVariablesCount++;
                basisLabels[row] = $"y{slackVariablesCount}=";
            }
        }

        for (int col = 0; col < variablesCount; col++)
        {
            coefficientsMatrix[constraints.Count, col] = objectiveCoefficients[col];
        }

        FixFloatingPointErrorsInTable(coefficientsMatrix);

        object[,] simplexTable = new object[coefficientsMatrix.GetLength(0) + 1, coefficientsMatrix.GetLength(1) + 1];
        simplexTable[0, 0] = "";

        for (int col = 0; col < variablesCount; col++)
        {
            simplexTable[0, col + 1] = $"-x{col + 1}";
        }
        simplexTable[0, variablesCount + 1] = "1";

        for (int row = 0; row < coefficientsMatrix.GetLength(0); row++)
        {
            simplexTable[row + 1, 0] = basisLabels[row];
            for (int col = 0; col < coefficientsMatrix.GetLength(1); col++)
            {
                simplexTable[row + 1, col + 1] = coefficientsMatrix[row, col];
            }
        }

        return simplexTable;
    }

    static double[] ParseConstraintCoefficients(string constraint, int variablesCount)
    {
        double[] coefficients = new double[variablesCount + 1];
        string[] comparisonOperators = { "<=", ">=", "=" };
        string operatorFound = comparisonOperators.FirstOrDefault(op => constraint.Contains(op));

        if (operatorFound == null)
            throw new ArgumentException("Неправильний формат обмеження:" + constraint);

        string[] parts = constraint.Split(new[] { operatorFound }, StringSplitOptions.None);

        if (parts.Length != 2)
            throw new ArgumentException("Обмеження має некоректний формат: " + constraint);

        string leftPart = parts[0].Replace(" ", "");
        string rightPart = parts[1].Trim();

        if (operatorFound == ">=")
        {
            for (int i = 0; i < coefficients.Length; i++)
            {
                coefficients[i] *= -1;
            }
        }

        for (int varIndex = 1; varIndex <= variablesCount; varIndex++)
        {
            string[] subParts = leftPart.Split(new[] { $"x{varIndex}" }, StringSplitOptions.None);
            string coefficient = subParts.Length > 1 ? subParts[0] : "0";
            leftPart = subParts.Length > 1 ? subParts[1] : leftPart;
            coefficients[varIndex - 1] = string.IsNullOrEmpty(coefficient) || coefficient == "+" ? 1 :
                                       coefficient == "-" ? -1 : double.Parse(coefficient);
        }

        if (!double.TryParse(rightPart, out coefficients[variablesCount]))
            throw new ArgumentException("Неправильний формат правої частини: " + constraint);

        return coefficients;
    }

    static void FixFloatingPointErrorsInTable(double[,] matrix)
    {
        for (int row = 0; row < matrix.GetLength(0); row++)
        {
            for (int col = 0; col < matrix.GetLength(1); col++)
            {
                if (Math.Abs(matrix[row, col]) < 1e-10)
                {
                    matrix[row, col] = 0.00;
                }
            }
        }
    }

    static void DisplayNormalizedConstraints(object[,] simplexTable, int constraintsCount, int variablesCount, List<string> originalConstraints)
    {
        Console.WriteLine($"x[j] >= 0, j=1,{variablesCount}");
        Console.WriteLine("Перепишемо систему обмежень:");

        for (int row = 0; row < constraintsCount; row++)
        {
            bool isFirst = true;
            for (int col = 0; col < variablesCount; col++)
            {
                double value = -Convert.ToDouble(simplexTable[row + 1, col + 1]);

                if (isFirst)
                {
                    Console.Write($"{(Math.Abs(value) < 1e-10 ? "0.00" : $"{value:F2}")} * X[{col + 1}] ");
                    isFirst = false;
                }
                else
                {
                    string sign = value >= 0 ? "+" : "-";
                    double absValue = Math.Abs(value);
                    Console.Write($"{sign}{(Math.Abs(absValue) < 1e-10 ? "0.00" : $"{absValue:F2}")} * X[{col + 1}] ");
                }
            }

            double constantTerm = Convert.ToDouble(simplexTable[row + 1, variablesCount + 1]);
            string comparisonOperator = originalConstraints[row].Contains(">=") ? ">=" :
                                      originalConstraints[row].Contains("<=") ? ">=" : "=";

            Console.WriteLine($"{(constantTerm >= 0 ? "+" : "")}{constantTerm:F2} {comparisonOperator} 0");
        }
    }

    static void ComputeOptimalSolution(object[,] simplexTable)
    {
        RemoveDegenerateRowsAndColumns(ref simplexTable);

        bool hasFractional;
        int iteration = 0;

        do
        {
            Console.WriteLine($"\n- Ітерація #{++iteration} -");

            FindFeasibleSolution(ref simplexTable);

            Console.WriteLine("Знайдено опорний розв’язок:");
            DisplayOptimalVariableValues(simplexTable);

            Console.WriteLine("\n-Фаза оптимізації-\n");
            DisplaySimplexTable(simplexTable);

            bool unbounded = OptimizeSolutionByPivoting(simplexTable);
            if (unbounded) return;

            Console.WriteLine("Знайдено оптимальний розв’язок:");
            string solutionStr = DisplayOptimalVariableValues(simplexTable);

            double z = GetFinalObjectiveValue(simplexTable);
            Console.WriteLine(isMinimization ? $"Min (Z) = {z:F2}"
                                             : $"Max (Z) = {z:F2}");

            hasFractional = ProcessGomoryCutLoop(solutionStr, ref simplexTable);

        } while (hasFractional);
    }


    static bool ProcessGomoryCutLoop(string solutionString, ref object[,] simplexTable)
    {
        string[] solutionParts = solutionString.Trim('X', '(', ')').Split(';');
        double[] solutionValues = solutionParts.Select(x => Convert.ToDouble(x.Trim())).ToArray();

        double maxFraction = 0;
        int maxFractionIndex = -1;
        bool hasFractions = false;

        for (int i = 0; i < solutionValues.Length; i++)
        {
            double value = solutionValues[i];
            double integerPart = Math.Floor(value);
            double fraction = value - integerPart;

            if (fraction < 0) fraction += 1;

            if (fraction > 0)
            {
                hasFractions = true;
                if (fraction > maxFraction)
                {
                    maxFraction = fraction;
                    maxFractionIndex = i + 1;
                }
            }
        }

        if (hasFractions)
        {
            Console.WriteLine($"\nМаксимум функції: x{maxFractionIndex} = {maxFraction:F2}");
            simplexTable = AddGomoryCutConstraint(simplexTable, maxFractionIndex);
            Console.WriteLine("\nТаблиця після додавання відсікання Ґоморі:");
            DisplaySimplexTable(simplexTable);
        }
        else
        {
            Console.WriteLine("\nУсі змінні мають цілі значення.");
        }

        return hasFractions;
    }

    static void RemoveDegenerateRowsAndColumns(ref object[,] simplexTable)
    {
        while (true)
        {
            int pivotCol = FindZeroPivotColumn(simplexTable);
            if (pivotCol == -1) break;

            int pivotRow = SelectPivotRowForZeroColumn(simplexTable, pivotCol);
            if (pivotRow == -1)
            {
                Console.WriteLine("Не вдалося знайти розв’язувальний елемент.");
                return;
            }

            Console.WriteLine($"\nРозв’язувальний стовпчик: {simplexTable[0, pivotCol]}");
            Console.WriteLine($"Розв’язувальний рядок: {simplexTable[pivotRow, 0].ToString().Replace("=", "").Trim()}");
            PerformJordanStepGeneric(simplexTable, pivotRow, pivotCol);

            simplexTable = TrimEmptyColumns(simplexTable);
            DisplaySimplexTable(simplexTable);
        }
    }

    static void FindFeasibleSolution(ref object[,] simplexTable)
    {
        Console.WriteLine("\n-Пошук опорного розв’язку-");
        while (CheckNegativeRightHandSides(simplexTable))
        {
            int pivotRow = SelectPivotRowForNegativeB(simplexTable);
            int pivotCol = SelectPivotColumnForNegativeBRow(simplexTable, pivotRow);

            if (pivotRow == -1 || pivotCol == -1)
            {
                Console.WriteLine("Не вдалося знайти розв’язувальний елемент.");
                return;
            }

            Console.WriteLine($"\nРозв’язувальний стовпчик: {simplexTable[0, pivotCol]}");
            Console.WriteLine($"Розв’язувальний рядок: {simplexTable[pivotRow, 0].ToString().Replace("=", "").Trim()}");
            PerformJordanStepGeneric(simplexTable, pivotRow, pivotCol);
            DisplaySimplexTable(simplexTable);
        }
    }

    static object[,] AddGomoryCutConstraint(object[,] simplexTable, int variableIndex)
    {
        int rows = simplexTable.GetLength(0);
        int cols = simplexTable.GetLength(1);
        object[,] newTable = new object[rows + 1, cols];

        int insertRow = rows - 1;

        for (int i = 0, newI = 0; i < rows; i++, newI++)
        {
            if (i == insertRow) newI++;
            for (int j = 0; j < cols; j++)
            {
                newTable[newI, j] = simplexTable[i, j];
            }
        }

        newTable[insertRow, 0] = $"s{variableIndex}=";

        List<string> constraintParts = new List<string>();

        for (int j = 1; j < cols; j++)
        {
            if (simplexTable[variableIndex, j] is double value)
            {
                double integerPart = Math.Floor(value);
                double fraction = value - integerPart;
                if (fraction < 0) fraction += 1;

                newTable[insertRow, j] = -fraction;

                string colName = simplexTable[0, j].ToString();
                if (colName.StartsWith("-")) colName = colName.Substring(1);

                if (j < cols - 1)
                {
                    constraintParts.Add($"{fraction:F2} * {colName}");
                }
                else if (fraction != 0)
                {
                    constraintParts.Add($"(-{fraction:F2})");
                }
            }
            else
            {
                newTable[insertRow, j] = 0.0;
            }
        }

        Console.WriteLine($"s{variableIndex} = " + string.Join(" + ", constraintParts));
        return newTable;
    }

    static object[,] TrimEmptyColumns(object[,] simplexTable)
    {
        int rows = simplexTable.GetLength(0);
        int cols = simplexTable.GetLength(1);
        List<int> validCols = new List<int>();

        for (int j = 0; j < cols; j++)
        {
            string header = simplexTable[0, j]?.ToString().Trim();
            if (header != "0") validCols.Add(j);
        }

        object[,] newTable = new object[rows, validCols.Count];

        for (int i = 0; i < rows; i++)
        {
            for (int newJ = 0; newJ < validCols.Count; newJ++)
            {
                newTable[i, newJ] = simplexTable[i, validCols[newJ]];
            }
        }

        return newTable;
    }

    static int FindZeroPivotColumn(object[,] simplexTable)
    {
        int rows = simplexTable.GetLength(0) - 1;
        for (int i = 1; i < rows; i++)
        {
            string value = simplexTable[i, 0].ToString().Replace("=", "").Trim();
            if (double.TryParse(value, out double num) && num == 0)
            {
                for (int j = 1; j < simplexTable.GetLength(1) - 1; j++)
                {
                    if (double.TryParse(simplexTable[i, j].ToString(), out double colValue) && colValue > 0)
                    {
                        return j;
                    }
                }
            }
        }
        return -1;
    }

    static int SelectPivotRowForZeroColumn(object[,] simplexTable, int pivotCol)
    {
        int rows = simplexTable.GetLength(0) - 1;
        int pivotRow = -1;
        double minRatio = double.MaxValue;

        for (int i = 1; i < rows; i++)
        {
            if (double.TryParse(simplexTable[i, pivotCol].ToString(), out double colValue) && colValue > 0)
            {
                if (double.TryParse(simplexTable[i, simplexTable.GetLength(1) - 1].ToString(), out double constTerm))
                {
                    double ratio = constTerm / colValue;
                    if (ratio >= 0 && ratio < minRatio)
                    {
                        minRatio = ratio;
                        pivotRow = i;
                    }
                }
            }
        }

        return pivotRow;
    }

    static double[] ParseObjectiveFunctionCoefficients(string input, int variablesCount)
    {
        isMinimization = input.ToLower().Contains("min");

        string expr = input.Split("->")[0].Replace(" ", "");

        double[] raw = new double[variablesCount];
        for (int i = 1; i <= variablesCount; i++)
        {
            var parts = expr.Split($"x{i}", 2, StringSplitOptions.None);
            string term = parts.Length > 1 ? parts[0] : "0";
            expr = parts.Length > 1 ? parts[1] : expr;
            raw[i - 1] = string.IsNullOrEmpty(term) || term == "+" ? 1 :
                         term == "-" ? -1 :
                                                            double.Parse(term, CultureInfo.InvariantCulture);
        }

       
        if (isMinimization)
            raw = raw.Select(x => -x).ToArray();

        return raw.Select(x => -x).ToArray();
    }







    static double GetFinalObjectiveValue(object[,] table)
    {
        int r = table.GetLength(0) - 1;
        int c = table.GetLength(1) - 1;
        double zPrime = Convert.ToDouble(table[r, c]); 

       
        return isMinimization
             ? -zPrime
             : zPrime;
    }






    static bool OptimizeSolutionByPivoting(object[,] simplexTable)
    {
        while (true)
        {
            int pivotCol = FindPivotColumnInObjectiveRow(simplexTable);

            if (pivotCol == -1)
            {
                Console.WriteLine("У рядку цільової функції немає від’ємних коефіцієнтів");
                return false;
            }

            Console.WriteLine($"Розв’язувальний стовпчик: {simplexTable[0, pivotCol]}");

            int rows = simplexTable.GetLength(0) - 1;         
            double[] θ = new double[rows];
            int lastB = simplexTable.GetLength(1) - 1;         

            for (int i = 1; i < rows; i++)
            {
                double a_ik = Convert.ToDouble(simplexTable[i, pivotCol]);
                double b_i = Convert.ToDouble(simplexTable[i, lastB]);

                θ[i] = (a_ik <= 0) ? double.MaxValue
                                   : Math.Round(b_i / a_ik, 10);
            }

            double minθ = double.MaxValue;
            int pivotRow = -1;
            for (int i = 1; i < rows; i++)
                if (θ[i] < minθ) { minθ = θ[i]; pivotRow = i; }

            if (pivotRow == -1)          
            {
                Console.WriteLine("Розв'язок необмежений.");
                return true;
            }

            Console.WriteLine($"Розв'язувальний рядок: {simplexTable[pivotRow, 0].ToString()
                                               .Replace("=", "")
                                               .Trim()}");
            PerformJordanStepGeneric(simplexTable, pivotRow, pivotCol);
            DisplaySimplexTable(simplexTable);
        }
    }

    static int FindPivotColumnInObjectiveRow(object[,] simplexTable)
    {
        int zRow = simplexTable.GetLength(0) - 1;   
        int lastVar = simplexTable.GetLength(1) - 2;   
        const double eps = 1e-9;

        for (int j = 1; j <= lastVar; j++)
        {
            double coeff = Convert.ToDouble(simplexTable[zRow, j]);
           
            if (coeff < -eps)
                return j;
        }
        return -1;  
    }






    static void PerformJordanStepGeneric(object[,] simplexTable, int pivotRow, int pivotCol)
    {
        int rows = simplexTable.GetLength(0);
        int cols = simplexTable.GetLength(1);
        double pivotValue = Convert.ToDouble(simplexTable[pivotRow, pivotCol]);
        Console.WriteLine($"Розв'язувальний елемент: {pivotValue:F2}");

        simplexTable[pivotRow, pivotCol] = NormalizeTinyValue(1.0);

        for (int i = 1; i < rows; i++)
        {
            for (int j = 1; j < cols; j++)
            {
                if (i != pivotRow && j != pivotCol)
                {
                    double newValue = (Convert.ToDouble(simplexTable[i, j]) * pivotValue -
                                     Convert.ToDouble(simplexTable[i, pivotCol]) *
                                     Convert.ToDouble(simplexTable[pivotRow, j])) / pivotValue;
                    simplexTable[i, j] = NormalizeTinyValue(newValue);
                }
            }
        }

        for (int i = 1; i < rows; i++)
        {
            if (i != pivotRow)
            {
                simplexTable[i, pivotCol] = NormalizeTinyValue(-Convert.ToDouble(simplexTable[i, pivotCol]));
            }
        }

        for (int i = 1; i < rows; i++)
        {
            if (i != pivotRow)
            {
                simplexTable[i, pivotCol] = NormalizeTinyValue(Convert.ToDouble(simplexTable[i, pivotCol]) / pivotValue);
            }
        }

        for (int j = 1; j < cols; j++)
        {
            simplexTable[pivotRow, j] = NormalizeTinyValue(Convert.ToDouble(simplexTable[pivotRow, j]) / pivotValue);
        }

        UpdateVariableLabelsAfterPivot(simplexTable, pivotRow, pivotCol);
    }

    static double NormalizeTinyValue(double value)
    {
        return Math.Abs(value) < 1e-9 ? 0.0 : value;
    }

    static void UpdateVariableLabelsAfterPivot(object[,] simplexTable, int pivotRow, int pivotCol)
    {
        string colName = simplexTable[0, pivotCol].ToString();
        string rowName = simplexTable[pivotRow, 0].ToString();

        string cleanColName = colName.StartsWith("-") ? colName.Substring(1) : colName;
        string cleanRowName = rowName.StartsWith("-") ? rowName.Substring(1) : rowName;

        bool hasEquals = cleanRowName.Contains("=");

        if (hasEquals)
        {
            cleanRowName = cleanRowName.Replace("=", "").Trim();
        }

        simplexTable[0, pivotCol] = cleanRowName == "0" ? "0" : "-" + cleanRowName;
        simplexTable[pivotRow, 0] = cleanColName + (hasEquals ? "=" : "");
    }

    static string DisplayOptimalVariableValues(object[,] simplexTable)
    {
        int variablesCount = simplexTable.GetLength(1) - 2;
        double[] solution = new double[variablesCount];

        for (int i = 1; i < simplexTable.GetLength(0) - 1; i++)
        {
            string rowName = simplexTable[i, 0].ToString();

            if (!rowName.StartsWith("x") || !rowName.Contains("=")) continue;

            rowName = rowName.Replace("=", "").Trim();

            if (int.TryParse(rowName.Substring(1), out int index))
            {
                index -= 1;
                if (index >= 0 && index < solution.Length)
                {
                    solution[index] = Convert.ToDouble(simplexTable[i, simplexTable.GetLength(1) - 1]);
                }
            }
        }

        StringBuilder solutionBuilder = new StringBuilder("X(");
        for (int i = 0; i < solution.Length; i++)
        {
            solutionBuilder.Append(solution[i].ToString("F2"));
            if (i < solution.Length - 1) solutionBuilder.Append("; ");
        }
        solutionBuilder.Append(")");

        string result = solutionBuilder.ToString();
        Console.WriteLine(result);
        return result;
    }

    static bool CheckNegativeRightHandSides(object[,] simplexTable)
    {
        for (int i = 1; i < simplexTable.GetLength(0) - 1; i++)
        {
            if (Convert.ToDouble(simplexTable[i, simplexTable.GetLength(1) - 1]) < 0)
                return true;
        }
        return false;
    }

    static int SelectPivotRowForNegativeB(object[,] simplexTable)
    {
        int rows = simplexTable.GetLength(0) - 1;
        for (int i = 1; i < rows; i++)
        {
            if (Convert.ToDouble(simplexTable[i, simplexTable.GetLength(1) - 1]) < 0)
                return i;
        }
        return -1;
    }

    static int SelectPivotColumnForNegativeBRow(object[,] simplexTable, int pivotRow)
    {
        for (int j = 1; j < simplexTable.GetLength(1) - 1; j++)
        {
            if (Convert.ToDouble(simplexTable[pivotRow, j]) < 0)
                return j;
        }
        return -1;
    }

    static void DisplaySimplexTable(object[,] simplexTable)
    {
        int rows = simplexTable.GetLength(0);
        int cols = simplexTable.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (simplexTable[i, j] is double)
                {
                    double value = Convert.ToDouble(simplexTable[i, j]);
                    string formatted = value == -0.00 ? "0.00" : value.ToString("F2");
                    Console.Write($"{formatted}\t");
                }
                else
                {
                    Console.Write($"{simplexTable[i, j]}\t");
                }
            }
            Console.WriteLine();
        }
    }
}
