using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Drawing.Configuration;
using System.Windows.Media;
using System.Windows.Markup;
using System.Drawing.Text;
using System.Globalization;
using System.Threading;
using System.Windows.Documents;
using System.Windows.Input;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Data;

namespace UCD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class Block
    {
        public string block { get; set; }
        public string begin { get; set; }
        public string end { get; set; }
    }
    public class Unicode : Button
    {
        public int Id { get; set; }
        public string hex { get; set; }
    }
    public partial class MainWindow : Window
    {
        FontFamily fontFamily = new FontFamily();
        Block block = new Block();
        List<Block> blocks = new List<Block>();
        List<Unicode> Unicodes = new List<Unicode>();
        static SortedDictionary<int, string> codes = new SortedDictionary<int, string>();
        string hex;

        public MainWindow()
        {
            InitializeComponent();

            string[] namesFile = System.IO.File.ReadAllLines(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + @"\Resources\UCD\UnicodeData.txt");

            Regex rx = new Regex(@"\;[A-Z\s\-]+\;");
            Regex n = new Regex(@"N;[A-Z\s\-]+;{4,}");
            foreach (string s in namesFile)
            {
                Match m = rx.Match(s);
                Match ns = n.Match(s);

                string[] sp = s.Split(';');
                if (m.Success)
                {
                    string t = m.Value.Trim(';');
                    codes.Add(int.Parse(sp[0], System.Globalization.NumberStyles.HexNumber), t);
                }
                if (ns.Success)
                {
                    string t = ns.Value.Trim(';');
                    string[] ta = ns.Value.ToString().Split(';');

                    codes.Remove(int.Parse(sp[0], System.Globalization.NumberStyles.HexNumber));
                    codes.Add(int.Parse(sp[0], System.Globalization.NumberStyles.HexNumber), ta[1]);
                }
            }

        }
        private void filterF()
        {
            TabItem i = tabs.SelectedItem as TabItem;
            if (i.Header.ToString() == "Blocks")
            {
                List<Block> filter = blocks.Where(w => w.block.ToLower().Contains(filterInput.Text.ToString().ToLower())).ToList();
                try
                {
                    blocksList.ItemsSource = filter;
                    filterInput.Foreground = Brushes.Black;
                }
                catch (Exception ex)
                {
                    blocksList.ItemsSource = blocks;
                    filterInput.Foreground = Brushes.Red;
                }
                if (filter.ToList().Count == 0)
                {
                    filterInput.Foreground = Brushes.Red;
                }
            }
            else if (i.Header.ToString() == "Names")
            {
                int c = codes.Where(w => w.Value.ToLower().Contains(filterInput.Text.ToString().ToLower())).ToList().Count;
                try
                {
                    namesList.ItemsSource = codes.Where(w => w.Value.ToLower().Contains(filterInput.Text.ToString().ToLower())).ToList();
                    filterInput.Foreground = Brushes.Black;
                }
                catch (Exception ex)
                {
                    namesList.ItemsSource = codes;
                    filterInput.Foreground = Brushes.Red;
                }
                if (c == 0)
                {
                    filterInput.Foreground = Brushes.Red;
                }
            }
        }
        private void filterEnter(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return)
            {
                filterF();
            }
        }
        void filterClick(object sender, RoutedEventArgs e)
        {
            filterF();
        }
        private void selectedName(object sender, RoutedEventArgs e)
        {
            var n = sender as ListBox;
            int v = (int)n.SelectedValue;

            id.Text = n.SelectedValue.ToString();
            hexText.Text = int.Parse(v.ToString("X"), NumberStyles.HexNumber).ToString("X");
            name.Text = codes[v];
            uniText.Text = Char.ConvertFromUtf32(v);
            display.Text = Char.ConvertFromUtf32(v);
        }
        void Blocks_Loaded(object sender, RoutedEventArgs e)
        {
            string blocksFile = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + @"/Resources\UCD\Blocks.txt";

            if (File.Exists(blocksFile))
            {
                using (StreamReader sr = File.OpenText(blocksFile))
                {
                    string s;
                    Regex rx = new Regex("(([0-9]|[a-z]|[A-Z])+)(..)(([0-9]|[a-z]|[A-Z])+)(; )([A-Z])+");
                    while ((s = sr.ReadLine()) != null)
                    {
                        if (rx.IsMatch(s))
                        {
                            string[] n = s.Split(';');
                            string[] set = n[0].Split("..");

                            Block block = new Block();
                            block.block = n[1];
                            block.begin = set[0];
                            block.end = set[1];

                            blocks.Add(block);
                        }
                    }
                }
                var combo = sender as ListBox;

                combo.ItemsSource = blocks;
                blocks.Sort(delegate (Block b1, Block b2)
                {
                    return b1.block.CompareTo(b2.block);
                });
            }
        }
        void fillWrap(Block block, FontFamily fontFamily)
        {

            Unicodes.Clear();
            wrapPanel.Children.Clear();

            try
            {
                int d = int.Parse(block.begin, System.Globalization.NumberStyles.HexNumber);
                int du = int.Parse(block.end, System.Globalization.NumberStyles.HexNumber);
                for (int i = d; i <= du; i++)
                {
                    Unicode btn = new Unicode();
                    if (codes.ContainsKey(i))
                    {
                        btn.Background = new SolidColorBrush(new Color
                        {
                            A = 255,
                            R = 230,
                            G = 230,
                            B = 230
                        });
                        btn.Id = i;
                        btn.hex = String.Format("{0:X}", i);
                        btn.Content = Char.ConvertFromUtf32(i);
                        btn.ToolTip = codes[i];
                        btn.FontFamily = fontFamily;
                    }
                    else
                    {
                        btn.Content = "";
                        btn.ToolTip = "Empty";
                        btn.Background = Brushes.DarkRed;
                    }

                    btn.Click += btnClick;

                    Unicodes.Add(btn);
                    wrapPanel.Children.Add(btn);
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        void ComboSelect(object sender, SelectionChangedEventArgs e)
        {
            var selected = sender as ListBox;
            var b = selected.SelectedValue as Block;

            block.begin = b.begin;
            block.end = b.end;

            fillWrap(block, fontFamily);
        }
        void btnClick(object sender, RoutedEventArgs e)
        {
            var b = sender as Unicode;
            id.Text = b.Id.ToString();
            hex = b.hex;
            display.Text = b.Content.ToString();
            display.FontFamily = b.FontFamily;
            hexText.Text = b.hex;
            name.Text = b.ToolTip.ToString();
            uniText.Text = b.Content.ToString();
        }
        void IdCopy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(id.Text);
        }
        void CopyHex(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(hexText.Text);
        }
        void CopyName(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(name.Text);
        }
        void CopyUni(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(uniText.Text);
        }
        void LoadFonts(object sender, RoutedEventArgs e)
        {
            fonts.ItemsSource = Fonts.SystemFontFamilies;
            fonts.SelectedIndex = 0;
        }
        void FontChanged(object sender, SelectionChangedEventArgs e)
        {
            var c = sender as ComboBox;
            var s = c.SelectedValue as FontFamily;

            fontFamily = s;
            fillWrap(block, fontFamily);
        }
    }
}
