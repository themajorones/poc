using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CupHeadClone.Prototype
{
    public sealed class TutorialHUDController : MonoBehaviour
    {
        private const string LocalizationAssetPath = "Assets/PrototypeGenerated/Config/TutorialLocalization.asset";
        private const string LocalizationResourcePath = "PrototypeGenerated/Config/TutorialLocalization";
        [SerializeField] private TutorialLocalizationAsset localization;
        [SerializeField] private CanvasGroup rootGroup;
        [SerializeField] private TMP_Text lessonTagText;
        [SerializeField] private TMP_Text objectiveText;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private TMP_Text hpRuleText;
        [SerializeField] private TMP_Text rageLabelText;
        [SerializeField] private TMP_Text footerHintText;
        [SerializeField] private Image rageFill;
        [SerializeField] private CanvasGroup bannerGroup;
        [SerializeField] private TMP_Text bannerTitle;
        [SerializeField] private TMP_Text bannerSubtitle;

        private GameController _game;
        private float _bannerTimer;
        private Tween _bannerPunch;

        public void Initialize(GameController game)
        {
            _game = game;
            EnsureLocalization();
            LocalizationRuntime.LocaleChanged -= HandleLocaleChanged;
            LocalizationRuntime.LocaleChanged += HandleLocaleChanged;

            if (rootGroup != null)
            {
                rootGroup.alpha = 0f;
                rootGroup.blocksRaycasts = false;
                rootGroup.interactable = false;
            }

            if (bannerGroup != null)
            {
                bannerGroup.alpha = 0f;
            }

            if (hpRuleText != null)
            {
                hpRuleText.text = T("hud.hp_rule", "Mất 1 HP = học lại đúng bài hiện tại");
            }

            if (footerHintText != null)
            {
                footerHintText.text = T("hud.footer_hint", "Mobile: kéo và hất lên để phản đòn. Desktop: giữ chuột và hất lên. Space / E = skill");
            }

            PolishLayout();
        }

        public void ShowGameplay(bool visible)
        {
            if (rootGroup == null)
            {
                return;
            }

            rootGroup.alpha = visible ? 1f : 0f;
            rootGroup.blocksRaycasts = false;
            rootGroup.interactable = false;
        }

        public void SetHeader(string lessonTag, string objective, string hint)
        {
            if (lessonTagText != null)
            {
                lessonTagText.text = ResolveTutorialText(lessonTag);
            }

            if (objectiveText != null)
            {
                objectiveText.text = ResolveTutorialText(objective);
            }

            if (hintText != null)
            {
                hintText.text = ResolveTutorialText(hint);
            }
        }

        public void SetProgress(string progress)
        {
            if (progressText != null)
            {
                progressText.text = ResolveTutorialText(progress);
            }
        }

        public void ShowBanner(string title, string subtitle, float duration = 1.6f)
        {
            if (bannerGroup == null)
            {
                return;
            }

            bannerTitle.text = ResolveTutorialText(title);
            bannerSubtitle.text = ResolveTutorialText(subtitle);
            bannerGroup.alpha = 1f;
            bannerGroup.transform.localScale = Vector3.one;
            _bannerTimer = duration;
            bannerGroup.DOKill();
            bannerGroup.transform.DOKill();
            _bannerPunch?.Kill();
            _bannerPunch = bannerGroup.transform.DOPunchScale(Vector3.one * 0.08f, 0.26f, 1).SetUpdate(true);
        }

        private void Update()
        {
            if (_game != null && rageFill != null)
            {
                rageFill.fillAmount = _game.RageSystem.Current / Mathf.Max(1f, _game.RageSystem.Max);
            }

            if (rageLabelText != null && _game != null)
            {
                rageLabelText.text = _game.RageSystem.IsFull
                    ? T(Application.isMobilePlatform ? "hud.rage_ready_mobile" : "hud.rage_ready_desktop", Application.isMobilePlatform ? "Chạm SKILL" : "Space / E để dùng skill")
                    : T("hud.rage_charge", "Phản đòn để nạp Rage");
                rageLabelText.color = _game.RageSystem.IsFull
                    ? new Color(1f, 0.89f, 0.62f, 1f)
                    : PrototypeVisualUtility.TextMuted;
            }

            if (_bannerTimer <= 0f)
            {
                return;
            }

            _bannerTimer -= Time.deltaTime;
            if (_bannerTimer <= 0f && bannerGroup != null)
            {
                bannerGroup.DOFade(0f, 0.18f).SetUpdate(true);
            }
        }

        private void OnDestroy()
        {
            LocalizationRuntime.LocaleChanged -= HandleLocaleChanged;
            _bannerPunch?.Kill();
        }

        private void PolishLayout()
        {
            foreach (var text in GetComponentsInChildren<TMP_Text>(true))
            {
                text.color = text == progressText ? PrototypeVisualUtility.HealMint : PrototypeVisualUtility.TextPrimary;
            }
        }

        private string T(string key, string fallback)
        {
            return localization != null ? localization.Get(key, fallback) : fallback;
        }

        private void HandleLocaleChanged()
        {
            if (hpRuleText != null)
            {
                hpRuleText.text = T("hud.hp_rule", "Mất 1 HP = học lại đúng bài hiện tại");
            }

            if (footerHintText != null)
            {
                footerHintText.text = T("hud.footer_hint", "Mobile: kéo và hất lên để phản đòn. Desktop: giữ chuột và hất lên. Space / E = skill");
            }
        }

        private string ResolveTutorialText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            if (value.Contains("PARRY C") || value.Contains("PHẢN ĐÒN CƠ BẢN"))
            {
                return T("lesson1.title", "BÀI 1 • PHẢN ĐÒN CƠ BẢN");
            }

            if (value.Contains("xếp ngang") || value.Contains("x §¨p ngang"))
            {
                return T("lesson1.objective", "Có 3 viên đạn tím xếp ngang sẵn trên màn. Phản đòn đủ 3 viên, và chỉ được tính khi đạn phản chạm receiver phía trên.");
            }

            if (value.Contains("receiver") && value.Contains("tiến trình"))
            {
                return T("lesson1.hint", "Phản đòn xong chưa tính ngay. Mỗi viên phải phản lại và chạm receiver mới được cộng tiến trình.");
            }

            if (value.StartsWith("Ti") && value.Contains("/ 3"))
            {
                if (value.Contains("1/3") || value.Contains("2/3") || value.Contains("3/3"))
                {
                    if (value.Contains("weak zone"))
                    {
                        if (value.Contains("1/3")) return T("lesson_break.progress_enter", "Tiến trình: 1/3 vào weak zone");
                        if (value.Contains("2/3")) return T("lesson_break.progress_charge", "Tiến trình: 2/3 giữ trong weak zone để nạp đầy Rage");
                        if (value.Contains("3/3")) return T("lesson_break.progress_skill", "Tiến trình: 3/3 dùng skill để hoàn tất tutorial");
                    }

                    return value;
                }

                var colon = value.IndexOf(':');
                var suffix = colon >= 0 ? value[(colon + 1)..].Trim() : value;
                return $"{T("lesson1.progress_label", "Tiến trình")}: {suffix}";
            }

            if (value.StartsWith("Enemy c") || value.StartsWith("Enemy còn"))
            {
                var colon = value.IndexOf(':');
                var suffix = colon >= 0 ? value[(colon + 1)..].Trim() : value;
                return $"{T("lesson2.progress_label", "Enemy còn lại")}: {suffix}";
            }

            if (value.StartsWith("Enemy gi") || value.StartsWith("Enemy giữa"))
            {
                var colon = value.IndexOf(':');
                var suffix = colon >= 0 ? value[(colon + 1)..].Trim() : value;
                return $"{T("lesson3.progress_label", "Enemy giữa HP")}: {suffix}";
            }

            if (value.Contains("NHE") || value.Contains("ĐẠN XANH") || value.Contains("ĐẠN TÍM"))
            {
                return T("lesson2.title", "BÀI 2 • ĐẠN XANH / ĐẠN TÍM");
            }

            if (value.Contains("m ¯-i 1 gi") || value.Contains("mỗi 1 giây"))
            {
                return T("lesson2.objective", "Cứ mỗi 1 giây sẽ có cả 1 đạn xanh và 1 đạn tím. Né đạn xanh, phản đòn đạn tím để hạ cả 2 enemy.");
            }

            if (value.Contains("Hai enemy") || value.Contains("đổi vai"))
            {
                return T("lesson2.hint", "Hai enemy bắn cùng nhịp và sẽ đổi vai giữa đạn xanh và đạn tím.");
            }

            if (value.Contains("NANG CAO") || value.Contains("NÂNG CAO"))
            {
                return T("lesson3.title", "BÀI 3 • PHẢN ĐÒN NÂNG CAO");
            }

            if (value.Contains("Enemy s") && value.Contains("lane"))
            {
                return T("lesson3.objective", "Enemy sẽ sang 1 lane, bắn 1 phát rồi dừng lại. Hãy đúng nhịp để phản đòn bằng parry.");
            }

            if (value.Contains("kho") && value.Contains("enemy"))
            {
                return T("lesson3.hint", "Sau mỗi phát bắn sẽ có khoảng dừng rõ ràng trước khi enemy đổi lane tiếp.");
            }

            if (value == "SKILL")
            {
                return T("lesson_skill.title", "SKILL");
            }

            if (value.Contains("phá barrier") || value.Contains("phA­ barrier"))
            {
                if (value.Contains("Ti") || value.Contains("tiến trình"))
                {
                    return T("lesson_skill.progress", "Tiến trình: nạp Rage đầy rồi dùng skill để phá barrier");
                }

                return T("lesson_skill.objective", "Phản đòn để nạp đầy Rage, rồi dùng skill để phá barrier.");
            }

            if (value.Contains("nạp Rage") || value.Contains("n §­p Rage"))
            {
                if (value.Contains("thanh") || value.Contains("nút") || value.Contains("SKILL"))
                {
                    return T("lesson_skill.hint", "Phản đòn sẽ nạp Rage. Khi nút SKILL sáng, dùng ngay để phá barrier.");
                }
            }

            if (value == "BOSS BREAK")
            {
                return T("lesson_break.title", "BOSS BREAK");
            }

            if (value.Contains("weak zone tr") || value.Contains("weak zone trước"))
            {
                return T("lesson_break.objective_enter", "Di chuyển vào weak zone trước.");
            }

            if (value.Contains("Boss") && value.Contains("vòng vàng"))
            {
                return T("lesson_break.hint_enter", "Boss đã break sẵn. Bước 1 là chạm đúng vòng vàng.");
            }

            if (value.Contains("nạp đầy Rage") || value.Contains("n §­p Ž` §y Rage"))
            {
                if (value.Contains("Đứng") || value.Contains("Ž?"))
                {
                    return T("lesson_break.objective_charge", "Đứng trong weak zone để nạp đầy Rage.");
                }

                if (value.Contains("Giữ") || value.Contains("Gi ¯_"))
                {
                    return T("lesson_break.hint_charge", "Giữ ship trong vòng vàng đến khi nút SKILL sáng.");
                }
            }

            if (value.Contains("kết thúc tutorial") || value.Contains("k §¨t thA§c tutorial"))
            {
                return T("lesson_break.objective_skill", "Rage đã đầy. Dùng skill để kết thúc tutorial.");
            }

            if (value.Contains("pass bài cuối") || value.Contains("pass bAÿi cu"))
            {
                return T("lesson_break.hint_skill", "Đầy Rage thôi chưa đủ. Bấm SKILL hoặc Space / E để pass bài cuối.");
            }

            if (value.Contains("Bài 1") || value.Contains("BAÿi 1"))
            {
                if (value.Contains("hoàn thành") || value.Contains("hoAÿn thAÿnh"))
                {
                    return T("lesson1.complete.title", "Bài 1 hoàn thành");
                }

                return T("lesson1.banner.title", "Bài 1");
            }

            if (value.Contains("receiver phía trên") || value.Contains("receiver phA-a trA¦n"))
            {
                if (value.Contains("mới được tính") || value.Contains("m ¯>i Ž`’ø ¯œc"))
                {
                    return T("lesson1.complete.subtitle", "Đạn phản phải chạm receiver phía trên mới được tính.");
                }

                return T("lesson1.banner.subtitle", "Phản đòn 3 viên tím và để đạn phản chạm receiver phía trên.");
            }

            if (value.Contains("Bài 2") || value.Contains("BAÿi 2"))
            {
                if (value.Contains("hoàn thành") || value.Contains("hoAÿn thAÿnh"))
                {
                    return T("lesson2.complete.title", "Bài 2 hoàn thành");
                }

                return T("lesson2.banner.title", "Bài 2");
            }

            if (value.Contains("một bên xanh") || value.Contains("bA¦n xanh"))
            {
                if (value.Contains("đúng nhịp") || value.Contains("nh ¯<p"))
                {
                    return T("lesson2.complete.subtitle", "Bạn đã né đạn xanh và phản đòn đúng nhịp đạn tím.");
                }

                return T("lesson2.banner.subtitle", "Mỗi nhịp: một bên xanh, một bên tím, rồi đổi vai.");
            }

            if (value.Contains("Bài 3") || value.Contains("BAÿi 3"))
            {
                if (value.Contains("hoàn thành") || value.Contains("hoAÿn thAÿnh"))
                {
                    return T("lesson3.complete.title", "Bài 3 hoàn thành");
                }

                return T("lesson3.banner.title", "Bài 3");
            }

            if (value.Contains("đổi 4 lane") || value.Contains("Ž` ¯i 4 lane"))
            {
                if (value.Contains("pattern lane"))
                {
                    return T("lesson3.complete.subtitle", "Bạn đã phản đòn ổn trong pattern lane đơn giản hơn.");
                }

                return T("lesson3.banner.subtitle", "Enemy đổi 4 lane và bắn từng nhịp ngắn để bạn đọc pattern.");
            }

            if (value.Contains("Skill thành công") || value.Contains("thAÿnh cA'ng"))
            {
                return T("lesson_skill.complete.title", "Skill thành công");
            }

            if (value.Contains("dùng skill đúng flow") || value.Contains("dA1ng skill Ž`A§ng flow"))
            {
                if (value.Contains("weak zone"))
                {
                    return T("lesson_break.complete.subtitle", "Bạn đã vào weak zone, nạp Rage đầy và dùng skill đúng flow.");
                }

                return T("lesson_skill.complete.subtitle", "Bạn đã nạp Rage và dùng skill đúng flow.");
            }

            if (value == "Skill")
            {
                return T("lesson_skill.banner.title", "Skill");
            }

            if (value.Contains("Dùng skill khi thanh đầy") || value.Contains("DA1ng skill khi thanh"))
            {
                return T("lesson_skill.banner.subtitle", "Phản đòn sẽ nạp Rage. Dùng skill khi thanh đầy.");
            }

            if (value == "Boss Break")
            {
                return T("lesson_break.banner.title", "Boss Break");
            }

            if (value.Contains("hoàn tất tutorial") || value.Contains("hoAÿn t §t tutorial"))
            {
                return T("lesson_break.banner.subtitle", "Vào weak zone, nạp đầy Rage rồi dùng skill để hoàn tất tutorial.");
            }

            if (value.Contains("Rage đầy") || value.Contains("Rage Ž` §y"))
            {
                return T("lesson_break.rage_full.title", "Rage đầy");
            }

            if (value.Contains("Đầy Rage thôi chưa đủ") || value.Contains("thA'i ch’øa"))
            {
                return T("lesson_break.rage_full.subtitle", "Đầy Rage thôi chưa đủ. Dùng skill để pass bài cuối.");
            }

            if (value.Contains("Tutorial hoàn tất") || value.Contains("Tutorial hoAÿn t §t"))
            {
                return T("lesson_break.complete.title", "Tutorial hoàn tất");
            }

            return value;
        }

        private void EnsureLocalization()
        {
#if UNITY_EDITOR
            if (localization == null)
            {
                localization = UnityEditor.AssetDatabase.LoadAssetAtPath<TutorialLocalizationAsset>(LocalizationAssetPath);
            }
#endif
            localization ??= Resources.Load<TutorialLocalizationAsset>(LocalizationResourcePath);
            if (localization != null && ProjectSettingsState.HasSavedLocale)
            {
                localization.ActiveLocale = ProjectSettingsState.Locale;
            }
        }
    }
}
