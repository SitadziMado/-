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
        private struct GraphInfo
        {
            public int Raduis { get; set; }
            public int Diameter { get; set; }
            public List<int> Centers { get; set; }
            public List<Tuple<int, int>> Bridges { get; set; }
            public int ChromaticNumber { get; set; }
        }

        // private List<List<int>> mMatrix;
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

        private GraphInfo GetGraphInfo()
        {
            var d = FillMatrix();

            for (int i = 0; i < mWidth; ++i)
                for (int j = 0; j < mWidth; ++j)
                    if (i != j && d[i][j] == 0)
                        d[i][j] = 1000000;

                    for (int k = 0; k < mWidth; ++k)
                for (int i = 0; i < mWidth; ++i)
                    for (int j = 0; j < mWidth; ++j)
                        d[i][j] = Math.Min(d[i][j], d[i][k] + d[k][j]);

            var distances = (
                from v
                in d
                select v.Max()
            ).ToList();
            
            var min = distances.Min();
            var max = distances.Max();

            var centers = (
                from v
                in distances
                where v == min
                select v
            );

            return new GraphInfo {
                Raduis = min,
                Diameter = max,
                Bridges = new List<Tuple<int, int>>(),
                Centers = centers.ToList(),
                ChromaticNumber = 0
            };
        }

        void IsBridge(int from, int to) 
            => mBridges.Add(new Tuple<int, int>(from, to));

        void Dfs(List<List<int>> graph, int v, int p = -1)
        {
            mUsed[v] = true;
            mTin[v] = mFup[v] = mTimer++;

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

        List<Tuple<int, int>> FindBridges(List<List<int>> graph)
        {
            mTimer = 0;
            mBridges.Clear();
            mUsed.Clear();
            mTin.Clear();
            mFup.Clear();

            for (int i = 0; i < mWidth; ++i)
            {
                mUsed.Add(false);
                mTin.Add(0);
                mFup.Add(1);
            }

            for (int i = 0; i < mWidth; ++i)
                if (!mUsed[i])
                    Dfs(graph, i);

            return mBridges;
        }

        int ChromaticNumber(List<List<int>> graph)
        {
           var colors = new List<int>(mWidth);

            for (int i = 0; i < mWidth; ++i)
                colors.Add(-1);

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

            return colors.Max();
        }

        private void GenerateButton_Click(object sender, EventArgs e)
        {
            GenerateMatrix();
        }

        private void ResultsButton_Click(object sender, EventArgs e)
        {
            var graph = FillMatrix();

            var gi = GetGraphInfo();
            gi.Bridges = FindBridges(graph);
            gi.ChromaticNumber = ChromaticNumber(graph);

            TabControl.SelectedTab = TabControl.TabPages[1];
        }
    }
}
