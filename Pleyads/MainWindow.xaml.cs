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
            controller = new PleyadsControll();
            controller.Download(@"../../data/crosscor.txt");
            NodesPanel.ItemsSource = controller.Nodes;
            EdgesPanel.ItemsSource = controller.Edges;
            ClasterPanel.ItemsSource = controller.Clasters;
            ParseFloat.Text = controller.EpsEdge.ToString();
            controller.Start();
        }

        private void SelectItem(object sender, MouseButtonEventArgs e)
        {
            IMoveable obj = (e.Source as Shape).DataContext as IMoveable;
            controller.SelectItem(obj);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            controller.EpsEdge = float.Parse(ParseFloat.Text.Replace(".", ","));
            controller.RebuildPleyads();
            NodesPanel.Items.Refresh();
            EdgesPanel.Items.Refresh();
            ClasterPanel.Items.Refresh();
        }

        private void HideClasters(object sender, RoutedEventArgs e)
        {
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
    }
}