import Foundation

/// Pure, deterministic Telex engine shared by the native input source and its tests.
/// It keeps only the current composition in RAM; it never reads other applications' text.
final class TelexComposer {
    private static let rows: [[Character]] = [
        Array("aáàảãạ"), Array("ăắằẳẵặ"), Array("âấầẩẫậ"),
        Array("eéèẻẽẹ"), Array("êếềểễệ"), Array("iíìỉĩị"),
        Array("oóòỏõọ"), Array("ôốồổỗộ"), Array("ơớờởỡợ"),
        Array("uúùủũụ"), Array("ưứừửữự"), Array("yýỳỷỹỵ")
    ]
    private static let bases = Array("aăâeêioôơuưy")
    private static let tones = Array("zsfrxj")
    var text = ""
    private(set) var raw = ""
    private var literal = false
    private var englishCandidate = false
    private var foreignWord = false
    private var telexShaped = false
    private var lastTelexKey = false
    private var lastToneApplied = false
    private var pendingEscapeRaw:String? = nil
    var smartEnglish = true
    var useVni = false
    var preferEnglishExact = false
    var restoreInvalid = true
    private(set) static var english = Set<String>()
    private(set) static var prefixes = Set<String>()
    private(set) static var vietnamese = Set<String>()
    private static var vietnamesePrefixes = Set<String>()
    private static var loaded = false

    init() { Self.loadLexicon() }
    static func loadLexicon() {
        guard !loaded else { return }
        loaded = true
        let builtin = Bundle.main.url(forResource: "english-core", withExtension: "txt")
            ?? URL(fileURLWithPath: FileManager.default.currentDirectoryPath)
                .appendingPathComponent("TelexMiniMac/Resources/english-core.txt")
        let custom = FileManager.default.homeDirectoryForCurrentUser
            .appendingPathComponent("Library/Application Support/TelexMini/EnglishWords.txt")
        let vn = FileManager.default.homeDirectoryForCurrentUser
            .appendingPathComponent("Library/Application Support/TelexMini/VietnameseOverrides.txt")
        for url in [builtin, custom].compactMap({ $0 }) {
            guard let data = try? String(contentsOf: url, encoding: .utf8) else { continue }
            for line in data.split(whereSeparator: \.isNewline) {
                let word = line.trimmingCharacters(in: .whitespaces).lowercased()
                if word.first == "#" || word.isEmpty || word.count > 64 ||
                    !word.utf8.allSatisfy({ $0 >= 97 && $0 <= 122 }) { continue }
                english.insert(word)
                for i in 1..<word.count { prefixes.insert(String(word.prefix(i))) }
            }
        }
        if let data = try? String(contentsOf: vn, encoding: .utf8) {
            for line in data.split(whereSeparator: \.isNewline) {
                let word=line.trimmingCharacters(in: .whitespaces).lowercased()
                if !word.isEmpty && word.first != "#" && vietnamese.insert(word).inserted {
                    for i in 1..<word.count {vietnamesePrefixes.insert(String(word.prefix(i)))}
                }
            }
        }
    }
    static func reloadLexicon() {
        loaded=false; english.removeAll(); prefixes.removeAll()
        vietnamese.removeAll(); vietnamesePrefixes.removeAll(); loadLexicon()
    }
    private static func preferVietnamesePrefix(_ raw:String) -> Bool {
        let word=raw.lowercased()
        return vietnamese.contains(word) || vietnamesePrefixes.contains(word)
    }
    private static func isEnglishPrefix(_ word: String) -> Bool {
        let w=word.lowercased()
        return !vietnamese.contains(w) && (english.contains(w) || prefixes.contains(w))
    }
    func reset() { text=""; raw=""; literal=false; englishCandidate=false; foreignWord=false; telexShaped=false; lastTelexKey=false; lastToneApplied=false; pendingEscapeRaw=nil }
    func backspace() {
        if !text.isEmpty { text.removeLast() }
        raw=text
        englishCandidate=smartEnglish && Self.isEnglishPrefix(raw)
        foreignWord=smartEnglish && Self.foreignStart(raw)
        telexShaped=text != raw
        lastTelexKey=false
        lastToneApplied=false
        pendingEscapeRaw=nil
        if text.isEmpty { literal=false }
    }
    private static func row(_ c: Character) -> Int? {
        let lower=Character(String(c).lowercased())
        return rows.firstIndex(where: { $0.contains(lower) })
    }
    private static func root(_ c: Character) -> Character {
        guard let index=row(c) else { return Character(String(c).lowercased()) }
        return bases[index]
    }
    private static func tone(_ c: Character) -> Int {
        guard let index=row(c) else { return 0 }
        return rows[index].firstIndex(of: Character(String(c).lowercased())) ?? 0
    }
    private static func alter(_ c: Character, shape: Character, mark: Int? = nil) -> Character {
        guard let index=bases.firstIndex(of: shape) else { return c }
        let base=rows[index][mark ?? tone(c)]
        return String(c).uppercased() == String(c) ? Character(String(base).uppercased()) : base
    }
    private static func vowels(_ a: [Character]) -> [Int] {
        var v=a.indices.filter { row(a[$0]) != nil }
        if v.count > 1 && v[0] == 1 &&
            ((root(a[0]) == "q" && root(a[1]) == "u") ||
             (root(a[0]) == "g" && root(a[1]) == "i")) { v.removeFirst() }
        return v
    }
    private static func toneIndex(_ a: [Character]) -> Int? {
        let v=vowels(a)
        if v.isEmpty { return nil }
        if let i=v.reversed().first(where: { "ăâêôơ".contains(root(a[$0])) }) { return i }
        if let i=v.reversed().first(where: { root(a[$0]) == "ư" }) { return i }
        if v.count == 1 { return v[0] }
        if v.count >= 3 { return v[v.count-2] }
        let pair=String([root(a[v[0]]),root(a[v[1]])])
        return v[1] < a.count-1 || ["oa","oe","uy"].contains(pair) ? v[1] : v[0]
    }
    private static func setTone(_ s: String, _ mark: Int) -> String {
        var a=Array(s)
        let target=toneIndex(a)
        for i in a.indices where row(a[i]) != nil {
            a[i]=alter(a[i],shape:root(a[i]),mark:i == target ? mark : 0)
        }
        return String(a)
    }
    private static func normalize(_ s: String) -> String {
        var mark=0
        for c in s where tone(c) != 0 { mark=tone(c) }
        return setTone(s,mark)
    }
    private func literalize(_ value: String) {
        literal=true
        text=value
    }
    private func core(_ key: Character) {
        let k=Character(String(key).lowercased())
        if text.count >= 64 { literal=true }
        if literal { text.append(key); return }
        let a=Array(text)
        if useVni {
            guard let digit=k.wholeNumberValue, digit>=1, digit<=9,
                !Self.impossibleVietnamese(text) else {
                text=Self.normalize(text+String(key)); return
            }
            if digit>=1 && digit<=5 && !Self.vowels(a).isEmpty {
                text=Self.setTone(text,digit); return
            }
            if digit==9 {
                if let i=a.firstIndex(where:{Self.root($0)=="d"}) {
                    var b=a;b[i]=String(a[i]).uppercased()==String(a[i]) ? "Đ" : "đ"
                    text=String(b);return
                }
            }
            if digit>=6 && digit<=8 {
                let v=Self.vowels(a)
                if digit==7 && v.count>=2 {
                    let i=v[v.count-2],j=v[v.count-1]
                    if j==i+1 && "uư".contains(Self.root(a[i])) &&
                        "oôơ".contains(Self.root(a[j])) {
                        var b=a
                        b[i]=Self.alter(a[i],shape:"ư")
                        b[j]=Self.alter(a[j],shape:"ơ")
                        text=Self.normalize(String(b));return
                    }
                }
                for i in v.reversed() {
                    let root=Self.root(a[i])
                    let shape:Character?
                    if digit==6 {
                        shape=root=="a" ? "â" : root=="e" ? "ê" : root=="o" ? "ô" : nil
                    } else if digit==7 {
                        shape=root=="u" ? "ư" : root=="o" ? "ơ" : nil
                    } else {shape=root=="a" ? "ă" : nil}
                    if let shape=shape {
                        var b=a;b[i]=Self.alter(a[i],shape:shape)
                        text=Self.normalize(String(b));return
                    }
                }
            }
            text.append(key);return
        }
        if let t=Self.tones.firstIndex(of:k), !Self.vowels(a).isEmpty {
            let current=a.map(Self.tone).last(where: { $0 > 0 }) ?? 0
            if t == 0 {
                if current != 0 { text=Self.setTone(text,0); return }
            } else {
                if current == t { literalize(Self.setTone(text,0)+String(key)) }
                else { text=Self.setTone(text,t) }
                return
            }
        }
        if k == "d" && !a.isEmpty {
            for i in a.indices {
                let lower=Character(String(a[i]).lowercased())
                if lower == "d" {
                    var b=a
                    b[i]=String(a[i]).uppercased() == String(a[i]) ? "Đ" : "đ"
                    text=String(b); return
                }
                if lower == "đ" {
                    var b=a; b[i]=String(a[i]).uppercased() == String(a[i]) ? "D" : "d"
                    literalize(String(b)+String(key)); return
                }
            }
        }
        let v=Self.vowels(a)
        if let shaped:Character = ["a":"â","e":"ê","o":"ô"][k] {
            for i in v.reversed() {
                let root=Self.root(a[i])
                if root == k {
                    var b=a; b[i]=Self.alter(a[i],shape:shaped); text=Self.normalize(String(b)); return
                }
                if root == shaped {
                    var b=a; b[i]=Self.alter(a[i],shape:k)
                    literalize(Self.normalize(String(b))+String(key)); return
                }
            }
        }
        if k == "w" {
            if v.count > 1 {
                for n in stride(from:v.count-1,through:1,by:-1) {
                    let i=v[n-1], j=v[n]
                    if j == i+1 && "uư".contains(Self.root(a[i])) && "oôơ".contains(Self.root(a[j])) {
                        let undo=Self.root(a[i]) == "ư" && Self.root(a[j]) == "ơ"
                        var b=a
                        b[i]=Self.alter(a[i],shape:undo ? "u" : "ư")
                        b[j]=Self.alter(a[j],shape:undo ? "o" : "ơ")
                        if undo { literalize(Self.normalize(String(b))+String(key)) }
                        else { text=Self.normalize(String(b)) }
                        return
                    }
                }
            }
            for i in v.reversed() {
                guard let at=Array("auoăươ").firstIndex(of:Self.root(a[i])) else { continue }
                var b=a; b[i]=Self.alter(a[i],shape:Array("ăươauo")[at])
                if at >= 3 { literalize(Self.normalize(String(b))+String(key)) }
                else { text=Self.normalize(String(b)) }
                return
            }
        }
        text=Self.normalize(text+String(key))
    }
    private static func foreignStart(_ raw:String) -> Bool {
        let word=raw.lowercased()
        guard word.count>=2 else { return false }
        let chars=Array(word)
        if "fjwz".contains(chars[0]) { return true }
        if chars[0]=="p" && chars[1] != "h" { return true }
        let pair=String(chars.prefix(2))
        let clusters:Set<String>=[
            "bl","br","cl","cr","dr","fl","fr","gl","gr","pl","pr",
            "sc","sk","sl","sm","sn","sp","st","sw","tw","wh","wr",
            "kn","ps","pt"
        ]
        return clusters.contains(pair)
    }
    private static func impossibleVietnamese(_ text:String) -> Bool {
        let word=Array(text.lowercased()).map(root)
        guard let first=word.firstIndex(where:{bases.contains($0)}) else {return false}
        let onset=String(word.prefix(first))
        let allowed:Set<String>=["","b","c","ch","d","đ","g","gh","h","k","kh",
                                  "l","m","n","ng","ngh","nh","p","ph","q","r",
                                  "s","t","th","tr","v","x"]
        if !allowed.contains(onset) {return true}
        var end=first
        while end<word.count && bases.contains(word[end]) {end+=1}
        let ending=Array(word.dropFirst(end))
        if ending.contains(where:{bases.contains($0)}) {return true}
        let codas:Set<String>=["","c","ch","m","n","ng","nh","p","t"]
        return !codas.contains(String(ending))
    }
    private static func immediateShapeKey(_ raw:String) -> Bool {
        let chars=Array(raw.lowercased())
        guard chars.count >= 2 else {return false}
        let last=chars[chars.count-1]
        return ("aeo".contains(last) && chars[chars.count-2] == last)
    }
    private static func immediateToneKey(_ raw:String,_ key:Character)->Bool {
        guard "sfrxj".contains(Character(String(key).lowercased())),
              !raw.isEmpty, !impossibleVietnamese(raw) else {return false}
        return raw.contains(where:{row($0) != nil})
    }
    private static func vowelAfterTone(_ raw:String,_ key:Character)->Bool {
        guard "aeiouy".contains(Character(String(key).lowercased())),
              let last=raw.last,raw.count>=2 else {return false}
        return "sfrxj".contains(Character(String(last).lowercased()))
    }
    func push(_ key: Character) -> String {
        if let pending=pendingEscapeRaw {
            let candidate=pending+String(key)
            pendingEscapeRaw=nil
            if smartEnglish && Self.isEnglishPrefix(candidate) &&
                !Self.preferVietnamesePrefix(candidate) {
                raw=candidate
                text=candidate
                literal=false
                foreignWord=false
                englishCandidate=true
                telexShaped=false
                lastTelexKey=false
                return text
            }
        }
        // The next copy of an effective Telex key is an escape command:
        // undo speculative accents and keep a literal token until boundary.
        let repeated=raw.last.map {String($0).lowercased()==String(key).lowercased()} ?? false
        // Very short tone sequences (as -> á, is -> í) can be protected
        // by the English prefix dictionary, so allow a repeat to escape
        // them even if no Telex transformation was displayed yet.
        let shortToneEscape=raw.count == 2 && repeated &&
            "sfrxj".contains(Character(String(key).lowercased())) &&
            "aeiouy".contains(Character(String(raw.first!).lowercased()))
        if repeated && !literal && (lastTelexKey || shortToneEscape) {
            pendingEscapeRaw=raw+String(key)
            text=raw
            literal=true
            englishCandidate=false
            foreignWord=false
            telexShaped=false
            lastTelexKey=false
            return text
        }
        let prior=text
        let toneNow = !useVni && !literal && !foreignWord &&
                      Self.immediateToneKey(raw,key)
        let withoutTone=String(raw.dropLast())+String(key)
        let afterTone = !useVni && !literal && lastToneApplied &&
                        Self.vowelAfterTone(raw,key) &&
                        !Self.looksVietnamese(withoutTone)
        raw.append(key)
        lastTelexKey=false
        lastToneApplied=false
        if afterTone {
            literal=true
            foreignWord=false
            englishCandidate=false
            telexShaped=false
            pendingEscapeRaw=nil
            text=raw
            return text
        }
        let shapeNow = !useVni && Self.immediateShapeKey(raw)
        if smartEnglish && !useVni && !literal && !Self.preferVietnamesePrefix(raw) {
            if foreignWord || Self.foreignStart(raw) {
                foreignWord=true
                englishCandidate=false
                telexShaped=false
                text=raw
                return text
            }
            // A valid Vietnamese syllable wins after the user explicitly
            // enters a Telex shaping key, even if English has the same spelling.
            if !shapeNow && !toneNow &&
               (!telexShaped ||
                (Self.english.contains(raw.lowercased()) &&
                 (preferEnglishExact || !Self.looksVietnamese(text)))) &&
               Self.isEnglishPrefix(raw) {
                englishCandidate=true
                telexShaped=false
                text=raw
                return text
            }
        }
        if englishCandidate {
            englishCandidate=false
            let old=raw
            text=""; literal=false
            for c in old { core(c) }
        } else {core(key)}
        lastTelexKey=(text != prior+String(key)) && !literal
        if (shapeNow || toneNow) && raw != text {telexShaped=true}
        lastToneApplied=toneNow && raw != text
        if smartEnglish && !useVni && !literal && !Self.preferVietnamesePrefix(raw) &&
           raw != text && Self.impossibleVietnamese(text) {
            foreignWord=true
            telexShaped=false
            text=raw
        }
        return text
    }
    func finish() -> String {
        if let pending=pendingEscapeRaw,smartEnglish,
            Self.english.contains(pending.lowercased()),!Self.vietnamese.contains(pending.lowercased()) {
            return pending
        }
        if smartEnglish && !useVni && !literal &&
            (foreignWord || (Self.english.contains(raw.lowercased()) &&
                             !(telexShaped && Self.looksVietnamese(text) && !preferEnglishExact))) &&
            !Self.vietnamese.contains(raw.lowercased()) { return raw }
        if englishCandidate {
            let replay=TelexComposer()
            replay.smartEnglish=false; replay.useVni=useVni
            replay.preferEnglishExact=preferEnglishExact; replay.restoreInvalid=restoreInvalid
            for c in raw { _ = replay.push(c) }
            return replay.finish()
        }
        if !restoreInvalid || literal || raw == text || raw.isEmpty { return text }
        if !raw.utf8.allSatisfy({ $0 < 128 }) { return text }
        return Self.looksVietnamese(text) ? text : raw
    }
    private static func looksVietnamese(_ s: String) -> Bool {
        let a=Array(s.lowercased()).map(root)
        guard let first=a.firstIndex(where: { bases.contains($0) }) else { return false }
        let onset=String(a.prefix(first))
        let allowedOnsets:Set<String>=["","b","c","ch","d","đ","g","gh","h","k","kh","l","m","n","ng","ngh","nh","p","ph","q","r","s","t","th","tr","v","x"]
        guard allowedOnsets.contains(onset) else { return false }
        var end=first
        while end<a.count && bases.contains(a[end]) { end+=1 }
        let ending=String(a.dropFirst(end))
        guard ["","c","ch","m","n","ng","nh","p","t"].contains(ending) else { return false }
        var cluster=String(a[first..<end])
        if onset=="q" {
            if !cluster.hasPrefix("u") || cluster.count<2 { return false }
            cluster.removeFirst()
        }
        if onset=="g" && cluster.hasPrefix("i") && cluster.count>1 { cluster.removeFirst() }
        let allowed:Set<String>=Set("a ă â e ê i o ô ơ u ư y ai ao au ay âu ây eo êu ia iê iu oa oă oe oi oo ôô ôi ơi ua uă uâ uê ui uo uô uơ uy ưa ưi ưu ươ yê iêu oai oao oay oeo uao uây uôi uya uyê uyu ươi ươu yêu uyêu".split(separator:" ").map(String.init))
        return allowed.contains(cluster)
    }
}
