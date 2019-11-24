using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor_Helper
{
    /// <summary>
    /// Хранилище статичных переменных
    /// </summary>
    public static class EH_StaticParametrs
    {
        public const string VERSION = "V 1.9.21";

        #region StringsConst
        public const string TUTOR_IN_START = "\tEditorHelper - предназначен для упрощения разработки и тестирования приложений. " +
                                          "Внизу есть вкладка Editor где можно включать, отключать и настраивать имеющиеся функции.\n" +
                                          "\tБольшинство параметров окна сохраняются в EditorPrefs и привзяваются к productName, " +
                                          "поэтому при смене названия имейте ввиду, что почти все настройки редактора собьются.";
        public const string TUTUR_TIME_SCALE = "\tУменьшение TimeScale позволяет (в большинстве случаев) замедлять игровой " +
                                             "процесс. В настройках можно выставить минимальное и максимальное значение. " +
                                             "По умолчанию минимальное значение стоит 0.00001f, тк при 0 аппа может ставиться на паузу.";
        public const string TUTOR_SCENES = "\tВкладка Scenes позволяет быстро переходить между сценами. По умолчанию редактор " +
                                          "подтягивает сцены забитые в BuildSettings. В настройках можно добавлять, удалять и " +
                                          "переименовывать сцены.";
        public const string TUTOR_AUTO_SAVE = "\tВкладка AutoSaveScene позволяет в процессе разработки автоматических сохранять " +
                                            "сцену. Выставите интервал сохранения. Галочка Use Notification AutoSave отвечает " +
                                            "за окно подтверждения сохранения, Use AutoSave за включение функции автосохранения.";
        public const string TUTOR_CLEAR_PREFS = "\tВкладка с кнопкой очистки префсов. Содержит скрытую кнопки очистки EditorPrefs " +
                                              "(Использовать в случае крайней необходимости!)";
        public const string TUTOR_SCREEN_SHOTS = "\tВо вкладке ScreenShot есть кнопка для создания скриншотов разных разрешений " +
                                              "одним нажатием.\n" +
                                              "\t1) Выберите папку куда будут сохраняться скрины. Если её не задавать они будут по " +
                                              "умолчанию сохраняться в папку Screenshots в папке с проектом. Адреса папок сохранятся " +
                                              "идивидуально для каждого проекта.\n" +
                                              "\t2) Добавьте разрешение для которых надо сделать скрины. Разрешения добавляются " +
                                              "на все платформы и хранятся в EditorPrefs, поэтому будут кочевать в другие проекты, на " +
                                              "другие платформы и на другие версии юнити. Они добавляются в редактор при запуске " +
                                              "EditorHelper и проверяются/добавляются при сохранении скриншота.\n" +
                                              "\t3) Если надо сделать скрины для конкретного разрешения или группы, уберите галочки " +
                                              "у неактуальных разрешений.\n" +
                                              "\t4) Галочка DisableInterface отвечает за отключение интерфейса при создании скрина.\n" +
                                              "\tPS: Скрины делаются с задержкой 0,5 секунд, тк они не сразу сохраняются.";
        public const string TUTOR_CHEATS = "\tВкладка Cheats отвечает за быстрое изменение префсов. \n" +
                                         "\t1) В настройках можно добавлять префсы руками поштучно. (Add cheat)\n" +
                                         "\t2) Найти автоматически в проекте. (Find all prefs). " +
                                         "Автоматически префсы находятся только с указанием простых ключей (PlayerPrefs.SetInt(\"Gold\",gold)).\n" +
                                         "\t Скрипт умеет работать как с PlayerPrefs так и с PlayerPrefsHelper (если в нем есть SetBool!). Для использования " +
                                         "второго надо добавить директиву PLAYER_PREFS_HELPER, она добавляется автоматически при открытие " +
                                         "окна. Но если вдруг надо удалить или добавить есть кнопка FIX PLAYER_PREFS_HELPER!";

        public const string EDITOR = "Editor";

        public const string ATTENTION = "Внимание!";

        public const string URL_LAST_VERSION = "https://github.com/Bald42/EditorHelper/tree/release";

        public const string URL_ALL_VERSION = "https://github.com/Bald42/EditorHelper/tree/old_versions";

        #endregion

        public static bool IsActiveClearPrefs = true;
        public static bool IsActiveCheats = true;
        public static bool IsActiveTimeScale = true;
        public static bool IsActiveScenes = true;
        public static bool IsActiveAutoSave = true;
        public static bool IsActiveScreenShot = true;

        public static float minTimeScale = float.Epsilon;
        public static float maxTimeScale = 5f;

        /// <summary>
        /// Сохраняем какие функции мы будем использовать
        /// </summary>
        public static void SaveEditorParams()
        {
            //TODO временное решение
            EditorPrefs.SetBool(Application.productName + "IsActiveCheats", IsActiveCheats);
            EditorPrefs.SetBool(Application.productName + "IsActiveTimeScale", IsActiveTimeScale);
            EditorPrefs.SetBool(Application.productName + "IsActiveScenes", IsActiveScenes);
            EditorPrefs.SetBool(Application.productName + "IsActiveAutoSave", IsActiveAutoSave);
            EditorPrefs.SetBool(Application.productName + "IsActiveClearPrefs", IsActiveClearPrefs);
            EditorPrefs.SetBool(Application.productName + "IsActiveScreenShot", IsActiveScreenShot);
        }

        /// <summary>
        /// Загружаем какие функции мы будем использовать
        /// </summary>
        public static void LoadEditorParams()
        {
            IsActiveCheats = EditorPrefs.GetBool(Application.productName + "IsActiveCheats", true);
            IsActiveTimeScale = EditorPrefs.GetBool(Application.productName + "IsActiveTimeScale", true);
            IsActiveScenes = EditorPrefs.GetBool(Application.productName + "IsActiveScenes", true);
            IsActiveAutoSave = EditorPrefs.GetBool(Application.productName + "IsActiveAutoSave", true);
            IsActiveClearPrefs = EditorPrefs.GetBool(Application.productName + "IsActiveClearPrefs", true);
            IsActiveScreenShot = EditorPrefs.GetBool(Application.productName + "IsActiveScreenShot", true);
        }
    }
}