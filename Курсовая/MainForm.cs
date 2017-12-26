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
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Формируем матрицу смежности, заполненную нулями.
        /// </summary>
        private void GenerateMatrix()
        {
            var width = (int)WidthUpDown.Value;

            // Задаем размеры матрицы смежности
            Matrix.ColumnCount = width;
            Matrix.RowCount = 0;

            var row = new List<string>();

            // Заполняем нулями
            for (int j = 0; j < width; ++j)
                row.Add("0");

            var array = row.ToArray();

            // Добавляем строки в массив
            for (int i = 0; i < width; ++i)
                Matrix.Rows.Add(array);

            // Задаем размеры ячеек
            for (int i = 0; i < width; ++i)
            {
                Matrix.Columns[i].Width = 22;
                Matrix.Columns[i].HeaderText = (i + 1).ToString();
                Matrix.Rows[i].HeaderCell.Value = (i + 1).ToString();
            }
        }

        private void GenerateButton_Click(object sender, EventArgs e)
        {
            GenerateMatrix();
        }

        private void ResultsButton_Click(object sender, EventArgs e)
        {
            TabControl.SelectedTab = TabControl.TabPages[1];
        }
    }
}
