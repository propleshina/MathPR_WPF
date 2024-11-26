using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TransportationProblem
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SolveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получение данных из полей ввода
                int[] supply = Array.ConvertAll(SupplyTextBox.Text.Split(' '), int.Parse);
                int[] demand = Array.ConvertAll(DemandTextBox.Text.Split(' '), int.Parse);
                string[] costLines = CostsTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                // Определяем количество потребителей и продавцов
                int numConsumers = demand.Length;
                int numSuppliers = supply.Length;
                int[,] costs = new int[numConsumers, numSuppliers];

                // Заполнение массива costs
                for (int i = 0; i < numConsumers; i++)
                {
                    // Проверка на наличие достаточного количества строк
                    if (i < costLines.Length)
                    {
                        int[] costRow = Array.ConvertAll(costLines[i].Split(' '), int.Parse);

                        // Проверка на количество элементов в строке
                        if (costRow.Length != numSuppliers)
                        {
                            throw new Exception($"Ошибка: строка {i + 1} содержит {costRow.Length} элементов, ожидается {numSuppliers}.");
                        }

                        for (int j = 0; j < numSuppliers; j++)
                        {
                            costs[i, j] = costRow[j];
                        }
                    }
                    else
                    {
                        throw new Exception($"Ошибка: недостаточно строк для матрицы затрат. Ожидалось {numConsumers}, получено {costLines.Length}.");
                    }
                }

                // Выбор метода
                if (MethodComboBox.SelectedIndex == 0)
                {
                    ResultTextBlock.Text = NorthwestCornerMethod(costs, supply, demand);
                }
                else if (MethodComboBox.SelectedIndex == 1)
                {
                    ResultTextBlock.Text = MinimalElementsMethod(costs, supply, demand);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
            }
        }


        private string NorthwestCornerMethod(int[,] costs, int[] supply, int[] demand)
        {
            Console.WriteLine("\nМЕТОД СЕВЕРО-ЗАПАДНОГО УГЛА\n");
            int numConsumers = costs.GetLength(0);
            int numSuppliers = costs.GetLength(1);
            int[,] allocation = new int[numConsumers, numSuppliers];

            int totalCost = 0;
            StringBuilder resultBuilder = new StringBuilder(); // Используем StringBuilder для сбора результатов

            int i = 0, j = 0;

            while (i < numConsumers && j < numSuppliers)
            {
                // Минимум между предложением и спросом
                int amount = Math.Min(supply[j], demand[i]);
                allocation[i, j] = amount;
                totalCost += amount * costs[i, j]; // Подсчет стоимости
                supply[j] -= amount;
                demand[i] -= amount;

                // Если предложение исчерпано, переходим к следующему поставщику
                if (supply[j] == 0)
                {
                    j++;
                }
                // Если спрос исчерпан, переходим к следующему потребителю
                else if (demand[i] == 0)
                {
                    i++;
                }
            }

            // Добавляем распределение в результат
            resultBuilder.AppendLine("Распределение ресурсов:");
            for (int row = 0; row < allocation.GetLength(0); row++)
            {
                for (int col = 0; col < allocation.GetLength(1); col++)
                {
                    resultBuilder.Append(allocation[row, col] + "\t");
                }
                resultBuilder.AppendLine();
            }

            resultBuilder.AppendLine($"Общая стоимость: {totalCost}\n");

            return resultBuilder.ToString(); // Возвращаем собранный результат
        }

        private string MinimalElementsMethod(int[,] costs, int[] supply, int[] demand)
        {
            Console.WriteLine("\nМЕТОД МИНИМАЛЬНЫХ ЭЛЕМЕНТОВ\n");
            int numConsumers = costs.GetLength(0);
            int numSuppliers = costs.GetLength(1);
            int[,] allocation = new int[numConsumers, numSuppliers];

            int totalCost = 0;
            StringBuilder resultBuilder = new StringBuilder(); // Используем StringBuilder для сбора результатов

            while (true)
            {
                // Находим минимальную стоимость
                int minCost = int.MaxValue;
                int minRow = -1, minCol = -1;

                for (int i = 0; i < numConsumers; i++)
                {
                    for (int j = 0; j < numSuppliers; j++)
                    {
                        if (allocation[i, j] == 0 && costs[i, j] < minCost && supply[j] > 0 && demand[i] > 0)
                        {
                            minCost = costs[i, j];
                            minRow = i;
                            minCol = j;
                        }
                    }
                }

                // Если не найдено минимальных элементов, выходим из цикла
                if (minRow == -1 || minCol == -1) break;

                // Минимум между спросом и предложением
                int amount = Math.Min(supply[minCol], demand[minRow]);
                allocation[minRow, minCol] = amount;
                totalCost += amount * costs[minRow, minCol]; // Подсчет стоимости
                supply[minCol] -= amount;
                demand[minRow] -= amount;

                // Проверяем оставшееся предложение и спрос
                if (supply[minCol] == 0)
                {
                    // Убираем столбец из рассмотрения
                    for (int i = 0; i < numConsumers; i++)
                    {
                        costs[i, minCol] = int.MaxValue; // Убираем столбец из рассмотрения
                    }
                }

                if (demand[minRow] == 0)
                {
                    // Убираем строку из рассмотрения
                    for (int j = 0; j < numSuppliers; j++)
                    {
                        costs[minRow, j] = int.MaxValue; // Убираем строку из рассмотрения
                    }
                }
            }

            // Добавляем распределение в результат
            resultBuilder.AppendLine("Распределение ресурсов:");
            for (int i = 0; i < allocation.GetLength(0); i++)
            {
                for (int j = 0; j < allocation.GetLength(1); j++)
                {
                    resultBuilder.Append(allocation[i, j] + "\t");
                }
                resultBuilder.AppendLine();
            }

            resultBuilder.AppendLine($"Общая стоимость: {totalCost}\n");

            return resultBuilder.ToString(); // Возвращаем собранный результат
        }


        static void PrintAllocation(int[,] allocation)
        {
            Console.WriteLine("Распределение ресурсов:");
            for (int i = 0; i < allocation.GetLength(0); i++)
            {
                for (int j = 0; j < allocation.GetLength(1); j++)
                {
                    Console.Write(allocation[i, j] + "\t");
                }
                Console.WriteLine();
            }
        }
    }
}