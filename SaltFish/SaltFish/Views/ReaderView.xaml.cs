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
        public ReaderView(string file)
        {
            InitializeComponent();
            filepath = file;
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
            string content = File.ReadAllText(filepath, Encoding.Default);
            File.WriteAllText(outputpath, content, Encoding.UTF8);
            //从缓存读取txt
            FileStream fs;
            if (File.Exists(outputpath))
            {
                fs = new FileStream(outputpath, FileMode.Open, FileAccess.Read);
                using (fs)
                {
                    TextRange text = new TextRange(richbox.Document.ContentStart, richbox.Document.ContentEnd);
                    text.Load(fs, DataFormats.Text);
                }
                fs.Close();
                fs.Dispose();
            }
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
    }
}
