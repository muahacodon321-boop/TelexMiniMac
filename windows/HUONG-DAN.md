# Telex Mini

Bộ gõ Telex nhỏ cho Windows, viết bằng C# / Windows Forms. Chạy trực tiếp `TelexMini.exe`, không cần trình cài đặt. Dùng .NET Framework 4.x có sẵn trên Windows 10/11 thông thường.

## Dùng ngay

1. Tắt bộ gõ khác như UniKey/EVKey nếu đang bật.
2. Nhấn **Win + Space**, chọn **English (United States) — US / ENG** của Windows. Không chọn Vietnamese Telex. Nếu chưa có ENG, thêm bàn phím English (United States) trong Settings → Time & language → Language & region.
3. Mở `TelexMini.exe`. Nút **V · TIẾNG VIỆT** là đang bật Telex.
4. Bấm nút V/E, bấm trái biểu tượng ở khay hệ thống, hoặc nhấn **Ctrl + Shift + Space** để chuyển Việt/Anh. Chuyển ở giữa một từ sẽ kết thúc bộ đệm của từ đó.
5. Nút **Thu xuống khay** hoặc nút dấu trừ ẩn cửa sổ, giữ biểu tượng **V/E ở khay hệ thống cạnh ENG và đồng hồ**. Bấm trái V/E để chuyển Anh/Việt; bấm phải → **Hiện nút chuyển** để mở lại cửa sổ. Windows có thể đặt biểu tượng trong dấu mũ ^; kéo V/E từ đó ra cạnh đồng hồ để luôn nhìn thấy. Nút X và nút Thoát dừng hẳn bộ gõ.

Cửa sổ nút chuyển luôn nổi và không lấy tiêu điểm khi bấm, để bạn tiếp tục gõ ở ứng dụng đang dùng. Nếu phím tắt đã bị ứng dụng khác chiếm, giao diện sẽ báo và các nút chuyển vẫn dùng được.

## Cài đặt nâng cao (TelexMini v3)

Chọn **Cài đặt nâng cao** tại sidebar hoặc menu biểu tượng khay để chỉnh:
- **Telex/VNI**: mặc định là Telex; VNI dùng số 1–5 cho dấu thanh, 6 cho â/ê/ô, 7 cho ư/ơ, 8 cho ă, 9 cho đ.
- **Ưu tiên ngôn ngữ**: Cân bằng (mặc định, kết quả Telex hợp lệ ưu tiên), tiếng Việt (giảm bảo vệ English), hoặc tiếng Anh (giữ từ Anh có trong từ điển khi mơ hồ).
- **Phím chuyển V/E**: Ctrl+Shift+Space, Ctrl+Alt+V hoặc Alt+Shift+V (có thể xung đột với phần mềm khác).
- **Tự khởi động cùng Windows**: lưu ở HKCU Run, không yêu cầu quản trị.
- **Ứng dụng bỏ qua**: nhập tên tiến trình như `code.exe, chrome.exe`, không dùng đường dẫn hay ký tự đại diện. Khi ứng dụng bỏ qua ở foreground, TelexMini nhường phím.
- **Nhập/Xuất**: sao lưu/khôi phục `preferences.ini`, `EnglishWords.txt` và `VietnameseOverrides.txt` vào thư mục riêng, không chạm dữ liệu gõ.

Lưu ý: danh sách ứng dụng được so khớp chính xác, không phải tìm chuỗi con. Không lưu mật khẩu hoặc lịch sử bàn phím. VNI sẽ không áp dụng dấu khi tiền tố rõ ràng không phải âm tiết tiếng Việt, chẳng hạn `abc123`.

## Telex

| Gõ | Kết quả |
|---|---|
| aa / aw | â / ă |
| ee | ê |
| oo / ow | ô / ơ |
| uw / uow | ư / ươ |
| dd | đ |
| s / f / r / x / j | sắc / huyền / hỏi / ngã / nặng |
| z | bỏ dấu thanh |

Ví dụ: `tieengs vieetj` → **tiếng việt**, `truowngf` → **trường**, `nguowif` → **người**.

Có thể gõ dấu thanh trước hoặc sau phụ âm cuối. Hỗ trợ chữ hoa, Backspace, đặt lại dấu khi thêm chữ và bỏ `u` trong `qu` / `i` trong `gi` khi xác định nguyên âm chính. Dùng cách đặt dấu **hoà, khoẻ, thuý**.

Nhấn lặp phím biến đổi để lấy ký tự thường: `aaa` → `aa`, `ass` → `as`, `ddd` → `dd`, `aww` → `aw`. **Sau thao tác này, phần còn lại của từ được giữ nguyên như tiếng Anh**, không tiếp tục xử lý dấu Telex.

Ví dụ: gõ `ess` được `es`, gõ tiếp `cape` được **escape** (toàn bộ phím: `esscape`). Tương tự `offfice` → **office**, `asssets` → **assets**. Sau dấu cách hoặc dấu câu, Telex tự hoạt động lại: `esscape tieengs vieetj` → **escape tiếng việt**. Backspace trong từ vẫn giữ chế độ này; xoá hết từ sẽ bật lại Telex. Di chuyển con trỏ, bấm chuột hoặc đổi V/E cũng kết thúc bộ đệm hiện tại.

Ngoài quy tắc gõ lặp, tuỳ chọn **Khôi phục từ không hợp lệ** kiểm tra cấu trúc âm tiết khi nhấn dấu cách, dấu câu, Tab hoặc Enter. Nếu kết quả Telex không hợp lệ, bộ gõ trả về các phím gốc. Ví dụ gõ `escape` rồi dấu cách → **escape**. Không dùng từ điển hay AI; không đoán đúng mọi từ Anh–Việt. Khi **tắt** bảo vệ tiếng Anh, `as` vẫn thành **á**, và `offfice` vẫn là cách chủ động gõ **office**. Có thể bỏ chọn tuỳ chọn này khi gõ tên riêng hoặc phiên âm đặc biệt.

Với cả câu/đoạn tiếng Anh, dùng **Ctrl + Shift + Space** để chuyển sang E.

## Tự dùng tiếng Anh theo ô nhập

Tuỳ chọn **Tự dùng English trong terminal** mặc định **tắt** để tiết kiệm tài nguyên. Bảo vệ ô mật khẩu hoạt động riêng và luôn bật khi V đang được chọn. Khi bật:

- Vào ứng dụng terminal được nhận diện → **E · TỰ ĐỘNG**.
- Vào ô mật khẩu được Windows/UI Automation báo là password → **E · MẬT KHẨU** (luôn áp dụng, dù không bật tùy chọn terminal).
- Rời các vị trí đó → trở lại chế độ bạn chọn trước đó. Nếu bạn đã chọn E bằng tay thì vẫn giữ E.
- Khi đang tự động E, bấm nút E hoặc phím tắt sẽ chọn E cho mọi nơi. Bấm lại để chọn V cho những ô nhập thông thường.

Danh sách terminal: Windows Terminal, Command Prompt, PowerShell/pwsh, conhost/OpenConsole, WSL/bash, mintty (Git Bash), PuTTY/KiTTY, WezTerm, Alacritty và Hyper. Đây là nhận diện ứng dụng đang ở trước; **terminal nhúng trong VS Code hoặc ứng dụng khác chưa được bảo đảm nhận diện**. Có thể dùng phím tắt chuyển E thủ công trong đó.

Nhận diện mật khẩu chỉ đọc cờ `IsPassword` hoặc thuộc tính kiểu ô nhập Windows; không đọc ValuePattern, TextPattern, giá trị, nhãn hoặc nội dung mật khẩu. Worker chạy riêng, không gọi UI Automation từ callback bàn phím. Khi bật, nó nhận sự kiện đổi tiêu điểm và kiểm tra dự phòng mỗi 750 ms. Khi vừa đổi tiêu điểm hoặc chưa nhận diện được, bộ gõ tạm giữ E; thời gian cập nhật phụ thuộc ứng dụng. Gõ ngay trong khoảng chuyển tiếp có thể ra chữ không dấu. Khi tắt tự dùng terminal, worker vẫn chạy để nhận diện mật khẩu nếu chế độ V được chọn. Khi chọn E thủ công hoặc tạm dừng hoàn toàn, worker ngủ và huỷ đăng ký sự kiện UI Automation; truy vấn đã bắt đầu trước lúc tắt có thể cần thời gian để trả về.

## Sửa lỗi gõ từ tiếng Anh như parameter

Bộ từ điển offline có thêm các họ từ `parameter`, `parameters`, `parameterized`, `parameterization`, `parametric`, `params`, `parser`, `parallel` cùng các từ liên quan. Trong chế độ **Bảo vệ từ tiếng Anh**, `p`, `pa`, `par` ... `parameter` đều phải giữ nguyên ngay khi gõ, không đợi dấu cách cuối từ. Bản test có kiểm tra từng ký tự này.

Trường hợp từ Anh hoàn toàn mới chưa có trong từ điển, vào mục **Từ điển Anh** ngay trong ứng dụng, thêm từ và bấm **Lưu** để áp dụng. Không cần Notepad. Lỗi trước đây nằm ở từ vắng mặt trong bộ từ điển gốc, không phải do tốc độ gõ.

## Dấu mũ Telex phải xuất hiện tức thời

Đã sửa trường hợp `boo` bị giữ nguyên vì đoán nhầm tiền tố tiếng Anh:
`bo` → nhấn `o` lần hai → **bô** ngay, `boon` → **bôn**, `boons`
→ **bốn**. `aa → â`, `ee → ê`, `oo → ô` cũng xuất hiện ngay khi
gõ phím lặp, không đợi dấu cách hay hết từ. Nếu tiếp tục gõ thành một từ
tiếng Anh đã nhận diện (`book`, `good`, `boost`) thì TelexMini sẽ hoàn
nguyên chữ gốc. Một vài từ Anh có thể tạm hiển thị dấu ở phần tiền tố
mơ hồ; đây là đánh đổi cần thiết để giữ nguyên yêu cầu bỏ dấu tức thời.

## Ưu tiên tiếng Việt khi đang bật V

Nếu người dùng chủ động gõ chuỗi tạo dấu mũ Telex, và kết quả là âm tiết
tiếng Việt hợp lệ, **không được cho từ điển tiếng Anh ghi đè**.
Ví dụ `bee → bê`, `been → bên`, `beens → bến`,
`teen → tên`, `seen → sên`. Dấu xuất hiện ngay trong lúc gõ
và giữ nguyên khi nhấn dấu cách.

Những từ như `book`, `good` vẫn được hoàn nguyên thành tiếng Anh vì
kết quả ghép âm tiết tiếng Việt không hợp lệ. Khi hoàn toàn mơ hồ,
ví dụ `boost → bốt`, ứng dụng ưu tiên tiếng Việt khi chế độ V đang bật;
chuyển sang E để gõ nguyên văn tiếng Anh.

## Sửa ưu tiên dấu tức thời

Khi đang chọn V (Telex, Cân bằng / ưu tiên Việt): `as → á` ngay sau
phím `s`, `ex → ẽ` ngay sau phím `x`. Việc chuỗi `as` có
trong từ điển tiếng Anh không được giữ nguyên `as` ở chế độ Cân bằng.
Nếu muốn gõ nguyên văn `as`, dùng `ass → as` (hủy dấu).

Quy tắc thứ tự dấu: `ee → ê`, sau đó `eex → ễ`.
Còn `exe → exe` (không được thành `ễ`), vì `x` chen
giữa hai nguyên âm không tạo thành quy trình gõ `ê` rồi thêm
dấu ngã. Bộ gõ phải hoàn nguyên chữ gốc ngay trong lúc gõ.

Riêng vần tiếng Việt hợp lệ được gõ dấu trước nguyên âm cuối vẫn phải hoạt
động: `chayj → chạy` và `chajy → chạy` đều được hỗ trợ. Không áp dụng
máy móc quy tắc `exe` cho vần `ay`, `uy`, ...

**Sửa lỗi kiểm tra ghép vần toàn cục:** thay vì kiểm tra chuỗi phím thô
(ví dụ `kieer` có `ee`), TelexMini kiểm tra âm tiết Unicode đã chuyển
dấu rồi mới quyết định có hoàn nguyên từ tiếng Anh hay không.
Các cách gõ sau đều phải hoạt động:
- `kieeur` và `kieeru` → **kiểu**
- `nhieeuf` và `nhieefu` → **nhiều**
- `vaayj` và `vaajy` → **vậy**
- `nguowif` và `nguowfi` → **người**
- `chayj` và `chajy` → **chạy**

Bộ kiểm thử còn so sánh 30 tổ hợp các dấu s/f/r/x/j đứng trước và sau
nguyên âm cuối trong sáu nhóm vần, bên cạnh các từ tiếng Anh và những
trường hợp như `exe → exe`. Các kiểm thử này đánh giá engine; tương
thích với mọi trình soạn thảo vẫn cần kiểm thử thực tế.

## Gõ lặp phím để hủy dấu (kiểu UniKey)

Trong chế độ V, nếu một phím Telex đã tạo dấu hoặc biến đổi chữ, gõ lại
phím đó **thêm một lần** sẽ hủy biến đổi và trả lại các phím gốc của
từ đang nhập. Sau đó các chữ tiếp theo được giữ nguyên, không cần
chuyển sang chế độ E.

Ví dụ:
- `boons → bốn`, nhưng `boosst → boost` (phím `s` thứ hai hủy dấu).
- `guitarr → guitar`; `ass → as`.
- `aaa → aa`, `eee → ee`, `ooo → oo`, `ddd → dd`,
  `uoww → uow`.
- `office` và `assets` vẫn nhập đúng: một phím lặp có thể là tiền tố
  tiếng Anh. Nếu chữ tiếp theo khớp từ tiếng Anh đã biết, bộ gõ khôi phục
  ký tự bị bỏ qua.
- Vẫn hỗ trợ các ví dụ gõ lặp cũ như `offfice → office`,
  `asssets → assets`.

**Lưu ý:** Cơ chế hủy này chỉ áp dụng cho từ đang gõ, không ghi lại
nội dung hay mật khẩu. Khi gặp từ không thể đoán chắc, mày có thể
chủ động hủy dấu bằng phím lặp.

## Thuật toán song ngữ v2 (không cần từ điển cho mọi từ)

TelexMini nhận diện Anh–Việt theo **3 lớp**:
- Từ điển English offline giữ nguyên các từ và tiền tố đã biết.
- Phát hiện đầu từ/cụm phụ âm thường chỉ có trong tiếng Anh (như `pr`, `fl`, `cr`, `st`, `wh`, `pa`). Những từ **chưa có trong từ điển** như `paradoxical`, `privatization`, `cryptanalysis` cũng không bị thêm dấu Telex.
- Nếu một từ ban đầu có thể là tiếng Việt nhưng dần xuất hiện cấu trúc không thể tạo thành âm tiết Việt (đuôi phụ âm không hợp lệ, hai nhóm nguyên âm bị ngăn cách), bộ gõ **khôi phục các ký tự gốc ngay trong lúc nhập** rồi giữ nguyên phần còn lại của từ.

Danh sách `VietnameseOverrides.txt` giờ được xem xét cả **tiền tố**, giúp ép ưu tiên Telex cho các chuỗi tùy chỉnh trước khi gõ hết từ. Dấu Telex vẫn đặt **ngay khi gõ**, kể cả dấu gõ trước phụ âm cuối (`tieesng` → `tiếng`).

**Giới hạn:** Không thuật toán chỉ nhìn từng phím nào luôn đoán đúng các trường hợp mơ hồ (`as` tiếng Anh / `á` tiếng Việt). Một số từ Anh hiếm có thể tạm hiện dấu trong vài ký tự đầu trước khi được hoàn nguyên, hoặc trùng với một âm tiết Việt hợp lệ. Có thể bổ sung `EnglishWords.txt`, tắt bảo vệ English hoặc chỉnh danh sách ưu tiên Telex. Không đọc nội dung ô mật khẩu và không gửi dữ liệu lên mạng.

## Nhận diện tiếng Anh thông minh (offline)

Mặc định bật **Bảo vệ từ tiếng Anh (offline)**. Telex Mini có từ điển gốc khoảng 1.500 từ phổ biến và kỹ thuật. Bộ gõ giữ nguyên chuỗi phím khi nó còn là tiền tố của một từ Anh đã biết, nhờ vậy giảm hiện tượng chữ bị biến thành dấu Telex trong lúc gõ. Khi tiền tố không còn khớp tiếng Anh, bộ gõ phát lại chuỗi phím qua Telex để người dùng vẫn có thể gõ dấu ngay sau nguyên âm hoặc bất kỳ chỗ nào trong từ. Không sử dụng Internet, AI, hay gửi văn bản đi đâu.

Ví dụ: `office`, `assets`, `testing`, `framework`, `plugin`, `password` được giữ nguyên trong chế độ V; `tieengs` → **tiếng**, `vieetj` → **việt** và `truowngf` → **trường**. Các từ có cách gõ trùng hoàn toàn như `as` (tiếng Anh) và `á` (Telex) không thể tự nhận diện 100%. Theo mặc định, `as` được ưu tiên tiếng Anh khi bật nhận diện thông minh; tắt tính năng này để trở lại Telex thuần, hoặc ghi `as` trong danh sách ưu tiên tiếng Việt.

Bấm phải biểu tượng V/E ở khay:
- **Từ điển Anh** mở cửa sổ chỉnh sửa ngay trong TelexMini: tìm kiếm, thêm, sửa, xóa, lưu từ tiếng Anh.
- **Ưu tiên Telex** mở cửa sổ tương tự để chỉnh các chuỗi phím ưu tiên tiếng Việt.
- **Nạp lại từ điển** đọc lại những thay đổi bên ngoài mà không cần khởi động lại.

Mỗi lần bấm **Lưu**, từ điển được cập nhật ngay. Dữ liệu cá nhân lưu tại `%LOCALAPPDATA%\\TelexMini\\EnglishWords.txt` và `%LOCALAPPDATA%\\TelexMini\\VietnameseOverrides.txt`, không lưu cùng EXE để tránh lỗi quyền ghi khi cài trong Program Files. Các file cũ bên cạnh EXE vẫn được đọc để tương thích bản portable. Chú ý: từ mới thêm có thể tạo thêm các tiền tố mơ hồ; nếu thấy từ Việt bị giữ dấu quá lâu, thêm chuỗi đó vào danh sách ưu tiên Telex hoặc tắt bảo vệ tiếng Anh.

## Bảo vệ ô mật khẩu (luôn bật)

Khi chế độ V đang bật, Telex Mini **luôn tự chuyển sang English trong trường nhập mật khẩu được Windows nhận diện**, kể cả khi bỏ chọn tự dùng English trong terminal. Khi chuyển ra ô nhập thường, bộ gõ khôi phục chế độ V mà bạn đã chọn. Trong lúc chưa xác định được loại ô nhập (Checking/Unavailable), ưu tiên không can thiệp bàn phím để tránh thay đổi mật khẩu.

Tính năng nhận diện chỉ đọc metadata `IsPassword` và cờ native password; **không đọc ký tự mật khẩu** và không lưu nội dung ô. Một số ô mật khẩu tự vẽ hoặc trang web thay đổi trường thành text có thể không cung cấp metadata chuẩn, nên không thể bảo đảm phát hiện tuyệt đối tất cả ô mật khẩu trên mọi ứng dụng. Nếu giao diện mật khẩu không được Windows báo là password, hãy chuyển E thủ công.

Tuỳ chọn **Tự dùng English trong terminal** chỉ điều khiển terminal; chức năng bảo vệ mật khẩu không thể tắt bằng checkbox này. Khi chọn E thủ công, Telex Mini gỡ hook và không theo dõi focus.

## Gõ trên Mac qua Steam Remote Play

Khi cửa sổ Steam Remote Play (`streaming_client.exe`) ở phía trước, Telex Mini hiển thị **E · NHƯỜNG PHÍM**, gỡ hook bàn phím/chuột và dừng nhận diện ô nhập. Các phím Telex được gửi nguyên sang Mac để bộ gõ bên Mac xử lý. Khi quay lại ứng dụng Windows, Telex Mini khôi phục lựa chọn trước đó. Cửa sổ thư viện Steam và Steam chat không bị loại trừ.

Trên Mac, vào **System Settings → Keyboard → Text Input → Edit → +**, thêm **Vietnamese → Telex / Simple Telex**, rồi chọn nguồn nhập đó từ menu bàn phím ở thanh menu. Thử trong TextEdit: gõ `tieengs vieetj` để kiểm tra. Windows vẫn dùng bố cục **ENG / US**.

Telex Mini không tự đổi nguồn nhập trên Mac. Nếu chưa bật bộ gõ ở Mac thì các phím vẫn là chữ Latin không dấu. Nếu bộ gõ mặc định macOS có gạch chân khi soạn thảo, đó là hành vi bên Mac, không do Telex Mini. Đã kiểm tra phân loại tiến trình và vòng đời hook; chưa kiểm tra đầu-cuối trên Mac của bạn.

Hướng dẫn Apple: https://support.apple.com/guide/mac-help/write-in-another-language-on-mac-mchlp1406/mac

## Hiệu năng khi chơi Fightcade

- Tự nhận diện cửa sổ Fightcade/Fightcade2, fcade*, FBNeo, FBA và Flycast ở phía trước rồi **gỡ cả hook bàn phím lẫn chuột**, dừng nhận diện ô nhập. Khi rời game sẽ khôi phục lựa chọn trước đó.
- Dùng thông báo đổi cửa sổ Windows và kiểm tra dự phòng nhẹ mỗi 500 ms; không quét cây giao diện game. Có thể có khoảng chuyển tiếp ngắn khi vừa chuyển cửa sổ. Không đảm bảo các bản fork có tên tiến trình khác được nhận diện.
- Chọn **E** thủ công cũng gỡ hai hook và dừng nhận diện ô nhập.
- Với game/ứng dụng remote khác hoặc khi vẫn thấy giật: bấm phải V/E ở khay → **Tạm dừng hoàn toàn: game / remote**. Bỏ chọn để tiếp tục. Chế độ này giữ bộ gõ tạm dừng cả khi Alt+Tab.
- Bản trước có lỗi: tắt tuỳ chọn tự chuyển hoặc chọn E vẫn để bộ nhận diện/hook hoạt động. Bản này sửa vòng đời đó. Kiểm thử xác nhận gỡ/khôi phục hook thật và không truy vấn UI Automation khi tự động tắt. Chưa đo FPS/frame time trong trận Fightcade trên máy người dùng, nên không khẳng định loại bỏ mọi nguồn giật.

Đã kiểm tra nhận diện ô mật khẩu Windows và `<input type="password">` trong trang HTML cục bộ trên Chrome, cùng việc trở về V khi rời ô. Chưa xác nhận riêng Edge, Firefox, ô mật khẩu tự vẽ hoặc mọi website. Nút hiện mật khẩu của website có thể đổi ô thành `type="text"`, khiến nó được xem là ô thường; dùng E thủ công nếu cần. Đây không phải cơ chế bảo vệ mật khẩu ở cấp hệ điều hành.

## Gạch chân và tương thích

Telex Mini thay phần đuôi từ bằng Backspace và ký tự Unicode qua Windows SendInput. Không tạo phiên soạn thảo IME, nên bản thân bộ gõ không tạo gạch chân lúc đang nhập. **Gạch đỏ/xanh của kiểm tra chính tả trong Word hoặc trình duyệt là tính năng riêng của ứng dụng**, cần tắt ở đó nếu muốn bỏ.

Phải dùng bố cục bàn phím **English US**; bộ gõ giả định vị trí phím A–Z theo bố cục này. Không dùng đồng thời với Vietnamese Telex của Windows hoặc bộ gõ khác.

Đã kiểm tra tự động bộ máy Telex và nhập Unicode thực tế vào TextBox Windows. Chưa kiểm tra từng phiên bản Word, trình duyệt, terminal, trò chơi hoặc ứng dụng dùng cách nhập đặc biệt. Những ứng dụng đó có thể xử lý Backspace/Unicode khác nhau. Undo do ứng dụng đích quản lý và có thể tách thành nhiều bước. Bộ gõ xử lý từ đang nhập; sau khi bấm chuột, di chuyển con trỏ hoặc dùng phím tắt, bộ đệm được xoá, không sửa dấu của từ cũ.

Ứng dụng chạy quyền quản trị có thể chặn SendInput từ bộ gõ chạy quyền thường. Nếu cần gõ trong ứng dụng đó, chạy Telex Mini cùng mức quyền. Không tự yêu cầu quyền quản trị hoặc tự chạy cùng Windows.

## Mã nguồn và biên dịch

- `TelexEngine.cs`: bộ quy tắc Telex và xử lý tiếng Anh từng phím.
- `EnglishLexicon.cs`: từ điển offline và tùy chỉnh ngoại lệ bằng file văn bản.
- `Keyboard.cs`: hook bàn phím/chuột và gửi Unicode.
- `InputContext.cs`: nhận diện ứng dụng terminal và metadata ô mật khẩu trên worker riêng.
- `Program.cs`: cửa sổ, biểu tượng khay, phím tắt.
- `Tests.cs`: kiểm tra quy tắc và nhập liệu thực tế.
- `build.ps1`: biên dịch bằng trình C# đi kèm .NET Framework.
- `test.ps1`: chạy kiểm tra; thêm `-Integration` để thử nhập vào cửa sổ kiểm thử.

Chạy từ PowerShell trong thư mục này:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\test.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\test.ps1 -Integration
```

Khi chạy kiểm tra tích hợp, bấm **Start input test** rồi để nguyên cửa sổ kiểm thử phía trước khoảng 20 giây. Bài kiểm tra dừng nếu cửa sổ mất tiêu điểm. Chế độ nhận phím giả lập chỉ được bật trong chương trình kiểm thử.

Bộ gõ không ghi log nội dung, không dùng mạng, không dùng clipboard và không lưu lịch sử. Bộ đệm chỉ giữ từ đang nhập và các phím gốc của từ đó trong bộ nhớ; xoá khi kết thúc từ, chuyển tiêu điểm hoặc đổi chế độ. Chuỗi dài từ 64 ký tự trở lên được giữ nguyên phần gõ tiếp như định danh tiếng Anh.

## Tham khảo cách làm

- OpenKey mô tả kiểm tra chính tả, khôi phục phím khi từ không hợp lệ và nhớ chế độ theo ứng dụng: https://github.com/tuyenvm/OpenKey
- Windows UI Automation cung cấp cờ IsPassword: https://learn.microsoft.com/en-us/dotnet/api/system.windows.automation.automationelement.ispasswordproperty

Telex Mini tự triển khai phần kiểm tra cấu trúc âm tiết và chính sách chuyển chế độ đơn giản; không sao chép mã nguồn OpenKey.

Đây là bản Telex tối giản tự viết, không phải mã nguồn UniKey và chưa có độ phủ tương thích như UniKey.
