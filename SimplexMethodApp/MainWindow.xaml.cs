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

        // Загрузка данных
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var lines = File.ReadAllLines(openFileDialog.FileName);
                    TextBoxA.Text = lines[0];
                    TextBoxB.Text = lines[1];
                    TextBoxC.Text = lines[2];
                    if (lines.Length > 3)
                    {
                        ResultTextBox.Text = string.Join(Environment.NewLine, lines.Skip(3));
                    }
                    MessageBox.Show("Данные успешно загружены!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки файла: " + ex.Message);
                }
            }
        }

        // Сохранение данных и результата
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files (*.txt)|*.txt";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string content = $"{TextBoxA.Text}\n{TextBoxB.Text}\n{TextBoxC.Text}\n\nРезультат:\n{ResultTextBox.Text}";
                    File.WriteAllText(saveFileDialog.FileName, content);
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
                string[] AStrings = TextBoxA.Text.Split(';');
                string[] bStrings = TextBoxB.Text.Split(';');
                string[] cStrings = TextBoxC.Text.Split(';');

                int m = AStrings.Length;
                int n = AStrings[0].Split(',').Length;

                if (bStrings.Length != m)
                {
                    MessageBox.Show($"Количество строк в A ({m}) не совпадает с размерностью вектора b ({bStrings.Length})");
                    return;
                }

                if (cStrings.Length != n)
                {
                    MessageBox.Show($"Количество столбцов в A ({n}) не совпадает с размерностью вектора c ({cStrings.Length})");
                    return;
                }

                double[,] A = new double[m, n];
                for (int i = 0; i < m; i++)
                {
                    string[] row = AStrings[i].Split(',');
                    for (int j = 0; j < n; j++)
                    {
                        A[i, j] = double.Parse(row[j]);
                    }
                }

                double[] b = bStrings.Select(double.Parse).ToArray();
                double[] c = cStrings.Select(double.Parse).ToArray();

                var result = SimplexMethod(A, b, c);

                ResultTextBox.Text = $"Оптимальное решение: {string.Join(", ", result.Item1)}\n";
                ResultTextBox.Text += $"Значение целевой функции: {result.Item2}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        // Симплекс-метод
        public Tuple<double[], double> SimplexMethod(double[,] A, double[] b, double[] c)
        {
            int m = b.Length;
            int n = c.Length;
            double[,] tableau = new double[m + 1, n + m + 1];

            for (int i = 0; i < m; i++)
                tableau[i, n + m] = b[i];

            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    tableau[i, j] = A[i, j];

            for (int i = 0; i < n; i++)
                tableau[m, i] = -c[i];

            while (true)
            {
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

                if (pivotCol == -1) break;

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

                if (pivotRow == -1) throw new Exception("Решение не существует (нерациональное или неограниченное).");

                double pivot = tableau[pivotRow, pivotCol];
                for (int i = 0; i < n + m + 1; i++)
                    tableau[pivotRow, i] /= pivot;

                for (int i = 0; i < m + 1; i++)
                {
                    if (i != pivotRow)
                    {
                        double factor = tableau[i, pivotCol];
                        for (int j = 0; j < n + m + 1; j++)
                            tableau[i, j] -= factor * tableau[pivotRow, j];
                    }
                }
            }

            double[] solution = new double[n];
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (Math.Abs(tableau[i, j] - 1) < 1e-6 && Enumerable.Range(0, m).Count(x => Math.Abs(tableau[x, j]) > 1e-6) == 1)
                    {
                        solution[j] = tableau[i, n + m];
                        break;
                    }
                }
            }

            double objectiveValue = tableau[m, n + m];
            return Tuple.Create(solution, objectiveValue);
        }
    }
}
