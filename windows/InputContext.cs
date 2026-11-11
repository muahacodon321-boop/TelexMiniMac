using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Automation;

namespace TelexMini
{
    // Foreground notifications only: no keyboard/mouse hook and no UI Automation.
    internal sealed class GameGuard : IDisposable
    {
        delegate void WinEventProc(IntPtr hook,uint evt,IntPtr window,int obj,int child,uint thread,uint time);
        [DllImport("user32.dll")] static extern IntPtr SetWinEventHook(uint min,uint max,IntPtr module,WinEventProc callback,uint process,uint thread,uint flags);
        [DllImport("user32.dll")] static extern bool UnhookWinEvent(IntPtr hook);
        [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr window,out uint process);
        readonly WinEventProc callback;
        readonly Action<bool> changed;
        IntPtr hook, lastWindow;
        uint lastProcess;
        bool game;
        internal GameGuard(Action<bool> onChanged) {
            changed=onChanged; callback=delegate { Check(); };
            hook=SetWinEventHook(3,3,IntPtr.Zero,callback,0,0,0); // EVENT_SYSTEM_FOREGROUND
            Check();
        }
        internal void RefreshPolicy() {lastWindow=IntPtr.Zero;lastProcess=0;Check();}
        internal void Check() {
            IntPtr window=Native.GetForegroundWindow();
            if(window==lastWindow) return;
            lastWindow=window;
            uint pid; GetWindowThreadProcessId(window,out pid);
            if(pid==lastProcess) return;
            lastProcess=pid;
            bool next=false;
            try { using(var process=Process.GetProcessById((int)pid)) next=ContextPolicy.ShouldYieldKeys(process.ProcessName); }
            catch(ArgumentException) { } catch(InvalidOperationException) { } catch(System.ComponentModel.Win32Exception) { }
            if(next!=game) { game=next; changed(game); }
        }
        public void Dispose() { if(hook!=IntPtr.Zero) { UnhookWinEvent(hook); hook=IntPtr.Zero; } }
    }
    internal enum InputContextKind { Text, Terminal, Password, Checking, Unavailable }
    internal static class ContextPolicy
    {
        internal static bool IsSteamRemote(string name) {
            name=(name ?? "").ToLowerInvariant();
            if(name.EndsWith(".exe")) name=name.Substring(0,name.Length-4);
            return name=="streaming_client";
        }
        internal static bool ShouldYieldKeys(string name) {
            return IsGame(name) || IsSteamRemote(name) || IsUserExcluded(name);
        }
        internal static bool IsUserExcluded(string name) {
            name=(name ?? "").Trim().ToLowerInvariant();
            if(name.EndsWith(".exe")) name=name.Substring(0,name.Length-4);
            if(name.Length==0) return false;
            return MatchesExcluded(name,UserPreferences.String("ExcludedApps",""));
        }
        internal static bool MatchesExcluded(string name,string list) {
            name=(name ?? "").Trim().ToLowerInvariant();
            if(name.EndsWith(".exe")) name=name.Substring(0,name.Length-4);
            foreach(string entry in (list ?? "").Split(new[]{','},StringSplitOptions.RemoveEmptyEntries)) {
                string item=entry.Trim().ToLowerInvariant();
                if(item.EndsWith(".exe")) item=item.Substring(0,item.Length-4);
                if(item==name) return true;
            }
            return false;
        }
        internal static bool IsGame(string name) {
            name=(name ?? "").ToLowerInvariant();
            if(name.EndsWith(".exe")) name=name.Substring(0,name.Length-4);
            return name.StartsWith("fightcade") || name.StartsWith("fcade") ||
                name=="fbneo" || name.StartsWith("fbneo_") || name=="fba" || name=="fba64" ||
                name=="flycast" || name=="flycast-dojo";
        }
        internal static bool IsTerminal(string name) {
            name=(name ?? "").ToLowerInvariant();
            if(name.EndsWith(".exe")) name=name.Substring(0,name.Length-4);
            switch(name) {
                case "windowsterminal": case "windowsterminalpreview": case "cmd":
                case "powershell": case "pwsh": case "conhost": case "openconsole":
                case "wsl": case "bash": case "mintty": case "putty": case "kitty":
                case "wezterm-gui": case "alacritty": case "hyper": return true;
                default: return false;
            }
        }
        internal static bool UseVietnamese(bool preferred, bool automatic, InputContextKind context) {
            if (!preferred) return false;
            // Fail closed while password classification is pending or unavailable.
            // Password protection is independent of optional terminal detection.
            if (context==InputContextKind.Password ||
                context==InputContextKind.Checking || context==InputContextKind.Unavailable) return false;
            return !automatic || context==InputContextKind.Text;
        }
    }
    // UIA runs only on this background MTA worker, never inside the keyboard hook.
    // Only focus metadata is queried. ValuePattern/TextPattern and field contents are never read.
    internal sealed class InputContextMonitor : IDisposable
    {
        internal sealed class Snapshot {
            internal readonly IntPtr Window, Focus;
            internal readonly InputContextKind Kind;
            internal readonly int Revision, Tick;
            internal Snapshot(IntPtr window, IntPtr focus, InputContextKind kind, int revision) {
                Window=window; Focus=focus; Kind=kind; Revision=revision; Tick=Environment.TickCount;
            }
        }
        readonly AutoResetEvent wake=new AutoResetEvent(false);
        readonly Thread worker;
        volatile bool stopped;
        volatile bool enabled;
        volatile Snapshot current=new Snapshot(IntPtr.Zero,IntPtr.Zero,InputContextKind.Checking,0);
        int revision;
        int accessibilityQueries;
        internal int AccessibilityQueries { get { return Volatile.Read(ref accessibilityQueries); } }
        uint lastProcess;
        string processName="";
        readonly AutomationFocusChangedEventHandler focusHandler;
        internal InputContextMonitor() {
            focusHandler=delegate { RequestCheck(); };
            worker=new Thread(Run) { IsBackground=true, Name="Telex Mini focus metadata" };
            worker.SetApartmentState(ApartmentState.MTA); worker.Start();
        }
        internal Snapshot Current { get { return current; } }
        internal bool Enabled {
            get { return enabled; }
            set { if(enabled==value) return; enabled=value; Interlocked.Increment(ref revision); wake.Set(); }
        }
        internal void RequestCheck() {
            if(stopped || !enabled) return;
            Interlocked.Increment(ref revision);
            wake.Set();
        }
        internal InputContextKind GetKind(IntPtr window,IntPtr focus) {
            Snapshot s=current;
            if(s.Window!=window || s.Focus!=focus || s.Revision!=Volatile.Read(ref revision) ||
                unchecked((uint)(Environment.TickCount-s.Tick))>2000) return InputContextKind.Checking;
            return s.Kind;
        }
        internal static IntPtr FocusOf(IntPtr window) {
            if(window==IntPtr.Zero) return IntPtr.Zero;
            var info=new Native.GUIInfo { Size=Marshal.SizeOf(typeof(Native.GUIInfo)) };
            Native.GetGUIThreadInfo(Native.GetWindowThreadProcessId(window,IntPtr.Zero),ref info);
            return info.Focus;
        }
        [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr window,out uint process);
        [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern int GetClassName(IntPtr window,StringBuilder name,int count);
        [DllImport("user32.dll",EntryPoint="GetWindowLongW")] static extern int GetWindowLong(IntPtr window,int index);
        internal static bool IsNativePassword(IntPtr focus) {
            if(focus==IntPtr.Zero) return false;
            var name=new StringBuilder(128); GetClassName(focus,name,name.Capacity);
            string value=name.ToString();
            return (value.Equals("Edit",StringComparison.OrdinalIgnoreCase) || value.IndexOf("EDIT",StringComparison.OrdinalIgnoreCase)>=0)
                && (GetWindowLong(focus,-16)&0x20)!=0;
        }
        void Refresh() {
            if(!enabled || stopped) return;
            int version=Volatile.Read(ref revision);
            IntPtr window=Native.GetForegroundWindow(), focus=FocusOf(window);
            InputContextKind kind=InputContextKind.Unavailable;
            try {
                uint pid; GetWindowThreadProcessId(window,out pid);
                if(pid!=lastProcess) {
                    using(var process=Process.GetProcessById((int)pid)) processName=process.ProcessName;
                    lastProcess=pid;
                }
                if(IsNativePassword(focus)) kind=InputContextKind.Password;
                else if(ContextPolicy.ShouldYieldKeys(processName) || ContextPolicy.IsTerminal(processName)) kind=InputContextKind.Terminal;
                else {
                    if(!enabled || stopped) return;
                    Interlocked.Increment(ref accessibilityQueries);
                    AutomationElement element=AutomationElement.FocusedElement;
                    if(element!=null) {
                        object password=element.GetCurrentPropertyValue(AutomationElement.IsPasswordProperty,true);
                        if(password is bool) kind=(bool)password ? InputContextKind.Password : InputContextKind.Text;
                    }
                }
            } catch(ElementNotAvailableException) { }
              catch(InvalidOperationException) { }
              catch(System.ComponentModel.Win32Exception) { }
              catch(ArgumentException) { }
              catch(COMException) { }
              catch(UnauthorizedAccessException) { }
            if(enabled && !stopped && version==Volatile.Read(ref revision) && window==Native.GetForegroundWindow() && focus==FocusOf(window))
                current=new Snapshot(window,focus,kind,version);
        }
        void Run() {
            bool subscribed=false;
            try {
                while(!stopped) {
                    if(!enabled) {
                        if(subscribed) {
                            try { Automation.RemoveAutomationFocusChangedEventHandler(focusHandler); }
                            catch(COMException) { } catch(InvalidOperationException) { }
                            subscribed=false;
                        }
                        // No polling and no accessibility subscription while disabled.
                        if(!enabled && !stopped) wake.WaitOne();
                        continue;
                    }
                    if(!subscribed) {
                        try { Automation.AddAutomationFocusChangedEventHandler(focusHandler); subscribed=true; }
                        catch(COMException) { } catch(InvalidOperationException) { }
                    }
                    Refresh(); wake.WaitOne(750);
                }
            } finally {
                if(subscribed) {
                    try { Automation.RemoveAutomationFocusChangedEventHandler(focusHandler); }
                    catch(COMException) { } catch(InvalidOperationException) { }
                }
                // A provider may be slow; process shutdown never waits indefinitely for it.
            }
        }
        public void Dispose() { stopped=true; wake.Set(); if(worker.Join(150)) wake.Dispose(); }
    }
}
