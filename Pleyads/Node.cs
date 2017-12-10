using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using System.ComponentModel;

namespace Pleyads
{
    public class Node : INotifyPropertyChanged, IMoveable
    {
        public string Name { get; set; }
        public string ShotName { get { return Name.Substring(0, 3); } }

        public int ID { get; set; }

        public List<int> Links { get; set; } = new List<int>();
        public Vector V;
        public Visibility Hide { get; set; } = Visibility.Hidden;
        public String CustomColor { get; set; }
        public Vector Pos { get; set; }
        public bool IsSelect { get; set; }

        public int ClasterId;

        public bool Sorted;
        public bool HaveClaster;

        public event PropertyChangedEventHandler PropertyChanged;

        public void Move()
        {
            if (IsSelect) return;
            Pos += V;
            RePrint();
        }

        public void RePrint()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Pos"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Hide"));
        }

        public void Clear()
        {
            Hide = Visibility.Hidden;
            Links.Clear();
            Sorted = false;
            HaveClaster = false;
            RePrint();
        }

        public void Move(Vector newPos)
        {
            newPos.X -= Edge.OffsetX;
            newPos.Y -= Edge.OffsetY;
            Pos = newPos;
            RePrint();
        }

        public void Select()
        {
            IsSelect = true;
            RePrint();
        }

        public void UnSelect()
        {
            IsSelect = false;
            RePrint();
        }
    }

    public class Edge : INotifyPropertyChanged
    {
        public const int OffsetX = 21;
        public const int OffsetY = 15;
        public float Weight { get; set; }
        public Node B { get; set; }
        public Node A { get; set; }
        public Vector BPos { get { return new Vector(B.Pos.X + OffsetX, B.Pos.Y + OffsetY); } }
        public Vector APos { get { return new Vector(A.Pos.X + OffsetX, A.Pos.Y + OffsetY); } }
        public Visibility Hide { get; set; } = Visibility.Hidden;
        public String CustomColor { get; set; }

        public Edge(float weight)
        {
            Weight = weight;
            if (weight >= 0)
                CustomColor = "#FF7A00";
            else
                CustomColor = "#03899C";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void UpdateVisible()
        {
            if (A.Hide != Visibility.Visible || B.Hide != Visibility.Visible)
                Hide = Visibility.Hidden;
            else
                Hide = Visibility.Visible;
            RePrint();
        }

        public void Update()
        {
            var N = A.Pos - B.Pos;
            var r = N.Length;
            var w = Math.Abs(Weight);

            N.Normalize();

            var L = 50 / (w);
            var f = w * (L - r) * 0.01;

            A.V = (A.V + f * N) * 0.9;
            B.V = (B.V - f * N) * 0.9;

            A.Move();
            B.Move();
            RePrint();
        }

        public void RePrint()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BPos"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("APos"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Hide"));
        }
    }

    public class Claster : INotifyPropertyChanged, IMoveable
    {
        private static string[] brushes = new string[]
        {
            "#679ED2",
            "#33CEC3",
            "#FFC700",
            "#DF38B1",
            "#FF7140",
            "#218555",
            "#9F3ED5"
        };

        private static Random r = new Random();

        public int ID;

        public Visibility Hide { get; set; } = Visibility.Visible;
        public int[] Nodes;

        public event PropertyChangedEventHandler PropertyChanged;

        public Vector Pos { get; set; }
        public Vector Center { get { return new Vector(Pos.X + Mass / 2, Pos.Y + Mass / 2); } }
        public string Tip { get { return Nodes.Length.ToString(); } }

        public float Mass { get { return 10 + Nodes.Length * 8; } }

        public String CustomColor { get; set; }
        public bool IsSelect { get; set; }

        public void Init(List<Node> nodes)
        {
            CustomColor = brushes[r.Next(0, brushes.Length)];
            for (int i = 0; i < Nodes.Length; i++)
            {
                nodes[Nodes[i]].HaveClaster = true;
                nodes[Nodes[i]].CustomColor = CustomColor;
                nodes[Nodes[i]].ClasterId = ID;
            }
        }

        public void Update(List<Node> nodes)
        {
            if (Hide == Visibility.Hidden) return;
            Vector A = Center;
            Node B;
            for (int i = 0; i < Nodes.Length; i++)
            {
                B = nodes[Nodes[i]];
                var N = A - B.Pos;
                var r = N.Length;

                N.Normalize();

                var L = 40;
                var f = (L - r) * 0.02;

                B.V = (B.V - f * N) * 0.9;

                B.Move();
            }
        }

        public void RePrint()
        {
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(Pos)));
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(Hide)));
        }

        public void Show(Vector pos)
        {
            if (Hide == Visibility.Visible) return;
            Pos = pos;
            Hide = Visibility.Visible;
        }

        public void Move(Vector newPos)
        {
            newPos.X -= Mass / 2;
            newPos.Y -= Mass / 2;
            Pos = newPos;
            RePrint();
        }

        public void Select()
        {
            IsSelect = true;
            RePrint();
        }

        public void UnSelect()
        {
            IsSelect = false;
            RePrint();
        }
    }

    public class PleyadsControll
    {
        private List<Node> nodes = new List<Node>();
        public List<Node> Nodes { get { return nodes; } }

        private List<Edge> edges = new List<Edge>();
        public List<Edge> Edges { get { return edges; } }

        private List<Claster> clasters = new List<Claster>();
        public List<Claster> Clasters { get { return clasters; } }

        private IMoveable curSelect = null;
        public float EpsEdge = 0.5f;

        private float[][] matrix;
        private string[] names;
        private DispatcherTimer timer;

        private Table table;
        private bool isInit = false;
        public bool IsInit { get { return isInit; } }

        public delegate void UpdateHandlerDelegate();

        public event UpdateHandlerDelegate UpdateHandler;

        private const float CentrX = 500;
        private const float CentrY = 500;
        private const float CentrXShift = 250;
        private const float CentrYShift = 250;

        public bool Download(string path, Action badPath, char delimiter = '\t')
        {
            string[][] t;
            clasters.Clear();
            nodes.Clear();
            edges.Clear();
            if (!File.Exists(path))
            {
                badPath();
                return isInit;
            }
            using (StreamReader sr = File.OpenText(path))
            {
                t = sr.ReadToEnd().Replace('.', ',').Split('\n').Select(x => x.Split(delimiter)).ToArray();
            }
            isInit = true;

            int id = 0;
            Random r = new Random();
            for (int i = 1; i < t[0].Length; i++)
            {
                nodes.Add(new Node() { Name = t[0][i], ID = id, Pos = new Vector(CentrX + (r.NextDouble() - 0.5f) * CentrXShift, CentrX + (r.NextDouble() - 0.5f) * CentrYShift) });
                id++;
            }

            matrix = t.Skip(1)
                .Select((y, i) => y.Skip(1)
                    .Select(
                        (x, j) =>
                        {
                            if (j > i)
                                return 0f;
                            else
                                return float.Parse(x);
                        }
                        )
                    .ToArray())
                    .Where(x => x.Length > 1)
                .ToArray();
            names = t.First().Skip(1).ToArray();
            BuldPleyads();
            return isInit;
        }

        private void Clear()
        {
            edges.Clear();
            clasters.Clear();
            foreach (var item in nodes)
            {
                item.Clear();
            }
        }

        public void RebuildPleyads()
        {
            Clear();
            BuldPleyads();
        }

        private void BuldPleyads()
        {
            for (int i = 1; i < matrix.Length; i++)
            {
                for (int j = 0; j < matrix[i].Length; j++)
                {
                    if (i == j) continue;
                    if (Math.Abs(matrix[i][j]) < 0.001f) continue;
                    if (Math.Abs(matrix[i][j]) < EpsEdge) continue;
                    edges.Add(new Edge(matrix[i][j]) { A = nodes[i], B = nodes[j] });
                    nodes[i].Links.Add(j);
                    nodes[j].Links.Add(i);
                }
            }
            List<int> nodesIds;
            // creating Clasters
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                if (nodes[i].Sorted) continue;
                nodesIds = Clastering(nodes[i]);
                if (nodesIds.Count < 2) continue;
                Claster clr = new Claster()
                {
                    Nodes = nodesIds.ToArray(),
                    Pos = new Vector(CentrX / 5 + CentrX / 5 * (clasters.Count() % 5), CentrY / 5 + CentrY / 5 * (int)(clasters.Count() / 5 + clasters.Count() % 2)),
                    ID = clasters.Count()
                };
                clr.Init(nodes);
                clasters.Add(clr);
            }

            nodesIds = new List<int>();

            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].HaveClaster) continue;
                nodesIds.Add(i);
            }

            clasters.Add(new Claster()
            {
                Nodes = nodesIds.ToArray(),
                Pos = new Vector(CentrX / 5 + CentrX / 5 * (clasters.Count() % 5), CentrY / 5 + CentrY / 5 * (int)(clasters.Count() / 5 + clasters.Count() % 2)),
                ID = clasters.Count()
            });
            clasters.Last().Init(nodes);
        }

        private List<int> Clastering(Node node)
        {
            node.Sorted = true;
            List<int> claster = new List<int>() { node.ID };
            if (node.Links.Count() == 0) return claster;

            Node curNode;
            for (int i = 0; i < node.Links.Count(); i++)
            {
                curNode = nodes[node.Links[i]];
                if (curNode.Sorted) continue;
                curNode.Sorted = true;
                claster.AddRange(Clastering(curNode));
            }
            return claster;
        }

        private Vector SetVisibilityNode(int[] ids, Visibility state)
        {
            Vector avgPos = new Vector();
            for (int i = 0; i < ids.Length; i++)
            {
                nodes[ids[i]].Hide = state;
                nodes[ids[i]].RePrint();
                avgPos += nodes[ids[i]].Pos;
            }
            for (int i = 0; i < Edges.Count; i++)
            {
                Edges[i].UpdateVisible();
            }

            return avgPos / ids.Length;
        }

        public void HideNodes(Claster claster)
        {
            claster.Show(SetVisibilityNode(claster.Nodes, Visibility.Hidden));
            claster.RePrint();
        }

        public void HideNodes(int clasterID)
        {
            HideNodes(clasters[clasterID]);
        }

        public void ShowNodes(Claster claster)
        {
            SetVisibilityNode(claster.Nodes, Visibility.Visible);
            claster.Hide = Visibility.Hidden;
            claster.RePrint();
        }

        public void ShowNodes(int clasterID)
        {
            ShowNodes(clasters[clasterID]);
        }

        public void Start()
        {
            timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(15),
            };
            timer.Tick += Update;
            timer.Start();
        }

        private void Update(object sender, EventArgs e)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                edges[i].Update();
            }
            for (int i = 0; i < Clasters.Count; i++)
            {
                Clasters[i].Update(nodes);
            }
            UpdateHandler?.Invoke();
        }

        public void SelectItem(IMoveable item)
        {
            curSelect = item;
            item.Select();
        }

        public void UnselectItem()
        {
            if (curSelect is null) return;
            curSelect.UnSelect();
            curSelect = null;
        }

        public void MoveSelectItem(Vector newPos)
        {
            if (curSelect is null) return;
            curSelect.Move(newPos);
        }

        public Table CreateTable()
        {
            table = new Table(matrix, names, EpsEdge);
            return table;
        }

        public void SelectRow(int row)
        {
            table.SelectRow(row);
        }

        public void SelectCollumn(int coll)
        {
            table.SelectCollumn(coll);
        }

        public void UnselectRow(int row)
        {
            table.UnselectRow(row);
        }

        public void UnselectCollumn(int coll)
        {
            table.UnselectCollumn(coll);
        }

        public void Print(Claster item)
        {
        }
    }

    public interface IMoveable
    {
        bool IsSelect { get; set; }
        Vector Pos { get; set; }

        void Move(Vector newPos);

        void Select();

        void UnSelect();
    }
}