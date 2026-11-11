using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TelexMini
{
    internal sealed class RoundedPanel : Panel
    {
        internal Color StartColor=Color.White, EndColor=Color.White, Outline=Color.Transparent;
        internal int CornerRadius=18;
        internal RoundedPanel() {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }
        internal static GraphicsPath Rounded(Rectangle bounds,int radius) {
            int r=Math.Min(radius*2,Math.Min(bounds.Width,bounds.Height));
            var path=new GraphicsPath();
            if(r<=0) { path.AddRectangle(bounds); return path; }
            path.AddArc(bounds.Left,bounds.Top,r,r,180,90);
            path.AddArc(bounds.Right-r,bounds.Top,r,r,270,90);
            path.AddArc(bounds.Right-r,bounds.Bottom-r,r,r,0,90);
            path.AddArc(bounds.Left,bounds.Bottom-r,r,r,90,90);
            path.CloseFigure();
            return path;
        }
        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            if(Width>0 && Height>0) {
                using(var path=Rounded(new Rectangle(0,0,Width,Height),CornerRadius)) {
                    Region=new Region(path);
                }
            }
        }
        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
            var b=new Rectangle(1,1,Math.Max(1,Width-3),Math.Max(1,Height-3));
            using(var path=Rounded(b,CornerRadius)) {
                if(StartColor==EndColor) using(var brush=new SolidBrush(StartColor)) e.Graphics.FillPath(brush,path);
                else using(var brush=new LinearGradientBrush(b,StartColor,EndColor,20f)) e.Graphics.FillPath(brush,path);
                if(Outline.A>0) using(var pen=new Pen(Outline,1f)) e.Graphics.DrawPath(pen,path);
            }
        }
    }

    // Pure local WinForms UI: no WebView, online assets, GPU dependencies or telemetry.
    internal sealed class ModernChrome
    {
        readonly Form form;
        readonly Button modeButton;
        readonly Label status,heroTag,modeCaption,modeReason, footer;
        readonly CheckBox autoEnglish,restoreWords,smartEnglish;
        readonly Button themeButton, hideButton, exitButton;
        readonly RoundedPanel hero, leftCard, rightCard, previewCard;
        readonly Panel sidebar;
        readonly TextBox inputPreview;
        bool dark;
        Color page,card,muted,ink,outline;

        internal ModernChrome(Form owner,Button toggle,Label statusLabel,CheckBox auto,CheckBox restore,CheckBox smart,
                              Button hide,Button exit,Action openEnglish,Action openVietnamese,Action reload)
        {
            form=owner; modeButton=toggle; status=statusLabel;
            hideButton=hide; exitButton=exit;
            autoEnglish=auto; restoreWords=restore; smartEnglish=smart;
            dark=UserPreferences.Bool("DarkTheme",true);
            form.SuspendLayout();
            form.Controls.Clear();
            // Keep the content separate from navigation at any DPI scaling.
            // The original Dock.Fill plus Dock.Left z-order let the sidebar
            // paint over the left edge of the content panel.
            form.ClientSize=new Size(990,680);
            form.MinimumSize=new Size(950,650);
            form.MaximumSize=new Size(1440,980);
            form.AutoScaleMode=AutoScaleMode.Dpi;
            form.FormBorderStyle=FormBorderStyle.Sizable;
            form.TopMost=false;
            form.AutoScroll=false;
            form.Text="TelexMini — Smart Bilingual Typing";
            form.Font=new Font("Segoe UI",9.5f);
            form.StartPosition=FormStartPosition.CenterScreen;

            var root=new TableLayoutPanel {
                Dock=DockStyle.Fill, Margin=Padding.Empty, Padding=Padding.Empty,
                ColumnCount=2, RowCount=1
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,216));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            root.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            form.Controls.Add(root);
            sidebar=new Panel { Dock=DockStyle.Fill, Margin=Padding.Empty, Padding=Padding.Empty };
            var main=new Panel {
                Dock=DockStyle.Fill, Margin=Padding.Empty, Padding=Padding.Empty,
                AutoScroll=true, AutoScrollMinSize=new Size(710,660)
            };
            root.Controls.Add(sidebar,0,0);
            root.Controls.Add(main,1,0);
            var logo=new RoundedPanel { Location=new Point(22,24),Size=new Size(42,42),CornerRadius=13,
                StartColor=Color.FromArgb(20,186,156),EndColor=Color.FromArgb(16,136,163) };
            sidebar.Controls.Add(logo);
            logo.Controls.Add(Label("T",new Rectangle(0,1,42,38),Color.White,21f,true,ContentAlignment.MiddleCenter));
            sidebar.Controls.Add(Label("telexmini",new Rectangle(72,23,137,28),Color.White,14f,true));
            sidebar.Controls.Add(Label("SMART INPUT",new Rectangle(73,51,130,19),Color.FromArgb(134,158,183),7.4f,true));
            var separator=new Panel { Location=new Point(22,91),Size=new Size(168,1),BackColor=Color.FromArgb(44,63,83) };
            sidebar.Controls.Add(separator);
            sidebar.Controls.Add(Label("WORKSPACE",new Rectangle(24,113,164,18),Color.FromArgb(124,149,171),8f,true));
            var navHome=SideButton("◈  Tổng quan",new Point(15,144)); sidebar.Controls.Add(navHome);
            navHome.Click+=delegate { main.AutoScrollPosition=new Point(0,0); inputPreview.Focus(); };
            var navEnglish=SideButton("＋  Từ điển Anh",new Point(15,199)); sidebar.Controls.Add(navEnglish);
            navEnglish.Click+=delegate { openEnglish(); };
            var navVietnamese=SideButton("≡  Ưu tiên Telex",new Point(15,248)); sidebar.Controls.Add(navVietnamese);
            navVietnamese.Click+=delegate { openVietnamese(); };
            var navReload=SideButton("↻  Nạp lại từ điển",new Point(15,297)); sidebar.Controls.Add(navReload);
            navReload.Click+=delegate { reload(); };
            sidebar.Controls.Add(Label("PRIVATE BY DESIGN",new Rectangle(23,473,174,19),Color.FromArgb(123,151,177),8f,true));
            sidebar.Controls.Add(Label("Không lưu nội dung gõ\nKhông gửi dữ liệu lên mạng",new Rectangle(23,500,174,47),
                Color.FromArgb(165,184,199),8.6f,false));
            sidebar.Controls.Add(Label("DESKTOP · WINDOWS",new Rectangle(23,565,172,19),
                Color.FromArgb(108,134,161),7.8f,true));

            var eyebrow=Label("OVERVIEW / BẢNG ĐIỀU KHIỂN",new Rectangle(30,19,360,20),Color.Empty,8f,true);
            main.Controls.Add(eyebrow);
            main.Controls.Add(Label("Gõ theo cách của bạn.",new Rectangle(30,42,500,38),Color.Empty,22.5f,true));
            main.Controls.Add(Label("Hai ngôn ngữ. Một trải nghiệm liền mạch.",new Rectangle(31,83,470,22),Color.Empty,10f,false));
            themeButton=FlatButton("☾  Giao diện",new Rectangle(512,36,150,37));
            main.Controls.Add(themeButton);
            themeButton.Click+=delegate { dark=!dark; UserPreferences.Set("DarkTheme",dark); ApplyTheme(); };

            hero=new RoundedPanel { Location=new Point(30,121),Size=new Size(632,160),CornerRadius=20 };
            main.Controls.Add(hero);
            heroTag=Label("CHẾ ĐỘ HIỆN TẠI",new Rectangle(22,20,250,20),Color.FromArgb(181,235,226),8.5f,true);
            hero.Controls.Add(heroTag);
            hero.Controls.Add(Label("Chuyển chế độ chỉ với một cú nhấn",new Rectangle(22,45,390,24),Color.White,12f,true));
            modeButton.Location=new Point(20,84); modeButton.Size=new Size(281,54);
            modeButton.FlatStyle=FlatStyle.Flat; modeButton.FlatAppearance.BorderSize=0;
            modeButton.Font=new Font("Segoe UI Semibold",13f,FontStyle.Bold);
            modeButton.ForeColor=Color.FromArgb(9,60,66);
            modeButton.Cursor=Cursors.Hand;
            hero.Controls.Add(modeButton);
            modeCaption=Label("LOCAL · PRIVATE · FAST",new Rectangle(331,92,271,19),Color.FromArgb(201,242,237),8f,true);
            hero.Controls.Add(modeCaption);
            modeReason=Label("Không thu thập nội dung bạn gõ",new Rectangle(331,113,265,20),Color.White,9f,false);
            hero.Controls.Add(modeReason);

            var secTitle=Label("Tùy chỉnh trải nghiệm",new Rectangle(31,296,410,25),Color.Empty,13.5f,true);
            main.Controls.Add(secTitle);
            leftCard=new RoundedPanel { Location=new Point(30,329),Size=new Size(305,156),CornerRadius=18 };
            main.Controls.Add(leftCard);
            leftCard.Controls.Add(Label("NGÔN NGỮ",new Rectangle(17,14,265,21),Color.Empty,8f,true));
            smartEnglish.Location=new Point(14,43); smartEnglish.Size=new Size(279,29);
            restoreWords.Location=new Point(14,83); restoreWords.Size=new Size(279,29);
            leftCard.Controls.Add(smartEnglish); leftCard.Controls.Add(restoreWords);
            leftCard.Controls.Add(Label("Nhận diện offline, đặt dấu ngay khi gõ",new Rectangle(19,126,280,19),Color.Empty,8.1f,false));

            rightCard=new RoundedPanel { Location=new Point(347,329),Size=new Size(315,156),CornerRadius=18 };
            main.Controls.Add(rightCard);
            rightCard.Controls.Add(Label("TƯƠNG THÍCH & AN TOÀN",new Rectangle(17,14,275,21),Color.Empty,8f,true));
            autoEnglish.Location=new Point(14,43); autoEnglish.Size=new Size(285,29);
            rightCard.Controls.Add(autoEnglish);
            rightCard.Controls.Add(Label("Mật khẩu: tự dùng tiếng Anh",new Rectangle(20,85,275,22),Color.FromArgb(49,188,153),9.6f,true));
            rightCard.Controls.Add(Label("Không xử lý nội dung ô mật khẩu",new Rectangle(20,118,275,22),Color.Empty,8.1f,false));

            previewCard=new RoundedPanel { Location=new Point(30,498),Size=new Size(632,67),CornerRadius=15 };
            main.Controls.Add(previewCard);
            previewCard.Controls.Add(Label("THỬ GÕ NGAY",new Rectangle(17,6,155,18),Color.Empty,8f,true));
            inputPreview=new TextBox { Location=new Point(17,27),Size=new Size(595,27),BorderStyle=BorderStyle.FixedSingle,
                                      Font=new Font("Segoe UI",11.5f),Text="",
                                      ShortcutsEnabled=true };
            previewCard.Controls.Add(inputPreview);

            hide.Size=new Size(120,30);hide.Location=new Point(30,572);hide.Text="Thu xuống khay";
            hide.Font=new Font("Segoe UI",8.5f); hide.FlatStyle=FlatStyle.Flat; hide.FlatAppearance.BorderSize=0;
            exit.Size=new Size(64,30); exit.Location=new Point(159,572);exit.Text="Thoát";
            exit.Font=new Font("Segoe UI",8.5f); exit.FlatStyle=FlatStyle.Flat; exit.FlatAppearance.BorderSize=0;
            main.Controls.Add(hide);main.Controls.Add(exit);
            footer=Label(EnglishLexicon.WordCount.ToString("N0")+" từ nền · Hoạt động offline",new Rectangle(386,577,274,20),Color.Empty,8f,false,
                ContentAlignment.MiddleRight);
            main.Controls.Add(footer);

            // The main content always begins AFTER the sidebar. An internal
            // scrollbar takes over instead of allowing overlap at narrow widths.
            form.Resize+=delegate {
                int available=Math.Max(710,main.ClientSize.Width);
                themeButton.Left=Math.Max(512,available-180);
            };
            form.ResumeLayout(true);
            ApplyTheme();
        }
        static Label Label(string text,Rectangle r,Color color,float size,bool strong,ContentAlignment align=ContentAlignment.MiddleLeft)
        {
            return new Label { Text=text,Location=r.Location,Size=r.Size,ForeColor=color,
                BackColor=Color.Transparent,TextAlign=align,
                Font=new Font("Segoe UI",size,strong ? FontStyle.Bold : FontStyle.Regular),AutoEllipsis=true };
        }
        static Button FlatButton(string text,Rectangle r)
        {
            var b=new Button { Text=text,Location=r.Location,Size=r.Size,
                FlatStyle=FlatStyle.Flat,Font=new Font("Segoe UI Semibold",9.5f,FontStyle.Bold),
                Cursor=Cursors.Hand,TabStop=false };
            b.FlatAppearance.BorderSize=0;
            return b;
        }
        static Button SideButton(string text,Point location)
        {
            var b=FlatButton(text,new Rectangle(location,new Size(183,40)));
            b.TextAlign=ContentAlignment.MiddleLeft;
            b.Padding=new Padding(12,0,0,0);
            return b;
        }
        void ApplyTheme()
        {
            page=dark ? Color.FromArgb(13,22,36) : Color.FromArgb(247,249,253);
            card=dark ? Color.FromArgb(24,36,55) : Color.White;
            outline=dark ? Color.FromArgb(45,64,84) : Color.FromArgb(228,234,242);
            ink=dark ? Color.FromArgb(236,244,248) : Color.FromArgb(28,43,64);
            muted=dark ? Color.FromArgb(147,166,184) : Color.FromArgb(112,127,148);
            form.BackColor=page;
            sidebar.BackColor=Color.FromArgb(14,26,43);
            foreach(Control child in form.Controls)
                if(child!=sidebar) ApplyToContainer(child);
            leftCard.StartColor=rightCard.StartColor=previewCard.StartColor=card;
            leftCard.EndColor=rightCard.EndColor=previewCard.EndColor=card;
            // A transparent child label paints the parent's BackColor, not
            // its custom gradient. Match both to avoid white rectangles.
            leftCard.BackColor=rightCard.BackColor=previewCard.BackColor=card;
            leftCard.Outline=rightCard.Outline=previewCard.Outline=outline;
            hero.StartColor=Color.FromArgb(12,119,112);
            hero.EndColor=Color.FromArgb(24,72,115);
            hero.BackColor=hero.StartColor;
            hero.Outline=Color.FromArgb(34,137,140);
            themeButton.Text=dark ? "☀  Giao diện sáng" : "☾  Giao diện tối";
            themeButton.BackColor=card;themeButton.ForeColor=ink;
            foreach(var button in new[]{hideButton,exitButton}) {
                button.ForeColor=ink;button.BackColor=card;
                button.FlatAppearance.MouseOverBackColor=outline;
            }
            for(int i=0;i<sidebar.Controls.Count;i++) {
                var b=sidebar.Controls[i] as Button;
                if(b!=null) { b.BackColor=i==5 ? Color.FromArgb(26,92,98) : Color.FromArgb(14,26,43);
                               b.ForeColor=Color.FromArgb(216,230,239); }
            }
            foreach(var box in new[]{autoEnglish,restoreWords,smartEnglish}) {
                box.ForeColor=ink;box.BackColor=card;
                box.Font=new Font("Segoe UI",9.3f);
            }
            inputPreview.BackColor=dark ? Color.FromArgb(36,52,72) : Color.FromArgb(239,245,252);
            inputPreview.ForeColor=ink;
            modeButton.BackColor=Color.FromArgb(230,255,248);
            modeButton.ForeColor=Color.FromArgb(9,79,75);
            heroTag.ForeColor=Color.FromArgb(195,246,232);
            modeCaption.ForeColor=Color.FromArgb(195,246,232);
            modeReason.ForeColor=Color.White;
            footer.ForeColor=muted;
            form.Invalidate(true);
        }
        void ApplyToContainer(Control root)
        {
            if(root is Panel && !(root is RoundedPanel)) root.BackColor=page;
            foreach(Control child in root.Controls) {
                var label=child as Label;
                if(label!=null && label.ForeColor!=Color.White &&
                   label.ForeColor!=Color.FromArgb(181,235,226) &&
                   label.ForeColor!=Color.FromArgb(201,242,237) &&
                   label.ForeColor!=Color.FromArgb(49,188,153) &&
                   label.ForeColor!=Color.FromArgb(195,246,232)) {
                    label.ForeColor=label.Font.Size<9f ? muted : ink;
                }
                if(child is Panel) ApplyToContainer(child);
            }
        }
        internal void UpdateMode(bool active,bool automatic,bool paused,InputContextKind kind)
        {
            if(paused) {
                modeReason.Text="Đang nhường phím cho game / remote";
                modeCaption.Text="INPUT HOOKS OFF";
            } else if(kind==InputContextKind.Password && automatic) {
                modeReason.Text="Mật khẩu được giữ nguyên";
                modeCaption.Text="PASSWORD PROTECTION";
            } else if(automatic) {
                modeReason.Text="Tạm dừng xử lý khi nhận diện ô nhập";
                modeCaption.Text="SAFE MODE";
            } else if(active) {
                modeReason.Text="Gõ tiếng Việt và tiếng Anh tự nhiên";
                modeCaption.Text="LOCAL · PRIVATE · FAST";
            } else {
                modeReason.Text="Tất cả phím được gửi nguyên bản";
                modeCaption.Text="ENGLISH MODE";
            }
        }
    }
}
