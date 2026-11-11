import Foundation

@main struct TelexMiniTests {
    static func main() {
        let vietnamese:[(String,String)] = [
            ("tieengs","tiếng"), ("vieetj","việt"), ("truowngf","trường"),
            ("nguowif","người"), ("toasn","toán"), ("ddawng","đăng"),
            ("caamr","cẩm"), ("duongdwf","đường"), ("khoer","khoẻ"),
            ("tieesng","tiếng"), ("uow","ươ"), ("esscape","escape")
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
        let e=TelexComposer();for c in "tieengs" { _=e.push(c) }
        e.backspace();_=e.push("g")
        if e.text=="tiếng" {passed+=1} else {failed+=1;print("FAIL backspace: \(e.text)")}
        print("Passed \(passed), failed \(failed), vocabulary \(TelexComposer.english.count)")
        if failed != 0 { exit(1) }
    }
}
