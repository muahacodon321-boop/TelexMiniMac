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
            ("phowr","phở"),("phuws","phứ"),("trawsng","trắng")
        ]
        let english=["as","office","assets","testing","framework","plugin",
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
                if actual != prefix {
                    failed+=1
                    print("FAIL English in-progress: \(raw) at \(prefix) => \(actual)")
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
        // Accent keys must be applied immediately, even if an English word
        // such as "book" shares the same prefix. An exact English word can
        // recover later without delaying Vietnamese composition.
        let immediate:[(String,String)] = [
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
        let e=TelexComposer();for c in "tieengs" { _=e.push(c) }
        e.backspace();_=e.push("g")
        if e.text=="tiếng" {passed+=1} else {failed+=1;print("FAIL backspace: \(e.text)")}
        print("Passed \(passed), failed \(failed), vocabulary \(TelexComposer.english.count)")
        if failed != 0 { exit(1) }
    }
}
