namespace CupHeadClone.Prototype
{
    public enum TutorialStepKind
    {
        Card,
        Lesson,
        End
    }

    public enum TutorialLessonId
    {
        None,
        Lesson1,
        Lesson2,
        Lesson3,
        Skill,
        Break
    }

    public sealed class TutorialStepDefinition
    {
        public TutorialStepDefinition(
            TutorialStepKind kind,
            TutorialLessonId lessonId,
            string title,
            string subtitle,
            string primaryButtonLabel,
            string secondaryButtonLabel = null,
            params string[] tips)
        {
            Kind = kind;
            LessonId = lessonId;
            Title = title;
            Subtitle = subtitle;
            PrimaryButtonLabel = primaryButtonLabel;
            SecondaryButtonLabel = secondaryButtonLabel;
            Tips = tips ?? System.Array.Empty<string>();
        }

        public TutorialStepKind Kind { get; }
        public TutorialLessonId LessonId { get; }
        public string Title { get; }
        public string Subtitle { get; }
        public string PrimaryButtonLabel { get; }
        public string SecondaryButtonLabel { get; }
        public string[] Tips { get; }
    }

    public static class TutorialLessonDefinitions
    {
        public const float EnemyBulletSpeed = 270f;
        public const float SkillParryRageGain = 28f;
        public const float BreakWeakZoneRagePerSecond = 82f;
        public const float BreakWeakZoneRadius = 44f;
        public const float LessonOneReceiverWidthPadding = 72f;
        public const float LessonOneReceiverY = 72f;
        public const float LessonOneReceiverHeight = 16f;

        public static readonly TutorialStepDefinition[] Steps =
        {
            new(
                TutorialStepKind.Card,
                TutorialLessonId.None,
                "Tutorial phản đòn",
                "Một scene riêng chỉ để dạy phản đòn, skill và boss break.\nKhông có boss rush sau khi xong.",
                "Bắt đầu tutorial",
                "Về Boss Rush",
                "Flow: 3 bài phản đòn -> skill -> boss break -> kết thúc.",
                "Fail rule: trúng 1 hit ở bài nào thì reset đúng bài đó.",
                "Goal: làm rõ mechanic cho người chơi mới."),
            new(
                TutorialStepKind.Card,
                TutorialLessonId.None,
                "Giới thiệu phản đòn",
                "Đạn tím có thể phản đòn.\nPhản đòn bằng cách hất nhanh lên phía trước.\nKhông có nút phản đòn riêng.",
                "Vào bài 1",
                null,
                "Giữ feel mobile: kéo nhân vật bằng chạm hoặc drag.",
                "Player nằm hơi phía trên ngón tay như pointer offset hiện tại.",
                "Hitbox player là hình tròn ở giữa sprite, vùng phản đòn lớn hơn sprite một chút."),
            new(TutorialStepKind.Lesson, TutorialLessonId.Lesson1, string.Empty, string.Empty, string.Empty),
            new(TutorialStepKind.Lesson, TutorialLessonId.Lesson2, string.Empty, string.Empty, string.Empty),
            new(TutorialStepKind.Lesson, TutorialLessonId.Lesson3, string.Empty, string.Empty, string.Empty),
            new(
                TutorialStepKind.Card,
                TutorialLessonId.None,
                "Giới thiệu skill",
                "Phản đòn sẽ nạp Rage.\nKhi thanh đầy, bấm SKILL hoặc Space / E để tung skill.",
                "Vào bài skill",
                null,
                "Trong bài sau, bạn cần phản đòn để nạp đầy Rage.",
                "Khi nút SKILL sáng lên, dùng ngay để phá mục tiêu."),
            new(TutorialStepKind.Lesson, TutorialLessonId.Skill, string.Empty, string.Empty, string.Empty),
            new(
                TutorialStepKind.Card,
                TutorialLessonId.None,
                "Boss break / weak zone",
                "Khi boss đang break, player có thể đứng trong weak zone để nạp Rage rất nhanh.",
                "Vào bài break",
                null,
                "Bài cuối vẫn là tutorial-only scene.",
                "Hoàn tất tutorial xong mới hiện nút quay lại boss rush."),
            new(TutorialStepKind.Lesson, TutorialLessonId.Break, string.Empty, string.Empty, string.Empty),
            new(
                TutorialStepKind.End,
                TutorialLessonId.None,
                "Tutorial hoàn tất",
                "Bạn đã đi hết flow tutorial-only.\nScene kết thúc ở đây và chỉ cho bạn quay lại boss rush.",
                "Back to Boss Rush",
                "Chơi lại tutorial",
                "Tutorial không tự nhảy sang boss rush.",
                "CTA cuối chỉ còn một hướng rõ ràng: quay lại boss rush.")
        };
    }
}
