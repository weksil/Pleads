using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Pleyads
{
    public class Table
    {
        private const string SelectColor = "#A5B92E";
        private const string DefaultColor = "#FFD073";
        public List<Cell> Cells = new List<Cell>();
        private float maxValue;

        public Table(float[][] values, string[] headers, float maxValue)
        {
            if (values.GetLength(0) != headers.Length) throw new Exception();
            Cell t;
            Cells.Add(new Cell(0, 0));
            string h;
            int maxLen = 7;
            this.maxValue = maxValue;
            for (int i = 0; i < headers.Length; i++)
            {
                h = headers[i];
                if (h.Length > maxLen)
                    h = h.Substring(0, maxLen);
                Cells.Add(new Cell(i + 1, 0) { Value = h });
            }
            for (int i = 0; i < values.Length; i++)
            {
                h = headers[i];
                if (h.Length > maxLen)
                    h = h.Substring(0, maxLen);
                Cells.Add(new Cell(0, i + 1) { Value = h });

                for (int j = 0; j < values[i].Length; j++)
                {
                    t = new Cell(j + 1, i + 1);

                    if (Math.Abs(values[i][j]) > maxValue)
                    {
                        t.Value = values[i][j].ToString();
                        t.Number = values[i][j];
                        t.Color = DefaultColor;
                    }
                    else
                    {
                        t.Value = "";
                        t.Number = 0;
                    }

                    Cells.Add(t);
                }
            }
        }

        public void SelectRow(int row)
        {
            var t = Cells.Where(x => x.CellIndex.Y == row).Skip(1);
            foreach (var item in t)
            {
                item.Update(SelectColor);
            }
        }

        public void UnselectRow(int row)
        {
            var t = Cells.Where(x => x.CellIndex.Y == row).Skip(1);
            foreach (var item in t)
            {
                item.Clear(maxValue, DefaultColor);
            }
        }

        public void SelectCollumn(int coll)
        {
            var t = Cells.Where(x => x.CellIndex.X == coll).Skip(1);
            foreach (var item in t)
            {
                item.Update(SelectColor);
            }
        }

        public void UnselectCollumn(int coll)
        {
            var t = Cells.Where(x => x.CellIndex.X == coll).Skip(1);
            foreach (var item in t)
            {
                item.Clear(maxValue, DefaultColor);
            }
        }

        public void ClearSelect()
        {
        }
    }

    public class Cell : INotifyPropertyChanged
    {
        public const string DefaultColor = "#fff";
        public string Value { get; set; }
        public string Color { get; set; } = DefaultColor;
        public Vector Pos { get; set; }

        private const int SizeX = 50;
        private const int SizeY = 20;
        public Vector2 CellIndex;
        public float Number = -2;
        public bool isSelected;

        public event PropertyChangedEventHandler PropertyChanged;

        public Cell(int x, int y)
        {
            Pos = new Vector(SizeX * x, SizeY * y);
            CellIndex = new Vector2(x, y);
        }

        public void Update(string newColor)
        {
            Color = newColor;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(Color)));
        }

        public void Clear(float maxValue, string defaulColor)
        {
            if (Math.Abs(Number) < maxValue)
                Update(DefaultColor);
            else
                Update(defaulColor);
        }
    }

    public class Vector2
    {
        public int X;
        public int Y;

        public Vector2(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}