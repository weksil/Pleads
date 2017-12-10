using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace Pleyads
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PleyadsControll controller;

        public MainWindow()
        {
            InitializeComponent();
            FilePath.Text = @"../../data/crosscor.txt";
            controller = new PleyadsControll();
            controller.Start();
        }

        private void InitTable()
        {
            var table = controller.CreateTable();
            dgNodeTable.ItemsSource = table.Cells;
        }

        private void Download(object sender, RoutedEventArgs e)
        {
            controller.EpsEdge = float.Parse(Eps.Text.Replace(".", ","));

            if (!controller.Download(FilePath.Text, BadPathFile)) return;
            NodesPanel.ItemsSource = controller.Nodes;
            EdgesPanel.ItemsSource = controller.Edges;
            ClasterPanel.ItemsSource = controller.Clasters;
            InitTable();
            UpdateView();
        }

        private void BadPathFile()
        {
            MessageBox.Show("Файл отсутствует");
        }

        private void SelectItem(object sender, MouseButtonEventArgs e)
        {
            IMoveable obj = (e.Source as Shape).DataContext as IMoveable;
            controller.SelectItem(obj);
        }

        private void UpdateEps(object sender, RoutedEventArgs e)
        {
            if (!controller.IsInit) return;
            controller.EpsEdge = float.Parse(Eps.Text.Replace(".", ","));
            controller.RebuildPleyads();
            InitTable();
            UpdateView();
        }

        private void UpdateView()
        {
            NodesPanel.Items.Refresh();
            EdgesPanel.Items.Refresh();
            ClasterPanel.Items.Refresh();
        }

        private void Ellipse_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0) return;
            var claster = (sender as Shape).DataContext as Claster;
            if (claster == null) return;
            controller.ShowNodes(claster);
        }

        private void Border_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) return;
            var node = (e.Source as Shape).DataContext as Node;
            if (node == null) return;
            controller.HideNodes(node.ClasterId);
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            var t = e.GetPosition(ClasterPanel);
            controller.MoveSelectItem(new Vector(t.X, t.Y));
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            controller.UnselectItem();
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            controller.UnselectItem();
        }

        private void SelectCell(object sender, MouseButtonEventArgs e)
        {
            var cell = (e.Source as Shape).DataContext as Cell;
            if (cell.isSelected)
            {
                cell.isSelected = false;
                if (cell.CellIndex.Y == 0)
                {
                    controller.UnselectCollumn(cell.CellIndex.X);
                }
                if (cell.CellIndex.X == 0)
                {
                    controller.UnselectRow(cell.CellIndex.Y);
                }
                if (cell.CellIndex.X != 0 && cell.CellIndex.Y != 0)
                {
                    controller.UnselectRow(cell.CellIndex.Y);
                    controller.UnselectCollumn(cell.CellIndex.X);
                }
            }
            else
            {
                cell.isSelected = true;
                if (cell.CellIndex.X == 0)
                {
                    controller.SelectRow(cell.CellIndex.Y);
                }
                if (cell.CellIndex.Y == 0)
                {
                    controller.SelectCollumn(cell.CellIndex.X);
                }
                if (cell.CellIndex.X != 0 && cell.CellIndex.Y != 0)
                {
                    controller.SelectRow(cell.CellIndex.Y);
                    controller.SelectCollumn(cell.CellIndex.X);
                }
            }
        }
    }
}