using System;
using System.Collections.Generic;
using System.Text;

namespace TelexMini
{
    // Pure text engine; no keyboard, filesystem or network access.
    public sealed class TelexEngine
    {
        const string Bases = "aăâeêioôơuưy";
        static readonly string[] Rows = {
            "aáàảãạ", "ăắằẳẵặ", "âấầẩẫậ", "eéèẻẽẹ", "êếềểễệ", "iíìỉĩị",
            "oóòỏõọ", "ôốồổỗộ", "ơớờởỡợ", "uúùủũụ", "ưứừửữự", "yýỳỷỹỵ"
        };
        public string Text { get; private set; }
        // Off by default for engine-only use; enabled in the keyboard application.
        public bool SmartEnglish { get; set; }
        bool literalWord, englishCandidate, foreignWord;
        string rawWord="";
        public TelexEngine() { Reset(); }
        public void Reset() { Text = ""; literalWord = false; englishCandidate=false; foreignWord=false; rawWord=""; }
        public void Backspace() {
            if (Text.Length > 0) Text = Text.Substring(0, Text.Length - 1);
            rawWord=Text; // Never restore deleted characters after an edit.
            englishCandidate=SmartEnglish && EnglishLexicon.HasPrefix(rawWord);
            foreignWord=SmartEnglish && HasForeignStart(rawWord);
            if (Text.Length == 0) literalWord = false;
        }
        string Literal(string text) { literalWord = true; return Text = text; }
        static int Row(char c) { return Bases.IndexOf(Base(c)); }
        static char Base(char c)
        {
            c = char.ToLowerInvariant(c);
            foreach (string row in Rows) if (row.IndexOf(c) >= 0) return row[0];
            return c;
        }
        static int Tone(char c)
        {
            c = char.ToLowerInvariant(c);
            foreach (string row in Rows) { int t = row.IndexOf(c); if (t >= 0) return t; }
            return 0;
        }
        static char Case(char value, char original) { return char.IsUpper(original) ? char.ToUpperInvariant(value) : value; }
        static char Mark(char c, int tone)
        {
            int r = Row(c);
            return r < 0 ? c : Case(Rows[r][tone], c);
        }
        static char Shape(char c, char shape)
        {
            int r = Bases.IndexOf(shape);
            return Case(Rows[r][Tone(c)], c);
        }
        static List<int> Vowels(string text)
        {
            var v = new List<int>();
            for (int i = 0; i < text.Length; i++) if (Row(text[i]) >= 0) v.Add(i);
            // The u in qu and i in gi are consonant components when followed by a vowel.
            if (v.Count > 1 && v[0] == 1 &&
                ((char.ToLowerInvariant(text[0]) == 'q' && Base(text[1]) == 'u') ||
                 (char.ToLowerInvariant(text[0]) == 'g' && Base(text[1]) == 'i'))) v.RemoveAt(0);
            return v;
        }
        static int TonePosition(string text)
        {
            List<int> v = Vowels(text);
            if (v.Count == 0) return -1;
            // Prefer the main vowel in shaped diphthongs (ươ, uô, iê, yê...).
            for (int i = v.Count - 1; i >= 0; i--)
                if ("ăâêôơ".IndexOf(Base(text[v[i]])) >= 0) return v[i];
            for (int i = v.Count - 1; i >= 0; i--)
                if (Base(text[v[i]]) == 'ư') return v[i];
            if (v.Count == 1) return v[0];
            if (v.Count >= 3) return v[v.Count - 2];
            string pair = "" + Base(text[v[0]]) + Base(text[v[1]]);
            bool closed = v[v.Count - 1] < text.Length - 1;
            return closed || pair == "oa" || pair == "oe" || pair == "uy" ? v[1] : v[0];
        }
        static string SetTone(string text, int tone)
        {
            int target = TonePosition(text);
            char[] chars = text.ToCharArray();
            for (int i = 0; i < chars.Length; i++) if (Row(chars[i]) >= 0) chars[i] = Mark(chars[i], i == target ? tone : 0);
            return new string(chars);
        }
        static string Normalize(string text)
        {
            int tone = 0;
            foreach (char c in text) if (Tone(c) != 0) tone = Tone(c);
            return SetTone(text, tone);
        }
        // Strong foreign onsets do not occur in native Vietnamese syllables.
        // Keep them verbatim without waiting for the end of a word. "ph", "tr",
        // "ch", "ng" etc. still follow normal Vietnamese Telex processing.
        static bool HasForeignStart(string raw)
        {
            if (raw.Length < 2) return false;
            string word=raw.ToLowerInvariant();
            char first=word[0];
            if (first=='f' || first=='j' || first=='w' || first=='z') return true;
            if (first=='p' && word[1]!='h') return true;
            const string onsets="|bl|br|cl|cr|dr|fl|fr|gl|gr|pl|pr|sc|sk|sl|sm|sn|sp|st|sw|tw|wh|wr|kn|ps|pt|";
            if (onsets.IndexOf("|"+word.Substring(0,2)+"|",StringComparison.Ordinal)>=0) return true;
            return false;
        }
        static bool ImpossibleVietnamese(string text)
        {
            var plain=new StringBuilder();
            foreach(char c in text) plain.Append(Base(c));
            string word=plain.ToString().ToLowerInvariant();
            int first=0;
            while(first<word.Length && Bases.IndexOf(word[first])<0) first++;
            if(first==word.Length) return false;
            string onset=word.Substring(0,first);
            const string onsets="||b|c|ch|d|đ|g|gh|h|k|kh|l|m|n|ng|ngh|nh|p|ph|q|r|s|t|th|tr|v|x|";
            if(onsets.IndexOf("|"+onset+"|",StringComparison.Ordinal)<0) return true;
            int end=first;
            while(end<word.Length && Bases.IndexOf(word[end])>=0) end++;
            string ending=word.Substring(end);
            // A second vowel after an intervening consonant makes this a
            // multi-syllable English token rather than a single Telex syllable.
            foreach(char c in ending) if(Bases.IndexOf(c)>=0) return true;
            // No Vietnamese coda ends in rs, st, nd, ck, etc.
            const string codas="||c|ch|m|n|ng|nh|p|t|";
            return codas.IndexOf("|"+ending+"|",StringComparison.Ordinal)<0;
        }
        public string Push(char key)
        {
            rawWord+=key;
            if (SmartEnglish && !literalWord && !EnglishLexicon.PreferVietnamesePrefix(rawWord)) {
                if (foreignWord || HasForeignStart(rawWord)) {
                    foreignWord=true;
                    englishCandidate=false;
                    return Text=rawWord;
                }
                if (EnglishLexicon.HasPrefix(rawWord)) {
                    englishCandidate=true;
                    return Text=rawWord;
                }
            }
            if (englishCandidate) {
                englishCandidate=false;
                string raw=rawWord;
                Text=""; literalWord=false;
                foreach(char c in raw) PushCore(c);
            } else PushCore(key);
            // If a transformed token cannot be a Vietnamese syllable any more,
            // undo the speculative accent immediately and keep subsequent keys
            // literal. No dictionary lookup or network request is required.
            if (SmartEnglish && !literalWord && !EnglishLexicon.PreferVietnamesePrefix(rawWord) &&
                rawWord!=Text && ImpossibleVietnamese(Text)) {
                foreignWord=true;
                return Text=rawWord;
            }
            return Text;
        }
        // Conservative syllable check + optional offline English recognition.
        public string Finish(bool restoreInvalid)
        {
            if (SmartEnglish && !literalWord && (foreignWord || EnglishLexicon.IsWord(rawWord))) return rawWord;
            if (englishCandidate) {
                // A protected English prefix did not finish as a recognized word.
                var replay=new TelexEngine();
                foreach(char c in rawWord) replay.Push(c);
                return replay.Finish(restoreInvalid);
            }
            if(!restoreInvalid || literalWord || rawWord==Text || rawWord.Length==0) return Text;
            foreach(char c in rawWord) if(c>127) return Text;
            return LooksVietnamese(Text) ? Text : rawWord;
        }
        static bool LooksVietnamese(string text)
        {
            var plain=new StringBuilder();
            foreach(char c in text) plain.Append(Base(c));
            string word=plain.ToString();
            int first=0; while(first<word.Length && Bases.IndexOf(word[first])<0) first++;
            if(first==word.Length) return false;
            string onset=word.Substring(0,first);
            if(("|b|c|ch|d|đ|g|gh|h|k|kh|l|m|n|ng|ngh|nh|p|ph|q|r|s|t|th|tr|v|x|").IndexOf("|"+onset+"|",StringComparison.Ordinal)<0 && onset.Length>0) return false;
            int end=first; while(end<word.Length && Bases.IndexOf(word[end])>=0) end++;
            string ending=word.Substring(end);
            if(ending.Length>0 && "|c|ch|m|n|ng|nh|p|t|".IndexOf("|"+ending+"|",StringComparison.Ordinal)<0) return false;
            string vowels=word.Substring(first,end-first);
            if(onset=="q") { if(!vowels.StartsWith("u") || vowels.Length<2) return false; vowels=vowels.Substring(1); }
            if(onset=="g" && vowels.StartsWith("i") && vowels.Length>1) vowels=vowels.Substring(1);
            const string clusters="|a|ă|â|e|ê|i|o|ô|ơ|u|ư|y|ai|ao|au|ay|âu|ây|eo|êu|ia|iê|iu|oa|oă|oe|oi|oo|ôô|ôi|ơi|ua|uă|uâ|uê|ui|uo|uô|uơ|uy|ưa|ưi|ưu|ươ|yê|iêu|oai|oao|oay|oeo|uao|uây|uôi|uya|uyê|uyu|ươi|ươu|yêu|uyêu|";
            return clusters.IndexOf("|"+vowels+"|",StringComparison.Ordinal)>=0;
        }
        string PushCore(char key)
        {
            char k = char.ToLowerInvariant(key);
            // A long token is probably a URL/identifier: pass through without deleting its prefix.
            if (Text.Length >= 64) literalWord=true;
            if (literalWord) return Text += key;
            int tone = "zsfrxj".IndexOf(k); // z=0,s=1,f=2,r=3,x=4,j=5
            if (tone >= 0 && Vowels(Text).Count > 0)
            {
                int current = 0;
                foreach (char c in Text) if (Tone(c) != 0) current = Tone(c);
                if (tone == 0)
                {
                    if (current != 0) return Text = SetTone(Text, 0);
                }
                else
                {
                    // Repeated tone key escapes to literal text: ass -> as.
                    return current == tone ? Literal(SetTone(Text, 0) + key) : Text = SetTone(Text, tone);
                }
            }
            char[] a = Text.ToCharArray();
            if (k == 'd' && a.Length > 0)
            {
                // dd can be typed after the vowels, as in duongd.
                for (int i = 0; i < a.Length; i++)
                {
                    if (char.ToLowerInvariant(a[i]) == 'd') { a[i] = Case('đ', a[i]); return Text = new string(a); }
                    if (char.ToLowerInvariant(a[i]) == 'đ') { a[i] = Case('d', a[i]); return Literal(new string(a) + key); }
                }
            }
            List<int> vowels = Vowels(Text);
            if (k == 'a' || k == 'e' || k == 'o')
            {
                char shaped = k == 'a' ? 'â' : k == 'e' ? 'ê' : 'ô';
                for (int n = vowels.Count - 1; n >= 0; n--)
                {
                    int i = vowels[n]; char b = Base(a[i]);
                    if (b == k) { a[i] = Shape(a[i], shaped); return Text = Normalize(new string(a)); }
                    if (b == shaped) { a[i] = Shape(a[i], k); return Literal(Normalize(new string(a)) + key); }
                }
            }
            if (k == 'w' && vowels.Count > 0)
            {
                // uo / uô / ưo -> ươ, including a trailing consonant.
                for (int n = vowels.Count - 1; n > 0; n--)
                {
                    int i = vowels[n - 1], j = vowels[n];
                    if (j == i + 1 && "uư".IndexOf(Base(a[i])) >= 0 && "oôơ".IndexOf(Base(a[j])) >= 0)
                    {
                        bool undo = Base(a[i]) == 'ư' && Base(a[j]) == 'ơ';
                        a[i] = Shape(a[i], undo ? 'u' : 'ư'); a[j] = Shape(a[j], undo ? 'o' : 'ơ');
                        return undo ? Literal(Normalize(new string(a)) + key) : Text = Normalize(new string(a));
                    }
                }
                for (int n = vowels.Count - 1; n >= 0; n--)
                {
                    int i = vowels[n]; char b = Base(a[i]);
                    int index = "auoăươ".IndexOf(b);
                    if (index < 0) continue;
                    a[i] = Shape(a[i], "ăươauo"[index]);
                    return index >= 3 ? Literal(Normalize(new string(a)) + key) : Text = Normalize(new string(a));
                }
            }
            return Text = Normalize(Text + key);
        }
    }
}
