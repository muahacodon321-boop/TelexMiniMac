import Foundation

@main struct TelexMiniTests {
    static func main() {
        let vietnamese:[(String,String)] = [
            ("tieengs","tiếng"), ("vieetj","việt"), ("truowngf","trường"),
            ("nguowif","người"), ("toasn","toán"), ("ddawng","đăng"),
            ("caamr","cẩm"), ("duongdwf","đường"), ("khoer","khoẻ"),
            ("tieesng","tiếng"), ("uow","ươ"), ("esscape","escape"),
            ("chaof","chào"),("duwowngf","dường"),("hoaf","hoà"),
            ("thuys","thuý"),("quas","quá"),("quys","quý"),
            ("gias","giá"),("gif","gì"),("muoons","muốn"),
            ("tuyeenr","tuyển"),("nguyeenx","nguyễn"),
            ("huowsng","hướng"),("ruouwj","rượu"),
            ("khuyur","khuỷu"),("cuwus","cứu"),("yeeus","yếu"),
            ("quoocs","quốc"),("duongdwf","đường"),
            ("ddoojng","động"),("cawsn","cắn"),
            ("tuwj","tự"),("nhowx","nhỡ"),("vieecj","việc"),
            ("phowr","phở"),("phuws","phứ"),("trawsng","trắng"),
            ("chayj","chạy"),("chajy","chạy"),("chay","chay"),
            ("kieeur","kiểu"),("kieeru","kiểu"),
            ("nhieeuf","nhiều"),("nhieefu","nhiều"),
            ("vaayj","vậy"),("vaajy","vậy"),
            ("yeeus","yếu"),("yeesu","yếu"),
            ("nguowif","người"),("nguowfi","người"),
            ("muoons","muốn"),("muoosn","muốn"),
            ("tuyeenr","tuyển"),("tuyeern","tuyển"),
            ("vieecj","việc"),("vieejc","việc"),
            ("duongdwf","đường"),("duongdfw","đường"),
            ("ngoais","ngoái"),("ngoasi","ngoái"),
            ("tieengs","tiếng"),("tieesng","tiếng"),
            ("cha","cha")
        ]
        let english=["office","assets","testing","framework","plugin",
                     "password","dashboard","release","browser","github","script","hello",
                     "parameter","parameters","parameterized","parameterization",
                     "parametric","parametrize","params","parentheses","parallel","parser"]
        var failed=0, passed=0
        for (raw,want) in vietnamese {
            let e=TelexComposer();e.smartEnglish=true;e.restoreInvalid=true
            for c in raw { _=e.push(c) }
            if e.text==want { passed+=1 } else { failed+=1; print("FAIL Vietnamese: \(raw) => \(e.text), expected \(want)") }
        }
        for raw in english {
            let e=TelexComposer();e.smartEnglish=true;e.restoreInvalid=true
            var prefix=""
            for c in raw {
                prefix.append(c)
                let actual=e.push(c)
                // A repeated Telex key is an immediate escape. It may
                // temporarily omit the duplicate at "off"/"ass" until the
                // next key proves the user meant "office"/"assets".
                let overrides:[String:String]=[
                    "of":"ò","as":"á","tes":"té","test":"tét",
                    "das":"dá","off":"of","ass":"as"
                ]
                let expectedPrefix=overrides[prefix] ?? prefix
                if actual != expectedPrefix {
                    failed+=1
                    print("FAIL English in-progress: \(raw) at \(prefix) => \(actual), expected \(expectedPrefix)")
                } else { passed+=1 }
            }
            if e.text==raw && e.finish()==raw { passed+=1 }
            else { failed+=1; print("FAIL English: \(raw) => \(e.text) / \(e.finish())") }
        }
        // Unlisted English words test the algorithm, not the dictionary.
        // A temporary Telex interpretation is allowed only while a prefix
        // remains ambiguous; the fully typed unknown word must be literal.
        let unknownEnglish=[
            "paradoxical","paramagnets","privatization","fluctuation",
            "cryptanalysis","synchronization","customizable","cursorial",
            "verifiability","transactional","gradients","kernelization",
            "robustness","photorealistic","subroutines","whimsical",
            "microarchitecture","hyperparameterization","reparameterizing",
            "asynchronousness","combinatorialism","cryptomining",
            "dataclassification","encapsulationist","serializationist",
            "decentralizationist","typographically","unpredictability",
            "metaprogramming","cryptographically","infrastructural",
            "managerialism","keyboardless","firmwareless"
        ]
        for raw in unknownEnglish {
            if TelexComposer.english.contains(raw) {
                failed+=1; print("FAIL unlisted-word fixture is in dictionary: \(raw)")
                continue
            }
            let e=TelexComposer()
            for c in raw { _=e.push(c) }
            if e.text==raw && e.finish()==raw {passed+=1}
            else {failed+=1;print("FAIL unknown English: \(raw) => \(e.text) / \(e.finish())")}
        }
        // Metamorphic coverage: for every tone key, moving it before the
        // final vowel of a legitimate diphthong must yield the same Unicode.
        // Covers 30 combinations beyond the manually curated word fixtures.
        for (stem,vowel) in [("kiee","u"),("nhiee","u"),
                             ("vaa","y"),("cha","y"),
                             ("nguow","i"),("yee","u")] {
            for tone in "sfrxj" {
                let before=stem+String(tone)+vowel
                let after=stem+vowel+String(tone)
                let first=TelexComposer(),second=TelexComposer()
                for c in before {_ = first.push(c)}
                for c in after {_ = second.push(c)}
                let wanted=second.finish()
                if first.text==wanted && first.finish()==wanted && wanted != before {
                    passed+=1
                } else {
                    failed+=1;print("FAIL tone placement \(before) => \(first.text), after \(after) => \(wanted)")
                }
            }
        }
        // Accent keys must be applied immediately, even if an English word
        // such as "book" shares the same prefix. An exact English word can
        // recover later without delaying Vietnamese composition.
        let immediate:[(String,String)] = [
            ("as","á"),("ex","ẽ"),("exe","exe"),("ee","ê"),("eex","ễ"),
            ("ass","as"),("boosst","boost"),
            ("b","b"),("bo","bo"),("boo","bô"),("boon","bôn"),
            ("boons","bốn"),("aa","â"),("ee","ê"),("oo","ô"),
            ("tiee","tiê"),("tieen","tiên"),("tieengs","tiếng")
        ]
        for (raw,expected) in immediate {
            let e=TelexComposer()
            for c in raw { _=e.push(c) }
            if e.text==expected {passed+=1}
            else {failed+=1;print("FAIL instant Telex \(raw) => \(e.text), expected \(expected)")}
        }
        for (raw,expected) in [("book","book"),("books","books"),("good","good"),("boost","bốt")] {
            let e=TelexComposer()
            for c in raw { _=e.push(c) }
            if e.text==expected && e.finish()==expected {passed+=1}
            else {failed+=1;print("FAIL English after immediate shape: \(raw) => \(e.text) / \(e.finish())")}
        }
        // Regression for tone position: a raw vowel + tone + vowel must
        // not let the final vowel re-shape an already toned vowel.
        for (raw,expected) in [
            ("as","á"),("ass","as"),("ex","ẽ"),("exe","exe"),
            ("ee","ê"),("eex","ễ"),("as","á"),("exe","exe")
        ] {
            let e=TelexComposer()
            for c in raw { _=e.push(c) }
            if e.text==expected && e.finish()==expected {passed+=1}
            else {failed+=1;print("FAIL immediate tone and placement \(raw): \(e.text) / \(e.finish()), expected \(expected)")}
        }
        // Vietnamese mode wins when a complete Telex syllable conflicts
        // with a dictionary English word (been -> bên).
        for (raw,expected) in [
            ("bee","bê"),("been","bên"),("beens","bến"),
            ("teen","tên"),("seen","sên"),("boost","bốt")
        ] {
            let e=TelexComposer()
            for c in raw { _=e.push(c) }
            if e.text==expected && e.finish()==expected { passed+=1 }
            else { failed+=1;print("FAIL Vietnamese priority: \(raw) => \(e.text) / \(e.finish()), expected \(expected)") }
        }
        // UniKey-style repeat: a second copy of the most recent active
        // Telex key cancels the conversion and keeps the word literal.
        let escapes:[(String,String)] = [
            ("booss","boos"), ("boosst","boost"),("boosster","booster"),
            ("guitarr","guitar"),("ass","as"),("aaa","aa"),
            ("eee","ee"),("ooo","oo"),("ddd","dd"),
            ("aass","aas"),("uoww","uow"),
            ("boons","bốn"),("been","bên")
        ]
        for (raw,want) in escapes {
            let e=TelexComposer()
            for c in raw { _=e.push(c) }
            if e.text==want && e.finish()==want {passed+=1}
            else {failed+=1;print("FAIL Telex repeat escape \(raw) => \(e.text) / \(e.finish()), expected \(want)")}
        }
        do {
            let en=TelexComposer();en.preferEnglishExact=true
            for c in "been" {_ = en.push(c)}
            if en.text=="been" && en.finish()=="been" {passed+=1}
            else {failed+=1;print("FAIL English-first collision")}
            let vi=TelexComposer();vi.smartEnglish=false
            for c in "been" {_ = vi.push(c)}
            if vi.text=="bên" {passed+=1}
            else {failed+=1;print("FAIL Vietnamese-first collision")}
        }
        let vni:[(String,String)]=[
            ("bo6n1","bốn"),("tie6ng1","tiếng"),("da9ng8","đăng"),
            ("ma1","má"),("mo7","mơ"),("mu7","mư"),("uo7","ươ"),
            ("abc123","abc123")
        ]
        for (raw,want) in vni {
            let e=TelexComposer();e.useVni=true
            for c in raw { _=e.push(c) }
            if e.text==want && e.finish()==want {passed+=1}
            else {failed+=1; print("FAIL VNI \(raw) => \(e.text) / \(e.finish()), wanted \(want)")}
        }
        let e=TelexComposer();for c in "tieengs" { _=e.push(c) }
        e.backspace();_=e.push("g")
        if e.text=="tiếng" {passed+=1} else {failed+=1;print("FAIL backspace: \(e.text)")}
        // Deterministic stress: random words, editing and mode switches.
        // Keep only in-memory buffers; never record the user's text.
        var seed:UInt32=0x47A2B199
        for useVni in [false,true] {
            for _ in 0..<1500 {
                let e=TelexComposer();e.useVni=useVni
                let alphabet=Array(useVni ? "abcdefghijklmnopqrstuvxyz123456789" :
                                            "abcdefghijklmnopqrstuvxyz")
                var valid=true
                for _ in 0..<24 {
                    seed=seed &* 1664525 &+ 1013904223
                    let key=alphabet[Int(seed % UInt32(alphabet.count))]
                    _=e.push(key)
                    if e.text.count>e.raw.count {valid=false}
                    if seed & 31==0 {e.backspace()}
                }
                if e.finish().count>e.raw.count {valid=false}
                if valid {passed+=1} else {failed+=1;print("FAIL randomized composition invariant")}
            }
        }
        print("Passed \(passed), failed \(failed), vocabulary \(TelexComposer.english.count)")
        if failed != 0 { exit(1) }
    }
}
