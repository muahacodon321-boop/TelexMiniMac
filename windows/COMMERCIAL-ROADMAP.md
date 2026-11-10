# TelexMini — Roadmap thương mại hóa

## Đã có (bản preview)
- Windows: TelexMini-Modern.exe, dashboard đa khu vực, dark/light theme, preview gõ, chỉnh từ điển, lưu lựa chọn trong %LOCALAPPDATA%/TelexMini/preferences.ini.
- Mac Apple Silicon: Swift InputMethodKit input source, SwiftUI Settings, từ điển Anh offline (1.566 từ), engine unit tests.
- Bảo vệ ô mật khẩu trên Windows dựa trên Windows UI Automation metadata (không đọc nội dung).
- Không có cloud logging, keylogging, thu thập mật khẩu, analytics hay network trong luồng gõ.
- Lưu ý: Mac bản preview mới build/run engine + settings + đăng ký input source, chưa được kiểm thử end-to-end sau relog.

## P0 — chưa được bán trước khi hoàn thành
1. Test end-to-end trên macOS sau đăng nhập lại, xác minh TIS input list và ít nhất TextEdit, Notes, Chrome, Safari, VS Code, Office, trang password, secure input.
2. Test trên Windows ở trình duyệt, Word/Office, terminal, IDE, desktop apps; kiểm thử screen reader, IME conflict, copy/paste, Undo/Redo, selection/caret movement.
3. Fuzz test tối thiểu 100k chuỗi Telex Anh-Việt, stress CPU/latency; chuẩn hóa Unicode NFC, xử lý chữ hoa/chữ thường và ký tự Unicode đặc biệt.
4. Không được xử lý bàn phím ở mật khẩu, game, terminal có bảo vệ; cơ chế fail-safe và xác minh metadata UIA chính xác.
5. Installer/uninstaller per-user, autostart opt-in, app versioning, signed updater với rollback, license/account activation riêng (không nằm trong callback phím).
6. Mac Developer ID Application signing, hardened runtime, notarization, stapling, Gatekeeper test; Windows code signing/MSIX + SmartScreen validation.
7. Chính sách quyền riêng tư, EULA, trợ giúp, khôi phục cài đặt, crash-report opt-in tách biệt khỏi nội dung gõ.

## P1 — tạo khác biệt thương mại
1. Per-app profiles: gõ tiếng Việt/Anh mặc định theo app, configurable sites/IDE/terminal.
2. Fuzzy English recognition + Vietnamese syllable scoring và user exceptions: ẩn/hiện gợi ý, ưu tiên tức thời.
3. Bilingual grammar & spelling suggestions chạy offline (tùy chọn, không làm chậm việc gõ).
4. Multi-platform đồng bộ profile và từ điển có mã hóa đầu-cuối, mặc định tắt.
5. Theme Gallery: Light/Dark/System, tint, compact tray, interactive onboarding, status indicator.
6. User dictionary UI đầy đủ với search/import/export/backup/versioning, không yêu cầu sửa file TXT.
7. Per-app keyboard shortcuts, auto suspend on games/remote.
8. Portable mode Windows và native launcher macOS; kênh beta độc lập production.

## P2 — sau khi có người dùng
- Layout VNI, Telex nâng cao, tốc ký, snippet/shortcut, bộ gõ đa ngôn ngữ.
- Từ điển lớn hơn và language detection theo ngữ cảnh trên máy.
- Cấp license: Free / Pro 1 lần hoặc theo thiết bị. Không chặn chức năng bảo mật/mật khẩu.
- Trang changelog, feedback ẩn danh opt-in, license restore, support, docs, accessibility.
- Cân nhắc chia sẻ lõi Telex bằng C/Rust để Windows/Mac không bị lệch hành vi.

## Release gate
Chỉ bán khi: cơ chế mật khẩu đã thử trên môi trường thực; IME macOS đăng ký thành công và gõ được ổn định sau đăng nhập lại; app ký đúng chứng thư và notarize; test suite / regression / installer / uninstaller / rollback đều PASS. Không suy diễn từ kết quả compile thành release-ready.

## Phương án giá khởi điểm (cần A/B test)
- Free: gõ Telex chuẩn, bảo vệ mật khẩu, từ điển offline.
- Pro: quản lý profile từng ứng dụng, thêm từ thông minh, theme nâng cao, sync mã hóa (khi đã triển khai).
- Thử nghiệm giá bản quyền một lần 149.000–299.000 VND hoặc $9–15; chưa phải giá thị trường đã xác minh.
