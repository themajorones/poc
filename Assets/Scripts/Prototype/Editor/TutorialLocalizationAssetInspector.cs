#if UNITY_EDITOR
using CupHeadClone.Prototype;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CupHeadClone.PrototypeEditor
{
    [CustomEditor(typeof(TutorialLocalizationAsset))]
    public sealed class TutorialLocalizationAssetInspector : Editor
    {
        private ReorderableList _localeList;

        private static readonly (string Title, (string key, string label, bool multiline)[] Fields)[] Sections =
        {
            ("Menu", new[]
            {
                ("menu.title", "Title", false),
                ("menu.subtitle", "Subtitle", true),
                ("menu.start", "Start Button", false),
                ("menu.tutorial", "Tutorial Button", false)
            }),
            ("Boss Rush", new[]
            {
                ("bossrush.title.start", "Start Title", false),
                ("bossrush.subtitle.start", "Start Subtitle", true),
                ("bossrush.title.win", "Win Title", false),
                ("bossrush.subtitle.win", "Win Subtitle", true),
                ("bossrush.title.lose", "Lose Title", false),
                ("bossrush.subtitle.lose", "Lose Subtitle", true),
                ("bossrush.start", "Start Button", false),
                ("bossrush.tutorial", "Tutorial Button", false),
                ("bossrush.restart", "Restart Button", false)
            }),
            ("Settings", new[]
            {
                ("settings.title", "Title", false),
                ("settings.master", "Master Label", false),
                ("settings.music", "Music Label", false),
                ("settings.sfx", "SFX Label", false),
                ("settings.language", "Language Label", false),
                ("settings.open", "Open Button", false),
                ("settings.close", "Close Button", false)
            }),
            ("Tutorial HUD", new[]
            {
                ("hud.hp_rule", "HP Rule", false),
                ("hud.footer_hint", "Footer Hint", true),
                ("hud.rage_ready_mobile", "Rage Ready Mobile", false),
                ("hud.rage_ready_desktop", "Rage Ready Desktop", false),
                ("hud.rage_charge", "Rage Charging", false)
            }),
            ("Tutorial Cards", new[]
            {
                ("card.intro.title", "Intro Title", false),
                ("card.intro.subtitle", "Intro Subtitle", true),
                ("card.intro.primary", "Intro Primary", false),
                ("card.intro.secondary", "Intro Secondary", false),
                ("card.parry.title", "Parry Title", false),
                ("card.parry.subtitle", "Parry Subtitle", true),
                ("card.parry.primary", "Parry Primary", false),
                ("card.skill.title", "Skill Title", false),
                ("card.skill.subtitle", "Skill Subtitle", true),
                ("card.skill.primary", "Skill Primary", false),
                ("card.break.title", "Break Title", false),
                ("card.break.subtitle", "Break Subtitle", true),
                ("card.break.primary", "Break Primary", false),
                ("card.end.title", "End Title", false),
                ("card.end.subtitle", "End Subtitle", true),
                ("card.end.primary", "End Primary", false),
                ("card.end.secondary", "End Secondary", false)
            }),
            ("Lesson Flow", new[]
            {
                ("lesson.fail.title", "Fail Title", false),
                ("lesson.fail.subtitle", "Fail Subtitle", false),
                ("lesson.restart.title", "Restart Title", false),
                ("lesson.restart.subtitle", "Restart Subtitle", false)
            }),
            ("Lesson 1", new[]
            {
                ("lesson.1.tag", "Tag", false),
                ("lesson.1.objective", "Objective", true),
                ("lesson.1.hint", "Hint", true),
                ("lesson.1.progress", "Progress", false),
                ("lesson.1.banner.title", "Banner Title", false),
                ("lesson.1.banner.subtitle", "Banner Subtitle", false),
                ("lesson.1.complete.title", "Complete Title", false),
                ("lesson.1.complete.subtitle", "Complete Subtitle", false)
            }),
            ("Lesson 2", new[]
            {
                ("lesson.2.tag", "Tag", false),
                ("lesson.2.objective", "Objective", true),
                ("lesson.2.hint", "Hint", true),
                ("lesson.2.progress", "Progress", false),
                ("lesson.2.banner.title", "Banner Title", false),
                ("lesson.2.banner.subtitle", "Banner Subtitle", false),
                ("lesson.2.complete.title", "Complete Title", false),
                ("lesson.2.complete.subtitle", "Complete Subtitle", false)
            }),
            ("Lesson 3", new[]
            {
                ("lesson.3.tag", "Tag", false),
                ("lesson.3.objective", "Objective", true),
                ("lesson.3.hint", "Hint", true),
                ("lesson.3.progress", "Progress", false),
                ("lesson.3.banner.title", "Banner Title", false),
                ("lesson.3.banner.subtitle", "Banner Subtitle", false),
                ("lesson.3.complete.title", "Complete Title", false),
                ("lesson.3.complete.subtitle", "Complete Subtitle", false)
            }),
            ("Lesson 4", new[]
            {
                ("lesson.4.tag", "Tag", false),
                ("lesson.4.objective", "Objective", true),
                ("lesson.4.hint", "Hint", true),
                ("lesson.4.progress", "Progress", false),
                ("lesson.4.banner.title", "Banner Title", false),
                ("lesson.4.banner.subtitle", "Banner Subtitle", false),
                ("lesson.4.complete.title", "Complete Title", false),
                ("lesson.4.complete.subtitle", "Complete Subtitle", false)
            }),
            ("Lesson 5", new[]
            {
                ("lesson.5.tag", "Tag", false),
                ("lesson.5.objective.enter", "Objective Enter", false),
                ("lesson.5.hint.enter", "Hint Enter", false),
                ("lesson.5.objective.charge", "Objective Charge", false),
                ("lesson.5.hint.charge", "Hint Charge", false),
                ("lesson.5.objective.skill", "Objective Skill", false),
                ("lesson.5.hint.skill", "Hint Skill", false),
                ("lesson.5.progress.enter", "Progress Enter", false),
                ("lesson.5.progress.charge", "Progress Charge", false),
                ("lesson.5.progress.skill", "Progress Skill", false),
                ("lesson.5.banner.title", "Banner Title", false),
                ("lesson.5.banner.subtitle", "Banner Subtitle", false),
                ("lesson.5.rage_full.title", "Rage Full Title", false),
                ("lesson.5.rage_full.subtitle", "Rage Full Subtitle", false),
                ("lesson.5.complete.title", "Complete Title", false),
                ("lesson.5.complete.subtitle", "Complete Subtitle", false)
            })
        };

        private void OnEnable()
        {
            _localeList = new ReorderableList(serializedObject, serializedObject.FindProperty("locales"), true, true, true, true);
            _localeList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Locales (+ / - / drag to reorder). First item is the default locale.");
            };
            _localeList.elementHeight = EditorGUIUtility.singleLineHeight * 2f + 10f;
            _localeList.drawElementCallback = (rect, index, active, focused) =>
            {
                var element = _localeList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2f;
                var codeRect = new UnityEngine.Rect(rect.x, rect.y, 110f, EditorGUIUtility.singleLineHeight);
                var nameRect = new UnityEngine.Rect(rect.x + 120f, rect.y, rect.width - 120f, EditorGUIUtility.singleLineHeight);
                var infoRect = new UnityEngine.Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 4f, rect.width, EditorGUIUtility.singleLineHeight);

                var codeProperty = element.FindPropertyRelative("localeCode");
                var nameProperty = element.FindPropertyRelative("displayName");

                codeProperty.stringValue = TutorialLocalizationAssetEditor.NormalizeLocaleCode(EditorGUI.TextField(codeRect, codeProperty.stringValue));
                nameProperty.stringValue = EditorGUI.TextField(nameRect, nameProperty.stringValue);
                EditorGUI.LabelField(infoRect, index == 0 ? "Default locale" : "Alternative locale");
            };
            _localeList.onAddCallback = list =>
            {
                var index = list.serializedProperty.arraySize;
                list.serializedProperty.InsertArrayElementAtIndex(index);
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("localeCode").stringValue = $"locale{index + 1}";
                element.FindPropertyRelative("displayName").stringValue = "New Locale";
                serializedObject.ApplyModifiedProperties();
            };
            _localeList.onRemoveCallback = list =>
            {
                if (list.index < 0 || list.index >= list.serializedProperty.arraySize)
                {
                    return;
                }

                list.serializedProperty.DeleteArrayElementAtIndex(list.index);
                serializedObject.ApplyModifiedProperties();
            };
            _localeList.onReorderCallback = _ => serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _localeList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

            var asset = (TutorialLocalizationAsset)target;
            TutorialLocalizationAssetEditor.NormalizeLocales(asset);

            DrawActiveLocale(asset);
            EditorGUILayout.Space(8f);

            var table = GetActiveTable(asset);
            if (table == null)
            {
                return;
            }

            foreach (var section in Sections)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField(section.Title, EditorStyles.boldLabel);
                foreach (var field in section.Fields)
                {
                    DrawEntry(table, field.key, field.label, field.multiline);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(4f);
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(asset);
            }
        }

        private static void DrawActiveLocale(TutorialLocalizationAsset asset)
        {
            var locales = asset.Locales;
            if (locales.Count == 0)
            {
                return;
            }

            var names = new string[locales.Count];
            var selectedIndex = 0;
            for (var i = 0; i < locales.Count; i++)
            {
                names[i] = string.IsNullOrWhiteSpace(locales[i].DisplayName) ? locales[i].LocaleCode : locales[i].DisplayName;
                if (locales[i].LocaleCode == asset.ActiveLocale)
                {
                    selectedIndex = i;
                }
            }

            var nextIndex = EditorGUILayout.Popup("Active Locale", selectedIndex, names);
            asset.ActiveLocale = locales[nextIndex].LocaleCode;
        }

        private static TutorialLocaleTable GetActiveTable(TutorialLocalizationAsset asset)
        {
            var locales = asset.Locales;
            for (var i = 0; i < locales.Count; i++)
            {
                if (locales[i].LocaleCode == asset.ActiveLocale)
                {
                    return locales[i];
                }
            }

            return locales.Count > 0 ? locales[0] : null;
        }

        private static void DrawEntry(TutorialLocaleTable table, string key, string label, bool multiline)
        {
            var current = table.Get(key);
            string next;
            if (multiline)
            {
                EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
                next = EditorGUILayout.TextArea(current, GUILayout.MinHeight(38f));
            }
            else
            {
                next = EditorGUILayout.TextField(label, current);
            }

            if (next != current)
            {
                table.Set(key, next);
                GUI.changed = true;
            }
        }
    }
}
#endif
