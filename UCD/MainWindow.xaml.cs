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

        public List<Unicode> unicodes = new List<Unicode>();
    }
    public class Unicode
    {
        public int Integer { get; set; }
        public string Hex { get; set; }
        public string Block { get; set; }
        public string Name { get; set; }
        public string DisplayCharacter { get; set; }
    }
    public class UnicodeBtn : Button
    {
        public int integerValue { get; set; }
        public string hexValue { get; set; }
        public string block { get; set; }
        public string descName { get; set; }
        public string format { get; set; }
    }
    public partial class MainWindow : Window
    {
        FontFamily MainFont = new FontFamily();
        Block MainBlock = new Block();
        List<Block> blocks = new List<Block>().Distinct().ToList();
        List<Unicode> names = new List<Unicode>();
        List<Unicode> Unicodes = new List<Unicode>();
        //static SortedDictionary<int, string> codes = new SortedDictionary<int, string>();
        Dictionary<int, string> delimiter = new Dictionary<int, string>();

        public MainWindow()
        {
            InitializeComponent();

            string[] blocksFile = System.IO.File.ReadAllLines(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + @"\Resources\UCD\Blocks.txt");
            string[] namesFile = System.IO.File.ReadAllLines(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + @"\Resources\UCD\extracted\DerivedName.txt");

            //Regex rx = new Regex(@"\;[A-Z\s\-]+\;");
            //Regex n = new Regex(@"N;[A-Z\s\-]+;{4,}");
            //Create block from each line in Blocks.txt
            foreach (string s in blocksFile)
            {
                if (s.Contains(';'))
                {
                    string[] split = s.Split(';');
                    string[] blSplit = split[0].Split("..");

                    blocks.Add(new Block
                    {
                        begin = blSplit[0].Trim(),
                        end = blSplit[1].Trim(),
                        block = split[1].Trim()
                    });
                }
            }
            //Assign begin and end for each block in delimiter dictionary
            foreach (string s in namesFile)
            {
                if (s.Contains(';') && !s.Contains(".."))
                {
                    string[] split = s.Split(';');
                    delimiter.Add(int.Parse(split[0].Trim(), NumberStyles.HexNumber), split[1].Trim());
                }
            }
            //Assign individual unicodes from each block
            foreach (Block b in blocks)
            {
                int beginInt = int.Parse(b.begin, NumberStyles.HexNumber);
                int endInt = int.Parse(b.end, NumberStyles.HexNumber);
                for (int i = beginInt; i <= endInt; i++)
                {
                    if (delimiter.ContainsKey(i))
                    {
                        Unicode u = new Unicode
                        {
                            Integer = i,
                            Hex = i.ToString("X"),
                            Block = b.block,
                            Name = delimiter[i],
                            DisplayCharacter = char.ConvertFromUtf32(i)
                        };
                        b.unicodes.Add(u);
                        names.Add(u);
                    }
                }
            }
        }
        private void filterEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    if (intRad.IsChecked == true)
                        data.ItemsSource = Unicodes.Where(w => w.Integer.ToString().Contains(filterInput.Text.ToString().ToLower())).ToList();
                    if (hexRad.IsChecked == true)
                        data.ItemsSource = Unicodes.Where(w => w.Hex.ToLower().Contains(filterInput.Text.ToString().ToLower())).ToList();
                    if (blRad.IsChecked == true)
                        data.ItemsSource = Unicodes.Where(w => w.Block.ToLower().Contains(filterInput.Text.ToString().ToLower())).ToList();
                    if (nameRad.IsChecked == true)
                        data.ItemsSource = Unicodes.Where(w => w.Name.ToLower().Contains(filterInput.Text.ToString().ToLower())).ToList();
                }
                catch (Exception ex)
                {
                    //Catch try is here to avoid the program from crashing
                }
            }
        }
        private void loadBlocks(object sender, RoutedEventArgs e)
        {
            foreach (Block b in blocks)
            {
                foreach (Unicode u in b.unicodes)
                {
                    Unicodes.Add(u);
                }
            }
            data.ItemsSource = Unicodes;
        }
        void dataSelection(object sender, SelectedCellsChangedEventArgs e)
        {
            DataGrid d = sender as DataGrid;
            Unicode b = d.SelectedItem as Unicode;
            integerText.Text = b.Integer.ToString();
            hexText.Text = b.Hex;
            blockN.Text = b.Block;
            name.Text = b.Name;
            format.Text = b.DisplayCharacter;
            display.Text = b.DisplayCharacter;
        }
        void IdCopy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(integerText.Text);
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
            Clipboard.SetText(format.Text);
        }
        void loadFonts(object sender, RoutedEventArgs e)
        {
            List<ComboBoxItem> fl = new List<ComboBoxItem>();
            int p = 10;
            foreach (FontFamily f in Fonts.SystemFontFamilies)
            {
                fl.Add(
                    new ComboBoxItem()
                    {
                        Content = f,
                        FontFamily = f,
                        FontSize = 24,
                        Padding = new Thickness { Left = p, Top = p, Right = p, Bottom = p }
                    });
            }
            fonts.ItemsSource = fl;
            fonts.SelectedIndex = 0;
        }
        void fontChanged(object sender, SelectionChangedEventArgs e)
        {
            var c = sender as ComboBox;
            var s = c.SelectedValue as ComboBoxItem;

            textFont.Text = s.FontFamily.Source;
            display.FontFamily = s.FontFamily;
        }
    }
}