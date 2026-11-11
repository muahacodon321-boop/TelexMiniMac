using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TelexMini
{
    // The settings dialog never reads keystrokes. Paths and process names only.
    internal sealed class AdvancedSettings : Form
    {
        const string RunKey=@"Software\Microsoft\Windows\CurrentVersion\Run";
        readonly ComboBox method,shortcut,priority;
        readonly TextBox excluded;
        readonly CheckBox start;
        readonly Label hint;
        internal static bool Open(IWin32Window owner) {
            using(var form=new AdvancedSettings()) return form.ShowDialog(owner)==DialogResult.OK;
        }
        static bool AutoStart {
            get {
                try {using(var k=Registry.CurrentUser.OpenSubKey(RunKey)) {
                    return k!=null && k.GetValue("TelexMini")!=null;
                }} catch(Exception) {return false;}
            }
            set {
                using(var k=Registry.CurrentUser.CreateSubKey(RunKey)) {
                    if(value) k.SetValue("TelexMini", "\""+Application.ExecutablePath+"\"");
                    else k.DeleteValue("TelexMini",false);
                }
            }
        }
        AdvancedSettings() {
            Text="TelexMini — Cài đặt nâng cao";
            StartPosition=FormStartPosition.CenterParent;
            ClientSize=new Size(610,510);MinimumSize=new Size(560,490);
            BackColor=Color.FromArgb(17,28,44);ForeColor=Color.White;
            Font=new Font("Segoe UI",10);AutoScaleMode=AutoScaleMode.Dpi;
            var panel=new TableLayoutPanel {
                Dock=DockStyle.Fill,Padding=new Padding(20),ColumnCount=1,RowCount=9,AutoScroll=true
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            for(int i=0;i<9;i++) panel.RowStyles.Add(new RowStyle(SizeType.Absolute,i==6?70:i==8?62:41));
            Controls.Add(panel);
            panel.Controls.Add(new Label {Text="CÀI ĐẶT GÕ SONG NGỮ",Font=new Font("Segoe UI",17,FontStyle.Bold),
                Dock=DockStyle.Fill,ForeColor=Color.FromArgb(81,229,183)},0,0);
            panel.Controls.Add(new Label {Text="Kiểu gõ: Telex / VNI",Dock=DockStyle.Fill},0,1);
            method=new ComboBox {Dock=DockStyle.Fill,DropDownStyle=ComboBoxStyle.DropDownList};
            method.Items.AddRange(new object[]{"Telex","VNI"});
            method.SelectedIndex=UserPreferences.String("InputMethod","Telex")=="VNI"?1:0;
            var inputChoices=new FlowLayoutPanel {Dock=DockStyle.Fill,WrapContents=false,
                FlowDirection=FlowDirection.LeftToRight};
            method.Width=145;method.Dock=DockStyle.None;
            priority=new ComboBox {Width=205,DropDownStyle=ComboBoxStyle.DropDownList};
            priority.Items.AddRange(new object[]{"Cân bằng","Ưu tiên tiếng Việt","Ưu tiên tiếng Anh"});
            string language=UserPreferences.String("LanguagePriority","Balanced");
            priority.SelectedIndex=language=="Vietnamese"?1:language=="English"?2:0;
            inputChoices.Controls.Add(method);
            inputChoices.Controls.Add(new Label {Text="Ưu tiên",AutoSize=true,
                ForeColor=Color.White,Margin=new Padding(12,6,6,0)});
            inputChoices.Controls.Add(priority);
            panel.Controls.Add(inputChoices,0,2);
            panel.Controls.Add(new Label {Text="Phím tắt chuyển V/E",Dock=DockStyle.Fill},0,3);
            shortcut=new ComboBox {Dock=DockStyle.Fill,DropDownStyle=ComboBoxStyle.DropDownList};
            shortcut.Items.AddRange(new object[]{"Ctrl + Shift + Space","Ctrl + Alt + V","Alt + Shift + V"});
            shortcut.SelectedIndex=Math.Max(0,shortcut.Items.IndexOf(UserPreferences.String("Hotkey","Ctrl + Shift + Space")));
            panel.Controls.Add(shortcut,0,4);
            start=new CheckBox {Text="Tự khởi động cùng Windows",Checked=AutoStart,
                Dock=DockStyle.Fill,ForeColor=Color.White};
            panel.Controls.Add(start,0,5);
            var excludedPanel=new FlowLayoutPanel {Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false};
            excludedPanel.Controls.Add(new Label {Text="Ứng dụng bỏ qua (tên tiến trình, cách nhau bằng dấu phẩy)",
                AutoSize=true,ForeColor=Color.White});
            excluded=new TextBox {Width=520,Text=UserPreferences.String("ExcludedApps","")};
            excludedPanel.Controls.Add(excluded);
            panel.Controls.Add(excludedPanel,0,6);
            hint=new Label {Dock=DockStyle.Fill,ForeColor=Color.FromArgb(159,189,205),
                Text="Cấu hình và từ điển lưu trên máy, không ghi lịch sử gõ."};
            panel.Controls.Add(hint,0,7);
            var actions=new FlowLayoutPanel {Dock=DockStyle.Fill,FlowDirection=FlowDirection.RightToLeft};
            var save=new Button {Text="Lưu và áp dụng",Width=135,Height=34};
            var cancel=new Button {Text="Hủy",Width=70,Height=34};
            var export=new Button {Text="Xuất",Width=75,Height=34};
            var import=new Button {Text="Nhập",Width=75,Height=34};
            actions.Controls.Add(save);actions.Controls.Add(cancel);
            actions.Controls.Add(export);actions.Controls.Add(import);
            panel.Controls.Add(actions,0,8);
            cancel.Click+=delegate {Close();};
            save.Click+=delegate {
                try {
                    AutoStart=start.Checked;
                    UserPreferences.Set("InputMethod",method.SelectedItem.ToString());
                    UserPreferences.Set("LanguagePriority",
                        priority.SelectedIndex==1?"Vietnamese":
                        priority.SelectedIndex==2?"English":"Balanced");
                    UserPreferences.Set("Hotkey",shortcut.SelectedItem.ToString());
                    string names=string.Join(",",excluded.Text.Split(',')
                        .Select(s=>s.Trim()).Where(s=>s.Length>0 &&
                         s.All(c=>char.IsLetterOrDigit(c)||c=='.'||c=='_'||c=='-')).Distinct().ToArray());
                    UserPreferences.Set("ExcludedApps",names);
                    DialogResult=DialogResult.OK;Close();
                } catch(Exception ex) {MessageBox.Show(this,ex.Message,"TelexMini",MessageBoxButtons.OK,MessageBoxIcon.Error);}
            };
            export.Click+=delegate {Backup(false);};
            import.Click+=delegate {Backup(true);};
        }
        void Backup(bool restore) {
            using(var chooser=new FolderBrowserDialog()) {
                chooser.Description=restore?"Chọn thư mục chứa bản sao lưu":"Chọn thư mục để xuất cấu hình";
                if(chooser.ShowDialog(this)!=DialogResult.OK) return;
                try {
                    string dir=Path.Combine(Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData),"TelexMini");
                    Directory.CreateDirectory(dir);
                    if(Path.GetFullPath(dir).TrimEnd('\\').Equals(
                        Path.GetFullPath(chooser.SelectedPath).TrimEnd('\\'),
                        StringComparison.OrdinalIgnoreCase)) {
                        MessageBox.Show(this,"Hãy chọn thư mục khác với thư mục cấu hình gốc.",
                            "TelexMini",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                        return;
                    }
                    foreach(var name in new[]{"preferences.ini","EnglishWords.txt","VietnameseOverrides.txt"}) {
                        string from=Path.Combine(restore?chooser.SelectedPath:dir,name);
                        string to=Path.Combine(restore?dir:chooser.SelectedPath,name);
                        if(File.Exists(from)) File.Copy(from,to,true);
                    }
                    if(restore) {
                        UserPreferences.Reload();EnglishLexicon.Reload();
                        MessageBox.Show(this,"Đã nhập cấu hình và từ điển.",
                            "TelexMini",MessageBoxButtons.OK,MessageBoxIcon.Information);
                        DialogResult=DialogResult.OK;Close();
                    } else {
                        MessageBox.Show(this,"Đã xuất cấu hình.",
                            "TelexMini",MessageBoxButtons.OK,MessageBoxIcon.Information);
                    }
                } catch(Exception ex) {
                    MessageBox.Show(this,ex.Message,"TelexMini",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
            }
        }
    }
}
