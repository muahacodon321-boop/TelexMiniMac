using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TelexMini
{
    internal static class Tests
    {
        [DllImport("user32.dll")] static extern IntPtr GetKeyboardLayout(uint thread);
        [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern IntPtr LoadKeyboardLayout(string name,uint flags);
        [DllImport("user32.dll")] static extern IntPtr ActivateKeyboardLayout(IntPtr layout,uint flags);
        static int failures;
        static readonly string[,] Cases = {
            {"tieengs","tiếng"}, {"Vieetj","Việt"}, {"chaof","chào"}, {"ddawng","đăng"},
            {"ddaas","đấ"}, {"truowngf","trường"}, {"nguowif","người"}, {"duwowngf","dường"},
            {"hoaf","hoà"}, {"khoer","khoẻ"}, {"thuys","thuý"}, {"quas","quá"},
            {"quys","quý"}, {"gias","giá"}, {"gif","gì"}, {"gins","gín"},
            {"muoons","muốn"}, {"tieesng","tiếng"}, {"tieesngz","tiêng"},
            {"ass","as"}, {"aaa","aa"}, {"eee","ee"}, {"ooo","oo"}, {"ddd","dd"},
            {"aww","aw"}, {"uww","uw"}, {"oww","ow"}, {"uoww","uow"},
            {"DDawNG","ĐăNG"}, {"TIEENGS","TIẾNG"}, {"aasf","ầ"}, {"asdf","àd"},
            {"toasn","toán"}, {"tuyeenr","tuyển"}, {"nguyeenx","nguyễn"},
            {"huowsng","hướng"}, {"ruouwj","rượu"}, {"khuyur","khuỷu"}, {"cuwus","cứu"},
            {"yeeus","yếu"}, {"quoocs","quốc"}, {"giaan","giân"}, {"duongdwf","đường"},
            {"ddoojng","động"}, {"caamr","cẩm"}, {"cawsn","cắn"}, {"tuwj","tự"},
            {"nhowx","nhỡ"}, {"vieecj","việc"}, {"nguowis","ngưới"},
            {"esscape","escape"}, {"esscapes","escapes"}, {"asssets","assets"},
            {"offfice","office"}, {"eeeror","eeror"}, {"dddriver","ddriver"},
            {"owwword","owword"}, {"uowwword","uowword"}, {"ESSCAPE","ESCAPE"}
        };
        static void Equal(string label,string expected,string actual)
        {
            bool pass=expected==actual;
            Console.WriteLine((pass ? "PASS " : "FAIL ")+label+" => "+actual+(pass ? "" : " (expected "+expected+")"));
            if(!pass) failures++;
        }
        [STAThread] static int Main(string[] args)
        {
            Console.OutputEncoding=System.Text.Encoding.UTF8;
            for(int i=0;i<Cases.GetLength(0);i++) {
                var engine=new TelexEngine(); foreach(char c in Cases[i,0]) engine.Push(c);
                Equal(Cases[i,0],Cases[i,1],engine.Text);
                Equal("Finish "+Cases[i,0],Cases[i,0]=="asdf" ? "asdf" : Cases[i,1],engine.Finish(true));
            }
            foreach(string word in new[] { "escape", "hello", "browser", "github", "script" }) {
                var english=new TelexEngine(); foreach(char c in word) english.Push(c);
                Equal("Restore English "+word,word,english.Finish(true));
                Equal("Restoration off "+word,english.Text,english.Finish(false));
            }
            var ambiguous=new TelexEngine(); foreach(char c in "as") ambiguous.Push(c);
            Equal("Ambiguous word retains Vietnamese","á",ambiguous.Finish(true));
            foreach(string word in new[] { "as","office","testing","password","framework","plugin","dashboard","release","browser","github","script","assets","developer","windows","hello","english" }) {
                var bilingual=new TelexEngine { SmartEnglish=true };
                foreach(char c in word) bilingual.Push(c);
                Equal("Smart English while typing "+word,word,bilingual.Text);
                Equal("Smart English boundary "+word,word,bilingual.Finish(true));
            }
            foreach(string word in new[] { "tieengs","vieetj","truowngf","nguowif","caamr","ddawng","toasn" }) {
                var bilingual=new TelexEngine { SmartEnglish=true };
                foreach(char c in word) bilingual.Push(c);
                var reference=new TelexEngine(); foreach(char c in word) reference.Push(c);
                Equal("Smart Vietnamese "+word,reference.Text,bilingual.Text);
                Equal("Smart Vietnamese boundary "+word,reference.Finish(true),bilingual.Finish(true));
            }
            Equal("Password always English", "False", ContextPolicy.UseVietnamese(true,false,InputContextKind.Password).ToString());
            Equal("Unknown field fail closed", "False", ContextPolicy.UseVietnamese(true,false,InputContextKind.Checking).ToString());
            Equal("Unavailable field fail closed", "False", ContextPolicy.UseVietnamese(true,false,InputContextKind.Unavailable).ToString());
            Console.WriteLine("Bundled English vocabulary: "+EnglishLexicon.WordCount);
            var longWord=new TelexEngine(); string longInput=new string('b',70)+"esscape";
            foreach(char c in longInput) longWord.Push(c);
            Equal("Long identifier retains prefix",longInput,longWord.Text);
            Equal("Terminal classifier","True",ContextPolicy.IsTerminal("WindowsTerminal.exe").ToString());
            Equal("PowerShell classifier","True",ContextPolicy.IsTerminal("pwsh").ToString());
            Equal("Browser remains text","False",ContextPolicy.IsTerminal("msedge").ToString());
            Equal("Steam remote executable","True",ContextPolicy.ShouldYieldKeys("streaming_client.exe").ToString());
            Equal("Steam remote case insensitive","True",ContextPolicy.ShouldYieldKeys("STREAMING_CLIENT").ToString());
            Equal("Steam library keeps Telex","False",ContextPolicy.ShouldYieldKeys("steam.exe").ToString());
            Equal("Steam chat keeps Telex","False",ContextPolicy.ShouldYieldKeys("steamwebhelper.exe").ToString());
            foreach(string name in new[] { "Fightcade2.exe", "fcadefbneo.exe", "fcadefbneo", "flycast.exe", "fbneo.exe" })
                Equal("Game bypass "+name,"True",ContextPolicy.IsGame(name).ToString());
            foreach(string name in new[] { "msedge", "notepad", "pwsh" })
                Equal("Non-game "+name,"False",ContextPolicy.IsGame(name).ToString());
            using(var hooks=new Keyboard()) {
                Equal("Hooks initially active","True",hooks.HooksInstalled.ToString());
                hooks.Enabled=false;
                Equal("Manual E removes both hooks","False",hooks.HooksInstalled.ToString());
                hooks.Enabled=true;
                Equal("Manual V restores hooks","True",hooks.HooksInstalled.ToString());
                hooks.Suspended=true;
                Equal("Game suspension removes both hooks","False",hooks.HooksInstalled.ToString());
                hooks.Enabled=false; hooks.Suspended=false;
                Equal("Leaving game preserves manual E","False",hooks.HooksInstalled.ToString());
                hooks.Enabled=true;
                Equal("Resume restores hooks","True",hooks.HooksInstalled.ToString());
            }
            using(var monitor=new InputContextMonitor()) {
                System.Threading.Thread.Sleep(1000);
                Equal("Disabled auto mode performs zero UIA queries","0",monitor.AccessibilityQueries.ToString());
            }
            foreach(InputContextKind context in Enum.GetValues(typeof(InputContextKind))) {
                Equal("Auto context "+context,(context==InputContextKind.Text).ToString(),ContextPolicy.UseVietnamese(true,true,context).ToString());
                Equal("Manual English stays English "+context,"False",ContextPolicy.UseVietnamese(false,true,context).ToString());
                Equal("Auto disabled "+context,(context==InputContextKind.Text || context==InputContextKind.Terminal).ToString(),ContextPolicy.UseVietnamese(true,false,context).ToString());
            }
            var edit=new TelexEngine(); foreach(char c in "tieengs") edit.Push(c);
            edit.Backspace(); Equal("Backspace removes visible character","tiến",edit.Text);
            edit.Push('g'); Equal("Continue after Backspace","tiếng",edit.Text);
            edit.Reset(); Equal("Reset","",edit.Text);
            foreach(char c in "ess") edit.Push(c);
            edit.Backspace();
            foreach(char c in "scape") edit.Push(c);
            Equal("English lock survives Backspace","escape",edit.Text);
            edit.Reset(); foreach(char c in "tieengs") edit.Push(c);
            Equal("Word boundary restores Telex","tiếng",edit.Text);
            edit.Reset(); foreach(char c in "ess") edit.Push(c);
            edit.Backspace(); edit.Backspace();
            foreach(char c in "as") edit.Push(c);
            Equal("Deleting whole word restores Telex","á",edit.Text);
            Console.WriteLine("INPUT struct size: "+Marshal.SizeOf(typeof(Native.INPUT)));
            if(Marshal.SizeOf(typeof(Native.INPUT))!=(IntPtr.Size==8 ? 40 : 28)) failures++;
            if(args.Length>0 && args[0]=="--integration") {
                IntPtr previous=GetKeyboardLayout(0);
                Console.WriteLine("Original keyboard layout: "+previous.ToInt64().ToString("X"));
                LoadKeyboardLayout("00000409",1);
                Console.WriteLine("Test keyboard layout: "+GetKeyboardLayout(0).ToInt64().ToString("X"));
                try { Application.EnableVisualStyles(); Application.Run(new IntegrationForm()); }
                finally { ActivateKeyboardLayout(previous,0); }
            }
            Console.WriteLine("Failures: "+failures); return failures==0 ? 0 : 1;
        }
        sealed class IntegrationForm : Form
        {
            readonly TextBox box=new TextBox { Dock=DockStyle.Fill, Multiline=true, ImeMode=ImeMode.Disable };
            readonly Timer timer=new Timer { Interval=45 };
            readonly Keyboard hook=new Keyboard { AcceptTestInput=true };
            readonly string[] input={"tieengs vieetj", "truowngf nguowif", "ass aaa ddd", "toasn", "tieengs\bg", "as", "as", "esscape tieengs vieetj", "ess\bscape", "esscape.as", "escape tieengs ", "hello.browser ", "github\t"};
            readonly string[] expected={"tiếng việt", "trường người", "as aa dd", "toán", "tiếng", "as", "á", "escape tiếng việt", "escape", "escape.á", "escape tiếng ", "hello.browser ", "github"};
            int test,position,delay=8;
            internal IntegrationForm() {
                Text="Telex Mini — automated input test"; Controls.Add(box);
                var start=new Button { Text="Start input test", Dock=DockStyle.Top, Height=40 };
                Controls.Add(start);
                start.Click+=delegate { start.Enabled=false; box.Focus(); LoadKeyboardLayout("00000409",1); timer.Start(); };
                timer.Tick+=Tick;
            }
            void Tick(object sender,EventArgs e) {
                if(delay-->0) return;
                if(Native.GetForegroundWindow()!=Handle) {
                    Console.WriteLine("FAIL integration aborted: test window lost foreground."); failures++; timer.Stop(); Close(); return;
                }
                if(position<input[test].Length) {
                    if(Environment.GetEnvironmentVariable("TELEX_TRACE")=="1") Console.WriteLine("TRACE "+position+" ["+box.Text+"]");
                    char c=input[test][position++]; ushort vk=c=='\b' ? (ushort)8 : c=='\t' ? (ushort)9 : c==' ' ? (ushort)32 : c=='.' ? (ushort)0xBE : (ushort)char.ToUpperInvariant(c);
                    var down=Native.Key(vk,0,0); down.Data.Keyboard.Extra=UIntPtr.Zero;
                    var up=Native.Key(vk,0,2); up.Data.Keyboard.Extra=UIntPtr.Zero;
                    Native.SendInput(2,new[] { down,up },Marshal.SizeOf(typeof(Native.INPUT)));
                    delay=1; return;
                }
                Equal("Windows TextBox: "+input[test],expected[test],box.Text);
                test++; position=0;
                if(test==input.Length) { timer.Stop(); Close(); return; }
                box.Clear(); hook.Enabled=test!=5; delay=3;
            }
            protected override void Dispose(bool disposing) { if(disposing) { timer.Dispose(); hook.Dispose(); } base.Dispose(disposing); }
        }
    }
}
