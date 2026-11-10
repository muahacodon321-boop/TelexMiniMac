using System;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TelexMini
{
    internal sealed class MainForm : Form
    {
        readonly Keyboard keyboard;
        readonly NotifyIcon tray;
        readonly Button toggle;
        readonly Label status;
        readonly ToolStripMenuItem switchItem;
        readonly InputContextMonitor context;
        readonly System.Windows.Forms.Timer modeTimer;
        readonly CheckBox autoEnglish, restoreWords, smartEnglish;
        readonly GameGuard gameGuard;
        readonly ToolStripMenuItem pauseItem;
        ModernChrome chrome;
        bool gameActive, manualPause;
        string displayedMode="";
        Icon vietIcon, englishIcon;
        bool registered, cleaned;
        internal MainForm()
        {
            Text="Telex Mini"; ClientSize=new Size(334,306); FormBorderStyle=FormBorderStyle.FixedSingle;
            MaximizeBox=false; ShowInTaskbar=true; StartPosition=FormStartPosition.CenterScreen; TopMost=true;
            BackColor=Color.FromArgb(246,248,251); Font=new Font("Segoe UI",10);
            vietIcon=MakeIcon("V",Color.FromArgb(18,122,86)); englishIcon=MakeIcon("E",Color.FromArgb(79,91,110));
            Icon=vietIcon;
            toggle=new Button { Location=new Point(18,18), Size=new Size(298,56), FlatStyle=FlatStyle.Flat,
                Font=new Font("Segoe UI",15,FontStyle.Bold), ForeColor=Color.White, Cursor=Cursors.Hand, TabStop=false };
            toggle.FlatAppearance.BorderSize=0;
            status=new Label { Location=new Point(18,83), Size=new Size(298,68), TextAlign=ContentAlignment.MiddleCenter };
            var hide=new Button { Text="Thu xuống khay", Location=new Point(18,262), Size=new Size(175,28), TabStop=false };
            var exit=new Button { Text="Thoát", Location=new Point(203,262), Size=new Size(113,28), TabStop=false };
            Controls.AddRange(new Control[] { toggle,status,hide,exit });
            autoEnglish=new CheckBox { Text="Tự dùng English trong terminal", Location=new Point(18,157), Size=new Size(298,25),
                Checked=UserPreferences.Bool("AutoEnglish",false), TabStop=false };
            restoreWords=new CheckBox { Text="Khôi phục từ không hợp lệ", Location=new Point(18,186), Size=new Size(298,25),
                Checked=UserPreferences.Bool("RestoreInvalid",true), TabStop=false };
            smartEnglish=new CheckBox { Text="Bảo vệ từ tiếng Anh (offline)", Location=new Point(18,215), Size=new Size(298,25),
                Checked=UserPreferences.Bool("SmartEnglish",true), TabStop=false };
            Controls.AddRange(new Control[] { autoEnglish,restoreWords,smartEnglish });
            context=new InputContextMonitor();
            keyboard=new Keyboard(context);
            keyboard.AutoEnglish=autoEnglish.Checked;
            keyboard.RestoreInvalid=restoreWords.Checked;
            keyboard.SmartEnglish=smartEnglish.Checked;
            autoEnglish.CheckedChanged+=delegate { keyboard.AutoEnglish=autoEnglish.Checked; UserPreferences.Set("AutoEnglish",autoEnglish.Checked); keyboard.Enabled=keyboard.Enabled; SyncPerformance(); UpdateMode(); };
            restoreWords.CheckedChanged+=delegate { keyboard.RestoreInvalid=restoreWords.Checked; UserPreferences.Set("RestoreInvalid",restoreWords.Checked); };
            smartEnglish.CheckedChanged+=delegate { keyboard.SmartEnglish=smartEnglish.Checked; UserPreferences.Set("SmartEnglish",smartEnglish.Checked); UpdateMode(); };
            var menu=new ContextMenuStrip();
            switchItem=new ToolStripMenuItem("",null,delegate { Toggle(); }); menu.Items.Add(switchItem);
            pauseItem=new ToolStripMenuItem("Tạm dừng hoàn toàn: game / remote") { CheckOnClick=true };
            pauseItem.CheckedChanged+=delegate { manualPause=pauseItem.Checked; SyncPerformance(); UpdateMode(); };
            menu.Items.Add(pauseItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Thêm từ tiếng Anh...",null,delegate { EditDictionary(EnglishLexicon.EnglishPath,"# Mỗi dòng một từ tiếng Anh cần giữ nguyên (không có dấu cách)."); });
            menu.Items.Add("Ưu tiên Telex cho từ...",null,delegate { EditDictionary(EnglishLexicon.VietnamesePath,"# Mỗi dòng một chuỗi phím Telex ưu tiên tiếng Việt. Ví dụ: as"); });
            menu.Items.Add("Nạp lại từ điển",null,delegate { EnglishLexicon.Reload(); keyboard.SmartEnglish=keyboard.SmartEnglish; UpdateMode(); });
            menu.Items.Add("Hiện nút chuyển",null,delegate { Show(); WindowState=FormWindowState.Normal; });
            menu.Items.Add(new ToolStripSeparator()); menu.Items.Add("Thoát",null,delegate { Close(); });
            tray=new NotifyIcon { Icon=vietIcon, Text="Telex Mini — Tiếng Việt", ContextMenuStrip=menu, Visible=true };
            tray.MouseClick+=delegate(object sender, MouseEventArgs e) { if(e.Button==MouseButtons.Left) Toggle(); };
            toggle.Click+=delegate { Toggle(); }; hide.Click+=delegate { Hide(); }; exit.Click+=delegate { Close(); };
            Resize+=delegate { if(WindowState==FormWindowState.Minimized) Hide(); };
            keyboard.InjectionFailed+=delegate { BeginInvoke((Action)delegate { tray.ShowBalloonTip(3500,"Không thay được ký tự","Ứng dụng đích có thể đang chạy quyền quản trị. Xem HUONG-DAN.md.",ToolTipIcon.Info); }); };
            registered=Native.RegisterHotKey(Handle,1,0x4006,0x20);
            modeTimer=new System.Windows.Forms.Timer { Interval=500 };
            gameGuard=new GameGuard(delegate(bool active) { gameActive=active; SyncPerformance(); UpdateMode(); });
            modeTimer.Tick+=delegate { gameGuard.Check(); UpdateMode(); }; modeTimer.Start();
            chrome=new ModernChrome(this,toggle,status,autoEnglish,restoreWords,smartEnglish,hide,exit,
                delegate { EditDictionary(EnglishLexicon.EnglishPath,"# Mỗi dòng một từ tiếng Anh"); },
                delegate { EditDictionary(EnglishLexicon.VietnamesePath,"# Mỗi dòng một chuỗi Telex"); },
                delegate { EnglishLexicon.Reload(); keyboard.SmartEnglish=keyboard.SmartEnglish; });
            SyncPerformance();
            UpdateMode();
        }
        static Icon MakeIcon(string letter, Color color)
        {
            using(var bitmap=new Bitmap(32,32))
            using(var graphics=Graphics.FromImage(bitmap))
            using(var font=new Font("Segoe UI",21,FontStyle.Bold,GraphicsUnit.Pixel))
            using(var brush=new SolidBrush(color))
            using(var format=new StringFormat { Alignment=StringAlignment.Center, LineAlignment=StringAlignment.Center })
            {
                graphics.Clear(Color.Transparent); graphics.FillEllipse(brush,0,0,31,31);
                graphics.DrawString(letter,font,Brushes.White,new RectangleF(0,0,32,31),format);
                IntPtr handle=bitmap.GetHicon(); var icon=(Icon)Icon.FromHandle(handle).Clone(); Native.DestroyIcon(handle); return icon;
            }
        }
        static void EditDictionary(string file, string instructions)
        {
            try {
                if(!File.Exists(file)) File.WriteAllText(file,instructions+Environment.NewLine,new UTF8Encoding(false));
                Process.Start("notepad.exe","\""+file+"\"");
            } catch(Exception ex) {
                MessageBox.Show("Không mở được từ điển: "+ex.Message,"Telex Mini");
            }
        }
        void SyncPerformance() {
            bool pause=gameActive || manualPause;
            keyboard.Suspended=pause;
            // Always classify password fields when V is selected.
            // Suspend metadata checks in manual E or when a game is foreground.
            context.Enabled=keyboard.Enabled && !pause;
        }
        void Toggle() { keyboard.Enabled=!keyboard.Enabled; SyncPerformance(); UpdateMode(); }
        void UpdateMode()
        {
            InputContextKind kind=keyboard.ContextKind;
            bool pause=gameActive || manualPause;
            bool v=!pause && ContextPolicy.UseVietnamese(keyboard.Enabled,keyboard.AutoEnglish,kind);
            bool automatic=!pause && keyboard.Enabled && !v &&
                (keyboard.AutoEnglish || kind==InputContextKind.Password ||
                 kind==InputContextKind.Checking || kind==InputContextKind.Unavailable);
            string signature=v+":"+automatic+":"+kind+":"+keyboard.Enabled+":"+keyboard.AutoEnglish+":"+pause;
            if(signature==displayedMode) return; displayedMode=signature;
            if(chrome!=null) chrome.UpdateMode(v,automatic,pause,kind);
            toggle.Text=pause ? "E  ·  NHƯỜNG PHÍM" : v ? "V  ·  TIẾNG VIỆT" :
                kind==InputContextKind.Password && keyboard.Enabled ? "E  ·  MẬT KHẨU" :
                automatic ? "E  ·  TỰ ĐỘNG" : "E  ·  ENGLISH";
            toggle.BackColor=v ? Color.FromArgb(18,122,86) : Color.FromArgb(79,91,110);
            tray.Icon=v ? vietIcon : englishIcon;
            Icon=tray.Icon;
            string reason=kind==InputContextKind.Password ? "Ô mật khẩu" : kind==InputContextKind.Terminal ? "Terminal" : kind==InputContextKind.Unavailable ? "Chưa nhận diện được ô nhập" : "Đang nhận diện ô nhập";
            tray.Text=pause ? "Telex Mini — Đã gỡ hook, tạm dừng bộ gõ" : v ? "Telex Mini — Tiếng Việt" : automatic ? "Telex Mini — English: "+reason : "Telex Mini — English (đã gỡ hook)";
            switchItem.Text=keyboard.Enabled ? "Dùng English cho mọi nơi" : "Dùng Tiếng Việt (trừ ô mật khẩu)";
            status.Text=pause ? "Đã dừng hook bàn phím / chuột\nĐã dừng nhận diện ô nhập\nMenu khay: bật / tắt tạm dừng" : automatic ? reason+" · tạm dùng Anh\nSẽ về V khi ô nhập phù hợp\nBấm E để dùng Anh mọi nơi" :
                registered ? "Ctrl + Shift + Space để chuyển\nMật khẩu luôn dùng English\nTelex + English thông minh" : "Phím tắt đang bị ứng dụng khác dùng.\nMật khẩu luôn dùng English\nBấm nút V/E để chuyển";
        }
        protected override void WndProc(ref Message m) { if(m.Msg==0x312 && m.WParam.ToInt32()==1) Toggle(); base.WndProc(ref m); }
        protected override void Dispose(bool disposing)
        {
            if(disposing && !cleaned) {
                cleaned=true;
                if(registered && IsHandleCreated) Native.UnregisterHotKey(Handle,1);
                registered=false;
                if(modeTimer!=null) modeTimer.Dispose();
                if(gameGuard!=null) gameGuard.Dispose();
                if(keyboard!=null) keyboard.Dispose();
                if(context!=null) context.Dispose();
                if(tray!=null) { tray.Visible=false; if(tray.ContextMenuStrip!=null) tray.ContextMenuStrip.Dispose(); tray.Dispose(); }
                if(vietIcon!=null) vietIcon.Dispose(); if(englishIcon!=null) englishIcon.Dispose();
            }
            base.Dispose(disposing);
        }
    }
    internal static class Program
    {
        [STAThread] static void Main()
        {
            bool first;
            using(var mutex=new Mutex(true,"Local\\TelexMini.Portable.v1",out first))
            {
                if(!first) { MessageBox.Show("Telex Mini đang chạy. Bấm biểu tượng V/E ở khay hệ thống.","Telex Mini"); return; }
                Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
                try { using(var form=new MainForm()) Application.Run(form); }
                catch(Exception error) { MessageBox.Show("Không khởi động được Telex Mini:\n"+error.Message,"Telex Mini",MessageBoxButtons.OK,MessageBoxIcon.Error); }
            }
        }
    }
}
