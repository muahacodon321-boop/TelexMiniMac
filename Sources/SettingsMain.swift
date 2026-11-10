import SwiftUI
import AppKit

private enum Palette {
    static let bg=Color(red:0.055,green:0.084,blue:0.14)
    static let sidebar=Color(red:0.07,green:0.115,blue:0.18)
    static let card=Color(red:0.103,green:0.16,blue:0.235)
    static let outline=Color(red:0.20,green:0.29,blue:0.36)
    static let muted=Color(red:0.62,green:0.71,blue:0.79)
    static let mint=Color(red:0.27,green:0.89,blue:0.72)
    static let blue=Color(red:0.24,green:0.67,blue:0.93)
}

final class SettingsStore: ObservableObject {
    @Published var smartEnglish: Bool { didSet { defaults.set(smartEnglish,forKey:"smartEnglish") } }
    @Published var restoreInvalid: Bool { didSet { defaults.set(restoreInvalid,forKey:"restoreInvalid") } }
    @Published var darkMode: Bool { didSet { defaults.set(darkMode,forKey:"darkMode") } }
    @Published var autoStart: Bool { didSet { defaults.set(autoStart,forKey:"autoStart") } }
    private let defaults=UserDefaults(suiteName:"com.telexmini.mac") ?? .standard
    init() {
        smartEnglish=defaults.object(forKey:"smartEnglish") as? Bool ?? true
        restoreInvalid=defaults.object(forKey:"restoreInvalid") as? Bool ?? true
        darkMode=defaults.object(forKey:"darkMode") as? Bool ?? true
        autoStart=defaults.object(forKey:"autoStart") as? Bool ?? false
    }
    static var directory: URL {
        let dir=FileManager.default.homeDirectoryForCurrentUser
           .appendingPathComponent("Library/Application Support/TelexMini",isDirectory:true)
        try? FileManager.default.createDirectory(at:dir,withIntermediateDirectories:true)
        return dir
    }
    func edit(_ file:String) {
        let url=Self.directory.appendingPathComponent(file)
        if !FileManager.default.fileExists(atPath:url.path) {
            try? "# Mỗi dòng một từ, không cần dấu cách.\n".write(to:url,atomically:true,encoding:.utf8)
        }
        NSWorkspace.shared.open(url)
    }
}

struct PreferenceRow: View {
    let icon:String
    let title:String
    let description:String
    @Binding var checked:Bool
    var body: some View {
        HStack(spacing:14) {
            Image(systemName:icon).font(.system(size:17,weight:.semibold))
                .foregroundStyle(Palette.mint)
                .frame(width:42,height:42)
                .background(Palette.mint.opacity(0.1),in:RoundedRectangle(cornerRadius:12))
            VStack(alignment:.leading,spacing:5) {
                Text(title).font(.system(size:13,weight:.semibold))
                Text(description).font(.system(size:11)).foregroundStyle(Palette.muted)
            }
            Spacer()
            Toggle("",isOn:$checked).toggleStyle(.switch).tint(Palette.mint).labelsHidden()
        }
        .padding(.vertical,10)
    }
}

struct SidebarButton:View {
    let icon:String
    let label:String
    let action:()->Void
    var selected=false
    var body:some View {
        Button(action:action) {
            HStack(spacing:14) {
                Image(systemName:icon).font(.system(size:15,weight:.medium)).frame(width:19)
                Text(label).font(.system(size:12,weight:.semibold))
                Spacer()
            }
            .foregroundStyle(selected ? Palette.mint : Color.white.opacity(0.78))
            .padding(.horizontal,16).frame(height:42)
            .background(selected ? Palette.mint.opacity(0.12) : .clear,
                        in:RoundedRectangle(cornerRadius:11))
        }.buttonStyle(.plain)
    }
}

struct SettingsView:View {
    @StateObject private var prefs=SettingsStore()
    @State private var preview=""
    @State private var section="dashboard"
    private var wordCount:String { "1.566+" }
    var body:some View {
        HStack(spacing:0) {
            sidebar
            ScrollView {
                VStack(alignment:.leading,spacing:22) {
                    header
                    hero
                    HStack(spacing:16) { statusCard; privacyCard }
                    VStack(alignment:.leading,spacing:8) {
                        Text("CÁ NHÂN HÓA").font(.system(size:11,weight:.bold))
                            .tracking(2).foregroundStyle(Palette.muted)
                        VStack(spacing:0) {
                            PreferenceRow(icon:"character.cursor.ibeam",title:"Nhận diện tiếng Anh",
                                          description:"Bảo vệ từ tiếng Anh ngay khi nhập",
                                          checked:$prefs.smartEnglish)
                            Divider().overlay(Palette.outline)
                            PreferenceRow(icon:"arrow.uturn.backward",title:"Khôi phục từ không hợp lệ",
                                          description:"Tự khôi phục phím gốc ở cuối từ",
                                          checked:$prefs.restoreInvalid)
                        }
                        .padding(.horizontal,20)
                        .background(Palette.card,in:RoundedRectangle(cornerRadius:18))
                    }
                    VStack(alignment:.leading,spacing:9) {
                        Label("THỬ GÕ NGAY",systemImage:"keyboard")
                            .font(.system(size:11,weight:.bold)).tracking(1.2)
                            .foregroundStyle(Palette.muted)
                        TextField("Nhập thử tiếng Việt hoặc tiếng Anh…",text:$preview)
                            .textFieldStyle(.plain).font(.system(size:14))
                            .padding(14)
                            .background(Palette.card,in:RoundedRectangle(cornerRadius:12))
                        Text("Chọn nguồn nhập TelexMini trong macOS để thử chuyển đổi dấu.")
                            .font(.system(size:11)).foregroundStyle(Palette.muted)
                    }
                    HStack {
                        Text("Native macOS · Apple Silicon · Offline first")
                        Spacer()
                        Text("TelexMini 1.0 Preview")
                    }
                    .font(.system(size:10,weight:.medium)).foregroundStyle(Palette.muted)
                }
                .padding(28)
            }
            .background(Palette.bg)
        }
        .frame(minWidth:930,minHeight:640)
        .preferredColorScheme(.dark)
    }
    private var sidebar:some View {
        VStack(alignment:.leading,spacing:20) {
            HStack(spacing:11) {
                Image(systemName:"character.bubble.fill")
                    .font(.system(size:24,weight:.semibold))
                    .foregroundStyle(Palette.mint)
                    .frame(width:42,height:42)
                    .background(Palette.mint.opacity(0.1),in:RoundedRectangle(cornerRadius:13))
                VStack(alignment:.leading,spacing:1) {
                    Text("telexmini").font(.system(size:20,weight:.bold,design:.rounded))
                    Text("SMART INPUT SYSTEM").font(.system(size:8,weight:.bold))
                        .tracking(1.4).foregroundStyle(Palette.muted)
                }
            }
            .padding(.top,16)
            Rectangle().fill(Palette.outline).frame(height:1)
            Text("WORKSPACE").font(.system(size:10,weight:.bold))
                .tracking(1.4).foregroundStyle(Palette.muted)
            VStack(spacing:5) {
                SidebarButton(icon:"square.grid.2x2.fill",label:"Tổng quan",action:{ section="dashboard" },selected:true)
                SidebarButton(icon:"text.badge.plus",label:"Từ điển tiếng Anh",action:{ prefs.edit("EnglishWords.txt") })
                SidebarButton(icon:"character.book.closed",label:"Ưu tiên Telex",action:{ prefs.edit("VietnameseOverrides.txt") })
                SidebarButton(icon:"arrow.clockwise",label:"Nạp lại từ điển",action:{
                    // Input-source process loads preferences/dictionary on its next activation.
                    let path=SettingsStore.directory.appendingPathComponent("dictionary-reload.flag")
                    try? Date().description.write(to:path,atomically:true,encoding:.utf8)
                })
            }
            Spacer()
            VStack(alignment:.leading,spacing:12) {
                Image(systemName:"lock.shield.fill").font(.system(size:20)).foregroundStyle(Palette.mint)
                Text("PRIVATE BY DESIGN").font(.system(size:10,weight:.bold))
                    .tracking(1.3).foregroundStyle(Palette.muted)
                Text("Không gửi phím đã gõ lên mạng. Không lưu mật khẩu hay lịch sử gõ.")
                    .font(.system(size:11)).foregroundStyle(.white.opacity(0.75))
                    .lineSpacing(5)
            }
            .padding(16).frame(maxWidth:.infinity,alignment:.leading)
            .background(Palette.card,in:RoundedRectangle(cornerRadius:14))
            Text("BUILT FOR macOS").font(.system(size:10,weight:.medium))
                .tracking(1.4).foregroundStyle(Palette.muted)
                .padding(.bottom,9)
        }
        .padding(.horizontal,19)
        .frame(width:235)
        .background(Palette.sidebar)
    }
    private var header:some View {
        HStack {
            VStack(alignment:.leading,spacing:5) {
                Text("OVERVIEW / BẢNG ĐIỀU KHIỂN")
                    .font(.system(size:10,weight:.bold)).tracking(1.8)
                    .foregroundStyle(Palette.mint)
                Text("Gõ theo cách của bạn.")
                    .font(.system(size:27,weight:.bold,design:.rounded))
                Text("Hai ngôn ngữ. Một trải nghiệm liền mạch.")
                    .font(.system(size:13)).foregroundStyle(Palette.muted)
            }
            Spacer()
            Label("LOCAL & SECURE",systemImage:"checkmark.shield.fill")
                .font(.system(size:11,weight:.bold))
                .foregroundStyle(Palette.mint)
                .padding(.horizontal,13).padding(.vertical,10)
                .background(Palette.mint.opacity(0.08),
                            in:Capsule())
        }
    }
    private var hero:some View {
        HStack(alignment:.center) {
            VStack(alignment:.leading,spacing:12) {
                Text("SMART BILINGUAL TYPING").font(.system(size:11,weight:.bold))
                    .tracking(1.9).foregroundStyle(.white.opacity(0.8))
                Text("Tiếng Việt tự nhiên.\nEnglish liền mạch.")
                    .font(.system(size:24,weight:.bold,design:.rounded))
                    .lineSpacing(5).fixedSize(horizontal:false,vertical:true)
                Text("Telex tức thời · Từ điển offline · Không gián đoạn")
                    .font(.system(size:12)).foregroundStyle(.white.opacity(0.85))
            }
            Spacer()
            Image(systemName:"keyboard")
                .font(.system(size:76,weight:.ultraLight))
                .foregroundStyle(.white.opacity(0.9))
                .padding(24)
                .background(.white.opacity(0.08),
                            in:RoundedRectangle(cornerRadius:24))
        }
        .padding(27)
        .frame(maxWidth:.infinity,alignment:.leading)
        .background(LinearGradient(colors:[
            Color(red:0.04,green:0.55,blue:0.49),
            Color(red:0.09,green:0.34,blue:0.53),
            Color(red:0.11,green:0.22,blue:0.43)
        ],startPoint:.topLeading,endPoint:.bottomTrailing),
        in:RoundedRectangle(cornerRadius:22))
    }
    private var statusCard:some View {
        HStack(alignment:.center,spacing:14) {
            Image(systemName:"globe.asia.australia.fill").font(.system(size:24))
                .foregroundStyle(Palette.mint)
            VStack(alignment:.leading,spacing:7) {
                Text("TỪ ĐIỂN OFFLINE").font(.system(size:10,weight:.bold))
                    .foregroundStyle(Palette.muted)
                Text(wordCount+" từ").font(.system(size:20,weight:.bold))
                Text("Có thể mở rộng").font(.system(size:11))
                    .foregroundStyle(Palette.muted)
            }
            Spacer()
        }
        .padding(20)
        .frame(maxWidth:.infinity,alignment:.leading)
        .background(Palette.card,in:RoundedRectangle(cornerRadius:17))
    }
    private var privacyCard:some View {
        HStack(alignment:.center,spacing:14) {
            Image(systemName:"lock.shield.fill").font(.system(size:26))
                .foregroundStyle(Palette.mint)
            VStack(alignment:.leading,spacing:7) {
                Text("MẬT KHẨU").font(.system(size:10,weight:.bold))
                    .foregroundStyle(Palette.muted)
                Text("Hệ thống bảo vệ").font(.system(size:17,weight:.bold))
                Text("Không xử lý Secure Input").font(.system(size:11))
                    .foregroundStyle(Palette.muted)
            }
            Spacer()
        }
        .padding(20)
        .frame(maxWidth:.infinity,alignment:.leading)
        .background(Palette.card,in:RoundedRectangle(cornerRadius:17))
    }
}

@main
struct TelexMiniSettingsApp: App {
    var body: some Scene {
        WindowGroup("TelexMini") { SettingsView() }
            .windowResizability(.contentMinSize)
    }
}
