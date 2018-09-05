using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Security.Cryptography;
using System.Windows.Forms;
namespace 校园网
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 
    public class login
    {
        public string id
        {
            set;get;
        }
        public string pass
        {
            set;get;
        }
        public string autologin
        {
            set;get;
        }
    }

    public partial class MainWindow : Window
    {
        
        login log = new login();
        bool thread = false;
        string key16 = "75fhiq285opdsnkl";
        string iv = "54wero2re90khfdj";
        private NotifyIcon notifyIcon;
        public MainWindow()
        {
            this.notifyIcon = new NotifyIcon();
            InitializeComponent();
            readFile();
            //this.notifyIcon.Icon = new System.Drawing.Icon(@"..\..\ico\net.ico");
            //this.notifyIcon.Icon = new Icon()
            this.notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            //pos();
            //this.notifyIcon.Visible = true;
            //打开菜单项

            System.Windows.Forms.MenuItem open = new System.Windows.Forms.MenuItem("还原");
            open.Click += new EventHandler(Show);
            //退出菜单项
            System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem("退出");
            exit.Click += new EventHandler(Close);
            //关联托盘控件
            System.Windows.Forms.MenuItem[] childen = new System.Windows.Forms.MenuItem[] { open, exit };
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(childen);

            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler((o, e) =>
            {
                if (e.Button == MouseButtons.Left) this.Show(o, e);
            });
            this.notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler((o, e) =>
            {
                if (e.Button == MouseButtons.Left) this.Show(o, e);
            });
            notifyIcon.Text="当前防断线未开启";

        }
         public string AesEncrypt(string rawInput, string key, string iv)
        {
            if (string.IsNullOrEmpty(rawInput))
            {
                return string.Empty;
            }

            if (key == null || iv == null || key.Length < 1 || iv.Length < 1)
            {
                throw new ArgumentException("Key/Iv is null.");
            }

            using (var rijndaelManaged = new RijndaelManaged()
            {
                //Key = key, // 密钥，长度可为128， 196，256比特位
                Key = System.Text.Encoding.Default.GetBytes(key),
                //IV = iv,  //初始化向量(Initialization vector), 用于CBC模式初始化
                IV = System.Text.Encoding.Default.GetBytes(iv),
                KeySize = 256,//接受的密钥长度
                BlockSize = 128,//加密时的块大小，应该与iv长度相同
                Mode = CipherMode.CBC,//加密模式
                Padding = PaddingMode.PKCS7
            }) //填白模式，对于AES, C# 框架中的 PKCS　＃７等同与Java框架中 PKCS #5
            {
                using (var transform = rijndaelManaged.CreateEncryptor(System.Text.Encoding.Default.GetBytes(key), System.Text.Encoding.Default.GetBytes(iv)))
                {
                    var inputBytes = Encoding.UTF8.GetBytes(rawInput);//字节编码， 将有特等含义的字符串转化为字节流
                    var encryptedBytes = transform.TransformFinalBlock(inputBytes, 0, inputBytes.Length);//加密
                    return Convert.ToBase64String(encryptedBytes);//将加密后的字节流转化为字符串，以便网络传输与储存。
                }
            }
        }
         public string AesDecrypt(string encryptedInput, string key, string iv)
        {
            if (string.IsNullOrEmpty(encryptedInput))
            {
                return string.Empty;
            }

            if (key == null || iv == null || key.Length < 1 || iv.Length < 1)
            {
                throw new ArgumentException("Key/Iv is null.");
            }

            using (var rijndaelManaged = new RijndaelManaged()
            {
                //Key = key,
                Key = System.Text.Encoding.Default.GetBytes(key),
                //IV = iv,
                IV = System.Text.Encoding.Default.GetBytes(iv),
                KeySize = 256,
                BlockSize = 128,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            })
            {
                using (var transform = rijndaelManaged.CreateDecryptor(System.Text.Encoding.Default.GetBytes(key), System.Text.Encoding.Default.GetBytes(iv)))
                {
                    var inputBytes = Convert.FromBase64String(encryptedInput);
                    var encryptedBytes = transform.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                    return Encoding.UTF8.GetString(encryptedBytes);
                }
            }
        }
        private void writeFile(bool flag)
        {
            string encodepass = AesEncrypt(log.pass, key16, iv);
            FileStream aFile = new FileStream("Config.ini", FileMode.Create);
            StreamWriter sw = new StreamWriter(aFile);
            sw.WriteLine($"Id={log.id};");            
            if (flag)
            { sw.WriteLine($"Pass={encodepass};");
              sw.WriteLine($"AutoLogin=true;");
            } 
            else
            { sw.WriteLine($"Pass=;");
              sw.WriteLine($"AutoLogin=false;");
            }
            sw.Close();
        }
        private void readFile()
        {
            string line;
            string dat = null;
            try {
                FileStream aFile = new FileStream("Config.ini", FileMode.Open);
                StreamReader sr = new StreamReader(aFile);
                line = sr.ReadLine();                
                while(line!=null)
                {
                    dat+=line;
                    line = sr.ReadLine();
                }
                sr.Close();
                if(dat!=null)
                {
                    //MessageBox.Show(dat);
                    string patten = @"Id=([^;]*)";                    
                    Regex.Matches(dat, patten).Cast<Match>().ToList().ForEach(x =>
                    {
                        // MessageBox.Show(string.Format("{0}", x.Groups[1].Value));
                        log.id = x.Groups[1].Value;
                        id.Text= x.Groups[1].Value;
                    });
                    patten = @"Pass=([^;]*)";
                    Regex.Matches(dat, patten).Cast<Match>().ToList().ForEach(x =>
                    {
                        //MessageBox.Show(string.Format("{0}", x.Groups[1].Value));
                        log.pass = AesDecrypt(x.Groups[1].Value, key16, iv);
                        pass.Password = AesDecrypt( x.Groups[1].Value,key16,iv);
                        //MessageBox.Show(pass.Password);
                    });
                    patten = @"AutoLogin=([^;]*)";
                    Regex.Matches(dat, patten).Cast<Match>().ToList().ForEach(x =>
                    {
                        //MessageBox.Show(string.Format("{0}", x.Groups[1].Value));
                        if(x.Groups[1].Value=="true")
                        {
                            checkbox1.IsChecked = true;
                        }
                        else
                        {
                            checkbox1.IsChecked = false;
                        }
                        //log.pass = x.Groups[1].Value;
                        //pass.Password = x.Groups[1].Value;
                    });
                }

            }
            catch { }
           
        }
        private string GetUrlHtml(string url)
        {
            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse hwrs = (HttpWebResponse)hwr.GetResponse();
            Stream stream = hwrs.GetResponseStream();
            StreamReader sr = new StreamReader(stream, Encoding.GetEncoding(hwrs.CharacterSet));
            string html = sr.ReadToEnd();
            sr.Close();
            return html;
        }
        private  bool Isonline()
        {
            string resultHtml = GetUrlHtml("http://172.16.255.253");
            string patten = @"(xmlns)=([^ ]*)";
            var match = Regex.Match(resultHtml, patten);
            if (match.Success)
            {
                //Console.WriteLine("登陆失败！");
                return false;
            }
            else
            {

                //Console.WriteLine("登陆成功！");
                return true;
            }
            //Console.ReadLine();
        }
        private void pos(string log_id ,string log_pass)
        {
           string txt_Url= "http://172.16.255.253/a70.htm";
           
            //postData += ("&vcode=" + txt_VCode);
            string postData = $"DDDDD={log_id}%40telecom&upass={log_pass}&R1=0&R2=&R3=0&R6=0&para=00&0MKKey=123456&buttonClicked=&redirect_url=&err_flag=&username=&password=&user=&cmd=&Login=";
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(postData);
            string url = txt_Url;
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
            webRequest.Method = "POST";             //POST
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = byteArray.Length;
            Stream newStream = webRequest.GetRequestStream();
            newStream.Write(byteArray, 0, byteArray.Length);
            newStream.Close();

        }
        private  void pos()
        {
            string txt_Url = "http://172.16.255.253/a70.htm";
            
            //postData += ("&vcode=" + txt_VCode);
            string postData = $"DDDDD={log.id}%40telecom&upass={log.pass}&R1=0&R2=&R3=0&R6=0&para=00&0MKKey=123456&buttonClicked=&redirect_url=&err_flag=&username=&password=&user=&cmd=&Login=";
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(postData);
            string url = txt_Url;
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
            webRequest.Method = "POST";             //POST
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = byteArray.Length;
            Stream newStream = webRequest.GetRequestStream();
            newStream.Write(byteArray, 0, byteArray.Length);
            newStream.Close();

        }
        private void PostWebRequest()
        {
            
            //Dispatcher.BeginInvoke(new Action(delegate
            //{
            //    loging.Visibility = Visibility;
            //}));
            string postUrl = "http://172.16.255.253/a70.htm";
            
            string postData = $"DDDDD={log.id}%40telecom&upass={log.pass}&R1=0&R2=&R3=0&R6=0&para=00&0MKKey=123456&buttonClicked=&redirect_url=&err_flag=&username=&password=&user=&cmd=&Login=";
            string ret = string.Empty;
            try
            {
                //if (!postUrl.StartsWith("http://"))
                    //return "";

                byte[] byteArray = Encoding.Default.GetBytes(postData); //转化
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(new Uri(postUrl));
                webReq.Method = "POST";
                webReq.ContentType = "application/x-www-form-urlencoded";

                webReq.ContentLength = byteArray.Length;
                Stream newStream = webReq.GetRequestStream();
                newStream.Write(byteArray, 0, byteArray.Length);//写入参数
                newStream.Close();
                HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                ret = sr.ReadToEnd();
                sr.Close();
                response.Close();
                //newStream.Close();
                
            }
            catch (Exception ex)
            {
                return;
            }
            return ;
        }
        private void online()
        {
            Thread.Sleep(2000);
            if (Isonline())
            {

                Dispatcher.BeginInvoke(new Action(delegate
                {
                    list1.Items.Add($"{DateTime.Now.ToString("HH:mm:ss")}  登陆成功！");
                }));
                
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    list1.Items.Add($"{DateTime.Now.ToString("HH:mm:ss")}  登陆失败！请检测账号或密码");
                }));
            }
            Dispatcher.BeginInvoke(new Action(delegate
            {
                loging.Visibility = Visibility.Hidden;
                list1.SelectedIndex = list1.Items.Count - 1;
                list1.ScrollIntoView(list1.SelectedItem);

            }));
            
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            loging.Visibility = Visibility;
            log.id = id.Text;
            log.pass = pass.Password;
            ////PostWebRequest();
            Thread thread2 = new Thread(new ThreadStart(PostWebRequest));
            //调用Start方法执行线程
            thread2.Start();
            Thread thread1 = new Thread(new ThreadStart(online));
            //调用Start方法执行线程
            thread1.Start();

            //if (Isonline())
            //{

            //    list1.Items.Add("登陆成功");
            //}
            //else
            //{
            //    list1.Items.Add("登陆失败");
            //}
            //MessageBox.Show($"{id.Text}+{pass.Password}");
            //MessageBox.Show("登陆成功！");
            if (checkbox1.IsChecked==true)
            {
                writeFile(true);
            }
           else
            {
                writeFile(false);
            }
            //FileStream aFile = new FileStream("temp.txt", FileMode.Create);
            //StreamWriter sw = new StreamWriter(aFile);
            //sw.WriteLine($"Id={log.id};");
            //sw.WriteLine($"Pass={log.pass};");
            //sw.Close();
            //aFile.Close();
        }
        private void logout()
        {
            Thread.Sleep(20);
            GetUrlHtml("http://172.16.255.253/F.htm");
        }
        private void outline()
        {
            if (Isonline())
            {

                Dispatcher.BeginInvoke(new Action(delegate
                {
                    list1.Items.Add($"{DateTime.Now.ToString("HH:mm:ss")}  正在注销");                    
                }));
                Thread.Sleep(100);
                logout();

            }
            if (Isonline()==false)
            {

                Dispatcher.BeginInvoke(new Action(delegate
                {
                    list1.Items.Add($"{DateTime.Now.ToString("HH:mm:ss")}  注销成功");

                }));
                
            }
            
            Dispatcher.BeginInvoke(new Action(delegate
            {
                list1.SelectedIndex = list1.Items.Count - 1;
                list1.ScrollIntoView(list1.SelectedItem);

            }));
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Thread thread1 = new Thread(new ThreadStart(logout));
                   thread1.Start();
            Thread thread2 = new Thread(new ThreadStart(outline));
            thread2.Start();


        }
         void login()
        {
            while (thread)
            {
                
                Thread.Sleep(2000);
                if (Isonline())
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        list1.Items.Add($"{DateTime.Now.ToString("HH:mm:ss")}  当前在线！");

                    }));

                else
                {
                    //Console.WriteLine("当前断线正在登陆..");
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        list1.Items.Add($"{DateTime.Now.ToString("HH:mm:ss")}  当前断线正在登陆..");

                    }));

                    PostWebRequest();
                    Thread.Sleep(1000);
                    if (Isonline())
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            list1.Items.Add($"{DateTime.Now.ToString("HH:mm:ss")}  重连成功");

                        }));
                }
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    list1.SelectedIndex = list1.Items.Count - 1;
                    list1.ScrollIntoView(list1.SelectedItem);

                }));
            }
            

        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            startbt.Visibility = Visibility.Collapsed;
            Stopbt.Visibility =Visibility.Visible;
            list1.Items.Add($"{DateTime.Now.ToString("HH:mm:ss")}  防断线开启成功!");
            Thread thread1 = new Thread(new ThreadStart(login));
            thread = true;
            thread1.Start();
            notifyIcon.Text = "防断线正在后台运行";
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            startbt.Visibility = Visibility.Visible;
            Stopbt.Visibility = Visibility.Collapsed;
            list1.Items.Add($"{DateTime.Now.ToString("HH:mm:ss")}  正在停止!");
            thread = false;
            notifyIcon.Text = "当前防断线未开启";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //thread = false;
            // 取消关闭窗体
            e.Cancel = true;
            // 将窗体变为最小化
            this.WindowState = WindowState.Minimized;
            notifyIcon.Visible = true; //托盘图标可见
            this.ShowInTaskbar = false; //不显示在系统任务栏            
            notifyIcon.ShowBalloonTip(1, "提示", "正在后台运行", ToolTipIcon.Info);//显示气泡
            
        }
        private void Show(object sender, EventArgs e)
        {
            this.WindowState = WindowState.Normal;
            this.Visibility = Visibility.Visible;
            this.ShowInTaskbar = true;
            this.Activate();
        }

        private void Hide(object sender, EventArgs e)
        {
            this.ShowInTaskbar = false;
            this.Visibility = System.Windows.Visibility.Hidden;
        }

        private void Close(object sender, EventArgs e)
        {
            notifyIcon.Dispose();
            thread = false;
            System.Windows.Application.Current.Shutdown();
        }
    }
}
