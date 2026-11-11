using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace TelexMini
{
    internal static class Native
    {
        internal delegate IntPtr HookProc(int code, IntPtr message, IntPtr data);
        [DllImport("user32.dll", SetLastError=true)] internal static extern IntPtr SetWindowsHookEx(int id, HookProc proc, IntPtr module, uint thread);
        [DllImport("user32.dll")] internal static extern bool UnhookWindowsHookEx(IntPtr hook);
        [DllImport("user32.dll")] internal static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr message, IntPtr data);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)] internal static extern IntPtr GetModuleHandle(string name);
        [DllImport("user32.dll")] internal static extern short GetAsyncKeyState(int key);
        [DllImport("user32.dll")] internal static extern short GetKeyState(int key);
        [DllImport("user32.dll")] internal static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] internal static extern uint GetWindowThreadProcessId(IntPtr window, IntPtr process);
        [DllImport("user32.dll")] internal static extern bool GetGUIThreadInfo(uint thread, ref GUIInfo info);
        [DllImport("user32.dll", SetLastError=true)] internal static extern uint SendInput(uint count, INPUT[] inputs, int size);
        [DllImport("user32.dll")] internal static extern bool RegisterHotKey(IntPtr window, int id, uint mods, uint key);
        [DllImport("user32.dll")] internal static extern bool UnregisterHotKey(IntPtr window, int id);
        [DllImport("user32.dll")] internal static extern bool DestroyIcon(IntPtr icon);
        [StructLayout(LayoutKind.Sequential)] internal struct Point { public int X,Y; }
        [StructLayout(LayoutKind.Sequential)] internal struct Rect { public int L,T,R,B; }
        [StructLayout(LayoutKind.Sequential)] internal struct GUIInfo {
            public int Size; public uint Flags; public IntPtr Active,Focus,Capture,MenuOwner,MoveSize,Caret; public Rect CaretRect;
        }
        [StructLayout(LayoutKind.Sequential)] internal struct KBD {
            public uint Key,Scan,Flags,Time; public UIntPtr Extra;
        }
        [StructLayout(LayoutKind.Sequential)] internal struct KEYINPUT {
            public ushort Key,Scan; public uint Flags,Time; public UIntPtr Extra;
        }
        [StructLayout(LayoutKind.Sequential)] internal struct MOUSEINPUT {
            public int X,Y; public uint Data,Flags,Time; public UIntPtr Extra;
        }
        [StructLayout(LayoutKind.Explicit)] internal struct UNION {
            [FieldOffset(0)] public KEYINPUT Keyboard;
            [FieldOffset(0)] public MOUSEINPUT Mouse;
        }
        [StructLayout(LayoutKind.Sequential)] internal struct INPUT { public uint Type; public UNION Data; }
        internal static readonly UIntPtr Marker = new UIntPtr(0x54454C58u);
        internal static INPUT Key(ushort key, ushort scan, uint flags) {
            return new INPUT { Type=1, Data=new UNION { Keyboard=new KEYINPUT { Key=key, Scan=scan, Flags=flags, Extra=Marker } } };
        }
        internal static bool Replace(int erase, string insert, ushort followingKey=0)
        {
            var inputs = new INPUT[(erase + insert.Length) * 2 + (followingKey==0 ? 0 : 2)]; int p=0;
            for (int i=0; i<erase; i++) { inputs[p++]=Key(8,0,0); inputs[p++]=Key(8,0,2); }
            foreach (char c in insert) { inputs[p++]=Key(0,c,4); inputs[p++]=Key(0,c,6); }
            if(followingKey!=0) { inputs[p++]=Key(followingKey,0,0); inputs[p++]=Key(followingKey,0,2); }
            return inputs.Length == 0 || SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT))) == inputs.Length;
        }
    }
    internal sealed class Keyboard : IDisposable
    {
        readonly Native.HookProc keyProc, mouseProc;
        IntPtr keyHook, mouseHook, foreground, focus;
        readonly TelexEngine engine = new TelexEngine { SmartEnglish=true };
        bool enabled=true;
        bool suspended, disposed;
        readonly InputContextMonitor context;
        int contextRevision=-1;
        internal bool AutoEnglish=false;
        internal bool PreferEnglishExact {
            get {return engine.PreferEnglishExact;}
            set {engine.PreferEnglishExact=value;engine.Reset();}
        }
        internal bool UseVni {
            get { return engine.UseVni; }
            set { engine.UseVni=value; engine.Reset(); }
        }
        internal bool RestoreInvalid=true;
        internal bool SmartEnglish {
            get { return engine.SmartEnglish; }
            set { engine.SmartEnglish=value; engine.Reset(); }
        }
        internal InputContextKind ContextKind {
            get {
                if(context==null || !enabled || suspended) return InputContextKind.Text;
                IntPtr window=Native.GetForegroundWindow();
                IntPtr focused=InputContextMonitor.FocusOf(window);
                if(InputContextMonitor.IsNativePassword(focused)) return InputContextKind.Password;
                return context.GetKind(window,focused);
            }
        }
        internal bool EffectiveEnabled { get { return ContextPolicy.UseVietnamese(enabled,AutoEnglish,ContextKind); } }
        internal bool AcceptTestInput = false;
        internal event Action InjectionFailed;
        internal bool Enabled { get { return enabled; } set { enabled=value; engine.Reset(); UpdateHooks(); } }
        internal bool Suspended { get { return suspended; } set { if(suspended==value) return; suspended=value; engine.Reset(); UpdateHooks(); } }
        internal bool HooksInstalled { get { return keyHook!=IntPtr.Zero || mouseHook!=IntPtr.Zero; } }
        internal Keyboard(InputContextMonitor monitor=null)
        {
            context=monitor;
            keyProc=OnKey; mouseProc=OnMouse;
            UpdateHooks();
        }
        void UpdateHooks()
        {
            if(disposed || suspended || !enabled) { RemoveHooks(); return; }
            if(keyHook!=IntPtr.Zero) return;
            IntPtr module=Native.GetModuleHandle(null);
            keyHook=Native.SetWindowsHookEx(13,keyProc,module,0);
            if (keyHook == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
            mouseHook=Native.SetWindowsHookEx(14,mouseProc,module,0);
            if (mouseHook == IntPtr.Zero) { int error=Marshal.GetLastWin32Error(); RemoveHooks(); throw new Win32Exception(error); }
        }
        static bool Down(int key) { return Native.GetAsyncKeyState(key) < 0; }
        IntPtr OnMouse(int code, IntPtr message, IntPtr data)
        {
            int m=message.ToInt32();
            if (code >= 0 && (m==0x201 || m==0x204 || m==0x207 || m==0x20B || m==0x20A || m==0x20E)) {
                engine.Reset(); if(context!=null) context.RequestCheck();
            }
            return Native.CallNextHookEx(mouseHook,code,message,data);
        }
        IntPtr OnKey(int code, IntPtr message, IntPtr data)
        {
            if (code < 0) return Native.CallNextHookEx(keyHook,code,message,data);
            var key=(Native.KBD)Marshal.PtrToStructure(data,typeof(Native.KBD));
            if (key.Extra == Native.Marker) return Native.CallNextHookEx(keyHook,code,message,data);
            if ((key.Flags & 0x10) != 0 && !AcceptTestInput) { engine.Reset(); return Native.CallNextHookEx(keyHook,code,message,data); }
            int m=message.ToInt32();
            if (m!=0x100 && m!=0x104) return Native.CallNextHookEx(keyHook,code,message,data);
            IntPtr window=Native.GetForegroundWindow();
            var info=new Native.GUIInfo { Size=Marshal.SizeOf(typeof(Native.GUIInfo)) };
            Native.GetGUIThreadInfo(Native.GetWindowThreadProcessId(window,IntPtr.Zero),ref info);
            if (foreground!=window || focus!=info.Focus) {
                engine.Reset(); foreground=window; focus=info.Focus;
                if(context!=null) context.RequestCheck();
            }
            if(context!=null && contextRevision!=context.Current.Revision) { engine.Reset(); contextRevision=context.Current.Revision; }
            InputContextKind kind=InputContextMonitor.IsNativePassword(info.Focus) ? InputContextKind.Password :
                context==null ? InputContextKind.Text : context.GetKind(window,info.Focus);
            if(!ContextPolicy.UseVietnamese(enabled,AutoEnglish,kind)) {
                engine.Reset(); return Native.CallNextHookEx(keyHook,code,message,data);
            }
            if (Down(0x11) || Down(0x12) || Down(0x5B) || Down(0x5C)) { engine.Reset(); return Native.CallNextHookEx(keyHook,code,message,data); }
            uint vk=key.Key;
            if (vk==8) { engine.Backspace(); return Native.CallNextHookEx(keyHook,code,message,data); }
            if ((vk>=0x41 && vk<=0x5A) ||
                (engine.UseVni && vk>=0x31 && vk<=0x39 && !Down(0x10))) {
                char c=(char)vk;
                if (!(Down(0x10) ^ ((Native.GetKeyState(0x14)&1)!=0))) c=char.ToLowerInvariant(c);
                string before=engine.Text, after=engine.Push(c);
                if (after==before+c) return Native.CallNextHookEx(keyHook,code,message,data);
                int same=0;
                while (same<before.Length && same<after.Length && before[same]==after[same]) same++;
                if (!Native.Replace(before.Length-same,after.Substring(same)))
                {
                    engine.Reset(); if (InjectionFailed!=null) InjectionFailed();
                }
                return new IntPtr(1);
            }
            if(vk==32 || vk==13 || vk==9 || (vk>=0xBA && vk<=0xC0) || (vk>=0xDB && vk<=0xDE)) {
                string before=engine.Text, after=engine.Finish(RestoreInvalid);
                engine.Reset();
                if(before!=after) {
                    int same=0; while(same<before.Length && same<after.Length && before[same]==after[same]) same++;
                    if(!Native.Replace(before.Length-same,after.Substring(same),(ushort)vk) && InjectionFailed!=null) InjectionFailed();
                    return new IntPtr(1);
                }
            }
            if (vk!=0x10 && vk!=0xA0 && vk!=0xA1 && vk!=0x14) engine.Reset();
            return Native.CallNextHookEx(keyHook,code,message,data);
        }
        public void Dispose()
        {
            disposed=true; RemoveHooks();
        }
        void RemoveHooks()
        {
            if (keyHook!=IntPtr.Zero) { Native.UnhookWindowsHookEx(keyHook); keyHook=IntPtr.Zero; }
            if (mouseHook!=IntPtr.Zero) { Native.UnhookWindowsHookEx(mouseHook); mouseHook=IntPtr.Zero; }
        }
    }
}
