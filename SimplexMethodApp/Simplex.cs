using System;

public class SimplexMethod
{
    public static (double[], double) Solve(double[,] A, double[] b, double[] c)
    {
        int m = A.GetLength(0);
        int n = A.GetLength(1);

        // Создание начальной симплекс-таблицы
        double[,] table = new double[m + 1, n + m + 1];

        // Заполнение таблицы
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                table[i, j] = A[i, j];
            }
            table[i, n + i] = 1;  // единичные элементы для базисных переменных
            table[i, n + m] = b[i]; // правые части
        }

        // Заполнение последней строки с целевой функцией
        for (int j = 0; j < n; j++)
        {
            table[m, j] = -c[j];
        }

        // Симплекс-метод
        while (true)
        {
            // Проверка на оптимальность
            int pivotCol = -1;
            for (int j = 0; j < n + m; j++)
            {
                if (table[m, j] < 0)
                {
                    pivotCol = j;
                    break;
                }
            }

            // Если все элементы последней строки >= 0, задача решена
            if (pivotCol == -1) break;

            // Находим строку для поворота
            int pivotRow = -1;
            double minRatio = double.MaxValue;

            for (int i = 0; i < m; i++)
            {
                if (table[i, pivotCol] > 0)
                {
                    double ratio = table[i, n + m] / table[i, pivotCol];
                    if (ratio < minRatio)
                    {
                        minRatio = ratio;
                        pivotRow = i;
                    }
                }
            }

            if (pivotRow == -1)
                throw new Exception("Задача неограничена!");

            // Поворот (пивотирование)
            Pivot(table, pivotRow, pivotCol);
        }

        // Получаем решение
        double[] solution = new double[n];
        for (int i = 0; i < m; i++)
        {
            if (table[i, n + i] == 1)
            {
                solution[i] = table[i, n + m];
            }
            else
            {
                solution[i] = 0;
            }
        }

        double objectiveValue = table[m, n + m];
        return (solution, objectiveValue);
    }

    private static void Pivot(double[,] table, int pivotRow, int pivotCol)
    {
        int m = table.GetLength(0) - 1;
        int n = table.GetLength(1) - 1;

        double pivotValue = table[pivotRow, pivotCol];
        // Делим всю строку на элемент в pivot
        for (int j = 0; j <= n; j++)
        {
            table[pivotRow, j] /= pivotValue;
        }

        // Обновляем все остальные строки
        for (int i = 0; i <= m; i++)
        {
            if (i != pivotRow)
            {
                double factor = table[i, pivotCol];
                for (int j = 0; j <= n; j++)
                {
                    table[i, j] -= factor * table[pivotRow, j];
                }
            }
        }
    }

    private static void PrintTable(double[,] table)
    {
        int m = table.GetLength(0) - 1;
        int n = table.GetLength(1) - 1;

        for (int i = 0; i <= m; i++)
        {
            for (int j = 0; j <= n; j++)
            {
                Console.Write(table[i, j].ToString("F2") + "\t");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }
}
