#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using CupHeadClone.Prototype;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CupHeadClone.PrototypeEditor
{
    public static class TutorialLocalizationAssetEditor
    {
        public const string LocalizationAssetPath = "Assets/PrototypeGenerated/Config/TutorialLocalization.asset";

        [MenuItem("Tools/ParryShooter/Create Tutorial Localization Asset")]
        public static void CreateOrUpdateDefaultAsset()
        {
            Directory.CreateDirectory("Assets/PrototypeGenerated");
            Directory.CreateDirectory("Assets/PrototypeGenerated/Config");

            var asset = AssetDatabase.LoadAssetAtPath<TutorialLocalizationAsset>(LocalizationAssetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<TutorialLocalizationAsset>();
                AssetDatabase.CreateAsset(asset, LocalizationAssetPath);
            }

            MergeMissingEntries(asset, BuildDefaultLocales());
            NormalizeLocales(asset);
            if (string.IsNullOrWhiteSpace(asset.ActiveLocale))
            {
                asset.ActiveLocale = asset.Locales.Count > 0 ? asset.Locales[0].LocaleCode : "en";
            }

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = asset;
        }

        [MenuItem("Tools/ParryShooter/Setup Project Localization Only")]
        public static void SetupProjectLocalizationOnly()
        {
            CreateOrUpdateDefaultAsset();

            var asset = AssetDatabase.LoadAssetAtPath<TutorialLocalizationAsset>(LocalizationAssetPath);
            var tutorialGame = Object.FindFirstObjectByType<TutorialGameController>();
            var tutorialHud = Object.FindFirstObjectByType<TutorialHUDController>();
            var mainMenu = Object.FindFirstObjectByType<MainMenuController>();
            var overlay = Object.FindFirstObjectByType<OverlayController>();

            if (tutorialGame == null && tutorialHud == null && mainMenu == null && overlay == null)
            {
                EditorUtility.DisplayDialog(
                    "Supported Scene Not Open",
                    "Open MainMenu, BossRush, or Tutorial scene first, then run Setup Project Localization Only.",
                    "OK");
                return;
            }

            if (tutorialGame != null)
            {
                SetSerialized(tutorialGame, "localization", asset);
                EditorUtility.SetDirty(tutorialGame);
            }

            if (tutorialHud != null)
            {
                SetSerialized(tutorialHud, "localization", asset);
                EditorUtility.SetDirty(tutorialHud);
            }

            if (mainMenu != null)
            {
                SetSerialized(mainMenu, "localization", asset);
                EditorUtility.SetDirty(mainMenu);
            }

            if (overlay != null)
            {
                SetSerialized(overlay, "localization", asset);
                EditorUtility.SetDirty(overlay);
            }

            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }

            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
        }

        [MenuItem("Tools/ParryShooter/Refresh Localized Text In Open Scene")]
        public static void RefreshLocalizedTextInOpenScene()
        {
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            var mainMenu = Object.FindFirstObjectByType<MainMenuController>();
            if (mainMenu != null)
            {
                mainMenu.RefreshLocalizedText();
                EditorUtility.SetDirty(mainMenu);
            }

            var overlay = Object.FindFirstObjectByType<OverlayController>();
            var game = Object.FindFirstObjectByType<GameController>();
            if (overlay != null && game != null)
            {
                overlay.RefreshState(game.State);
                EditorUtility.SetDirty(overlay);
            }

            var tutorialHud = Object.FindFirstObjectByType<TutorialHUDController>();
            if (tutorialHud != null)
            {
                EditorUtility.SetDirty(tutorialHud);
            }

            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void NormalizeLocales(TutorialLocalizationAsset asset)
        {
            if (asset == null)
            {
                return;
            }

            var source = new List<TutorialLocaleTable>(asset.Locales);
            var merged = new List<TutorialLocaleTable>();

            MergeLocaleCode(source, merged, "en", "English");
            MergeLocaleCode(source, merged, "vi", "Tiếng Việt");

            for (var i = 0; i < source.Count; i++)
            {
                var locale = source[i];
                if (locale == null || string.IsNullOrWhiteSpace(locale.LocaleCode))
                {
                    continue;
                }

                var normalizedCode = NormalizeLocaleCode(locale.LocaleCode);
                if (FindLocale(merged, normalizedCode) != null)
                {
                    continue;
                }

                var copy = new TutorialLocaleTable(normalizedCode, GetDisplayName(normalizedCode, locale.DisplayName));
                CopyEntries(locale, copy, false);
                merged.Add(copy);
            }

            if (merged.Count == 0)
            {
                merged.Add(new TutorialLocaleTable("en", "English"));
                merged.Add(new TutorialLocaleTable("vi", "Tiếng Việt"));
            }

            asset.SetLocales(merged);
            asset.ActiveLocale = NormalizeLocaleCode(asset.ActiveLocale);
            if (string.IsNullOrWhiteSpace(asset.ActiveLocale) || FindLocale(merged, asset.ActiveLocale) == null)
            {
                asset.ActiveLocale = merged[0].LocaleCode;
            }
        }

        public static string NormalizeLocaleCode(string localeCode)
        {
            if (string.IsNullOrWhiteSpace(localeCode))
            {
                return string.Empty;
            }

            var normalized = localeCode.Trim().ToLowerInvariant();
            return normalized switch
            {
                "english" => "en",
                "vietnamese" => "vi",
                "tiengviet" => "vi",
                "tiếngviệt" => "vi",
                "tieng viet" => "vi",
                "tiếng việt" => "vi",
                _ => normalized
            };
        }

        private static void MergeLocaleCode(List<TutorialLocaleTable> source, List<TutorialLocaleTable> target, string localeCode, string displayName)
        {
            TutorialLocaleTable merged = null;
            for (var i = 0; i < source.Count; i++)
            {
                var locale = source[i];
                if (locale == null || NormalizeLocaleCode(locale.LocaleCode) != localeCode)
                {
                    continue;
                }

                merged ??= new TutorialLocaleTable(localeCode, displayName);
                CopyEntries(locale, merged, true);
            }

            if (merged != null)
            {
                target.Add(merged);
            }
        }

        private static void CopyEntries(TutorialLocaleTable from, TutorialLocaleTable to, bool onlyWhenMissing)
        {
            var entries = from.Entries;
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.Key))
                {
                    continue;
                }

                if (onlyWhenMissing && !string.IsNullOrWhiteSpace(to.Get(entry.Key)))
                {
                    continue;
                }

                to.Set(entry.Key, entry.Value);
            }
        }

        private static void MergeMissingEntries(TutorialLocalizationAsset asset, List<TutorialLocaleTable> defaults)
        {
            var mergedLocales = new List<TutorialLocaleTable>(asset.Locales);
            for (var i = 0; i < defaults.Count; i++)
            {
                var defaultLocale = defaults[i];
                var targetLocale = FindLocale(mergedLocales, defaultLocale.LocaleCode);
                if (targetLocale == null)
                {
                    mergedLocales.Add(defaultLocale);
                    continue;
                }

                CopyEntries(defaultLocale, targetLocale, true);
            }

            asset.SetLocales(mergedLocales);
        }

        private static TutorialLocaleTable FindLocale(List<TutorialLocaleTable> locales, string code)
        {
            var normalizedCode = NormalizeLocaleCode(code);
            for (var i = 0; i < locales.Count; i++)
            {
                if (NormalizeLocaleCode(locales[i].LocaleCode) == normalizedCode)
                {
                    return locales[i];
                }
            }

            return null;
        }

        private static string GetDisplayName(string code, string fallback)
        {
            return NormalizeLocaleCode(code) switch
            {
                "en" => "English",
                "vi" => "Tiếng Việt",
                _ => string.IsNullOrWhiteSpace(fallback) ? code : fallback
            };
        }

        private static void SetSerialized(Object target, string propertyName, Object value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static List<TutorialLocaleTable> BuildDefaultLocales()
        {
            var en = new TutorialLocaleTable("en", "English");
            en.Set("menu.title", "Parry Shooter");
            en.Set("menu.subtitle", "Start enters boss rush. Tutorial opens the separate lesson scene.");
            en.Set("menu.start", "Start");
            en.Set("menu.tutorial", "Tutorial");
            en.Set("bossrush.title.start", "Boss Rush");
            en.Set("bossrush.subtitle.start", "Start the boss rush or open the tutorial first.");
            en.Set("bossrush.title.win", "Boss Rush Cleared");
            en.Set("bossrush.subtitle.win", "All bosses defeated. Press Restart to play again.");
            en.Set("bossrush.title.lose", "Ship Destroyed");
            en.Set("bossrush.subtitle.lose", "Read the lanes, parry purple shots, then use skill when Rage is full.");
            en.Set("bossrush.start", "Start Rush");
            en.Set("bossrush.tutorial", "Tutorial");
            en.Set("bossrush.restart", "Restart");
            en.Set("settings.title", "Settings");
            en.Set("settings.master", "Master Volume");
            en.Set("settings.music", "Music");
            en.Set("settings.sfx", "SFX");
            en.Set("settings.language", "Language");
            en.Set("settings.open", "Settings");
            en.Set("settings.close", "Close");
            en.Set("settings.main_menu", "Main Menu");
            en.Set("hud.hp_rule", "Lose 1 HP = restart the current lesson");
            en.Set("hud.footer_hint", "Mobile: drag and flick upward to parry. Desktop: hold mouse and flick upward. Space / E = skill");
            en.Set("hud.rage_ready_mobile", "Tap SKILL");
            en.Set("hud.rage_ready_desktop", "Space / E to use skill");
            en.Set("hud.rage_charge", "Parry to charge Rage");
            en.Set("card.intro.title", "Parry Tutorial");
            en.Set("card.intro.subtitle", "A separate scene that teaches parry, skill, and boss break.\nNo boss rush starts after this.");
            en.Set("card.intro.primary", "Start Tutorial");
            en.Set("card.intro.secondary", "Back to Boss Rush");
            en.Set("card.intro.tip1", "Flow: 3 parry lessons -> skill -> boss break -> finish.");
            en.Set("card.intro.tip2", "Fail rule: taking 1 hit resets the current lesson.");
            en.Set("card.intro.tip3", "Goal: make each mechanic clear for new players.");
            en.Set("card.parry.title", "Parry Basics");
            en.Set("card.parry.subtitle", "Purple bullets can be parried.\nParry by flicking upward.\nThere is no separate parry button.");
            en.Set("card.parry.primary", "Enter Lesson 1");
            en.Set("card.parry.tip1", "Keep the mobile feel: move the ship with touch or drag.");
            en.Set("card.parry.tip2", "The player stays slightly above the finger because of the pointer offset.");
            en.Set("card.parry.tip3", "The player hitbox is a small circle in the center, while the parry area is a bit larger.");
            en.Set("card.skill.title", "Skill Basics");
            en.Set("card.skill.subtitle", "Parrying charges Rage.\nWhen the gauge is full, press SKILL or Space / E.");
            en.Set("card.skill.primary", "Enter Skill Lesson");
            en.Set("card.skill.tip1", "In the next lesson, you must parry to fill the Rage gauge.");
            en.Set("card.skill.tip2", "Use skill as soon as the SKILL button lights up.");
            en.Set("card.break.title", "Boss Break / Weak Zone");
            en.Set("card.break.subtitle", "When the boss is broken, stand in the weak zone to fill Rage very quickly.");
            en.Set("card.break.primary", "Enter Break Lesson");
            en.Set("card.break.tip1", "The final lesson is still inside the tutorial-only scene.");
            en.Set("card.break.tip2", "After finishing the tutorial, the clear next step is returning to boss rush.");
            en.Set("card.end.title", "Tutorial Complete");
            en.Set("card.end.subtitle", "You cleared the full tutorial-only flow.\nReturn to boss rush or replay the tutorial.");
            en.Set("card.end.primary", "Back to Boss Rush");
            en.Set("card.end.secondary", "Replay Tutorial");
            en.Set("card.end.tip1", "The tutorial does not jump into boss rush automatically.");
            en.Set("card.end.tip2", "The last CTA keeps the choice simple: return to boss rush.");
            en.Set("tutorial.fail.title", "Hit Taken");
            en.Set("tutorial.fail.subtitle", "Taking 1 hit restarts the current lesson.");
            en.Set("tutorial.restart.title", "Restart Current Lesson");
            en.Set("tutorial.restart.subtitle", "R resets the current lesson.");
            en.Set("lesson1.title", "LESSON 1 • BASIC PARRY");
            en.Set("lesson1.objective", "Three purple bullets are lined up on screen. Parry all 3, and only count progress when the reflected shot hits the receiver above.");
            en.Set("lesson1.hint", "Parrying alone is not enough. Each reflected shot must touch the receiver to count.");
            en.Set("lesson1.progress_label", "Progress");
            en.Set("lesson1.banner.title", "Lesson 1");
            en.Set("lesson1.banner.subtitle", "Parry 3 purple bullets and send the reflected shots into the top receiver.");
            en.Set("lesson1.complete.title", "Lesson 1 Complete");
            en.Set("lesson1.complete.subtitle", "Only reflected shots that hit the top receiver count.");
            en.Set("lesson2.title", "LESSON 2 • BLUE / PURPLE SHOTS");
            en.Set("lesson2.objective", "Every beat has one blue shot and one purple shot. Dodge blue, parry purple, and defeat both enemies.");
            en.Set("lesson2.hint", "The two enemies fire on the same rhythm and swap bullet colors.");
            en.Set("lesson2.progress_label", "Enemies Left");
            en.Set("lesson2.banner.title", "Lesson 2");
            en.Set("lesson2.banner.subtitle", "Each beat: one side fires blue, the other purple, then they swap.");
            en.Set("lesson2.complete.title", "Lesson 2 Complete");
            en.Set("lesson2.complete.subtitle", "You dodged blue shots and parried the purple rhythm correctly.");
            en.Set("lesson3.title", "LESSON 3 • ADVANCED PARRY");
            en.Set("lesson3.objective", "The enemy moves to one lane, fires once, then pauses. Read the rhythm and parry the shot.");
            en.Set("lesson3.hint", "There is always a clear pause after each shot before the enemy changes lane.");
            en.Set("lesson3.progress_label", "Enemy HP");
            en.Set("lesson3.banner.title", "Lesson 3");
            en.Set("lesson3.banner.subtitle", "The enemy rotates through 4 lanes and fires short beats so you can read the pattern.");
            en.Set("lesson3.complete.title", "Lesson 3 Complete");
            en.Set("lesson3.complete.subtitle", "You handled the lane pattern and parried consistently.");
            en.Set("lesson_skill.title", "SKILL");
            en.Set("lesson_skill.objective", "Parry to fill Rage, then use skill to break the barrier.");
            en.Set("lesson_skill.hint", "Parrying fills Rage. Use skill as soon as the button lights up.");
            en.Set("lesson_skill.progress", "Progress: fill Rage, then use skill to break the barrier");
            en.Set("lesson_skill.banner.title", "Skill");
            en.Set("lesson_skill.banner.subtitle", "Parrying charges Rage. Use skill when the gauge is full.");
            en.Set("lesson_skill.complete.title", "Skill Success");
            en.Set("lesson_skill.complete.subtitle", "You filled Rage and used skill at the right time.");
            en.Set("lesson_break.title", "BOSS BREAK");
            en.Set("lesson_break.objective_enter", "Move into the weak zone first.");
            en.Set("lesson_break.hint_enter", "The boss is already broken. Step 1 is touching the yellow zone.");
            en.Set("lesson_break.objective_charge", "Stay in the weak zone to fill Rage.");
            en.Set("lesson_break.hint_charge", "Keep the ship inside the yellow zone until the SKILL button lights up.");
            en.Set("lesson_break.objective_skill", "Rage is full. Use skill to finish the tutorial.");
            en.Set("lesson_break.hint_skill", "Full Rage alone is not enough. Press SKILL or Space / E to clear the last lesson.");
            en.Set("lesson_break.progress_enter", "Progress: 1/3 enter the weak zone");
            en.Set("lesson_break.progress_charge", "Progress: 2/3 stay in the weak zone to fill Rage");
            en.Set("lesson_break.progress_skill", "Progress: 3/3 use skill to finish the tutorial");
            en.Set("lesson_break.banner.title", "Boss Break");
            en.Set("lesson_break.banner.subtitle", "Enter the weak zone, fill Rage, then use skill to complete the tutorial.");
            en.Set("lesson_break.rage_full.title", "Rage Full");
            en.Set("lesson_break.rage_full.subtitle", "Full Rage is not enough. Use skill to clear the last lesson.");
            en.Set("lesson_break.complete.title", "Tutorial Complete");
            en.Set("lesson_break.complete.subtitle", "You entered the weak zone, filled Rage, and used skill in the intended flow.");

            var vi = new TutorialLocaleTable("vi", "Tiếng Việt");
            vi.Set("menu.title", "Parry Shooter");
            vi.Set("menu.subtitle", "Bắt đầu để vào boss rush. Tutorial mở scene hướng dẫn riêng.");
            vi.Set("menu.start", "Bắt đầu");
            vi.Set("menu.tutorial", "Tutorial");
            vi.Set("bossrush.title.start", "Boss Rush");
            vi.Set("bossrush.subtitle.start", "Bắt đầu boss rush hoặc vào tutorial trước.");
            vi.Set("bossrush.title.win", "Đã thắng Boss Rush");
            vi.Set("bossrush.subtitle.win", "Bạn đã hạ hết boss. Bấm chơi lại để vào lại từ đầu.");
            vi.Set("bossrush.title.lose", "Tàu đã bị phá hủy");
            vi.Set("bossrush.subtitle.lose", "Quan sát lane, phản đòn đạn tím, rồi dùng skill khi Nộ đầy.");
            vi.Set("bossrush.start", "Bắt đầu");
            vi.Set("bossrush.tutorial", "Tutorial");
            vi.Set("bossrush.restart", "Chơi lại");
            vi.Set("settings.title", "Cài đặt");
            vi.Set("settings.master", "Âm lượng tổng");
            vi.Set("settings.music", "Nhạc");
            vi.Set("settings.sfx", "Hiệu ứng");
            vi.Set("settings.language", "Ngôn ngữ");
            vi.Set("settings.open", "Cài đặt");
            vi.Set("settings.close", "Đóng");
            vi.Set("settings.main_menu", "Menu chính");
            vi.Set("hud.hp_rule", "Mất 1 HP = học lại đúng bài hiện tại");
            vi.Set("hud.footer_hint", "Mobile: kéo và hất lên để phản đòn. Desktop: giữ chuột và hất lên. Space / E = skill");
            vi.Set("hud.rage_ready_mobile", "Chạm SKILL");
            vi.Set("hud.rage_ready_desktop", "Space / E để dùng skill");
            vi.Set("hud.rage_charge", "Phản đòn để nạp Rage");
            vi.Set("card.intro.title", "Tutorial phản đòn");
            vi.Set("card.intro.subtitle", "Một scene riêng chỉ để dạy phản đòn, skill và boss break.\nKhông có boss rush sau khi xong.");
            vi.Set("card.intro.primary", "Bắt đầu tutorial");
            vi.Set("card.intro.secondary", "Về Boss Rush");
            vi.Set("card.parry.title", "Giới thiệu phản đòn");
            vi.Set("card.parry.subtitle", "Đạn tím có thể phản đòn.\nPhản đòn bằng cách hất nhanh lên phía trước.\nKhông có nút phản đòn riêng.");
            vi.Set("card.parry.primary", "Vào bài 1");
            vi.Set("card.skill.title", "Giới thiệu skill");
            vi.Set("card.skill.subtitle", "Phản đòn sẽ nạp Rage.\nKhi thanh đầy, bấm SKILL hoặc Space / E để tung skill.");
            vi.Set("card.skill.primary", "Vào bài skill");
            vi.Set("card.break.title", "Boss break / weak zone");
            vi.Set("card.break.subtitle", "Khi boss đang break, player có thể đứng trong weak zone để nạp Rage rất nhanh.");
            vi.Set("card.break.primary", "Vào bài break");
            vi.Set("card.end.title", "Tutorial hoàn tất");
            vi.Set("card.end.subtitle", "Bạn đã đi hết flow tutorial-only.\nScene kết thúc ở đây và chỉ cho bạn quay lại boss rush.");
            vi.Set("card.end.primary", "Back to Boss Rush");
            vi.Set("card.end.secondary", "Chơi lại tutorial");
            vi.Set("tutorial.fail.title", "Trúng đòn");
            vi.Set("tutorial.fail.subtitle", "Trúng 1 hit nên reset đúng bài đang học.");
            vi.Set("tutorial.restart.title", "Reset bài hiện tại");
            vi.Set("tutorial.restart.subtitle", "R reset đúng bài đang học.");
            vi.Set("lesson1.title", "BÀI 1 • PHẢN ĐÒN CƠ BẢN");
            vi.Set("lesson1.objective", "Có 3 viên đạn tím xếp ngang sẵn trên màn. Phản đòn đủ 3 viên, và chỉ được tính khi đạn phản chạm receiver phía trên.");
            vi.Set("lesson1.hint", "Phản đòn xong chưa tính ngay. Mỗi viên phải phản lại và chạm receiver mới được cộng tiến trình.");
            vi.Set("lesson1.progress_label", "Tiến trình");
            vi.Set("lesson1.banner.title", "Bài 1");
            vi.Set("lesson1.banner.subtitle", "Phản đòn 3 viên tím và để đạn phản chạm receiver phía trên.");
            vi.Set("lesson1.complete.title", "Bài 1 hoàn thành");
            vi.Set("lesson1.complete.subtitle", "Đạn phản phải chạm receiver phía trên mới được tính.");
            vi.Set("lesson2.title", "BÀI 2 • ĐẠN XANH / ĐẠN TÍM");
            vi.Set("lesson2.objective", "Cứ mỗi 1 giây sẽ có cả 1 đạn xanh và 1 đạn tím. Né đạn xanh, phản đòn đạn tím để hạ cả 2 enemy.");
            vi.Set("lesson2.hint", "Hai enemy bắn cùng nhịp và sẽ đổi vai giữa đạn xanh và đạn tím.");
            vi.Set("lesson2.progress_label", "Enemy còn lại");
            vi.Set("lesson2.banner.title", "Bài 2");
            vi.Set("lesson2.banner.subtitle", "Mỗi nhịp: một bên xanh, một bên tím, rồi đổi vai.");
            vi.Set("lesson2.complete.title", "Bài 2 hoàn thành");
            vi.Set("lesson2.complete.subtitle", "Bạn đã né đạn xanh và phản đòn đúng nhịp đạn tím.");
            vi.Set("lesson3.title", "BÀI 3 • PHẢN ĐÒN NÂNG CAO");
            vi.Set("lesson3.objective", "Enemy sẽ sang 1 lane, bắn 1 phát rồi dừng lại. Hãy đúng nhịp để phản đòn bằng parry.");
            vi.Set("lesson3.hint", "Sau mỗi phát bắn sẽ có khoảng dừng rõ ràng trước khi enemy đổi lane tiếp.");
            vi.Set("lesson3.progress_label", "Enemy giữa HP");
            vi.Set("lesson3.banner.title", "Bài 3");
            vi.Set("lesson3.banner.subtitle", "Enemy đổi 4 lane và bắn từng nhịp ngắn để bạn đọc pattern.");
            vi.Set("lesson3.complete.title", "Bài 3 hoàn thành");
            vi.Set("lesson3.complete.subtitle", "Bạn đã phản đòn ổn trong pattern lane đơn giản hơn.");
            vi.Set("lesson_skill.title", "SKILL");
            vi.Set("lesson_skill.objective", "Phản đòn để nạp đầy Rage, rồi dùng skill để phá barrier.");
            vi.Set("lesson_skill.hint", "Phản đòn sẽ nạp Rage. Khi nút SKILL sáng, dùng ngay để phá barrier.");
            vi.Set("lesson_skill.progress", "Tiến trình: nạp Rage đầy rồi dùng skill để phá barrier");
            vi.Set("lesson_skill.banner.title", "Skill");
            vi.Set("lesson_skill.banner.subtitle", "Phản đòn sẽ nạp Rage. Dùng skill khi thanh đầy.");
            vi.Set("lesson_skill.complete.title", "Skill thành công");
            vi.Set("lesson_skill.complete.subtitle", "Bạn đã nạp Rage và dùng skill đúng flow.");
            vi.Set("lesson_break.title", "BOSS BREAK");
            vi.Set("lesson_break.objective_enter", "Di chuyển vào weak zone trước.");
            vi.Set("lesson_break.hint_enter", "Boss đã break sẵn. Bước 1 là chạm đúng vòng vàng.");
            vi.Set("lesson_break.objective_charge", "Đứng trong weak zone để nạp đầy Rage.");
            vi.Set("lesson_break.hint_charge", "Giữ ship trong vòng vàng đến khi nút SKILL sáng.");
            vi.Set("lesson_break.objective_skill", "Rage đã đầy. Dùng skill để kết thúc tutorial.");
            vi.Set("lesson_break.hint_skill", "Đầy Rage thôi chưa đủ. Bấm SKILL hoặc Space / E để pass bài cuối.");
            vi.Set("lesson_break.progress_enter", "Tiến trình: 1/3 vào weak zone");
            vi.Set("lesson_break.progress_charge", "Tiến trình: 2/3 giữ trong weak zone để nạp đầy Rage");
            vi.Set("lesson_break.progress_skill", "Tiến trình: 3/3 dùng skill để hoàn tất tutorial");
            vi.Set("lesson_break.banner.title", "Boss Break");
            vi.Set("lesson_break.banner.subtitle", "Vào weak zone, nạp đầy Rage rồi dùng skill để hoàn tất tutorial.");
            vi.Set("lesson_break.rage_full.title", "Rage đầy");
            vi.Set("lesson_break.rage_full.subtitle", "Đầy Rage thôi chưa đủ. Dùng skill để pass bài cuối.");
            vi.Set("lesson_break.complete.title", "Tutorial hoàn tất");
            vi.Set("lesson_break.complete.subtitle", "Bạn đã vào weak zone, nạp Rage đầy và dùng skill đúng flow.");

            return new List<TutorialLocaleTable> { en, vi };
        }
    }
}
#endif
