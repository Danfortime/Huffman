using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Huffman
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            treePanel.Paint += TreePanel_Paint;
        }

        private HuffmanTree huffmanTree;
        private Dictionary<char, string> huffmanCodes;
        private Dictionary<char, int> frequencies;

        private void btnEncode_Click(object sender, EventArgs e)
        {
            string inputText = txtInput.Text;
            if (string.IsNullOrEmpty(inputText))
            {
                MessageBox.Show("Введите текст для кодирования");
                return;
            }

            // Строим дерево Хаффмана
            huffmanTree = new HuffmanTree();
            huffmanTree.Build(inputText);
            frequencies = huffmanTree.GetFrequencies();
            huffmanCodes = huffmanTree.GetCodes();

            // Обновляем таблицы
            UpdateFrequencyTable();
            UpdateCodeTable();

            // Показываем ASCII представление (группы по 4 бита)
            txtAscii.Text = GetAsciiBinaryString(inputText);

            // Кодируем текст по Хаффману
            string encodedText = huffmanTree.Encode(inputText);
            txtEncoded.Text = encodedText;

            // Перерисовываем дерево
            treePanel.Invalidate();
        }

        private string GetAsciiBinaryString(string text)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in text)
            {
                string binary = Convert.ToString(c, 2).PadLeft(8, '0');
                for (int i = 0; i < binary.Length; i += 4)
                {
                    int length = Math.Min(4, binary.Length - i);
                    sb.Append(binary.Substring(i, length) + " ");
                }
            }
            return sb.ToString().Trim();
        }

        private void UpdateFrequencyTable()
        {
            dataGridViewFrequencies.Rows.Clear();
            foreach (var pair in frequencies.OrderByDescending(p => p.Value))
            {
                dataGridViewFrequencies.Rows.Add(pair.Key, pair.Value);
            }
        }

        private void UpdateCodeTable()
        {
            dataGridViewCodes.Rows.Clear();
            foreach (var pair in huffmanCodes.OrderBy(p => p.Value.Length))
            {
                dataGridViewCodes.Rows.Add(pair.Key, pair.Value);
            }
        }

        private void TreePanel_Paint(object sender, PaintEventArgs e)
        {
            if (huffmanTree == null) return;

            Graphics g = e.Graphics;
            g.Clear(Color.White);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int startX = treePanel.Width / 2;
            int startY = 30;
            int nodeRadius = 20;
            int verticalSpacing = 60;

            DrawTree(g, huffmanTree.Root, startX, startY, nodeRadius, verticalSpacing, treePanel.Width / 4);
        }

        private void DrawTree(Graphics g, HuffmanNode node, int x, int y, int radius, int verticalSpacing, int horizontalSpacing)
        {
            if (node == null) return;

            // Рисуем узел
            g.FillEllipse(Brushes.LightBlue, x - radius, y - radius, radius * 2, radius * 2);
            g.DrawEllipse(Pens.Black, x - radius, y - radius, radius * 2, radius * 2);

            string nodeText = node.Symbol == '\0' ? node.Frequency.ToString() : $"{node.Symbol}:{node.Frequency}";
            SizeF textSize = g.MeasureString(nodeText, Font);
            g.DrawString(nodeText, Font, Brushes.Black, x - textSize.Width / 2, y - textSize.Height / 2);

            // Рисуем связи с детьми
            if (node.Left != null)
            {
                int childX = x - horizontalSpacing;
                int childY = y + verticalSpacing;
                g.DrawLine(Pens.Black, x, y + radius, childX, childY - radius);
                g.DrawString("0", Font, Brushes.Red, (x + childX) / 2, (y + childY) / 2);
                DrawTree(g, node.Left, childX, childY, radius, verticalSpacing, horizontalSpacing / 2);
            }

            if (node.Right != null)
            {
                int childX = x + horizontalSpacing;
                int childY = y + verticalSpacing;
                g.DrawLine(Pens.Black, x, y + radius, childX, childY - radius);
                g.DrawString("1", Font, Brushes.Red, (x + childX) / 2, (y + childY) / 2);
                DrawTree(g, node.Right, childX, childY, radius, verticalSpacing, horizontalSpacing / 2);
            }
        }

        private void treePanel_Paint_1(object sender, PaintEventArgs e)
        {

        }
    }

    public class HuffmanNode : IComparable<HuffmanNode>
    {
        public char Symbol { get; set; }
        public int Frequency { get; set; }
        public HuffmanNode Left { get; set; }
        public HuffmanNode Right { get; set; }

        public int CompareTo(HuffmanNode other)
        {
            return Frequency.CompareTo(other.Frequency);
        }

        public bool IsLeaf()
        {
            return Left == null && Right == null;
        }
    }

    public class HuffmanTree
    {
        public HuffmanNode Root { get; private set; }
        private Dictionary<char, int> frequencies;

        public void Build(string source)
        {
            frequencies = new Dictionary<char, int>();
            foreach (char c in source)
            {
                if (!frequencies.ContainsKey(c))
                    frequencies[c] = 0;
                frequencies[c]++;
            }

            var priorityQueue = new List<HuffmanNode>();
            foreach (var symbol in frequencies)
            {
                priorityQueue.Add(new HuffmanNode { Symbol = symbol.Key, Frequency = symbol.Value });
            }

            while (priorityQueue.Count > 1)
            {
                priorityQueue.Sort();

                var left = priorityQueue[0];
                var right = priorityQueue[1];

                var parent = new HuffmanNode()
                {
                    Symbol = '\0',
                    Frequency = left.Frequency + right.Frequency,
                    Left = left,
                    Right = right
                };

                priorityQueue.Remove(left);
                priorityQueue.Remove(right);
                priorityQueue.Add(parent);
            }

            Root = priorityQueue.FirstOrDefault();
        }

        public Dictionary<char, string> GetCodes()
        {
            var codes = new Dictionary<char, string>();
            Traverse(Root, "", codes);
            return codes;
        }

        private void Traverse(HuffmanNode node, string code, Dictionary<char, string> codes)
        {
            if (node == null) return;

            if (node.IsLeaf())
            {
                if (code == "")
                    code = "0"; // для случая, когда всего один символ
                codes[node.Symbol] = code;
                return;
            }

            Traverse(node.Left, code + "0", codes);
            Traverse(node.Right, code + "1", codes);
        }

        public Dictionary<char, int> GetFrequencies()
        {
            return frequencies;
        }

        public string Encode(string source)
        {
            var codes = GetCodes();
            string encoded = "";
            foreach (char c in source)
            {
                encoded += codes[c];
            }
            return encoded;
        }
    }
}