using SaltFish.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SaltFish.Views
{
    /// <summary>
    /// ReaderView.xaml 的交互逻辑
    /// </summary>
    public partial class ReaderView : Window
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        private string filepath;
        /// <summary>
        /// 行间距
        /// </summary>
        private int LineHeight;
        public ReaderView(string file,int lineheight)
        {
            InitializeComponent();
            filepath = file;
            LineHeight = lineheight;
            //窗口拖动
            this.MouseDown += (x, y) =>
            {
                if (y.LeftButton == MouseButtonState.Pressed)
                {
                    this.DragMove();
                }
            };
        }
        /// <summary>
        /// 窗口加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //先将txt转换为utf-8存到缓存
            string outputpath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "temp.txt");
            Encoding filetype = FileHelper.GetType(filepath);
            string content = File.ReadAllText(filepath, filetype);
            File.WriteAllText(outputpath, content, Encoding.UTF8);
            ////从缓存异步读取txt
            var doc = new FlowDocument();
            doc.LineHeight = LineHeight;
            Paragraph graph = new Paragraph();
            doc.Blocks.Add(graph);
            richbox.Document = doc;
            Action action = new Action(() => { ReadText(graph, outputpath); });
            action.BeginInvoke(null, null);
        }
        /// <summary>
        /// 窗口渲染
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            string file = System.IO.Path.GetFileNameWithoutExtension(filepath);
            if (AppConfig.History.ContainsKey(file))
            {
                double offset = AppConfig.History[file];
                richbox.ScrollToVerticalOffset(offset);
            }
        }
        /// <summary>
        /// 异步加载txt文件
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="file"></param>
        private static void ReadText(Paragraph graph, string file)
        {
            try
            {
                using (FileStream stream = new FileStream(file, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        while (!reader.EndOfStream)
                        {
                            var content = reader.ReadLine();
                            if (!string.IsNullOrEmpty(content))
                            {
                                graph.Dispatcher.Invoke(() =>
                                {
                                    graph.Inlines.Add(content);
                                    graph.Inlines.Add(Environment.NewLine);
                                });
                                System.Threading.Thread.Sleep(5);
                            }
                        }
                        reader.Close();
                        reader.Dispose();
                        stream.Close();
                        stream.Dispose();
                    }
                }
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
    }
}
