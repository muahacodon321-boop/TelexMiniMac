using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TelexMini
{
    // First-party local dictionary editor: no Notepad, browser or external UI.
    // Files contain only words explicitly saved by the user, never typing history.
    internal sealed class DictionaryEditor : Form
    {
        readonly string path;
        readonly bool vietnamese;
        readonly ListBox wordList;
        readonly TextBox searchBox, wordBox;
        readonly Label count, message;
        readonly HashSet<string> words = new HashSet<string>(StringComparer.Ordinal);
        bool changed;

        internal static Form CreateForTests(bool vietnamese, string testPath) {
            return new DictionaryEditor(vietnamese,testPath);
        }

        internal static bool Edit(IWin32Window owner, bool vietnamese)
        {
            using(var dialog=new DictionaryEditor(vietnamese))
                return dialog.ShowDialog(owner)==DialogResult.OK;
        }

        DictionaryEditor(bool isVietnamese) : this(isVietnamese,null) { }

        DictionaryEditor(bool isVietnamese,string testPath)
        {
            vietnamese=isVietnamese;
            path=testPath ?? (vietnamese ? EnglishLexicon.VietnamesePath : EnglishLexicon.EnglishPath);
            Text=vietnamese ? "TelexMini · Ưu tiên tiếng Việt" : "TelexMini · Từ điển tiếng Anh";
            StartPosition=FormStartPosition.CenterParent;
            ClientSize=new Size(600,570);
            MinimumSize=new Size(560,500);
            FormBorderStyle=FormBorderStyle.Sizable;
            MaximizeBox=true;
            ShowInTaskbar=false;
            BackColor=Color.FromArgb(15,25,42);
            ForeColor=Color.FromArgb(233,245,250);
            Font=new Font("Segoe UI",10);
            AutoScaleMode=AutoScaleMode.Dpi;
            KeyPreview=true;

            var outer=new TableLayoutPanel {
                Dock=DockStyle.Fill,Padding=new Padding(22),
                BackColor=BackColor,ColumnCount=1,RowCount=5
            };
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            outer.RowStyles.Add(new RowStyle(SizeType.Absolute,85));
            outer.RowStyles.Add(new RowStyle(SizeType.Absolute,56));
            outer.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            outer.RowStyles.Add(new RowStyle(SizeType.Absolute,57));
            outer.RowStyles.Add(new RowStyle(SizeType.Absolute,61));
            Controls.Add(outer);

            var header=new Panel { Dock=DockStyle.Fill,BackColor=BackColor };
            var title=new Label {
                Text=isVietnamese ? "Ưu tiên Telex" : "Từ điển tiếng Anh",
                Dock=DockStyle.Top,Height=36,Font=new Font("Segoe UI",19,FontStyle.Bold),
                ForeColor=ForeColor,AutoEllipsis=true
            };
            var subtitle=new Label {
                Text=isVietnamese ?
                    "Các chuỗi phím cần ưu tiên xử lý như tiếng Việt." :
                    "Thêm, sửa, xóa từ. Bộ gõ sẽ nhận diện ngay sau khi lưu.",
                Dock=DockStyle.Bottom,Height=30,
                ForeColor=Color.FromArgb(154,174,191),AutoEllipsis=true
            };
            header.Controls.Add(title);header.Controls.Add(subtitle);
            outer.Controls.Add(header,0,0);

            var findRow=new TableLayoutPanel {
                Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Margin=Padding.Empty
            };
            findRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            findRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,145));
            searchBox=new TextBox {
                Dock=DockStyle.Top,Margin=new Padding(0,10,12,4),
                Font=new Font("Segoe UI",11),Height=31,
                BackColor=Color.FromArgb(29,42,60),ForeColor=Color.White,
                BorderStyle=BorderStyle.FixedSingle
            };
            count=new Label {
                Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleRight,
                ForeColor=Color.FromArgb(137,166,186),
                Font=new Font("Segoe UI",9),Margin=Padding.Empty
            };
            findRow.Controls.Add(searchBox,0,0);findRow.Controls.Add(count,1,0);
            outer.Controls.Add(findRow,0,1);

            wordList=new ListBox {
                Dock=DockStyle.Fill,
                IntegralHeight=false,HorizontalScrollbar=true,
                BackColor=Color.FromArgb(25,38,56),ForeColor=Color.White,
                Font=new Font("Consolas",11),
                BorderStyle=BorderStyle.None,Margin=new Padding(0,0,0,11)
            };
            outer.Controls.Add(wordList,0,2);

            var editor=new TableLayoutPanel {
                Dock=DockStyle.Fill,ColumnCount=3,RowCount=1,Margin=Padding.Empty
            };
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,100));
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,100));
            wordBox=new TextBox {
                Dock=DockStyle.Top,Margin=new Padding(0,8,12,0),
                Font=new Font("Segoe UI",11),Height=31,
                BackColor=Color.FromArgb(29,42,60),ForeColor=Color.White,
                BorderStyle=BorderStyle.FixedSingle
            };
            var add=ActionButton("Thêm / sửa",Color.FromArgb(18,147,133));
            var remove=ActionButton("Xóa từ",Color.FromArgb(45,63,82));
            editor.Controls.Add(wordBox,0,0);
            editor.Controls.Add(add,1,0);
            editor.Controls.Add(remove,2,0);
            outer.Controls.Add(editor,0,3);

            var footer=new TableLayoutPanel {
                Dock=DockStyle.Fill,ColumnCount=3,RowCount=1,Margin=Padding.Empty
            };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,110));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,110));
            message=new Label {
                Text="Chỉ hỗ trợ một từ a-z trên mỗi dòng.",
                Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,
                ForeColor=Color.FromArgb(144,169,187),
                Font=new Font("Segoe UI",8.6f),AutoEllipsis=true
            };
            var cancel=ActionButton("Hủy",Color.FromArgb(45,63,82));
            var save=ActionButton("Lưu",Color.FromArgb(18,147,133));
            footer.Controls.Add(message,0,0);footer.Controls.Add(cancel,1,0);footer.Controls.Add(save,2,0);
            outer.Controls.Add(footer,0,4);

            LoadWords();
            searchBox.TextChanged+=delegate {RefreshList();};
            wordList.SelectedIndexChanged+=delegate {
                if(wordList.SelectedItem!=null) wordBox.Text=wordList.SelectedItem.ToString();
            };
            add.Click+=delegate {AddWord();};
            remove.Click+=delegate {RemoveWord();};
            save.Click+=delegate {SaveWords();};
            cancel.Click+=delegate {Close();};
            wordBox.KeyDown+=delegate(object sender,KeyEventArgs e) {
                if(e.KeyCode==Keys.Enter) {e.SuppressKeyPress=true;AddWord();}
            };
            FormClosing+=delegate(object sender,FormClosingEventArgs e) {
                if(!changed || DialogResult==DialogResult.OK) return;
                var answer=MessageBox.Show(this,"Bạn có thay đổi chưa lưu. Đóng mà không lưu?",
                    "TelexMini",MessageBoxButtons.YesNo,MessageBoxIcon.Question);
                if(answer==DialogResult.No) e.Cancel=true;
            };
        }

        static Button ActionButton(string text,Color back)
        {
            var button=new Button {
                Dock=DockStyle.Fill,Text=text,Margin=new Padding(5,6,0,5),
                BackColor=back,ForeColor=Color.White,
                FlatStyle=FlatStyle.Flat,Cursor=Cursors.Hand,
                Font=new Font("Segoe UI",9.2f,FontStyle.Bold)
            };
            button.FlatAppearance.BorderSize=0;
            return button;
        }

        static bool IsWord(string word)
        {
            if(word.Length==0 || word.Length>64) return false;
            foreach(char c in word) if(c<'a' || c>'z') return false;
            return true;
        }

        void LoadWords()
        {
            try {
                // Portable legacy file may exist even when user dictionary has moved
                // to LocalAppData. Preserve these entries on the next save.
                string legacy=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    vietnamese ? "VietnameseOverrides.txt" : "EnglishWords.txt");
                // Once AppData exists it is authoritative; reading the
                // legacy file again would resurrect words deleted in the UI.
                foreach(string file in new[] {File.Exists(path) ? path : legacy}) {
                    if(!File.Exists(file)) continue;
                    foreach(string line in File.ReadAllLines(file,Encoding.UTF8)) {
                        string word=line.Trim().ToLowerInvariant();
                        if(IsWord(word)) words.Add(word);
                    }
                }
                RefreshList();
            }
            catch(Exception ex) {
                message.Text="Không đọc được từ điển: "+ex.Message;
            }
        }

        void RefreshList()
        {
            string filter=searchBox.Text.Trim().ToLowerInvariant();
            string selected=wordList.SelectedItem as string;
            wordList.BeginUpdate();
            wordList.Items.Clear();
            foreach(string word in words.OrderBy(s=>s,StringComparer.Ordinal)) {
                if(word.Contains(filter)) wordList.Items.Add(word);
            }
            if(selected!=null && wordList.Items.Contains(selected))
                wordList.SelectedItem=selected;
            wordList.EndUpdate();
            count.Text=wordList.Items.Count+" / "+words.Count+" từ";
        }

        void AddWord()
        {
            string word=wordBox.Text.Trim().ToLowerInvariant();
            if(!IsWord(word)) {
                message.Text="Từ phải dài 1–64 ký tự, chỉ gồm a-z.";
                return;
            }
            string selected=wordList.SelectedItem as string;
            if(selected!=null && selected!=word && words.Contains(selected)) words.Remove(selected);
            if(words.Add(word) || (selected!=null && selected!=word)) changed=true;
            searchBox.Clear();
            RefreshList();
            wordList.SelectedItem=word;
            message.Text="Đã thay đổi. Bấm Lưu để áp dụng.";
        }

        void RemoveWord()
        {
            string selected=wordList.SelectedItem as string;
            if(selected==null) return;
            if(words.Remove(selected)) changed=true;
            wordBox.Clear();
            RefreshList();
            message.Text="Đã xóa khỏi danh sách. Bấm Lưu để áp dụng.";
        }

        void SaveWords()
        {
            try {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                string tmp=path+".tmp";
                File.WriteAllLines(tmp,words.OrderBy(s=>s,StringComparer.Ordinal).ToArray(),
                                   new UTF8Encoding(false));
                if(File.Exists(path)) File.Replace(tmp,path,null);
                else File.Move(tmp,path);
                DialogResult=DialogResult.OK;
                Close();
            }
            catch(Exception ex) {
                MessageBox.Show(this,"Không lưu được từ điển:\n"+ex.Message,
                    "TelexMini",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }
    }
}
