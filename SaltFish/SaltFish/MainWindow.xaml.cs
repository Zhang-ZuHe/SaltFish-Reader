using SaltFish.Classes;
using SaltFish.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SaltFish
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>
        /// 任务栏图标
        /// </summary>
        private NotifyIcon notifyIcon;
        /// <summary>
        /// 键盘钩子
        /// </summary>
        private KeyboardHook keyboardHook = new KeyboardHook();
        /// <summary>
        /// 阅读窗口
        /// </summary>
        private ReaderView ReaderView { get; set; }
        /// <summary>
        /// 文件名
        /// </summary>
        private string filename;

        public MainWindow()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// 窗体加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string configpath = FileHelper.AssemblyDirectory + "/config.json";
            if (!File.Exists(configpath))
            {
                AppConfig.History = new Dictionary<string, double>();
                File.WriteAllText(configpath, JSON.stringify(AppConfig.History));
            }
            AppConfig.History = JSON.parse<Dictionary<string, double>>(File.ReadAllText(configpath));
            Version.Text = "欢迎使用摸鱼小助手  v." + System.Windows.Forms.Application.ProductVersion;
            this.notifyIcon = new NotifyIcon();
            this.notifyIcon.BalloonTipText = "记事本"; //设置程序启动时显示的文本
            this.notifyIcon.Text = "记事本";//最小化到托盘时，鼠标点击时显示的文本
            this.notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);//程序图标
            this.notifyIcon.Visible = true;
            //右键菜单--退出菜单项
            System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem("退出");
            exit.Click += new EventHandler(Icon_Quit);
            //关联托盘控件
            System.Windows.Forms.MenuItem[] childen = new System.Windows.Forms.MenuItem[] { exit };
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(childen);
            keyboardHook.KeyPressEvent += KeyboardHook_KeyPressEvent;
            this.MouseDown += (x, y) =>
            {
                if (y.LeftButton == MouseButtonState.Pressed)
                {
                    this.DragMove();
                }
            };
            keyboardHook.Start();
        }
        /// <summary>
        /// 导入文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Import_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Title = "导入文档";
            ofd.DefaultExt = ".txt*";
            ofd.Filter = "文档文件(*.txt)|*.txt;";//;*.ebb
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == true)
            {
                string file = ofd.FileName;
                filename = System.IO.Path.GetFileNameWithoutExtension(file);
                if (FileHelper.IsFileInUse(file))
                {
                    System.Windows.Forms.MessageBox.Show("文件被占用，请关闭后重试");
                    return;
                }
                else
                {
                    ReaderView = new ReaderView(file);
                    //大小设置
                    ReaderView.Width = double.Parse(windowwidth.Text);
                    ReaderView.Height = double.Parse(windowheight.Text);
                    //字号设置
                    ReaderView.richbox.FontSize = int.Parse(windowfontsize.Text);
                    //行距设置
                    ReaderView.lineheight.LineHeight = int.Parse(windowlineheight.Text);
                    //深色模式
                    if ((bool)nightmode.IsChecked)
                        ReaderView.richbox.Foreground = new SolidColorBrush(Colors.White);
                    this.Hide();

                    ReaderView.ShowDialog();
                }
            }
        }

        /// <summary>
        /// 关闭按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            keyboardHook.Stop();
            this.notifyIcon.Visible = false;
            this.notifyIcon.Dispose();
            Process.GetCurrentProcess().Kill();
            System.Windows.Application.Current.Shutdown();
        }

        /// <summary>
        /// 托盘退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Icon_Quit(object sender, EventArgs e)
        {
            keyboardHook.Stop();
            FileHelper.SaveProgress(filename);
            this.notifyIcon.Visible = false;
            this.notifyIcon.Dispose();
            Process.GetCurrentProcess().Kill();
            System.Windows.Application.Current.Shutdown();
        }

        /// <summary>
        /// 键盘按下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyboardHook_KeyPressEvent(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '`')
            {
                if (ReaderView == null)
                    this.Visibility = this.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                else
                    ReaderView.Visibility = ReaderView.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        #region 输入框限制输入
        private void Textbox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            bool isNumber = e.Key >= Key.D0 && e.Key <= Key.D9 || e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9;
            bool isCtrlA = e.Key == Key.A && e.KeyboardDevice.Modifiers == ModifierKeys.Control;
            bool isCtrlV = e.Key == Key.V && e.KeyboardDevice.Modifiers == ModifierKeys.Control;
            bool isBack = e.Key == Key.Back;
            bool isLeftOrRight = e.Key == Key.Left || e.Key == Key.Right;

            if (isNumber || isCtrlA || isCtrlV || isBack || isLeftOrRight)
                e.Handled = false;
            else
                e.Handled = true;
        }

        private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!isNumberic(text))
                { e.CancelCommand(); }
            }
            else { e.CancelCommand(); }
        }

        private bool isNumberic(string _string)
        {
            if (string.IsNullOrEmpty(_string))
                return false;
            foreach (char c in _string)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }
        #endregion

    }
}
