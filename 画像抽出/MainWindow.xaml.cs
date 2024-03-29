﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
using System.Security.Permissions;
using System.IO;
using Microsoft.Win32;  //ファイル選択ダイアログ使用のため追加
using System.Text.RegularExpressions;  //ファイルから文字列を検索するためのメソッドを呼び出すため追加
using System.Runtime.InteropServices;  //マウスクリックイベント処理のため追加
using Microsoft.VisualBasic; //ソリューションエクスプローラーの参照から追加する必要がある。
using System.Windows.Diagnostics;

/// </summary>



namespace 画像抽出
{
    class DllImportSample
    {
        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void SetCursorPos(int X, int Y);

        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

    }
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public DateTime dt1 = new DateTime(2019,1,1,0,0,00); //基準時刻を設定するために使用する。
        public DateTime dt2; //タグ設定ボタン押下時の時間を設定するために使用する。
        public DateTime dt3; //初期時間判定用に使用する。
        public DateTime dt4; //現時刻設定するために使用する。

        public string now; //基準時刻を"yyyyMMddHHmmss"に変換して設定するように使用する。
        public string filePath2; //@"C:\TagAdding\TagDate\";
        public string filePath3; //@"C:\TagAdding\Kiridashi\"
        public string comboValue2; //タグ設定時に②H名前の情報を設定するために使用する。
        public string comboValue3; //タグ設定時に①Hチームの情報を設定するために使用する。
        public string comboValue4; //タグ設定時に⑤回数の情報を設定するために使用する。
        public string comboValue5; //タグ設定時に⑥カウントの情報を設定するために使用する。
        public string comboValue6; //タグ設定時に⑦球速の情報を設定するために使用する。
        public string comboValue7; //タグ設定時に③Vチームの情報を設定するために使用する。
        public string comboValue8; //タグ設定時に④V名前の情報を設定するために使用する。
        public string line1; //Name-List1に記載されている名前をホームチーム用名前リストに表示する際に使用する。
        public string line2; //Name-List2に記載されている名前をビジターチーム用名前リストに表示する際に使用する。
        public string line3; //タグ抽出時の③に表示される名前リストを格納するために使用する。
        public string line4; //タグ抽出時の④にて抽出する情報を1行ずつ格納するために使用する。
        public string line5; //動画切り出し時に切出し用の情報1行を格納するために使用する。
        public string filename1; // 動画切り出し時に使用する抽出元ファイル名を設定するために使用する。
        public string file_KiridashiTag; // 動画切り出し時に使用する抽出用タグファイル名を設定するために使用する。
        public string[] file_MP4 = new string[7]; // 動画切り出し時に使用する抽出元ファイル名(複数)を設定するために使用する。なお、カメラ台数が想定される7としている。
        public string[] HS_FILES; // HSカメラで撮影した動画ファイルを格納する。
        public string str_start_seconds; // 切り出し開始時間を抽出するために指標となる文字列|start|を格納するために使用する。
        public string str_stop_seconds; // 切り出し停止時間を抽出するために指標となる文字列|stop|を格納するために使用する。
        public string str_interval_seconds; // 切り出し間隔を抽出するために指標となる文字列|interval|を格納するために使用する。
        public string output_folder; // 動画切り出し用フォルダ作成時にフォルダ名として使用する文字列を格納するために使用する。
        public string HS_folder_name; // HSカメラで撮影した動画ファイルが格納されているフォルダ名を格納する。

        public StreamWriter sw2; //NWカメラ、HSカメラ用のタグ設定ファイル作成時に使用する。
        public StreamWriter sw3; //「C:\TagAdding\TagDate」「C:\TagAdding\TagKiridashi」にファイル作成時に使用する。

        public int cnt = 0;  // タグファイルが作成されたかどうかを判定するために使用する。
        public int cnt2 = 1; // hsカメラが30分ごとにファイル分割するので、その判定のために使用する。
        public int cnt3 = 0; // 動画切り出し時のMP4ファイルを複数読み込んだ際にstring配列のカウント用に使用する。
        public int cnt4 = 0; // 切出すために読み込んだMP4ファイル数を格納するために使用する。
        public int cnt5 = 0; // タグ抽出時の該当件数をカウントするために使用する。
        public int cnt6 = 0; // タグ抽出処理時に抽出元の動画ファイルが選択されているかどうかを判定するために使用する。
        public int cnt7 = 0; // タグ抽出処理時に切出し時間ファイルが選択されているかどうかを判定するために使用する。
        public int cnt8 = 0; // 動画切り出し時の進捗バーの表示用に使用する。(分子)
        public int cnt9 = 0; // 動画切り出し処理時に事前に同一フォルダ名やファイル名が存在するかどうか判定するために使用する。
        public int cnt10 = 0; // 動画切り出しポイントの開始と終了を判定するために使用する。
        public int cnt11 = 0; // 動画切り出し処理に進むかどうかをチェックするために使用する。
        public int cnt16 = 0; // 動画切り出し数の総数をカウントするために使用する。

        public int kiridashi_cnt1 = 1;  //動画切り出し用タグファイル作成時のカウント数を設定するために使用する。
        public int start_seconds;       //動画切り出し用タグファイル作成時に、開始時間を設定するために使用する。
        public int stop_seconds;        //動画切り出し用タグファイル作成時に、終了時間を設定するために使用する。
        public int interval_seconds;    //動画切り出し用タグファイル作成時に、切り出し間隔を設定するために使用する。
        public int n; //HSカメラの倍率

        public float value;

        private const int MOUSEEVENTF_LEFTDOWN = 0x2;
        private const int MOUSEEVENTF_LEFTUP = 0x4;
        public const int WM_LBUTTONDOWN = 0x201;
        public const int WM_LBUTTONUP = 0x202;
        public const int MK_LBUTTON = 0x0001;
        public const int BM_CLICK = 0x00F5;
        public const int VM_COMMAND = 0x0111;
        public const int CB_SELSTRING = 0x014D;


        [DllImport("user32.dll")]
        public static extern int PostMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, string lParam);

        [DllImport("user32.dll",CharSet =CharSet.Auto)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndparent, IntPtr hwndChildafter, string lpszClass, string lpszWindow);

        public MainWindow()
        {
            InitializeComponent();

           

            //TagDateフォルダが作成されているかチェックを行う。なければ作成する。
            string folderpath1 = @"C:\TagAdding\TagDate";

            if (Directory.Exists(folderpath1))
            {
                //Folderがある場合は何もしない
            }
            else
            {
                DirectoryInfo di1 = new DirectoryInfo(folderpath1);
                di1.Create();

                System.Windows.MessageBox.Show("TagDateフォルダを作成しました。");
            }


            //TagListフォルダが作成されているかチェックを行う。なければ作成する。
            string folderpath3 = @"C:\TagAdding\TagList";

            if (Directory.Exists(folderpath3))
            {
                //Folderがある場合は何もしない
            }
            else
            {
                DirectoryInfo di3 = new DirectoryInfo(folderpath3);
                di3.Create();

                System.Windows.MessageBox.Show("TagListフォルダを作成しました。");
            }

            //Wowza-vbsフォルダが作成されているかチェックを行う。なければ作成する。
            string folderpath4 = @"C:\TagAdding\Wowza-vbs";

            if (Directory.Exists(folderpath4))
            {
                //Folerがある場合は何もしない
            }
            else
            {
                DirectoryInfo di4 = new DirectoryInfo(folderpath4);
                di4.Create();

                System.Windows.MessageBox.Show("Wowza-vbsフォルダを作成しました。");
            }



            /// ComboBox2に、Name-List1で記載した名前一覧を名前のComboBoxに表示させる処理
            try
            {
                StreamReader file1 = new StreamReader(@"C:\TagAdding\TagList\\Name-List1.txt", Encoding.Default);
                {
                    while ((line1 = file1.ReadLine()) != null)
                    {
                        comboBox2.Items.Add(line1);
                    }
                }
            }
            catch
            {
                System.Windows.MessageBox.Show("Name-List1ファイルが見つかりませんでした。");
                System.Windows.MessageBox.Show(@"C:\TagAdding\TagList\Name-List1.txt" + "を格納してください。");
            }


            /// ComboBox8に、Name-List2で記載した名前一覧を名前のComboBoxに表示させる処理
            try
            {
                StreamReader file2 = new StreamReader(@"C:\TagAdding\TagList\\Name-List2.txt", Encoding.Default);
                {
                    while ((line2 = file2.ReadLine()) != null)
                    {
                        comboBox8.Items.Add(line2);
                    }
                }
            }
            catch
            {
                System.Windows.MessageBox.Show("Name-List2ファイルが見つかりませんでした。");
                System.Windows.MessageBox.Show(@"C:\TagAdding\TagList\Name-List2.txt" + "を格納してください。");
            }

            //Kiridashiフォルダが作成されているかチェックを行う。なければ作成する。
            string folderpath5 = @"C:\TagAdding\Kiridashi";

            if (Directory.Exists(folderpath5))
            {
                //Folerがある場合は何もしない
            }
            else
            {
                DirectoryInfo di5 = new DirectoryInfo(folderpath5);
                di5.Create();

                System.Windows.MessageBox.Show("Kiridashiフォルダを作成しました。");
            }

            //HS-Camerフォルダが作成されているかチェックを行う。なければ作成する。
            string folderpath6 = @"C:\TagAdding\HS-Camera";

            if (Directory.Exists(folderpath6))
            {
                //Folerがある場合は何もしない
            }
            else
            {
                DirectoryInfo di6 = new DirectoryInfo(folderpath6);
                di6.Create();

                System.Windows.MessageBox.Show("HS-Cameraフォルダを作成しました。");
            }

            comboValue3 = comboBox3.Text; //Hチーム
            textBox1.AppendText("①" + comboValue3);

            comboValue2 = comboBox2.Text; //H名前
            textBox1.AppendText("②" + comboValue2);

            comboValue7 = comboBox7.Text; //Vチーム
            textBox1.AppendText("③" + comboValue7);

            comboValue8 = comboBox8.Text; //V名前
            textBox1.AppendText("④" + comboValue8);

            comboValue4 = comboBox4.Text; //回数
            textBox1.AppendText("⑤" + comboValue4);

            comboValue5 = comboBox5.Text; //カウント
            textBox1.AppendText("⑥" + comboValue5);

            comboValue6 = comboBox6.Text; //球速
            textBox1.AppendText("⑦" + comboValue6 + "⑧");
        }


        private void Button_Click(object sender, RoutedEventArgs e) ///"録画開始+基準時間記録"ボタン押下時の処理
        {
            

            if (cnt == 0)
            {
                if (ComboBox10.Text == "NW Cam(x1)")　//NWカメラ録画開始の場合
                {
                    // NWカメラの録画開始(wowza録画vbsファイルの呼び出し)
                    System.Diagnostics.Process p = System.Diagnostics.Process.Start(@"C:\TagAdding\Wowza-vbs\\Wowza record start.vbs");
                }
                else if (ComboBox10.Text == "HS Cam(x2)") //HSカメラ（×２）録画開始の場合
                {
                    if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 1) //Windows7の場合
                    {
                        int n = 2;

                        // HSカメラの録画開始メソッド呼び出し
                        HS_REC_START_Win7(n);

                    }
                    else if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 2) //Windows10(Windows8互換性)の場合
                    {
                        int n = 2;

                        // HSカメラの録画開始メソッド呼び出し
                        HS_REC_START_Win10(n);

                    }
                    else if (System.Environment.OSVersion.Version.Major == 10 && System.Environment.OSVersion.Version.Minor == 0) //Windows10(Windows8互換性)の場合
                    {
                        int n = 2;

                        // HSカメラの録画開始メソッド呼び出し
                        HS_REC_START_Win10(n);

                    }
                }
                else if (ComboBox10.Text == "HS Cam(x4)") //HSカメラ（×４）録画開始の場合
                {
                    if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 1) //Windows7の場合
                    {
                        int n = 4;

                        // HSカメラの録画開始メソッド呼び出し
                        HS_REC_START_Win7(n);

                    }
                    else if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 2) //Windows10(Windows8互換性)の場合
                    {
                        int n = 4;

                        // HSカメラの録画開始メソッド呼び出し
                        HS_REC_START_Win10(n);

                    }
                    else if (System.Environment.OSVersion.Version.Major == 10 && System.Environment.OSVersion.Version.Minor == 0) //Windows10(Windows8互換性)の場合
                    {
                        int n = 4;

                        // HSカメラの録画開始メソッド呼び出し
                        HS_REC_START_Win10(n);

                    }
                }
                else if (ComboBox10.Text == "HS Cam(x5)") //HSカメラ（×５）録画開始の場合
                {
                    if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 1) //Windows7の場合
                    {
                        int n = 5;

                        // HSカメラの録画開始メソッド呼び出し
                        HS_REC_START_Win7(n);

                    }
                    else if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 2) //Windows10(Windows8互換性)の場合
                    {
                        int n = 5;

                        // HSカメラの録画開始メソッド呼び出し
                        HS_REC_START_Win10(n);

                    }
                    else if (System.Environment.OSVersion.Version.Major == 10 && System.Environment.OSVersion.Version.Minor == 0) //Windows10(Windows8互換性)の場合
                    {
                        int n = 5;

                        // HSカメラの録画開始メソッド呼び出し
                        HS_REC_START_Win10(n);

                    }
                }
                else if (ComboBox10.Text == "HS Cam(x10)") //HSカメラ（×１０）録画開始の場合
                {
                    if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 1) //Windows7の場合
                    {
                        int n = 10;

                        // HSカメラの録画開始メソッド呼び出し
                        HS_REC_START_Win7(n);

                    }
                    else if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 2) //Windows10(Windows8互換性)の場合
                    {
                        int n = 10;

                        // HSカメラの録画開始メソッド呼び出し
                        HS_REC_START_Win10(n);

                    }
                    else if (System.Environment.OSVersion.Version.Major == 10 && System.Environment.OSVersion.Version.Minor == 0) //Windows10(Windows8互換性)の場合
                    {
                        int n = 10;

                        // HSカメラの録画開始メソッド呼び出し
                        HS_REC_START_Win10(n);

                    }
                }
                else if (ComboBox10.Text == "Multi Cam(x2)")
                {
                    if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 1) //Windows7の場合
                    {
                        int n = 2;

                        // HSカメラの録画開始メソッド呼び出し
                        HS_REC_START_Win7(n);

                    }
                    else if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 2) //Windows10(Windows8互換性)の場合
                    {
                        int n = 2;

                        // HSカメラの録画開始メソッド呼び出し
                        HS_REC_START_Win10(n);

                    }
                    else if (System.Environment.OSVersion.Version.Major == 10 && System.Environment.OSVersion.Version.Minor == 0) //Windows10(Windows8互換性)の場合
                    {
                        int n = 2;

                        // HSカメラの録画開始メソッド呼び出し
                        HS_REC_START_Win10(n);

                    }

                    System.Threading.Thread.Sleep(300); // HSカメラの録画開始がNSカメラの録画より0.3秒ほど遅いのでWaitを設定

                    // NWカメラの録画開始(wowza録画vbsファイルの呼び出し)
                    System.Diagnostics.Process p = System.Diagnostics.Process.Start(@"C:\TagAdding\Wowza-vbs\\Wowza record start.vbs");

                }
                else if (ComboBox10.Text == "Multi Cam(x4)")
                {
                    if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 1) //Windows7の場合
                    {
                        int n = 4;

                        // HSカメラの録画開始メソッド呼び出し
                        HS_REC_START_Win7(n);

                    }
                    else if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 2) //Windows10(Windows8互換性)の場合
                    {
                        int n = 4;

                        // HSカメラの録画開始メソッド呼び出し
                        HS_REC_START_Win10(n);

                    }
                    else if (System.Environment.OSVersion.Version.Major == 10 && System.Environment.OSVersion.Version.Minor == 0) //Windows10(Windows8互換性)の場合
                    {
                        int n = 4;

                        // HSカメラの録画開始メソッド呼び出し
                        HS_REC_START_Win10(n);

                    }

                    System.Threading.Thread.Sleep(300); // HSカメラの録画開始がNSカメラの録画より0.3秒ほど遅いのでWaitを設定

                    // NWカメラの録画開始(wowza録画vbsファイルの呼び出し)
                    System.Diagnostics.Process p = System.Diagnostics.Process.Start(@"C:\TagAdding\Wowza-vbs\\Wowza record start.vbs");

                }
                else if (ComboBox10.Text == "Multi Cam(x5)")
                {
                    if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 1) //Windows7の場合
                    {
                        int n = 5;

                        // HSカメラの録画開始メソッド呼び出し
                        HS_REC_START_Win7(n);

                    }
                    else if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 2) //Windows10(Windows8互換性)の場合
                    {
                        int n = 5;

                        // HSカメラの録画開始メソッド呼び出し
                        HS_REC_START_Win10(n);

                    }
                    else if (System.Environment.OSVersion.Version.Major == 10 && System.Environment.OSVersion.Version.Minor == 0) //Windows10(Windows8互換性)の場合
                    {
                        int n = 5;

                        // HSカメラの録画開始メソッド呼び出し
                        HS_REC_START_Win10(n);

                    }

                    System.Threading.Thread.Sleep(300); // HSカメラの録画開始がNSカメラの録画より0.8秒ほど遅いのでWaitを設定

                    // NWカメラの録画開始(wowza録画vbsファイルの呼び出し)
                    System.Diagnostics.Process p = System.Diagnostics.Process.Start(@"C:\TagAdding\Wowza-vbs\\Wowza record start.vbs");

                }
                else if (ComboBox10.Text == "Multi Cam(x10)")
                {
                    if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 1) //Windows7の場合
                    {
                        int n = 10;

                        // HSカメラの録画開始メソッド呼び出し
                        HS_REC_START_Win7(n);

                    }
                    else if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 2) //Windows10(Windows8互換性)の場合
                    {
                        int n = 10;

                        // HSカメラの録画開始メソッド呼び出し
                        HS_REC_START_Win10(n);

                    }
                    else if (System.Environment.OSVersion.Version.Major == 10 && System.Environment.OSVersion.Version.Minor == 0) //Windows10(Windows8互換性)の場合
                    {
                        int n = 10;

                        // HSカメラの録画開始メソッド呼び出し
                        HS_REC_START_Win10(n);

                    }

                    System.Threading.Thread.Sleep(300); // HSカメラの録画開始がNSカメラの録画より0.3秒ほど遅いのでWaitを設定

                    // NWカメラの録画開始(wowza録画vbsファイルの呼び出し)
                    System.Diagnostics.Process p = System.Diagnostics.Process.Start(@"C:\TagAdding\Wowza-vbs\\Wowza record start.vbs");

                }

                string filePath1 = @"C:\TagAdding\TagDate\genzaijikoku.txt";

                dt1 = DateTime.Now;

                StreamWriter sw1 = new StreamWriter(filePath1, false, Encoding.UTF8);

                string result1 = dt1.ToString("HH:mm:ss");

               //MessageBox.Show(dt1.ToString());

                sw1.Write(result1);

                sw1.Close();

                Button2.IsEnabled = true; //録画停止ボタンを活性化
                Button1.IsEnabled = false; //録画開始ボタンを非活性化
                Button3.IsEnabled = true; //切り出し開始ボタンを活性化

                //カメラ種別を選択できなくするためにCombBox10を無効にする。
                ComboBox10.IsEnabled = false;

            }
            else
            {
                System.Windows.Forms.MessageBox.Show("既に録画中です。「録画停止+タグ停止」ボタンを押下してください。");
            }

            // Button1が押された場合にスペースキーをForm1にて受け付けるように設定する。
            
            

        }

        private void Button_Click_1(object sender, RoutedEventArgs e) ///"タグ時間記録"ボタン押下時の処理
        {

            DateTime dt3 = new DateTime(2019, 1, 1, 0, 0, 00);

            if (dt1 == dt3)
            {
                System.Windows.Forms.MessageBox.Show("基準時間が取得されていません。");
            }
            else if (ComboBox10.Text == "NW Cam(x1)")
            {
                if (cnt10 == 0)
                {
                    nwcam_seconds_calculation();
                }

                nwcam_kiridashi_time();

            }
            else if (ComboBox10.Text == "HS Cam(x2)")
            {
                n = 2;

                hscam_seconds_calculation(n);

            }
            else if (ComboBox10.Text == "HS Cam(x4)")
            {
                n = 4;

                hscam_seconds_calculation(n);

            }
            else if (ComboBox10.Text == "HS Cam(x5)")
            {
                n = 5;

                hscam_seconds_calculation(n);

            }
            else if (ComboBox10.Text == "HS Cam(x10)")
            {
                n = 10;

                hscam_seconds_calculation(n);

            }
            else if (ComboBox10.Text == "Multi Cam(x2)")
            {
                if (cnt10 == 0)
                {
                    nwcam_seconds_calculation();
                }

                nwcam_kiridashi_time();

                n = 2;

                hscam_seconds_calculation(n);
            }
            else if (ComboBox10.Text == "Multi Cam(x4)")
            {
                if (cnt10 == 0)
                {
                    nwcam_seconds_calculation();
                }

                nwcam_kiridashi_time();

                n = 4;

                hscam_seconds_calculation(n);
            }
            else if (ComboBox10.Text == "Multi Cam(x5)")
            {
                if (cnt10 == 0)
                {
                    nwcam_seconds_calculation();
                }

                nwcam_kiridashi_time();

                n = 5;

                hscam_seconds_calculation(n);
            }
            else if (ComboBox10.Text == "Multi Cam(x10)")
            {
                if (cnt10 == 0)
                {
                    nwcam_seconds_calculation();
                }

                nwcam_kiridashi_time();

                n = 10;

                hscam_seconds_calculation(n);
            }

            if (ComboBox10.Text == "NW Cam(x1)" || ComboBox10.Text == "Multi Cam(x2)" || ComboBox10.Text == "Multi Cam(x4)" || ComboBox10.Text == "Multi Cam(x5)" || ComboBox10.Text == "Multi Cam(x10)")
            {
                if (cnt10 == 0)
                {
                    Button3.Content = "切出状態：終了";
                }
                else if (cnt10 == 1)
                {
                    Button3.Content = "切出状態：開始";
                }
            }
            else if (ComboBox10.Text == "HS Cam(x2)" || ComboBox10.Text == "HS Cam(x4)" || ComboBox10.Text == "HS Cam(x5)" || ComboBox10.Text == "HS Cam(x10)")
            {
                
                    Button3.Content = "タグ付け";  
            }
        }


        private void Button_Click_6(object sender, RoutedEventArgs e) ///クリアボタン
        {
            textBox1.Clear();

            comboValue3 = comboBox3.Text; //Hチーム
            textBox1.AppendText("①" + comboValue3);

            comboValue2 = comboBox2.Text; //H名前
            textBox1.AppendText("②" + comboValue2);

            comboValue7 = comboBox7.Text; //Vチーム
            textBox1.AppendText("③" + comboValue7);

            comboValue8 = comboBox8.Text; //V名前
            textBox1.AppendText("④" + comboValue8);

            comboValue4 = comboBox4.Text; //回数
            textBox1.AppendText("⑤" + comboValue4);

            comboValue5 = comboBox5.Text; //カウント
            textBox1.AppendText("⑥" + comboValue5);

            comboValue6 = comboBox6.Text; //球速
            textBox1.AppendText("⑦" + comboValue6 + "⑧");
        }


        private void ComboBox3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string str1_bf = "";
            string str1_af = "";
            int check1 = 0;

            comboValue3 = comboBox3.SelectedItem.ToString();
            comboValue3 = comboValue3.Replace("System.Windows.Controls.ListBoxItem: ", "");

            str1_bf = textBox1.Text;

            for (int ch1 = 0; ch1 < str1_bf.Length; ch1++)
            {
                if (str1_bf[ch1] == '①')
                {
                    str1_af += str1_bf[ch1].ToString();　//①を設定
                    check1 = 1;
                }
                else if (str1_bf[ch1] == '②')
                {
                    str1_af += comboValue3; //comboValue3の文字列を設定する
                    str1_af += str1_bf[ch1].ToString(); //②を設定
                    check1 = 0;
                }
                else if (check1 == 1)
                {
                    //①と②の間の文字は設定せずに破棄する
                }
                else if (check1 == 0)
                {
                    str1_af += str1_bf[ch1].ToString();  //①と②の間以外はそのまま設定する
                }
            }
            textBox1.Text = str1_af;


        }


        private void Button_Click_3(object sender, RoutedEventArgs e) //"録画停止"ボタン押下時の処理
        {
            if (cnt == 0) //タグファイルが生成されていない状態で録画停止ボタンが押された場合の処理
            {
                string message = "タグファイルが作成されていません。録画停止しますか？";
                string caption = "Delete";

                MessageBoxResult result = System.Windows.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.YesNo);


                if (result == MessageBoxResult.Yes) //タグファイル未作成+録画停止"Yes"の場合
                {
                    //NWカメラ単独の録画停止
                    if (ComboBox10.Text == "NW Cam(x2)") 
                    {
                        System.Diagnostics.Process p = System.Diagnostics.Process.Start(@"C:\TagAdding\Wowza-vbs\\Wowza record stop.vbs");
                    }
                    //HSカメラ単独の録画停止
                    else if (ComboBox10.Text == "HS Cam(x2)" || ComboBox10.Text == "HS Cam(x4)" || ComboBox10.Text == "HS Cam(x5)" || ComboBox10.Text == "HS Cam(x10)")
                    {
                        //Windows7の場合
                        if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 1) 
                        {
                            HS_REC_STOP_Win7();

                        }//Windows10(Windows8互換性)の場合
                        else if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 2) 
                        {
                            HS_REC_STOP_Win10();
                        }
                        //Windows10の場合
                        else if (System.Environment.OSVersion.Version.Major == 10 && System.Environment.OSVersion.Version.Minor == 0) 
                        {
                            HS_REC_STOP_Win10();
                        }
                    }
                    else if (ComboBox10.Text == "Multi Cam(x2)" || ComboBox10.Text == "Multi Cam(x4)" || ComboBox10.Text == "Multi Cam(x5)" || ComboBox10.Text == "Multi Cam(x10)")
                    {
                        // NWカメラの停止処理
                        System.Diagnostics.Process p = System.Diagnostics.Process.Start(@"C:\TagAdding\Wowza-vbs\\Wowza record stop.vbs");

                        // HSカメラの停止処理
                        // Windows7の場合
                        if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 1)
                        {
                            HS_REC_STOP_Win7();

                        }
                        // Windows10(Windows8互換性)の場合
                        else if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 2)
                        {
                            HS_REC_STOP_Win10();
                        }
                        // Windows10の場合
                        else if (System.Environment.OSVersion.Version.Major == 10 && System.Environment.OSVersion.Version.Minor == 0)
                        {
                            HS_REC_STOP_Win10();
                        }
                    }
                    cnt = 0;

                    dt1 = new DateTime(2019, 1, 1, 0, 0, 00);

                }
            }
            else if (cnt != 0) //初期化処理＋録画停止処理(タグファイル作成済みの場合)
            {
                // NWカメラ録画停止場合
                if (ComboBox10.Text == "NW Cam(x1)")
                {
                    System.Diagnostics.Process p = System.Diagnostics.Process.Start(@"C:\TagAdding\Wowza-vbs\\Wowza record stop.vbs");
                }
                // HSカメラ録画停止の場合
                else if (ComboBox10.Text == "HS Cam(x2)" || ComboBox10.Text == "HS Cam(x4)" || ComboBox10.Text == "HS Cam(x5)" || ComboBox10.Text == "HS Cam(x10)")
                {
                    // Windows7の場合
                    if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 1) //Windows7の場合
                    {
                        HS_REC_STOP_Win7();
                    }
                    // Windows10(Windows8互換性)の場合
                    else if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 2) //Windows10(Windows8との互換性)の場合
                    {
                        HS_REC_STOP_Win10();
                    }
                    // Windows10の場合
                    else if (System.Environment.OSVersion.Version.Major == 10 && System.Environment.OSVersion.Version.Minor == 0) //Windows10の場合
                    {
                        HS_REC_STOP_Win10();
                    }
                }
                //NW+HSカメラ録画停止の場合
                else if (ComboBox10.Text == "Multi Cam(x2)" || ComboBox10.Text == "Multi Cam(x4)" || ComboBox10.Text == "Multi Cam(x5)" || ComboBox10.Text == "Multi Cam(x10)")
                {
                    // NWカメラの停止処理
                    System.Diagnostics.Process p = System.Diagnostics.Process.Start(@"C:\TagAdding\Wowza-vbs\\Wowza record stop.vbs");

                    // HSカメラの停止処理
                    // Windows7の場合
                    if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 1) //Windows7の場合
                    {
                        HS_REC_STOP_Win7();
                    }
                    // Windows10(Windows8互換性)の場合
                    else if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 2) //Windows10(Windows8との互換性)の場合
                    {
                        HS_REC_STOP_Win10();
                    }
                    // Windows10の場合
                    else if (System.Environment.OSVersion.Version.Major == 10 && System.Environment.OSVersion.Version.Minor == 0) //Windows10の場合
                    {
                        HS_REC_STOP_Win10();
                    }
                }
                cnt = 0;

                dt1 = new DateTime(2019, 1, 1, 0, 0, 00);
            }

            Button2.IsEnabled = false; //録画停止ボタンを非活性化
            Button1.IsEnabled = true;  //録画再生ボタンを活性化
            Button3.IsEnabled = false; //切り出し開始ボタンを非活性化


            // 録画停止したことによりカメラ種別を選択出来るようにComboBox10を有効にする。
            ComboBox10.IsEnabled = true;


            kiridashi_cnt1 = 1;

        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)  //①抽出するファイルパス取得ボタンの処理
        {

            textBox3.Clear(); //textBox3の初期値をクリア

            var dialog1 = new Microsoft.Win32.OpenFileDialog();

            dialog1.InitialDirectory = @"C:\TagAdding\TagDate"; //フォルダ指定

            dialog1.Title = "抽出元のファイルを選んでください"; //ダイアログタイトル指定

            dialog1.Filter = "テキストファイル(*.txt)|*.txt|全てのファイル(*.*)|*.*";

            if (dialog1.ShowDialog() == true)
            {
                textBox3.AppendText(dialog1.FileName);
            }

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged_2(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click_5(object sender, RoutedEventArgs e) //④抽出ボタンの処理
        {
            Regex rgx = new Regex(comboBox9.Text, RegexOptions.IgnoreCase); //comboBox9に表示される名前をrgxに設定

            if (comboBox9.Text == "③名前を選択")
            {
                System.Windows.MessageBox.Show("抽出する名前が選ばれていません");
            }

            if (textBox3.Text == "")
            {
                System.Windows.MessageBox.Show("ベースタグファイルが選ばれていません");
            }
            else
            {
                StreamReader file4 = new StreamReader(textBox3.Text, Encoding.Default); //textBox3に表示されたファイルをfile4に設定
                {
                    line4 = "";　//line4初期化

                    string str1 = Regex.Replace(textBox3.Text, @"[^0-9]", ""); //TextBox3に表示されたパスから数値だけを抜き出す。ファイル名に使用するため

                    while ((line4 = file4.ReadLine()) != null) //file4の情報を1行ずつ読み込み。情報なくなったら終了
                    {
                        if (rgx.Match(line4).Success)　//comboBox9で表示された名前がtextBox3の1行にあるかどうか判定。ある場合処理を行う
                        {
                            
                            StreamWriter sw3 = new StreamWriter(@"C:\TagAdding\TagDate\" + str1 +  "-" + comboBox9.Text + ".txt", true, Encoding.Default);

                            sw3.Write(line4);
                            sw3.Write(Environment.NewLine);
                            sw3.Close();

                            cnt5++;
                        }
                    }

                    if (cnt5 == 0)
                    {
                        System.Windows.MessageBox.Show("該当するデータがありませんでした。");
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("抽出完了" + " " + "該当するデータは"+cnt5+"件でした。");
                        cnt5 = 0;
                    }
                }
            }
        }

        private void ComboBox9_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) //②Name-Listを選択ボタンの処理
        {
            comboBox9.Items.Clear();
            comboBox9.Items.Add("③名前を選択");

            var dialog2 = new Microsoft.Win32.OpenFileDialog();

            dialog2.InitialDirectory = @"C:\TagAdding\TagList";

            dialog2.Title = "抽出したい名前があるName-Listを選んでください";

            dialog2.Filter = "テキストファイル(*.txt)|*.txt|全てのファイル(*.*)|*.*";

            if (dialog2.ShowDialog() == true)
            {
                StreamReader file3 = new StreamReader(dialog2.FileName, Encoding.Default);
                {
                    while ((line3 = file3.ReadLine()) != null)
                    {
                        comboBox9.Items.Add(line3);
                    }
                }
            }

        }


        private void ComboBox5_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string str1_bf = "";
            string str1_af = "";
            int check1 = 0;

            comboValue5 = comboBox5.SelectedItem.ToString();
            comboValue5 = comboValue5.Replace("System.Windows.Controls.ListBoxItem: ", "");

            str1_bf = textBox1.Text;

            for (int ch1 = 0; ch1 < str1_bf.Length; ch1++)
            {
                if (str1_bf[ch1] == '⑥')
                {
                    str1_af += str1_bf[ch1].ToString();　//⑥を設定
                    check1 = 1;
                }
                else if (str1_bf[ch1] == '⑦')
                {
                    str1_af += comboValue5; //comboValue5の文字列を設定する
                    str1_af += str1_bf[ch1].ToString(); //⑦を設定
                    check1 = 0;
                }
                else if (check1 == 1)
                {
                    //⑥と⑦の間の文字は設定せずに破棄する
                }
                else if (check1 == 0)
                {
                    str1_af += str1_bf[ch1].ToString();  //①と②の間以外はそのまま設定する
                }
            }
            textBox1.Text = str1_af;
        }

        private void ComboBox7_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string str1_bf = "";
            string str1_af = "";
            int check1 = 0;

            comboValue7 = comboBox7.SelectedItem.ToString();
            comboValue7 = comboValue7.Replace("System.Windows.Controls.ListBoxItem: ", "");

            str1_bf = textBox1.Text;

            for (int ch1 = 0; ch1 < str1_bf.Length; ch1++)
            {
                if (str1_bf[ch1] == '③')
                {
                    str1_af += str1_bf[ch1].ToString();　//③を設定
                    check1 = 1;
                }
                else if (str1_bf[ch1] == '④')
                {
                    str1_af += comboValue7; //comboValue7の文字列を設定する
                    str1_af += str1_bf[ch1].ToString(); //④を設定
                    check1 = 0;
                }
                else if (check1 == 1)
                {
                    //③と④の間の文字は設定せずに破棄する
                }
                else if (check1 == 0)
                {
                    str1_af += str1_bf[ch1].ToString();  //③と④の間以外はそのまま設定する
                }
            }
            textBox1.Text = str1_af;
        }

        private void ComboBox6_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string str1_bf = "";
            string str1_af = "";
            int check1 = 0;

            comboValue6 = comboBox6.SelectedItem.ToString();
            comboValue6 = comboValue6.Replace("System.Windows.Controls.ListBoxItem: ", "");

            str1_bf = textBox1.Text;

            for (int ch1 = 0; ch1 < str1_bf.Length; ch1++)
            {
                if (str1_bf[ch1] == '⑦')
                {
                    str1_af += str1_bf[ch1].ToString();　//⑦を設定
                    check1 = 1;
                }
                else if (str1_bf[ch1] == '⑧')
                {
                    str1_af += comboValue6; //comboValue6の文字列を設定する
                    str1_af += str1_bf[ch1].ToString(); //⑧を設定
                    check1 = 0;
                }
                else if (check1 == 1)
                {
                    //①と②の間の文字は設定せずに破棄する
                }
                else if (check1 == 0)
                {
                    str1_af += str1_bf[ch1].ToString();  //①と②の間以外はそのまま設定する
                }
            }
            textBox1.Text = str1_af;
        }

        private void ComboBox8_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string str1_bf = "";
            string str1_af = "";
            int check1 = 0;

            comboValue8 = comboBox8.SelectedItem.ToString();
            comboValue8 = comboValue8.Replace("System.Windows.Controls.ListBoxItem: ", "");

            str1_bf = textBox1.Text;

            for (int ch1 = 0; ch1 < str1_bf.Length; ch1++)
            {
                if (str1_bf[ch1] == '④')
                {
                    str1_af += str1_bf[ch1].ToString();　//④を設定
                    check1 = 1;
                }
                else if (str1_bf[ch1] == '⑤')
                {
                    str1_af += comboValue8; //comboValue8の文字列を設定する
                    str1_af += str1_bf[ch1].ToString(); //④を設定
                    check1 = 0;
                }
                else if (check1 == 1)
                {
                    //④と⑤の間の文字は設定せずに破棄する
                }
                else if (check1 == 0)
                {
                    str1_af += str1_bf[ch1].ToString();  //④と⑤の間以外はそのまま設定する
                }
            }
            textBox1.Text = str1_af;
        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            string str1_bf = "";
            string str1_af = "";
            int check1 = 0;

            comboValue4 = comboBox4.SelectedItem.ToString();
            comboValue4 = comboValue4.Replace("System.Windows.Controls.ListBoxItem: ", "");

            str1_bf = textBox1.Text;

            for (int ch1 = 0; ch1 < str1_bf.Length; ch1++)
            {
                if (str1_bf[ch1] == '⑤')
                {
                    str1_af += str1_bf[ch1].ToString();　//⑤を設定
                    check1 = 1;
                }
                else if (str1_bf[ch1] == '⑥')
                {
                    str1_af += comboValue4; //comboValue4の文字列を設定する
                    str1_af += str1_bf[ch1].ToString(); //⑥を設定
                    check1 = 0;
                }
                else if (check1 == 1)
                {
                    //⑤と⑥の間の文字は設定せずに破棄する
                }
                else if (check1 == 0)
                {
                    str1_af += str1_bf[ch1].ToString();  //⑤と⑥の間以外はそのまま設定する
                }
            }
            textBox1.Text = str1_af;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string str1_bf = "";
            string str1_af = "";
            int check1 = 0;

            comboValue2 = comboBox2.SelectedItem.ToString();
            comboValue2 = comboValue2.Replace("System.Windows.Controls.ListBoxItem: ", "");

            str1_bf = textBox1.Text;

            for (int ch1 = 0; ch1 < str1_bf.Length; ch1++)
            {
                if (str1_bf[ch1] == '②')
                {
                    str1_af += str1_bf[ch1].ToString();　//②を設定
                    check1 = 1;
                }
                else if (str1_bf[ch1] == '③')
                {
                    str1_af += comboValue2; //comboValue2の文字列を設定する
                    str1_af += str1_bf[ch1].ToString(); //③を設定
                    check1 = 0;
                }
                else if (check1 == 1)
                {
                    //②と③の間の文字は設定せずに破棄する
                }
                else if (check1 == 0)
                {
                    str1_af += str1_bf[ch1].ToString();  //②と③の間以外はそのまま設定する
                }
            }
            textBox1.Text = str1_af;
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {

        }

        private IntPtr Multi_Video_Handle9_Win7()
        {
            IntPtr hWnd = FindWindow(null, "Multi-Video Viewer");
            System.Diagnostics.Trace.WriteLine("①" + hWnd);

            IntPtr hWndc1 = FindWindowEx(hWnd, IntPtr.Zero, "WindowsForms10.Window.8.app.0.fb11c8_r17_ad1", "");
            System.Diagnostics.Trace.WriteLine("②" + hWndc1);

            IntPtr hWndc2 = FindWindowEx(hWndc1, IntPtr.Zero, "WindowsForms10.Window.8.app.0.fb11c8_r17_ad1", "");
            System.Diagnostics.Trace.WriteLine("③" + hWndc2);

            IntPtr hWndc3 = FindWindowEx(hWndc2, IntPtr.Zero, "WindowsForms10.SysTabControl32.app.0.fb11c8_r17_ad1", "");
            System.Diagnostics.Trace.WriteLine("④" + hWndc3);

            IntPtr hWndc4 = FindWindowEx(hWndc3, IntPtr.Zero, "WindowsForms10.Window.8.app.0.fb11c8_r17_ad1", "Rec");
            System.Diagnostics.Trace.WriteLine("⑤" + hWndc4);

            IntPtr hWndc5 = FindWindowEx(hWndc4, IntPtr.Zero, "WindowsForms10.Window.8.app.0.fb11c8_r17_ad1", "");
            System.Diagnostics.Trace.WriteLine("⑥" + hWndc5);

            IntPtr hWndc6 = FindWindowEx(hWndc5, IntPtr.Zero, "WindowsForms10.Window.8.app.0.fb11c8_r17_ad1", "");
            System.Diagnostics.Trace.WriteLine("⑦" + hWndc6);

            IntPtr hWndc7 = FindWindowEx(hWndc6, IntPtr.Zero, "WindowsForms10.BUTTON.app.0.fb11c8_r17_ad1", "REC START");
            System.Diagnostics.Trace.WriteLine("REC START_" + hWndc7);

            IntPtr hWndc9 = FindWindowEx(hWndc5, hWndc6, "WindowsForms10.Window.8.app.0.fb11c8_r17_ad1", "");
            System.Diagnostics.Trace.WriteLine("⑨" + hWndc9);


            return hWndc9;

        }

        private IntPtr Multi_Video_Handle9_Win10()
        {
            IntPtr hWnd = FindWindow(null, "Multi-Video Viewer");
            System.Diagnostics.Trace.WriteLine("①" + hWnd);

            IntPtr hWndc1 = FindWindowEx(hWnd, IntPtr.Zero, "WindowsForms10.Window.8.app.0.fb11c8_r9_ad1", "");
            System.Diagnostics.Trace.WriteLine("②" + hWndc1);

            IntPtr hWndc2 = FindWindowEx(hWndc1, IntPtr.Zero, "WindowsForms10.Window.8.app.0.fb11c8_r9_ad1", "");
            System.Diagnostics.Trace.WriteLine("③" + hWndc2);

            IntPtr hWndc3 = FindWindowEx(hWndc2, IntPtr.Zero, "WindowsForms10.SysTabControl32.app.0.fb11c8_r9_ad1", "");
            System.Diagnostics.Trace.WriteLine("④" + hWndc3);

            IntPtr hWndc4 = FindWindowEx(hWndc3, IntPtr.Zero, "WindowsForms10.Window.8.app.0.fb11c8_r9_ad1", "Rec");
            System.Diagnostics.Trace.WriteLine("⑤" + hWndc4);

            IntPtr hWndc5 = FindWindowEx(hWndc4, IntPtr.Zero, "WindowsForms10.Window.8.app.0.fb11c8_r9_ad1", "");
            System.Diagnostics.Trace.WriteLine("⑥" + hWndc5);

            IntPtr hWndc6 = FindWindowEx(hWndc5, IntPtr.Zero, "WindowsForms10.Window.8.app.0.fb11c8_r9_ad1", "");
            System.Diagnostics.Trace.WriteLine("⑦" + hWndc6);

            IntPtr hWndc7 = FindWindowEx(hWndc6, IntPtr.Zero, "WindowsForms10.BUTTON.app.0.fb11c8_r9_ad1", "REC START");
            System.Diagnostics.Trace.WriteLine("REC START_" + hWndc7);

            IntPtr hWndc9 = FindWindowEx(hWndc5, hWndc6, "WindowsForms10.Window.8.app.0.fb11c8_r9_ad1", "");
            System.Diagnostics.Trace.WriteLine("⑨" + hWndc9);

            return hWndc9;
        }

        private IntPtr Multi_Video_Handle6_Win7()
        {
            IntPtr hWnd = FindWindow(null, "Multi-Video Viewer");
            System.Diagnostics.Trace.WriteLine("①" + hWnd);

            IntPtr hWndc1 = FindWindowEx(hWnd, IntPtr.Zero, "WindowsForms10.Window.8.app.0.fb11c8_r17_ad1", "");
            System.Diagnostics.Trace.WriteLine("②" + hWndc1);

            IntPtr hWndc2 = FindWindowEx(hWndc1, IntPtr.Zero, "WindowsForms10.Window.8.app.0.fb11c8_r17_ad1", "");
            System.Diagnostics.Trace.WriteLine("③" + hWndc2);

            IntPtr hWndc3 = FindWindowEx(hWndc2, IntPtr.Zero, "WindowsForms10.SysTabControl32.app.0.fb11c8_r17_ad1", "");
            System.Diagnostics.Trace.WriteLine("④" + hWndc3);

            IntPtr hWndc4 = FindWindowEx(hWndc3, IntPtr.Zero, "WindowsForms10.Window.8.app.0.fb11c8_r17_ad1", "Rec");
            System.Diagnostics.Trace.WriteLine("⑤" + hWndc4);

            IntPtr hWndc5 = FindWindowEx(hWndc4, IntPtr.Zero, "WindowsForms10.Window.8.app.0.fb11c8_r17_ad1", "");
            System.Diagnostics.Trace.WriteLine("⑥" + hWndc5);

            IntPtr hWndc6 = FindWindowEx(hWndc5, IntPtr.Zero, "WindowsForms10.Window.8.app.0.fb11c8_r17_ad1", "");
            System.Diagnostics.Trace.WriteLine("⑦" + hWndc6);

            return hWndc6;
        }

        private IntPtr Multi_Video_Handle6_Win10()
        {
            IntPtr hWnd = FindWindow(null, "Multi-Video Viewer");
            System.Diagnostics.Trace.WriteLine("①" + hWnd);

            IntPtr hWndc1 = FindWindowEx(hWnd, IntPtr.Zero, "WindowsForms10.Window.8.app.0.fb11c8_r9_ad1", "");
            System.Diagnostics.Trace.WriteLine("②" + hWndc1);

            IntPtr hWndc2 = FindWindowEx(hWndc1, IntPtr.Zero, "WindowsForms10.Window.8.app.0.fb11c8_r9_ad1", "");
            System.Diagnostics.Trace.WriteLine("③" + hWndc2);

            IntPtr hWndc3 = FindWindowEx(hWndc2, IntPtr.Zero, "WindowsForms10.SysTabControl32.app.0.fb11c8_r9_ad1", "");
            System.Diagnostics.Trace.WriteLine("④" + hWndc3);

            IntPtr hWndc4 = FindWindowEx(hWndc3, IntPtr.Zero, "WindowsForms10.Window.8.app.0.fb11c8_r9_ad1", "Rec");
            System.Diagnostics.Trace.WriteLine("⑤" + hWndc4);

            IntPtr hWndc5 = FindWindowEx(hWndc4, IntPtr.Zero, "WindowsForms10.Window.8.app.0.fb11c8_r9_ad1", "");
            System.Diagnostics.Trace.WriteLine("⑥" + hWndc5);

            IntPtr hWndc6 = FindWindowEx(hWndc5, IntPtr.Zero, "WindowsForms10.Window.8.app.0.fb11c8_r9_ad1", "");
            System.Diagnostics.Trace.WriteLine("⑦" + hWndc6);

            return hWndc6;
        }

        private void hscam_seconds_calculation(int n)
        {
            DateTime dt2 = DateTime.Now;

            filePath2 = @"C:\TagAdding\TagDate\";

            TimeSpan interval = dt2 - dt1;

            int milliseconds = interval.Milliseconds * n;
            milliseconds = milliseconds / 1000;
            int seconds = interval.Seconds * n;
            int minutes_seconds = (interval.Minutes * n) * 60;
            int hours_seconds = (interval.Hours * n) * 3600;

            seconds = milliseconds + seconds + minutes_seconds + hours_seconds;

            //MessageBox.Show(milliseconds.ToString());
            //MessageBox.Show(seconds.ToString());
            //MessageBox.Show(interval.ToString());

            while (seconds >= 1801)
            {
                seconds -= 1800;
                cnt2++;
            }

            int minutes = seconds / 60;
            seconds = seconds % 60;

            TimeSpan ts1 = new TimeSpan(0, minutes, seconds);

            if (cnt == 0)
            {
                now = dt1.ToString("yyyyMMddHHmmss");

            }

            // ファイル数が10未満の場合のファイル名付与(-0をつけている)
            if (cnt2 < 10)
            {
                sw2 = new StreamWriter(filePath2 + now + "-0" + cnt2 + ".txt", true, Encoding.UTF8);
            }
            // ファイル数が10以上の場合のファイル名付与(カウント数がそのままファイル名末尾に付く)
            else if (cnt2 >= 10)
            {
                sw2 = new StreamWriter(filePath2 + now + "-" + cnt2 + ".txt", true, Encoding.UTF8);
            }

            cnt++;
            cnt2 = 1;



            sw2.Write(ts1 + "…");

            string textValue = textBox1.Text;

            sw2.Write(textValue);
            sw2.Write(Environment.NewLine);
            sw2.Close();
        }

        private void nwcam_seconds_calculation()
        {
            DateTime dt2 = DateTime.Now;

            filePath2 = @"C:\TagAdding\TagDate\";

            TimeSpan interval = dt2 - dt1;

            int seconds = interval.Seconds;
            int minutes = interval.Minutes;
            int hours = interval.Hours;

            TimeSpan ts1 = new TimeSpan(hours, minutes, seconds);

            if (cnt == 0)
            {
                now = dt1.ToString("yyyyMMddHHmmss");

            }

            sw2 = new StreamWriter(filePath2 + now + ".txt", true, Encoding.UTF8);
            cnt++;


            sw2.Write(ts1 + "…");

            string textValue = textBox1.Text;
            //MessageBox.Show(textValue);

            sw2.Write(textValue);
            sw2.Write(Environment.NewLine);
            sw2.Close();
        }



        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            // 抽出用元の動画ファイル名を取り込む処理
            var dialog3 = new Microsoft.Win32.OpenFileDialog();

            dialog3.Multiselect = true;

            dialog3.InitialDirectory = @"C:\TagAdding\Kiridashi"; //フォルダ指定

            dialog3.Title = "抽出元の動画ファイルを選んでください"; //ダイアログタイトル指定

            dialog3.Filter = "MP4ファイル(*.MP4)|*.MP4|全てのファイル(*.*)|*.*";

            if (dialog3.ShowDialog() == true)
            {
                if (dialog3.FileNames.Length < 8)
                {
                    System.Diagnostics.Trace.WriteLine(dialog3.FileName);
                    cnt4 = dialog3.FileNames.Length;
                    cnt6 = 1;

                    for (cnt3 = 0; cnt3 < dialog3.FileNames.Length; cnt3++)
                    {
                        file_MP4[cnt3] = dialog3.FileNames[cnt3];

                        System.Diagnostics.Trace.WriteLine(dialog3.FileNames.Length);
                        System.Diagnostics.Trace.WriteLine(cnt3);
                        //System.Diagnostics.Trace.WriteLine(dialog3.FileNames[cnt3]);
                        //System.Diagnostics.Trace.WriteLine(file_MP4[cnt3]);
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("ファイル数が8ファイル以上選ばれています。7以下としてください。");
                }

            }
        }

        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            // 抽出用の切り出しタグファイル名を取り込む処理
            var dialog4 = new Microsoft.Win32.OpenFileDialog();

            dialog4.InitialDirectory = @"C:\TagAdding\Kiridashi"; //フォルダ指定

            dialog4.Title = "切り出しタグファイルを選んでください"; //ダイアログタイトル指定

            dialog4.Filter = "テキストファイル(*.txt)|*.txt|全てのファイル(*.*)|*.*";

            if (dialog4.ShowDialog() == true)
            {
                System.Diagnostics.Trace.WriteLine(dialog4.FileName);
                file_KiridashiTag = dialog4.FileName;
                cnt7 = 1;

                ProgressBar1.Value = 0;
                textBox4.Clear();
                value = 0;
                cnt16 = 0;

                // 動画切り出し数の総数をカウントする
                StreamReader file6 = new StreamReader(file_KiridashiTag, Encoding.Default);
                {
                    while ((file6.ReadLine()) != null)
                    {
                        cnt16++;
                    }
                }

            }


        }

        private void Button_Click_11(object sender, RoutedEventArgs e)
        {
            ProgressBar1.Minimum = 0;
            ProgressBar1.Maximum = 0;// cnt16 * (cnt4 - cnt9));
            ProgressBar1.Value = 0;

            if (cnt6 == 0)
            {
                System.Windows.MessageBox.Show("抽出元のMP4ファイルが選択されていません。");
            }
            else if (cnt7 == 0)
            {
                System.Windows.MessageBox.Show("抽出用の時間ファイルが選択されていません。");
            }
            else
            {

                for (int num = 0; num < cnt4; num++)
                {


                    var outputfile = "";
                    var outputfile2 = "";

                    //抽出元ファイルのファイル名から.mp4/.MP4を削除
                    outputfile = file_MP4[num].Replace(".MP4", "");
                    outputfile = outputfile.Replace(".mp4", "");

                    output_folder = outputfile;

                    if (Directory.Exists(output_folder))
                    {
                        System.Windows.MessageBox.Show("動画切出し用フォルダ" + outputfile + "が既に存在します。");
                        cnt9 ++;

                    }
                    else if (File.Exists(output_folder))
                    {
                        System.Windows.MessageBox.Show("動画切出し用フォルダと同一ファイル名" + outputfile + "が存在するため、フォルダが作成できません。") ;
                        cnt9 ++;

                    }
                    else
                    {
                        DirectoryInfo di6 = new DirectoryInfo(output_folder);
                        di6.Create();

                        string outputfile_name = System.IO.Path.GetFileName(file_MP4[num]);

                        outputfile_name = outputfile_name.Replace(".MP4", "");
                        outputfile_name = outputfile_name.Replace(".mp4", "");

                        StreamReader file5 = new StreamReader(file_KiridashiTag, Encoding.Default);
                        {
                            while ((line5 = file5.ReadLine()) != null)
                            {
                                cnt8++;
                                string str_start_seconds = "|start|";
                                string str_stop_seconds = "|stop|";
                                string str_interval_seconds = "|interval|";
                                string filename_add = "";

                                //切出し開始時間抽出関数呼び出し
                                string StartSeconds = GetStartSeconds(str_start_seconds, str_stop_seconds, line5);
                                //System.Diagnostics.Trace.WriteLine(StartSeconds);
                                //System.Diagnostics.Trace.WriteLine(line5);

                                //切出し時間抽出関数呼び出し
                                string IntervalSeconds = GetIntervalSeconds(str_interval_seconds, line5);
                                //System.Diagnostics.Trace.WriteLine(IntervalSeconds);

                                string str_cnt8 = String.Format("{0:000}", cnt8);

                                //ffmpegの仕様でフォルダ名+ファイル名を指定する場合にフォルダ名に空白がある場合は""で囲む必要がある
                                outputfile2 = "\"" + output_folder + "\\" + outputfile_name + "-" + str_cnt8 + ".MP4\"";
                                filename_add = "\"" + file_MP4[num] + "\"";

                                // ffpmegにて切出しを実施
                                var arguments = string.Format("-ss {0} -i {1} -t {2} {3}", StartSeconds, filename_add, IntervalSeconds, outputfile2);
                                //MessageBox.Show(arguments);

                                if (IntervalSeconds != "0")
                                {
                                    System.Diagnostics.Process pro = new System.Diagnostics.Process();
                                    pro.StartInfo.FileName = "ffmpeg.exe";
                                    pro.StartInfo.Arguments = arguments;
                                    pro.StartInfo.CreateNoWindow = true;
                                    pro.StartInfo.UseShellExecute = false;
                                    //System.Diagnostics.Trace.WriteLine(arguments);
                                    pro.Start();
                                    pro.WaitForExit();
                                }
                            }

                        }
                    }
                    ProgressBar1.Value = cnt8;
                    System.Diagnostics.Trace.WriteLine("ProgressBar1.Value = " + ProgressBar1.Value);
                    System.Diagnostics.Trace.WriteLine("cnt8 = " + cnt8);

                    float value = ((float)cnt8 * 100) / (cnt16 * (cnt4 - cnt9));
                    System.Diagnostics.Trace.WriteLine("cnt4 = " + cnt4);
                    System.Diagnostics.Trace.WriteLine("cnt9 = " + cnt9);

                    if (cnt9 != 7)
                    {
                        textBox4.Text = value.ToString() + "%";
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("切出し実施可能なファイルはありませんでした。");
                    }

                    System.Windows.Forms.Application.DoEvents();

                }
                System.Diagnostics.Trace.WriteLine("cnt9 = " + cnt9);

                if (cnt8 != 0)
                {
                    System.Windows.MessageBox.Show("動画切出し完了");
                }
                cnt9 = 0;
                cnt8 = 0;
                cnt6 = 0;
                cnt7 = 0;
            }
        }
        private string GetStartSeconds(string str_start_seconds, string str_stop_seconds, string line5)
        {
            int line5Len = line5.Length; //抽出した行の文字列の長さ
            int str_star_seconds_Len = str_start_seconds.Length; //|start|の長さ

            int str_star_seconds_Num = line5.IndexOf(str_start_seconds); //|start|がline5のどの位置にあるか

            string s = ""; //返却する文字列(開始時間)

            try
            {
                s = line5.Remove(0, str_star_seconds_Num + str_star_seconds_Len); //line5の最初から|start|の文字列までを削除
                int str_stop_seconds_Num = s.IndexOf(str_stop_seconds); //|stop|の文字列がどの位置にあるか
                s = s.Remove(str_stop_seconds_Num); //sの|stop|のある位置から最後までを削除
            }
            catch(Exception)
            {
                return line5; //エラーの場合、原文まま返す
            }

            return s; //戻り値(開始時の秒数)
            

        }
        private string GetIntervalSeconds(string str_interval_seconds, string line5)
        {
            int line5Len = line5.Length; //抽出した行の文字列の長さ
            int str_interval_seconds_Len = str_interval_seconds.Length; //|interval|の長さ

            int str_interval_seconds_Num = line5.IndexOf(str_interval_seconds); //|interval|がline5のどの位置にあるか

            string s = ""; //返却する文字列(開始時間)

            try
            {
                s = line5.Remove(0, str_interval_seconds_Num + str_interval_seconds_Len); //line5の最初から|interval|の文字列までを削除
             }
            catch (Exception)
            {
                return line5; //エラーの場合、原文まま返す
            }

            return s; //戻り値(開始時の秒数)


        }
        private void HS_REC_START_Win7(int n)
        {
            // hWndc9のハンドルを取り出すメソッドを呼び出し
            IntPtr hWndc9 = Multi_Video_Handle9_Win7();

            IntPtr hWndc10 = FindWindowEx(hWndc9, IntPtr.Zero, "WindowsForms10.BUTTON.app.0.fb11c8_r17_ad1", "フレームレート");
            //System.Diagnostics.Trace.WriteLine("フレームレート：" + hWndc10);

            IntPtr hWndc11 = FindWindowEx(hWndc9, IntPtr.Zero, "WindowsForms10.COMBOBOX.app.0.fb11c8_r17_ad1", "");
            //System.Diagnostics.Trace.WriteLine("120FPS：" + hWndc11);

            if (n == 2)
            {
                SendMessage(hWndc11, CB_SELSTRING, -1, "120 FPS (640x360)"); //ComboBox 120FPS選択
            }
            else if (n == 4)
            {
                SendMessage(hWndc11, CB_SELSTRING, -1, "240 FPS (640x360)"); //ComboBox 240FPS選択
            }
            else if (n == 5)
            {
                SendMessage(hWndc11, CB_SELSTRING, -1, "300 FPS (640x360)"); //ComboBox 300FPS選択
            }
            else if (n == 10)
            {
                SendMessage(hWndc11, CB_SELSTRING, -1, "600 FPS (320x176)"); //ComboBox 600FPS選択
            }


            PostMessage(hWndc10, BM_CLICK, 0, 0); //フレームレートボタン押下

            System.Threading.Thread.Sleep(10000);

            // hWndc6のハンドルを取り出すメソッドを呼び出し
            IntPtr hWndc6 = Multi_Video_Handle6_Win7();

            IntPtr hWndc7 = FindWindowEx(hWndc6, IntPtr.Zero, "WindowsForms10.BUTTON.app.0.fb11c8_r17_ad1", "REC START");
            System.Diagnostics.Trace.WriteLine("REC START_" + hWndc7);

            PostMessage(hWndc7, BM_CLICK, 0, 0);  //REC STARTボタン押下

        }
        private void HS_REC_START_Win10(int n)
        {
            // hWndc9のハンドルを取り出すメソッドを呼び出し
            IntPtr hWndc9 = Multi_Video_Handle9_Win10();

            IntPtr hWndc10 = FindWindowEx(hWndc9, IntPtr.Zero, "WindowsForms10.BUTTON.app.0.fb11c8_r9_ad1", "フレームレート");
            System.Diagnostics.Trace.WriteLine("フレームレート：" + hWndc10);

            IntPtr hWndc11 = FindWindowEx(hWndc9, IntPtr.Zero, "WindowsForms10.COMBOBOX.app.0.fb11c8_r9_ad1", "");
            System.Diagnostics.Trace.WriteLine("120FPS：" + hWndc11);

            if (n == 2)
            {
                SendMessage(hWndc11, CB_SELSTRING, -1, "120 FPS (640x360)"); //ComboBox 120FPS選択
            }
            else if (n == 4)
            {
                SendMessage(hWndc11, CB_SELSTRING, -1, "240 FPS (640x360)"); //ComboBox 240FPS選択
            }
            else if (n == 5)
            {
                SendMessage(hWndc11, CB_SELSTRING, -1, "300 FPS (640x360)"); //ComboBox 300FPS選択
            }
            else if (n == 10)
            {
                SendMessage(hWndc11, CB_SELSTRING, -1, "600 FPS (320x176)"); //ComboBox 600FPS選択
            }


            PostMessage(hWndc10, BM_CLICK, 0, 0); //フレームレートボタン押下
            

            System.Threading.Thread.Sleep(10000);

            // hWndc6のハンドルを取り出すメソッドを呼び出し
            IntPtr hWndc6 = Multi_Video_Handle6_Win10();

            IntPtr hWndc7 = FindWindowEx(hWndc6, IntPtr.Zero, "WindowsForms10.BUTTON.app.0.fb11c8_r9_ad1", "REC START");
            System.Diagnostics.Trace.WriteLine("REC START_" + hWndc7);

            PostMessage(hWndc7, BM_CLICK, 0, 0);  //REC STARTボタン押下
        }
        private void HS_REC_STOP_Win7()
        {
            IntPtr hWndc6 = Multi_Video_Handle6_Win7();

            // REC STOPのハンドル取り出し
            IntPtr hWndc8 = FindWindowEx(hWndc6, IntPtr.Zero, "WindowsForms10.BUTTON.app.0.fb11c8_r17_ad1", "REC STOP");
            System.Diagnostics.Trace.WriteLine("REC STOP_" + hWndc8);

            // REC STOPボタンを押下
            PostMessage(hWndc8, BM_CLICK, 0, 0);
        }
        private void HS_REC_STOP_Win10()
        {
            IntPtr hWndc6 = Multi_Video_Handle6_Win10();

            // REC STOPのハンドル取り出し
            IntPtr hWndc8 = FindWindowEx(hWndc6, IntPtr.Zero, "WindowsForms10.BUTTON.app.0.fb11c8_r9_ad1", "REC STOP");
            System.Diagnostics.Trace.WriteLine("REC STOP_" + hWndc8);

            // REC STOPボタンを押下
            PostMessage(hWndc8, BM_CLICK, 0, 0);
        }

        private void nwcam_kiridashi_time()
        {
            DateTime dt2 = DateTime.Now;

            filePath2 = @"C:\TagAdding\TagDate\";

            TimeSpan interval = dt2 - dt1;

            int seconds = interval.Seconds;
            int minutes = interval.Minutes;
            int hours = interval.Hours;

            TimeSpan ts1 = new TimeSpan(hours, minutes, seconds);

            if (cnt10 == 0)
            {
                // 動画切り出し開始ポイント処理
                filePath3 = @"C:\TagAdding\Kiridashi\";

                start_seconds = interval.Seconds + (interval.Minutes * 60) + (interval.Hours * 3600);

                sw3 = new StreamWriter(filePath3 + now + "-Kiridashi.txt", true, Encoding.UTF8);

                sw3.Write("Kiridashi-" + kiridashi_cnt1 + "|start|" + start_seconds);
                sw3.Close();

                cnt10 = 1;
                //Button3.Content = "タグ+切出し停止";
                //System.Drawing.Image img = System.Drawing.Image.FromFile("Resources / TagZuke_1.png");

            }
            else if (cnt10 == 1)
            {
                // 動画切り出し停止ポイント処理
                filePath3 = @"C:\TagAdding\Kiridashi\";
                sw3 = new StreamWriter(filePath3 + now + "-Kiridashi.txt", true, Encoding.UTF8);

                stop_seconds = interval.Seconds + (interval.Minutes * 60) + (interval.Hours * 3600);
                interval_seconds = stop_seconds - start_seconds;

                sw3.Write("|stop|" + stop_seconds + "|interval|" + interval_seconds);
                sw3.Write(Environment.NewLine);
                sw3.Close();

                kiridashi_cnt1++;

                cnt10 = 0;
               
            }
        }

        private void Form1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            System.Windows.Forms.Keys aa = e.KeyCode;

            switch (aa)
            {
                case System.Windows.Forms.Keys.ShiftKey:
                    Console.WriteLine("Shiftが押されました");
                    break;
                case System.Windows.Forms.Keys.Menu:
                    Console.WriteLine("Altが押されました");
                    break;
                case System.Windows.Forms.Keys.Enter:
                    Console.WriteLine("Enterが押されました");
                    break;
                case System.Windows.Forms.Keys.Space:
                    Console.WriteLine("Spaceが押されました");
                    break;
                default:
                    Console.WriteLine("その他が押されました");
                    break;
            }
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            
            
        }

        private void TextBox_TextChanged_3(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox5_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox4_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void ProgressBar1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void ComboBox10_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox10.SelectedItem.ToString() == "System.Windows.Controls.ListBoxItem: NW Cam(x1)" || ComboBox10.SelectedItem.ToString() == "System.Windows.Controls.ListBoxItem: Multi Cam(x2)" || ComboBox10.SelectedItem.ToString() == "System.Windows.Controls.ListBoxItem: Multi Cam(x4)" || ComboBox10.SelectedItem.ToString() == "System.Windows.Controls.ListBoxItem: Multi Cam(x5)" || ComboBox10.SelectedItem.ToString() == "System.Windows.Controls.ListBoxItem: Multi Cam(x10)")
            {
                Button3.Content = "タグ＋切出しポイント記録";
                Console.WriteLine(ComboBox10.SelectedItem.ToString());
            }
            else if (ComboBox10.SelectedItem.ToString() == "System.Windows.Controls.ListBoxItem: HS Cam(x2)" || ComboBox10.SelectedItem.ToString() == "System.Windows.Controls.ListBoxItem: HS Cam(x4)" || ComboBox10.SelectedItem.ToString() == "System.Windows.Controls.ListBoxItem: HS Cam(x5)" || ComboBox10.SelectedItem.ToString() == "System.Windows.Controls.ListBoxItem: HS Cam(x10)")
            {
                Button3.Content = "切出しポイント記録";
                Console.WriteLine(ComboBox10.SelectedItem.ToString());
            }

        }

        private void TextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged_4(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click_12(object sender, RoutedEventArgs e)
        {
            var dialog5 = new System.Windows.Forms.FolderBrowserDialog();

            dialog5.SelectedPath = @"C:\TagAdding\HS-Camera"; //フォルダ指定

            dialog5.Description= "HSカメラ動画ファイルの格納フォルダを選んでください"; //ダイアログタイトル指定

            dialog5.ShowNewFolderButton = true;

            if (dialog5.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Console.WriteLine(dialog5.SelectedPath);
                HS_folder_name = dialog5.SelectedPath;

                HS_FILES = System.IO.Directory.GetFiles(HS_folder_name, "*", System.IO.SearchOption.AllDirectories);
                Console.WriteLine(HS_FILES[0]);
                Console.WriteLine(HS_FILES.Length);

                dt4 = DateTime.Now;

                string result2 = dt4.ToString("yyyyMMddHHmmss");
                Console.WriteLine(result2);


                StreamWriter sw5_1 = new StreamWriter(@"C:\TagAdding\HS-Camera\" + result2 + "_CAM01.txt", true, Encoding.Default);
                StreamWriter sw5_2 = new StreamWriter(@"C:\TagAdding\HS-Camera\" + result2 + "_CAM02.txt", true, Encoding.Default);
                StreamWriter sw5_3 = new StreamWriter(@"C:\TagAdding\HS-Camera\" + result2 + "_CAM03.txt", true, Encoding.Default);
                StreamWriter sw5_4 = new StreamWriter(@"C:\TagAdding\HS-Camera\" + result2 + "_CAM04.txt", true, Encoding.Default);

                for (int i = 0; i < HS_FILES.Length; i++)
                {


                    if (0 <= HS_FILES[i].IndexOf("CAM01"))
                    {
                        sw5_1.Write("file '");
                        sw5_1.Write(HS_FILES[i]);
                        sw5_1.Write("'");
                        sw5_1.Write(Environment.NewLine);
                    }
                    else if (0 <= HS_FILES[i].IndexOf("CAM02"))
                    {
                        sw5_2.Write("file '");
                        sw5_2.Write(HS_FILES[i]);
                        sw5_2.Write("'");
                        sw5_2.Write(Environment.NewLine);
                    }
                    else if (0 <= HS_FILES[i].IndexOf("CAM03"))
                    {
                        sw5_3.Write("file '");
                        sw5_3.Write(HS_FILES[i]);
                        sw5_3.Write("'");
                        sw5_3.Write(Environment.NewLine);
                    }
                    else if (0 <= HS_FILES[i].IndexOf("CAM04"))
                    {
                        sw5_4.Write("file '");
                        sw5_4.Write(HS_FILES[i]);
                        sw5_4.Write("'");
                        sw5_4.Write(Environment.NewLine);
                    }
                }

                sw5_1.Close();
                sw5_2.Close();
                sw5_3.Close();
                sw5_4.Close();

                // CAM01用の動画ファイル統合処理
                var arguments_1 = string.Format("-f concat -safe 0 -i {0} -c copy {1}",@"C:\TagAdding\HS-Camera\" + result2 + "_CAM01.txt", @"C:\TagAdding\HS-Camera\" + result2 + "_CAM01.MP4");

                System.Diagnostics.Process pro2_1 = new System.Diagnostics.Process();
                pro2_1.StartInfo.FileName = "ffmpeg.exe";
                pro2_1.StartInfo.Arguments = arguments_1;
                pro2_1.StartInfo.CreateNoWindow = true;
                pro2_1.StartInfo.UseShellExecute = false;
                System.Diagnostics.Trace.WriteLine(arguments_1);
                pro2_1.Start();
                pro2_1.WaitForExit();

                // CAM02用の動画ファイル統合処理
                var arguments_2 = string.Format("-f concat -safe 0 -i {0} -c copy {1}", @"C:\TagAdding\HS-Camera\" + result2 + "_CAM02.txt", @"C:\TagAdding\HS-Camera\" + result2 + "_CAM02.MP4");

                System.Diagnostics.Process pro2_2 = new System.Diagnostics.Process();
                pro2_2.StartInfo.FileName = "ffmpeg.exe";
                pro2_2.StartInfo.Arguments = arguments_2;
                pro2_2.StartInfo.CreateNoWindow = true;
                pro2_2.StartInfo.UseShellExecute = false;
                System.Diagnostics.Trace.WriteLine(arguments_2);
                pro2_2.Start();
                pro2_2.WaitForExit();

                // CAM03用の動画ファイル統合処理
                var arguments_3 = string.Format("-f concat -safe 0 -i {0} -c copy {1}", @"C:\TagAdding\HS-Camera\" + result2 + "_CAM03.txt", @"C:\TagAdding\HS-Camera\" + result2 + "_CAM03.MP4");

                System.Diagnostics.Process pro2_3 = new System.Diagnostics.Process();
                pro2_3.StartInfo.FileName = "ffmpeg.exe";
                pro2_3.StartInfo.Arguments = arguments_3;
                pro2_3.StartInfo.CreateNoWindow = true;
                pro2_3.StartInfo.UseShellExecute = false;
                System.Diagnostics.Trace.WriteLine(arguments_3);
                pro2_3.Start();
                pro2_3.WaitForExit();

                // CAM04用の動画ファイル統合処理
                var arguments_4 = string.Format("-f concat -safe 0 -i {0} -c copy {1}", @"C:\TagAdding\HS-Camera\" + result2 + "_CAM04.txt", @"C:\TagAdding\HS-Camera\" + result2 + "_CAM04.MP4");

                System.Diagnostics.Process pro2_4 = new System.Diagnostics.Process();
                pro2_4.StartInfo.FileName = "ffmpeg.exe";
                pro2_4.StartInfo.Arguments = arguments_4;
                pro2_4.StartInfo.CreateNoWindow = true;
                pro2_4.StartInfo.UseShellExecute = false;
                System.Diagnostics.Trace.WriteLine(arguments_4);
                pro2_4.Start();
                pro2_4.WaitForExit();

            }
            else
            {
                Console.WriteLine("キャンセルされました。");
            }

        }

        // WPFでKeyPreviewプロパティをTrueにするための設定
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            int k = 0;

           if (e.Key == Key.Space)
            {
                Console.WriteLine("Ｋｅｙ読み込まれました" + k++);

                Button_Click_1(sender,e);


            }

        }

    }
}
