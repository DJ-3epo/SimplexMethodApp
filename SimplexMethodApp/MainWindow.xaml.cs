using System;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;

namespace SimplexMethodApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Метод для загрузки данных из файла
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var lines = File.ReadAllLines(openFileDialog.FileName);

                    // Загрузка матрицы A
                    TextBoxA.Text = lines[0];

                    // Загрузка вектора b
                    TextBoxB.Text = lines[1];

                    // Загрузка вектора c
                    TextBoxC.Text = lines[2];

                    MessageBox.Show("Данные успешно загружены!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки файла: " + ex.Message);
                }
            }
        }

        // Метод для сохранения входных данных и результатов в файл
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files (*.txt)|*.txt";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Получаем входные данные
                    string aData = TextBoxA.Text;
                    string bData = TextBoxB.Text;
                    string cData = TextBoxC.Text;

                    // Получаем результат
                    string result = ResultTextBox.Text;

                    // Сохраняем данные в файл
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                    {
                        // Сохраняем матрицу A
                        writer.WriteLine("Матрица A:");
                        writer.WriteLine(aData);

                        // Сохраняем вектор b
                        writer.WriteLine("Вектор b:");
                        writer.WriteLine(bData);

                        // Сохраняем вектор c
                        writer.WriteLine("Вектор c:");
                        writer.WriteLine(cData);

                        // Сохраняем результат
                        writer.WriteLine("Результат:");
                        writer.WriteLine(result);
                    }

                    MessageBox.Show("Данные и результат успешно сохранены!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка сохранения файла: " + ex.Message);
                }
            }
        }

        private void SolveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем данные с текстовых полей
                string[] AStrings = TextBoxA.Text.Split(';');
                string[] bStrings = TextBoxB.Text.Split(';');
                string[] cStrings = TextBoxC.Text.Split(';');

                // Проверка: матрица A
                int m = AStrings.Length;
                int n = AStrings[0].Split(',').Length;

                // Проверка: размерность вектора b
                if (bStrings.Length != m)
                {
                    MessageBox.Show($"Количество строк в A ({m}) не совпадает с размерностью вектора b ({bStrings.Length})");
                    return;
                }

                // Проверка: размерность вектора c
                if (cStrings.Length != n)
                {
                    MessageBox.Show($"Количество столбцов в A ({n}) не совпадает с размерностью вектора c ({cStrings.Length})");
                    return;
                }

                // Создание матрицы A
                double[,] A = new double[m, n];
                for (int i = 0; i < m; i++)
                {
                    string[] row = AStrings[i].Split(',');
                    for (int j = 0; j < n; j++)
                    {
                        A[i, j] = double.Parse(row[j]);
                    }
                }

                // Создание вектора b
                double[] b = new double[m];
                for (int i = 0; i < m; i++)
                {
                    b[i] = double.Parse(bStrings[i]);
                }

                // Создание вектора c
                double[] c = new double[n];
                for (int i = 0; i < n; i++)
                {
                    c[i] = double.Parse(cStrings[i]);
                }

                // Решение задачи с помощью метода Симплекса
                var result = SimplexMethod(A, b, c);

                // Вывод результата
                ResultTextBox.Text = $"Оптимальное решение: {string.Join(", ", result.Item1)}\n";
                ResultTextBox.Text += $"Значение целевой функции: {result.Item2}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        // Метод решения задачи методом Симплекса
        public Tuple<double[], double> SimplexMethod(double[,] A, double[] b, double[] c)
        {
            int m = b.Length;
            int n = c.Length;

            // Инициализация симплекс-таблицы
            double[,] tableau = new double[m + 1, n + m + 1];

            // Заполнение столбца правых частей
            for (int i = 0; i < m; i++)
            {
                tableau[i, n + m] = b[i];
            }

            // Заполнение таблицы для A
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    tableau[i, j] = A[i, j];
                }
            }

            // Заполнение целевой функции (с учетом знака)
            for (int i = 0; i < n; i++)
            {
                tableau[m, i] = -c[i]; // Минус для максимизации
            }

            // Симплексный метод
            while (true)
            {
                // Поиск столбца с наибольшим отрицательным значением в последней строке
                int pivotCol = -1;
                double minVal = 0;
                for (int i = 0; i < n + m; i++)
                {
                    if (tableau[m, i] < minVal)
                    {
                        minVal = tableau[m, i];
                        pivotCol = i;
                    }
                }

                if (pivotCol == -1) break; // Оптимальное решение найдено

                // Поиск строки для поворота
                int pivotRow = -1;
                double minRatio = double.MaxValue;
                for (int i = 0; i < m; i++)
                {
                    if (tableau[i, pivotCol] > 0)
                    {
                        double ratio = tableau[i, n + m] / tableau[i, pivotCol];
                        if (ratio < minRatio)
                        {
                            minRatio = ratio;
                            pivotRow = i;
                        }
                    }
                }

                // Поворот таблицы
                double pivot = tableau[pivotRow, pivotCol];
                for (int i = 0; i < n + m + 1; i++)
                {
                    tableau[pivotRow, i] /= pivot;
                }

                for (int i = 0; i < m + 1; i++)
                {
                    if (i != pivotRow)
                    {
                        double factor = tableau[i, pivotCol];
                        for (int j = 0; j < n + m + 1; j++)
                        {
                            tableau[i, j] -= factor * tableau[pivotRow, j];
                        }
                    }
                }
            }

            // Извлечение решения из таблицы
            double[] solution = new double[n];
            for (int i = 0; i < n; i++)
            {
                solution[i] = 0;
            }

            for (int i = 0; i < m; i++)
            {
                if (tableau[i, n + m] != 0)
                {
                    solution[i] = tableau[i, n + m];
                }
            }

            // Целевая функция
            double objectiveValue = tableau[m, n + m];

            return Tuple.Create(solution, objectiveValue);
        }
    }
}
