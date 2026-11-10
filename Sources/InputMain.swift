import AppKit
import InputMethodKit

/// System input source; no global keyboard hooks or Accessibility permissions.
@objc(TelexInputController)
final class TelexInputController: IMKInputController {
    private let composer=TelexComposer()
    private var isComposing=false

    private func preferences() {
        let defaults=UserDefaults(suiteName:"com.telexmini.mac") ?? .standard
        composer.smartEnglish=defaults.object(forKey:"smartEnglish") as? Bool ?? true
        composer.restoreInvalid=defaults.object(forKey:"restoreInvalid") as? Bool ?? true
    }

    private func clientText(_ sender: Any?) -> IMKTextInput? {
        return sender as? IMKTextInput
    }
    private var notFound: NSRange { NSRange(location:NSNotFound,length:NSNotFound) }
    private func mark(_ sender: Any?) {
        guard let client=clientText(sender) else { return }
        let length=(composer.text as NSString).length
        client.setMarkedText(composer.text, selectionRange:NSRange(location:length,length:0), replacementRange:notFound)
        isComposing = !composer.text.isEmpty
    }
    private func finish(_ sender: Any?) {
        if isComposing {
            clientText(sender)?.insertText(composer.finish(),replacementRange:notFound)
        }
        composer.reset()
        isComposing=false
    }

    override func handle(_ event: NSEvent!, client sender: Any!) -> Bool {
        guard event.type == .keyDown else { return false }
        preferences()
        // Secure fields are normally handled by the system's Roman input source.
        // Never inspect the receiving application's existing text.
        if event.modifierFlags.intersection([.command,.control,.option]).isEmpty == false {
            finish(sender)
            return false
        }
        guard let key=event.characters, !key.isEmpty else { return false }
        if event.keyCode == 51 { // Backspace: adjust current composition only.
            if isComposing { composer.backspace(); mark(sender); return true }
            return false
        }
        if event.keyCode == 53 { finish(sender); return false }
        if event.keyCode == 36 || event.keyCode == 48 || event.keyCode == 49 ||
           key.unicodeScalars.first?.value ?? 0 < 0x20 {
            finish(sender)
            return false
        }
        if key.count == 1, let char=key.first,
           (char >= "a" && char <= "z") || (char >= "A" && char <= "Z") {
            if composer.raw.count >= 63 { finish(sender); return false }
            _ = composer.push(char)
            mark(sender)
            return true
        }
        finish(sender)
        return false
    }

    override func commitComposition(_ sender: Any!) { finish(sender) }

    override func activateServer(_ sender: Any!) {
        composer.reset()
        TelexComposer.reloadLexicon()
        preferences()
    }
    override func deactivateServer(_ sender: Any!) {
        finish(sender)
    }
}

@main
enum TelexMiniInputMain {
    static func main() {
        let app=NSApplication.shared
        app.setActivationPolicy(.prohibited)
        let server=IMKServer(name:"TelexMini_Connection",
                             bundleIdentifier:"com.telexmini.mac.input")
        guard server != nil else {
            fputs("TelexMini: failed to initialize IMKServer\n",stderr)
            exit(1)
        }
        app.run()
    }
}
