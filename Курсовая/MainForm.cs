using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Курсовая
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// Структура данных для хранения информации о графе.
        /// </summary>
        private struct GraphInfo
        {
            public int Raduis { get; set; }
            public int Diameter { get; set; }
            public List<int> Centers { get; set; }
            public List<Tuple<int, int>> Bridges { get; set; }
            public int ChromaticNumber { get; set; }
        }

        // Задаём переменные для класса.
        private int mWidth = -1;
        private int mTimer;
        private List<bool> mUsed = new List<bool>();
        private List<int> mTin = new List<int>();
        private List<int> mFup = new List<int>();
        private List<Tuple<int, int>> mBridges = new List<Tuple<int, int>>();

        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Формируем матрицу смежности, заполненную нулями.
        /// </summary>
        private void GenerateMatrix()
        {
            mWidth = (int)WidthUpDown.Value;

            // Задаем размеры матрицы смежности
            Matrix.ColumnCount = mWidth;
            Matrix.RowCount = 0;

            var row = new List<string>();

            // Заполняем нулями
            for (int j = 0; j < mWidth; ++j)
                row.Add("0");

            var array = row.ToArray();

            // Добавляем строки в массив
            for (int i = 0; i < mWidth; ++i)
                Matrix.Rows.Add(array);

            // Задаем размеры ячеек
            for (int i = 0; i < mWidth; ++i)
            {
                Matrix.Columns[i].Width = 22;
                Matrix.Columns[i].HeaderText = (i + 1).ToString();
                Matrix.Rows[i].HeaderCell.Value = (i + 1).ToString();
            }
        }

        /// <summary>
        /// Заполнить матрицу значениями из DataGridView.
        /// </summary>
        /// <returns>Заполненная матрица.</returns>
        private List<List<int>> FillMatrix()
        {
            var m = new List<List<int>>(mWidth);

            for (int i = 0; i < mWidth; ++i)
            {
                m.Add(new List<int>(mWidth));

                for (int j = 0; j < mWidth; ++j)
                    m[i].Add(int.Parse((string)Matrix.Rows[i].Cells[j].Value));
            }

            return m;
        }

        /// <summary>
        /// Выяснить диаметр, радиус, центральные вершины.
        /// </summary>
        /// <returns>Структура с информацией.</returns>
        private GraphInfo GetGraphInfo()
        {
            var d = FillMatrix();

            // Заполняем отсутствующие ребра большими числами.
            for (int i = 0; i < mWidth; ++i)
                for (int j = 0; j < mWidth; ++j)
                    if (i != j && d[i][j] == 0)
                        d[i][j] = 1000000;

            // Алгоритм Флойда-Уоршелла
            for (int k = 0; k < mWidth; ++k)
                for (int i = 0; i < mWidth; ++i)
                    for (int j = 0; j < mWidth; ++j)
                        d[i][j] = Math.Min(d[i][j], d[i][k] + d[k][j]);

            // Создаем список дистанций.
            var distances = (
                from v
                in d
                select v.Max()
            ).ToList();

            // Находим радиус и диаметр.
            var min = distances.Min();
            var max = distances.Max();

            // Составляем список центральных вершин.
            var centers = (
                from v
                in distances
                where v == min
                select v
            );

            // Возвращаем структуру, содержащую информацию о графе.
            return new GraphInfo
            {
                Raduis = min,
                Diameter = max,
                Bridges = new List<Tuple<int, int>>(),
                Centers = centers.ToList(),
                ChromaticNumber = 0
            };
        }

        /// <summary>
        /// Добавляем мост в коллекцию.
        /// </summary>
        /// <param name="from">Откуда.</param>
        /// <param name="to">Куда.</param>
        void IsBridge(int from, int to) 
            => mBridges.Add(new Tuple<int, int>(from, to));

        /// <summary>
        /// Поиск в глубину для нахождения мостов.
        /// </summary>
        /// <param name="graph">Граф для обработки.</param>
        /// <param name="v">Текущая вершина.</param>
        /// <param name="p">Предыдущая вершина.</param>
        void Dfs(List<List<int>> graph, int v, int p = -1)
        {
            mUsed[v] = true;
            mTin[v] = mFup[v] = mTimer++; // Время вхождения в текущую вершину.

            // Обходим соседей.
            for (int i = 0; i < mWidth; ++i)
            {
                int to;

                if (graph[v][i] == 1)
                    to = i;
                else
                    continue;

                if (to == p)
                    continue;

                if (mUsed[to])
                {
                    mFup[v] = Math.Min(mFup[v], mTin[to]);
                }
                else
                {
                    Dfs(graph, to, v);
                    mFup[v] = Math.Min(mFup[v], mFup[to]);

                    if (mFup[to] > mTin[v])
                        IsBridge(v, to);
                }
            }
        }

        /// <summary>
        /// Найти мосты в заданном графе.
        /// </summary>
        /// <param name="graph">Граф, заданный матрицей смежности.</param>
        /// <returns>Список мостов.</returns>
        List<Tuple<int, int>> FindBridges(List<List<int>> graph)
        {
            // Обнуляем переменные и очищаем массивы.
            mTimer = 0;
            mBridges.Clear();
            mUsed.Clear();
            mTin.Clear();
            mFup.Clear();

            // Заполняем массивы нулями.
            for (int i = 0; i < mWidth; ++i)
            {
                mUsed.Add(false);
                mTin.Add(0);
                mFup.Add(0);
            }

            // Заупскаем поиск в глубину из каждой вершины.
            for (int i = 0; i < mWidth; ++i)
                if (!mUsed[i])
                    Dfs(graph, i);

            return mBridges;
        }

        /// <summary>
        /// Найти хроматическое число графа.
        /// </summary>
        /// <param name="graph">Граф.</param>
        /// <returns>Хроматическое число.</returns>
        int ChromaticNumber(List<List<int>> graph)
        {
            var colors = new List<int>(mWidth);

            // Заполняем массив цветов "пустыми" значениями.
            for (int i = 0; i < mWidth; ++i)
                colors.Add(-1);

            // Поочерёдно расскрашиваем вершины графа, учитывая инцидентные вершины.
            for (int i = 0; i < mWidth; ++i)
            {
                var cols = new SortedSet<int>();

                for (int j = 0; j < mWidth; ++j)
                    if (graph[i][j] == 1)
                        cols.Add(colors[j]);

                int color = 0;

                while (cols.Contains(color))
                    ++color;

                colors[i] = color;
            }

            return colors.Max() + 1;
        }

        private void GenerateButton_Click(object sender, EventArgs e)
        {
            GenerateMatrix();
        }

        private void ResultsButton_Click(object sender, EventArgs e)
        {
            // Приводим информацию о графе в формат вывода.
            var graph = FillMatrix();

            var gi = GetGraphInfo();
            gi.Bridges = FindBridges(graph);
            gi.ChromaticNumber = ChromaticNumber(graph);

            var sb = new StringBuilder();

            foreach (var v in gi.Bridges)
                sb.Append($"{v.Item1}-{v.Item2}, ");

            if (sb.Length > 0)
                sb.Length -= 2;

            var bridgeString = sb.ToString();

            sb = new StringBuilder();

            foreach (var v in gi.Centers)
                sb.Append($"{v}, ");

            if (sb.Length > 0)
                sb.Length -= 2;

            var centersString = sb.ToString();

            // Вывести gi.Raduis 
            // Вывести gi.Diameter
            // Вывести gi.ChromaticNumber
            // Вывести bridgeString
            // Вывести centersString

            TabControl.SelectedTab = TabControl.TabPages[1];
        }

        private void Matrix_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int num;

            DataGridViewCell c = null;
            
            try
            {
                c = Matrix.Rows[e.RowIndex].Cells[e.ColumnIndex];

                if (e.RowIndex == e.ColumnIndex)
                {
                    c.Value = "0";
                    return;
                }

                num = int.Parse((string)c.Value);

                if (num == 0)
                    c.Value = "0";
                else
                    c.Value = "1";
            }
            catch (FormatException)
            {
                c.Value = "1";
            }
            catch (ArgumentOutOfRangeException)
            {

            }
        }
    }
}
