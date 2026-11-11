import Foundation
import Carbon

let app=URL(fileURLWithPath:CommandLine.arguments.count>1 ? CommandLine.arguments[1] : "")
guard FileManager.default.fileExists(atPath:app.path) else {
    fputs("INPUT_MISSING\n",stderr);exit(2)
}
let status=TISRegisterInputSource(app as CFURL)
print("REGISTER_STATUS=\(status)")
// Include installed-but-disabled sources when diagnosing macOS registration.
let items=TISCreateInputSourceList(nil,true).takeRetainedValue() as! [TISInputSource]
var matches=0
for item in items {
    guard let p=TISGetInputSourceProperty(item,kTISPropertyInputSourceID) else {continue}
    let id=Unmanaged<CFString>.fromOpaque(p).takeUnretainedValue() as String
    if id.localizedCaseInsensitiveContains("telexmini") {
        matches+=1
        print("FOUND_SOURCE=\(id)")
        let result=TISEnableInputSource(item)
        print("ENABLE_STATUS=\(result)")
    }
}
print("SOURCE_COUNT=\(matches)")
if matches==0 {
    print("ACTION_REQUIRED=Log out and back in, then enable TelexMini under Keyboard > Text Input > Edit")
}
