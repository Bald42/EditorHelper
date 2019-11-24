using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;

namespace Editor_Helper
{
    /// <summary>
    /// Инструмент для упрощения разработки, отладки и тестирования
    /// </summary>
    public class EditorHelper : EditorWindow
    {
        public static EditorHelper Instance = null;

        private Vector2 scrollPosGlobal = Vector2.zero;
        private Vector2 scrollPosEditor = Vector2.zero;
        private Vector2 scrollTempPrefs = Vector2.zero;
        private Vector2 scrollTempHardPrefs = Vector2.zero;

        //TODO сделать адекватно, отдельными классами
        public List<ClassScenes> classScenes = new List<ClassScenes>();
        private List<ClassPrefs> classPrefs = new List<ClassPrefs>();
        private List<ClassPrefs> tempFindClassPrefs = new List<ClassPrefs>();
        private ClassPrefs tempClassPrefs = new ClassPrefs();
        private ClassAutoSave classAutoSave = new ClassAutoSave();
        public ClassScreenShot classScreenShot = null;
         
        private bool isActiveEditor = false;
        private bool isEditorSave = false;

        private bool isViewClearPrefs = false;
        private bool isViewClearEditorPrefs = false;
        private bool isViewCheats = false;
        private bool isViewCheatsEdit = false;
        private bool isViewCreateCheats = false;
        private bool isViewFindAllPrefs = false;
        private bool isViewTimeScale = false;
        private bool isViewScenes = false;
        
        private bool isViewAutoSave = false;
        private bool isViewScreenShot = false;
        private bool isViewScreenShotParams = false;
        private bool isFixTimeScale = false;
        private bool isScreenShotDisableInterface = false;

        private List<string> listAllPathScripts = new List<string>();
        private List<string> listAllScriptsWithPlayerPrefs = new List<string>();
        private List<string> listPathsAllScriptsWithPlayerPrefs = new List<string>();
        private List<string> listAllPrefs = new List<string>();
        private List<string> listHardPrefs = new List<string>();

        private const float FIX_TIME_SCALE = 0.02F;

        #region StartMethods
        [MenuItem("Tools/EditorHelper")]
        /// <summary>
        /// Инициализация
        /// Обязательно должна быть статичной!!!!
        /// </summary>
        private static void Init()
        {
            EditorHelper window = (EditorHelper)EditorWindow.GetWindow(typeof(EditorHelper));
            Instance = window;
            Instance.title = "EditorHelper";
            Instance.Show();
        }

        private void OnDestroy ()
        {
            Instance = null;
        }

        /// <summary>
        /// Показываем окно
        /// </summary>
        //TODO не помню зачем это, но кажется это песполезная хрень
        /*public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(EditorHelper));
        }*/

        private void Awake()
        {
            EditorUtility.DisplayDialog(EH_StaticParametrs.ATTENTION, EH_StaticParametrs.TUTOR_IN_START + "\n\n" + EH_StaticParametrs.VERSION, "Ok");
            classScreenShot = new ClassScreenShot();
            FindScriptPlayerPrefsHelper();
            CheckClassScene();
            CheckClassPrefs();
            CheckAutoSave();
            CheckScreenShots();
            LoadEditorParams();
        }

        /// <summary>
        /// Поиск PlayerPrefsHelper.cs в проекте
        /// </summary>
        private void FindScriptPlayerPrefsHelper()
        {
            string[] assetPaths = AssetDatabase.GetAllAssetPaths();
            foreach (string assetPath in assetPaths)
            {
                if (assetPath.Contains("PlayerPrefsHelper.cs"))
                {
                    //Если даже PlayerPrefsHelper существует, но в нём нет SetBool 
                    //скрипт сломается, поэтому добавляем проверку и игнорим
                    //сомнительный костыль :( тк он может быть закоменчен 
                    string tempScript = File.ReadAllText(assetPath);
                    if (tempScript.Contains("SetBool") || tempScript.Contains("GetBool"))
                    {
                        AddDirectivePlayerPrefsHelper();
                    }
                }
            }
        }

        /// <summary>
        /// Чистим информацию о кнопках в EditorPrefs, 
        /// скорей всего будет использоваться только для тестов
        /// </summary>
        private void ClearPrefsButtonScene()
        {
            Debug.Log("<color=red>ClearPrefsButtonScene</color>");
            for (int i = 0; i < 100; i++)
            {
                if (EditorPrefs.HasKey(Application.productName + "PathScene" + i))
                {
                    EditorPrefs.DeleteKey(Application.productName + "NameScene" + i);
                    EditorPrefs.DeleteKey(Application.productName + "PathScene" + i);
                }
                else
                {
                    i = 100;
                }
            }
        }

        /// <summary>
        /// Проверяем есть ли у нас параметры кнопок, для смены сцен
        /// </summary>
        private void CheckClassScene()
        {
            if (!EditorPrefs.HasKey(Application.productName + "PathScene0"))
            {
                for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                {
                    if (EditorBuildSettings.scenes.Length == 0)
                    {
                        i = 100;
                    }

                    ClassScenes tempClassScenes = new ClassScenes();
                    string tempPath = EditorBuildSettings.scenes[i].path;
                    string[] tempArrayPath = tempPath.Split('/');
                    string tempName = tempArrayPath[tempArrayPath.Length - 1].Replace(".unity", "");

                    tempClassScenes.PathScene = tempPath.Replace(".unity", "");
                    tempClassScenes.NameScene = tempName;
                    tempClassScenes.SceneObject = AssetDatabase.LoadAssetAtPath(tempPath, typeof(object));

                    classScenes.Add(tempClassScenes);
                }
            }
            else
            {
                for (int i = 0; i < 100; i++)
                {
                    if (EditorPrefs.HasKey(Application.productName + "PathScene" + i))
                    {
                        ClassScenes tempClassScenes = new ClassScenes();
                        tempClassScenes.NameScene = EditorPrefs.GetString(Application.productName + "NameScene" + i);
                        tempClassScenes.PathScene = EditorPrefs.GetString(Application.productName + "PathScene" + i);
                        tempClassScenes.SceneObject = AssetDatabase.LoadAssetAtPath(tempClassScenes.PathScene + ".unity", typeof(object));
                        classScenes.Add(tempClassScenes);
                    }
                    else
                    {
                        i = 100;
                    }
                }
            }
        }

        /// <summary>
        /// Загружаем из префсов редактора, параметры для быстрого изменения плеер префсов
        /// </summary>
        private void CheckClassPrefs()
        {
            if (EditorPrefs.HasKey(Application.productName + "NamePrefs0"))
            {
                for (int i = 0; i < 100; i++)
                {
                    if (EditorPrefs.HasKey(Application.productName + "NamePrefs" + i))
                    {
                        ClassPrefs tempClassPrefs = new ClassPrefs();
                        tempClassPrefs.NamePrefs = EditorPrefs.GetString(Application.productName + "NamePrefs" + i);
                        tempClassPrefs.TypeThisPrefs = ClassPrefs.SetTypePrefs(EditorPrefs.GetString(Application.productName + "TypePrefs" + i));
                        tempClassPrefs.ValuePrefs = EditorPrefs.GetString(Application.productName + "ValuePrefs" + i);
                        tempClassPrefs.IsPlayerPrefsHelper =
                            EditorPrefs.GetString(Application.productName + "IsHelperPrefs" + i) == "True" ? true : false;
#if PLAYER_PREFS_HELPER
                    if (tempClassPrefs.TypeThisPrefs == ClassPrefs.TypePrefs.Bool)
                    {
                        tempClassPrefs.IsBool = tempClassPrefs.ValuePrefs == "True" ? true : false;
                    }
#endif
                        classPrefs.Add(tempClassPrefs);
                    }
                    else
                    {
                        i = 100;
                    }
                }
            }
        }

        /// <summary>
        /// Загружаем параметры автосейва
        /// </summary>
        private void CheckAutoSave()
        {
            if (EditorPrefs.HasKey(Application.productName + "IsActiveAutoSave"))
            {
                classAutoSave.GetClassAutoSave(classAutoSave);
                classAutoSave.IsActiveGui = !classAutoSave.IsActive;
                classAutoSave.IsActiveNotificationGui = !classAutoSave.IsActiveNotification;
            }
        }

        /// <summary>
        /// Загружаем параметры скриншотов
        /// </summary>
        private void CheckScreenShots()
        {
            classScreenShot.PathFolderForScreenShot = EditorPrefs.GetString(Application.productName + "PathForScreenShots");
            for (int i = 0; i < 100; i++)
            {
                if (EditorPrefs.HasKey("Resolution" + i + "NameResolution"))
                {
                    ClassResolution tempClassResolution = new ClassResolution();
                    classScreenShot.M_ClassResolutionScreenShots.Add(tempClassResolution);

                    classScreenShot.M_ClassResolutionScreenShots[i].NameResolution =
                    EditorPrefs.GetString("Resolution" + i + "NameResolution");
                    classScreenShot.M_ClassResolutionScreenShots[i].Width =
                    EditorPrefs.GetInt("Resolution" + i + "Width");
                    classScreenShot.M_ClassResolutionScreenShots[i].Height =
                    EditorPrefs.GetInt("Resolution" + i + "Height");

                    AddResolution(classScreenShot.M_ClassResolutionScreenShots[i].NameResolution,
                    classScreenShot.M_ClassResolutionScreenShots[i].Width,
                    classScreenShot.M_ClassResolutionScreenShots[i].Height);
                }
                else
                {
                    i = 100;
                }
            }
        }

        /// <summary>
        /// Добавляем новое разрешение во время создания скриншота
        /// </summary>
        public void AddResolution(string name, int width, int height)
        {
            if (ClassScreenShot.FindSize(ClassScreenShot.GetCurrentGroupType(), name) == -1)
            {
                ClassScreenShot.AddCustomSize(ClassScreenShot.GetCurrentGroupType(),
                    width,
                    height,
                    name);
            }
        }
        #endregion StartMethods

        #region Updates
        /// <summary>
        /// Отрисовка гуя
        /// </summary>
        private void OnGUI()
        {
            scrollPosGlobal = GUILayout.BeginScrollView(scrollPosGlobal);

            ViewGuiScenesButtons();
            ViewGuiAutoSave();
            ViewGuiClearPrefs();
            ViewGuiScreenShot();
            ViewGuiTimeScale();
            ViewGuiCheats();

            //TODO дописать управление с клавы
            //Хуйня
            //KeyboardControl();

            GUILayout.EndScrollView();

            ViewEditor();
        }

        //private void Update()
        //{
        //    KeyboardControl();
        //}

        private void OnInspectorUpdate()
        {
            TrySave();
            TryMakeScreenShot();
            //KeyboardControl();
        }
        #endregion Updates

        #region Editor
        /// <summary>
        /// Метод редактирования окна
        /// </summary>
        private void ViewEditor()
        {
            GUILayout.FlexibleSpace();
            scrollPosEditor = GUILayout.BeginScrollView(scrollPosEditor);

            GUILayout.Label(EH_StaticParametrs.VERSION, EditorStyles.boldLabel);

            if (GUILayout.Button(EH_StaticParametrs.EDITOR))
            {
                EH_Editor.Open();
            }


            GUILayout.Label("------------------------", EditorStyles.boldLabel);
            isActiveEditor = GUILayout.Toggle(isActiveEditor,
                                                (isActiveEditor == true ? "↑  " : "↓  ") + "Editor",
                                                EditorStyles.boldLabel);
            if (isActiveEditor)
            {
                //ViewEditorTimeScale();
                //ViewEditorScenes();
                //ViewEditorAutoSave();
                //ViewEditorClearPrefs();
                //ViewEditorScreenShot();
                ViewCheatsEdit();

                if (isEditorSave)
                {
                    isEditorSave = false;
                }
            }
            else
            {
                if (!isEditorSave)
                {
                    SaveEditorParams();
                }
            }
            GUILayout.EndScrollView();
        }   

        /// <summary>
        /// Сохраняем какие функции мы будем использовать
        /// </summary>
        private void SaveEditorParams()
        {
            isEditorSave = true;
            EH_StaticParametrs.SaveEditorParams();
        }

        /// <summary>
        /// Загружаем какие функции мы будем использовать
        /// </summary>
        private void LoadEditorParams()
        {
            isEditorSave = false;
            EH_StaticParametrs.LoadEditorParams();
        }
        #endregion Editor

        #region CheatsMethods
        /// <summary>
        /// Отрисовка чит панели
        /// </summary>
        private void ViewGuiCheats()
        {
            if (EH_StaticParametrs.IsActiveCheats)
            {
                isViewCheats = GUILayout.Toggle(isViewCheats,
                                                (isViewCheats == true ? "↑  " : "↓  ") + "Cheats",
                                                EditorStyles.boldLabel);
                if (isViewCheats)
                {

                    if (classPrefs.Count == 0)
                    {
                        GUILayout.Label("Не прописанно ни одного префса,\nдобавьте кнопку!", EditorStyles.boldLabel);
                    }
                    else
                    {
                        if (GUILayout.Button("Read all prefs"))
                        {
                            for (int i = 0; i < classPrefs.Count; i++)
                            {
                                SetValuePrefs(i);
                            }
                        }
                    }
                    ViewPrefsButton();
                }
            }
        }

        /// <summary>
        /// Получаем значение префса 
        /// </summary>
        private void SetValuePrefs(int numberPrefs)
        {
            classPrefs[numberPrefs].ValuePrefs = ClassPrefs.GetPrefs(classPrefs[numberPrefs].TypeThisPrefs,
                                                                                   classPrefs[numberPrefs].NamePrefs,
                                                                                   classPrefs[numberPrefs].IsPlayerPrefsHelper);
#if PLAYER_PREFS_HELPER
        if (classPrefs[numberPrefs].TypeThisPrefs == ClassPrefs.TypePrefs.Bool)
        {
            classPrefs[numberPrefs].IsBool = classPrefs[numberPrefs].ValuePrefs == "True" ? true : false;
        }
#endif
        }

        /// <summary>
        /// Отрисовка забитых кнопок префсов
        /// </summary>
        private void ViewPrefsButton()
        {
            for (int i = 0; i < classPrefs.Count; i++)
            {
#if PLAYER_PREFS_HELPER
            if (classPrefs[i].TypeThisPrefs == ClassPrefs.TypePrefs.Bool)
            {
                classPrefs[i].IsBool = GUILayout.Toggle(classPrefs[i].IsBool, "Value");
                classPrefs[i].ValuePrefs = classPrefs[i].IsBool == true ? "True" : "False";
            }
            else
#endif
                {
                    classPrefs[i].ValuePrefs = EditorGUILayout.TextField
                        ("Value (" + classPrefs[i].TypeThisPrefs + "): ", classPrefs[i].ValuePrefs);
                }

#if PLAYER_PREFS_HELPER
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(classPrefs[i].IsPlayerPrefsHelper == true ? "PlayerPrefsHelper" : "PlayerPrefs", EditorStyles.label);

            if (GUILayout.Button("Apply " + classPrefs[i].NamePrefs, GUILayout.MaxWidth(150.0f)))
            {
                ClassPrefs.SetPrefs(classPrefs[i].TypeThisPrefs,
                                    classPrefs[i].NamePrefs,
                                    classPrefs[i].ValuePrefs,
                                    classPrefs[i].IsPlayerPrefsHelper);
            }
            EditorGUILayout.EndHorizontal();
#else
                if (GUILayout.Button("Apply " + classPrefs[i].NamePrefs))
                {
                    ClassPrefs.SetPrefs(classPrefs[i].TypeThisPrefs,
                                        classPrefs[i].NamePrefs,
                                        classPrefs[i].ValuePrefs,
                                        classPrefs[i].IsPlayerPrefsHelper);
                }
#endif
                GUILayout.Space(10f);
            }
        }

        /// <summary>
        /// Отрисовываем редактор читов
        /// </summary>
        private void ViewCheatsEdit()
        {
            isViewCheatsEdit = GUILayout.Toggle(isViewCheatsEdit,
                                                                  (isViewCheatsEdit == true ? "↑↑  " : "↓↓  ") + "Cheats editor",
                                                                  EditorStyles.boldLabel);
            if (isViewCheatsEdit)
            {
                ViewCreateCheats();
                ViewFindAllPrefsInProject();
                ViewFixPlayerPrefsHelper();
                ViewDeleteLastCheat();
                ViewButtonSaveAllCheats();
                GUILayout.Label("------------------------", EditorStyles.boldLabel);
            }
        }

        /// <summary>
        /// Добавляем/удаляем директиву PLAYER_PREFS_HELPER (скорее для тестов, но пусть будет на всякий случай)
        /// </summary>
        private void ViewFixPlayerPrefsHelper()
        {
            if (GUILayout.Button("FIX PLAYER_PREFS_HELPER"))
            {
                int optionFixPlayerPrefsHelper = EditorUtility.DisplayDialogComplex("FIX PLAYER_PREFS_HELPER",
                    "Добавляем/удаляем директиву PLAYER_PREFS_HELPER",
                    "Добавить",
                    "Закрыть",
                    "Удалить");

                switch (optionFixPlayerPrefsHelper)
                {
                    case 0:
                        {
                            AddDirectivePlayerPrefsHelper();
                            break;
                        }
                    case 1:
                        {
                            break;
                        }
                    case 2:
                        {
                            DeleteDirectivePlayerPrefsHelper();
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Добавляем PLAYER_PREFS_HELPER на все платформы
        /// </summary>
        private void AddDirectivePlayerPrefsHelper()
        {
            for (int i = 0; i < Enum.GetNames(typeof(BuildTargetGroup)).Length; i++)
            {
                string scriptingDefineSymbolsForGroup =
                    PlayerSettings.GetScriptingDefineSymbolsForGroup((BuildTargetGroup)i);

                //Добавил проверки на актуальные платформы иначе срёт эрорами на устаревшие платформы
                if (PlayerSettings.GetScriptingDefineSymbolsForGroup((BuildTargetGroup)i) != "")
                {
                    //TODO изъябнись и пофикси!
                    if (!scriptingDefineSymbolsForGroup.Contains(";PLAYER_PREFS_HELPER") &&
                       (i != 2 && i != 15 && i != 16 && i != 22))
                    {
                        scriptingDefineSymbolsForGroup += ";PLAYER_PREFS_HELPER";
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(
                            (BuildTargetGroup)i,
                            scriptingDefineSymbolsForGroup);
                    }
                }
            }
            //По идеи он должен начать компилить скрипты, а потом выдавать сообщение, но я хз почему происходит наоборот
            EditorUtility.DisplayDialog("",
                "Добавляем директиву PLAYER_PREFS_HELPER на ВСЕ актуальные платформы для фикса хэлпера.\n" +
                "Подожди пару секунд пока скоммпилятся скрипты!",
                "Ok");
        }

        /// <summary>
        /// Добавляем PLAYER_PREFS_HELPER на все платформы
        /// </summary>
        private void DeleteDirectivePlayerPrefsHelper()
        {
            for (int i = 0; i < Enum.GetNames(typeof(BuildTargetGroup)).Length; i++)
            {
                string scriptingDefineSymbolsForGroup =
                    PlayerSettings.GetScriptingDefineSymbolsForGroup((BuildTargetGroup)i);

                //Добавил проверки на актуальные платформы иначе срёт эрорами на устаревшие платформы
                if (PlayerSettings.GetScriptingDefineSymbolsForGroup((BuildTargetGroup)i) != "")
                {
                    //TODO изъябнись и пофикси!
                    if (scriptingDefineSymbolsForGroup.Contains(";PLAYER_PREFS_HELPER") &&
                       (i != 2 && i != 15 && i != 16 && i != 22))
                    {
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(
                            (BuildTargetGroup)i,
                            scriptingDefineSymbolsForGroup.Replace(";PLAYER_PREFS_HELPER", ""));
                    }
                }
            }
            EditorUtility.DisplayDialog("",
                "Удаляем директиву PLAYER_PREFS_HELPER на ВСЕХ актуальных платформах.\n" +
                "Подожди пару секунд пока скоммпилятся скрипты!",
                "Ok");
        }

        /// <summary>
        /// Отрисовываем добавление одного префса
        /// </summary>
        private void ViewCreateCheats()
        {
            isViewCreateCheats = GUILayout.Toggle(isViewCreateCheats,
                (isViewCreateCheats == true ? "↑↑↑  " : "↓↓↓  ") + "Add cheat",
                EditorStyles.boldLabel);

            if (isViewCreateCheats)
            {
                tempClassPrefs.NamePrefs = EditorGUILayout.TextField("Name prefs: ", tempClassPrefs.NamePrefs);
                tempClassPrefs.TypeThisPrefs = (ClassPrefs.TypePrefs)EditorGUILayout.EnumPopup("Change type prefs: ", tempClassPrefs.TypeThisPrefs);
#if PLAYER_PREFS_HELPER
            tempClassPrefs.IsPlayerPrefsHelper = GUILayout.Toggle(tempClassPrefs.IsPlayerPrefsHelper, "Use PlayerPrefsHelper");
#endif
                if (GUILayout.Button("Add cheat"))
                {
                    if (tempClassPrefs.NamePrefs != "")
                    {
#if PLAYER_PREFS_HELPER
                    if (tempClassPrefs.TypeThisPrefs == ClassPrefs.TypePrefs.Bool &&
                        tempClassPrefs.IsPlayerPrefsHelper == false)
                    {
                        Debug.Log("<color=red>У PlayerPrefs-а нет параметра bool !</color>");
                    }
                    else
#endif
                        {
                            ClassPrefs newClassPrefs = new ClassPrefs();
                            classPrefs.Add(newClassPrefs);
                            classPrefs[classPrefs.Count - 1].NamePrefs = tempClassPrefs.NamePrefs;
                            classPrefs[classPrefs.Count - 1].TypeThisPrefs = tempClassPrefs.TypeThisPrefs;
                            classPrefs[classPrefs.Count - 1].IsPlayerPrefsHelper = tempClassPrefs.IsPlayerPrefsHelper;
                            DoSaveAllCheats();
                        }
                    }
                    else
                    {
                        Debug.Log("<color=red>Напиши имя для нового префса</color>");
                    }
                    tempClassPrefs.NamePrefs = "";
                }
            }
        }

        /// <summary>
        /// Отрисовываем удаление последнего префса
        /// </summary>
        private void ViewDeleteLastCheat()
        {
            if (GUILayout.Button("Delete last cheat"))
            {
                if (EditorUtility.DisplayDialog("",
                         "Удаляем последний префс?",
                         "Удалить",
                         "Отмена"))
                {
                    if (classPrefs.Count > 0)
                    {
                        EditorPrefs.DeleteKey(Application.productName + "TypePrefs" + (classPrefs.Count - 1));
                        EditorPrefs.DeleteKey(Application.productName + "NamePrefs" + (classPrefs.Count - 1));
                        EditorPrefs.DeleteKey(Application.productName + "ValuePrefs" + (classPrefs.Count - 1));
                        EditorPrefs.DeleteKey(Application.productName + "IsHelperPrefs" + (classPrefs.Count - 1));
                        classPrefs.RemoveAt(classPrefs.Count - 1);
                    }
                }
            }
        }

        /// <summary>
        /// Отрисовываем кнопку сохранения всех префсов
        /// </summary>
        private void ViewButtonSaveAllCheats()
        {
            if (GUILayout.Button("Save all cheats"))
            {
                DoSaveAllCheats();
            }
        }

        /// <summary>
        /// Сохраняем все префсы
        /// </summary>
        private void DoSaveAllCheats()
        {
            for (int i = 0; i < classPrefs.Count; i++)
            {
                if (classPrefs.Count > 0)
                {
                    EditorPrefs.SetString(Application.productName + "TypePrefs" + i, classPrefs[i].TypeThisPrefs.ToString());
                    EditorPrefs.SetString(Application.productName + "NamePrefs" + i, classPrefs[i].NamePrefs);
                    EditorPrefs.SetString(Application.productName + "ValuePrefs" + i, classPrefs[i].ValuePrefs);
                    EditorPrefs.SetString(Application.productName + "IsHelperPrefs" + i, classPrefs[i].IsPlayerPrefsHelper.ToString());
                }
            }
            Debug.Log("<color=green>Сохранили все читы в EditorPrefs</color>");
        }

        /// <summary>
        /// Отрисовываем глобальный поиск префсов по проекту
        /// </summary>
        private void ViewFindAllPrefsInProject()
        {
            isViewFindAllPrefs = GUILayout.Toggle(isViewFindAllPrefs,
                (isViewFindAllPrefs == true ? "↑↑↑  " : "↓↓↓  ") + "Find All Prefs",
                EditorStyles.boldLabel);

            if (isViewFindAllPrefs)
            {
                if (GUILayout.Button("Find prefs in all scripts"))
                {
                    FindAllPathScripts();
                    DoScripts();
                    FindAllPrefsInProject();
                }

                ViewTempCheats();

                GUILayout.Label("--------------------", EditorStyles.boldLabel);
            }
        }

        /// <summary>
        /// Поиск путей всех скриптов в проекте
        /// </summary>
        private void FindAllPathScripts()
        {
            listAllPathScripts.Clear();
            string[] assetPaths = AssetDatabase.GetAllAssetPaths();

            for (int i = 0; i < assetPaths.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Ждём пока найдём все скрипты",
                                                (i + 1) + " / " + assetPaths.Length,
                                                (float)i / assetPaths.Length);
                if (assetPaths[i].Contains(".cs") &&
                !assetPaths[i].Contains("EditorHelper.cs") &&
                !assetPaths[i].Contains("PlayerPrefsHelper.cs"))
                {
                    listAllPathScripts.Add(assetPaths[i]);
                }
            }
            EditorUtility.ClearProgressBar();
            Debug.Log("<color=green>Проект содержит </color>" + listAllPathScripts.Count + "<color=green> скриптов</color>");
        }

        /// <summary>
        /// Заводим лист скриптов
        /// </summary>
        private void DoScripts()
        {
            listAllScriptsWithPlayerPrefs.Clear();
            listPathsAllScriptsWithPlayerPrefs.Clear();
            for (int i = 0; i < listAllPathScripts.Count; i++)
            {
                EditorUtility.DisplayProgressBar("Ждём пока скрипты преобразуются в строки",
                                                (i + 1) + " / " + listAllPathScripts.Count,
                                                (float)i / listAllPathScripts.Count);
                string tempScript = File.ReadAllText(listAllPathScripts[i]);
                if (tempScript.Contains("PlayerPrefs.Set") || tempScript.Contains("PlayerPrefsHelper.Set"))
                {
                    listAllScriptsWithPlayerPrefs.Add(tempScript);
                    listPathsAllScriptsWithPlayerPrefs.Add(listAllPathScripts[i]);
                }
            }
            Debug.Log("<color=green>Скриптов содержащих PlayerPrefs </color>" + listAllScriptsWithPlayerPrefs.Count);
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// Находим все префсы в проекте
        /// </summary>
        private void FindAllPrefsInProject()
        {
            listAllPrefs.Clear();
            listHardPrefs.Clear();
            tempFindClassPrefs.Clear();
            string[] tempArrayLine = null;
            string[] tempArrayTwoVolue = null;
            string[] tempArrayKey = null;

            for (int i = 0; i < listAllScriptsWithPlayerPrefs.Count; i++)
            {
                EditorUtility.DisplayProgressBar("Ждём ",
                                                (i + 1) + " / " + listAllScriptsWithPlayerPrefs.Count,
                                                (float)i + 1 / listAllScriptsWithPlayerPrefs.Count);
                tempArrayLine = listAllScriptsWithPlayerPrefs[i].Split('\n');
                for (int j = 0; j < tempArrayLine.Length; j++)
                {
                    //Получаем массив строк содержащих PlayerPrefs.Set
                    if (tempArrayLine[j].Contains("PlayerPrefs.Set") || tempArrayLine[j].Contains("PlayerPrefsHelper.Set"))
                    {
                        tempArrayTwoVolue = tempArrayLine[j].Split('(');
                        for (int k = 0; k < tempArrayTwoVolue.Length; k++)
                        {
                            //Разбиваем строку на подстроки содержащие тип префса и ключ
                            if (tempArrayTwoVolue[k].Contains("PlayerPrefs.Set") || tempArrayTwoVolue[k].Contains("PlayerPrefsHelper.Set"))
                            {
                                int numberStartSimbol = tempArrayTwoVolue[k].IndexOf("PlayerPrefs");
                                tempArrayTwoVolue[k] = tempArrayTwoVolue[k].Substring(numberStartSimbol).Replace(" ", "");

                                tempArrayKey = tempArrayTwoVolue[k + 1].Split(',');

                                if (!tempArrayKey[0].Contains("\""))
                                {
                                    AddHardPrefs(i);
                                }
                                else
                                {
                                    if (!tempArrayKey[0].Contains("+"))
                                    {
                                        tempArrayTwoVolue[k + 1] = tempArrayKey[0].Replace("\"", "");
                                        AddTempCheats(tempArrayTwoVolue[k], tempArrayTwoVolue[k + 1]);
                                        //Дебаги для тестов, нехочу их заново писать в случае поиска ошибок
                                        //Debug.Log("<color=green>Тип префса = !</color>" + tempArrayTwoVolue[k] + "<color=green>!</color>");
                                        //Debug.Log("<color=red>Ключ = !</color>" + tempArrayTwoVolue[k + 1] + "<color=red>!</color>");
                                    }
                                    else
                                    {
                                        AddHardPrefs(i);
                                    }
                                }

                            }
                        }
                    }
                }
            }
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// Делаем лист префсов коотрые записывают сложные ключи
        /// </summary>
        private void AddHardPrefs(int numberScript)
        {
            bool isExists = false;
            for (int i = 0; i < listHardPrefs.Count; i++)
            {
                if (listHardPrefs[i] == listPathsAllScriptsWithPlayerPrefs[numberScript])
                {
                    isExists = true;
                }
            }

            if (!isExists)
            {
                listHardPrefs.Add(listPathsAllScriptsWithPlayerPrefs[numberScript]);
                //Дебаг для отладки
                //Debug.Log("<color=red>В этом скрипте нет явного ключа = !</color>" +
                //listPathsAllScriptsWithPlayerPrefs[numberScript] +
                //"<color=red>!</color>");
            }
        }

        /// <summary>
        /// Добавляем временные префсы, чтоб пользователь мог выбрать нужные 
        /// </summary>
        private void AddTempCheats(string typePrefs, string key)
        {
            ClassPrefs tempClassPrefs = new ClassPrefs();
            string newTypePrefs = "";

            if (typePrefs.Contains("Int"))
            {
                newTypePrefs = "Int";
            }
            else if (typePrefs.Contains("Float"))
            {
                newTypePrefs = "Float";
            }
            else if (typePrefs.Contains("Bool"))
            {
                newTypePrefs = "Bool";
            }
            else if (typePrefs.Contains("String"))
            {
                newTypePrefs = "String";
            }

            bool isExists = false;

            for (int i = 0; i < tempFindClassPrefs.Count; i++)
            {
                if (tempFindClassPrefs[i].NamePrefs == key)
                {
                    bool isPlayerPrefsHelper = typePrefs.Contains("Helper") ? true : false;
                    ClassPrefs.TypePrefs tempTypePrefs = ClassPrefs.SetTypePrefs(newTypePrefs);
                    if (tempFindClassPrefs[i].IsPlayerPrefsHelper == isPlayerPrefsHelper &&
                    tempFindClassPrefs[i].TypeThisPrefs == tempTypePrefs)
                    {
                        //Такой префс уже существует игнорим
                        isExists = true;
                    }
                }
            }

            if (!isExists)
            {
                tempFindClassPrefs.Add(tempClassPrefs);
                tempFindClassPrefs[tempFindClassPrefs.Count - 1].TypeThisPrefs = ClassPrefs.SetTypePrefs(newTypePrefs);
                tempFindClassPrefs[tempFindClassPrefs.Count - 1].NamePrefs = key;
                tempFindClassPrefs[tempFindClassPrefs.Count - 1].ValuePrefs = "";
                tempFindClassPrefs[tempFindClassPrefs.Count - 1].IsPlayerPrefsHelper = typePrefs.Contains("Helper") ? true : false;
            }
        }

        /// <summary>
        /// Отрисовываем потенциально полезные префсы найденные в скриптах
        /// </summary>
        private void ViewTempCheats()
        {
            if (tempFindClassPrefs.Count > 0)
            {
                GUILayout.Space(10f);
                GUILayout.Label("Эти префсы содержаться в проекте:", EditorStyles.boldLabel);
                scrollTempPrefs = GUILayout.BeginScrollView(scrollTempPrefs);
                for (int i = 0; i < tempFindClassPrefs.Count; i++)
                {
                    if (GUILayout.Button("ADD " +
                    (tempFindClassPrefs[i].IsPlayerPrefsHelper == true ? "PlayerPrefsHelper" : "PlayerPrefs") +
                    " " + tempFindClassPrefs[i].TypeThisPrefs +
                    " " + tempFindClassPrefs[i].NamePrefs))
                    {
                        ClassPrefs newClassPrefs = new ClassPrefs();
                        classPrefs.Add(newClassPrefs);
                        classPrefs[classPrefs.Count - 1].NamePrefs = tempFindClassPrefs[i].NamePrefs;
                        classPrefs[classPrefs.Count - 1].TypeThisPrefs = tempFindClassPrefs[i].TypeThisPrefs;
                        classPrefs[classPrefs.Count - 1].IsPlayerPrefsHelper = tempFindClassPrefs[i].IsPlayerPrefsHelper;
                        SetValuePrefs(classPrefs.Count - 1);
                        Debug.Log("<color=green>Добавил префс </color>" + tempFindClassPrefs[i].NamePrefs);
                        DoSaveAllCheats();
                    }
                }
                GUILayout.EndScrollView();
            }

            if (listHardPrefs.Count > 0)
            {
                GUILayout.Space(10f);
                GUILayout.Label("Эти скрипты содержат префсы\n" +
                    "со сложными ключами:", EditorStyles.boldLabel);
                scrollTempHardPrefs = GUILayout.BeginScrollView(scrollTempHardPrefs);
                for (int i = 0; i < listHardPrefs.Count; i++)
                {
                    string scriptName = "";
                    string[] tempScriptName = listHardPrefs[i].Split('/');
                    for (int j = 0; j < tempScriptName.Length; j++)
                    {
                        if (tempScriptName[j].Contains(".cs"))
                        {
                            scriptName = tempScriptName[j];
                        }
                    }

                    if (GUILayout.Button("Open script " + scriptName))
                    {
                        //TODO нормально открыть в редакторе 
                        EditorUtility.RevealInFinder(listHardPrefs[i]);
                        //AssetDatabase.GUIDToAssetPath(listHardPrefs[i]);
                        //AssetDatabase(listHardPrefs[i]);
                        //AssetDatabase.FindAssets(listHardPrefs[i]);
                    }
                }
                GUILayout.EndScrollView();
            }
        }
        #endregion CheatsMethods

        #region TimeScaleMethods
        /// <summary>
        /// Отрисовка слайдера тайм скейла
        /// </summary>
        private void ViewGuiTimeScale()
        {
            if (EH_StaticParametrs.IsActiveTimeScale)
            {
                isViewTimeScale = GUILayout.Toggle(isViewTimeScale,
                (isViewTimeScale == true ? "↑  " : "↓  ") + "Change TimeScale",
                EditorStyles.boldLabel);
                if (isViewTimeScale)
                {
                    Time.timeScale = EditorGUILayout.Slider(Time.timeScale, EH_StaticParametrs.minTimeScale, EH_StaticParametrs.maxTimeScale);
                    Time.fixedDeltaTime = FIX_TIME_SCALE * Time.timeScale;
                }
            }
        }        
        #endregion TimeScaleMethods

        #region ClearPrefsMethods
        /// <summary>Отрисовка кнопки очистки префсов (отдельно, чтоб её можно было
        ///  перемещать по панели от часто используемых кнопок)</summary>
        private void ViewGuiClearPrefs()
        {
            if (EH_StaticParametrs.IsActiveClearPrefs)
            {
                isViewClearPrefs = GUILayout.Toggle(isViewClearPrefs,
                                                    (isViewClearPrefs == true ? "↑  " : "↓  ") + "ClearPrefs",
                                                    EditorStyles.boldLabel);
                if (isViewClearPrefs)
                {
                    ViewGuiClearEditorPrefs();

                    if (GUILayout.Button("Clear PlayerPrefs"))
                    {
#if !PLAYER_PREFS_HELPER
                        PlayerPrefs.DeleteAll();
#else
                    PlayerPrefsHelper.DeleteAll();
#endif
                        Debug.Log("<color=green>Все префсы удалены</color>");
                    }
                }
            }
        }

        private void ViewGuiClearEditorPrefs()
        {
            isViewClearEditorPrefs = GUILayout.Toggle(isViewClearEditorPrefs,
                                                              (isViewClearEditorPrefs == true ? "↑↑  " : "↓↓  ") + "ClearEditorPrefs",
                                                              EditorStyles.boldLabel);
            if (isViewClearEditorPrefs)
            {
                GUILayout.Label("АХТУНГ!!!Может сломаться \nкомпиляция скриптов!!!", EditorStyles.boldLabel);
                if (GUILayout.Button("Clear EditorPrefs"))
                {
                    if (EditorUtility.DisplayDialog("АХТУНГ!!!", "Если ты проигнорировал сообщение над кнопкой знай, " +
                             "что всё может решительно пойти по пизде!!!\n" +
                             "Точно перестают компилиться ВСЕ скрипты в проекте " +
                             "(лечится путём перезапуска Юньки)\n" +
                             "Возмонжно у тебя будет импотенция, сдвинется ось земли, " +
                             "а президентом России станет негр гей. Я хз какие будут последствия!\n" +
                             "Но если тебе очень надо почистить EditorPrefs жмякай ОК на свой страх и риск."
                             , "OK", "Нахуй нахуй"))
                    {
                        Debug.Log("<color=red>Про секс можешь забыть</color>");
                        EditorPrefs.DeleteAll();
                    }
                    else
                    {
                        Debug.Log("<color=green>Одобряю) реально непредсказуемая хуйня</color>");
                    }
                }
            }
        }
        #endregion ClearPrefsMethods

        #region ScenesMethods
        /// <summary>
        /// Отрисовка кнопок переключения сцен
        /// </summary>
        private void ViewGuiScenesButtons()
        {
            if (EH_StaticParametrs.IsActiveScenes)
            {
                isViewScenes = GUILayout.Toggle(isViewScenes,
                                                (isViewScenes == true ? "↑  " : "↓  ") + "Scenes",
                                                EditorStyles.boldLabel);
                if (isViewScenes)
                {
                    for (int i = 0; i < classScenes.Count; i++)
                    {
                        if (GUILayout.Button(classScenes[i].NameScene))
                        {
                            LoadScene(classScenes[i].PathScene, classScenes[i].NameScene);
                        }
                        GUILayout.Space(10f);
                    }
                }
            }
        }
                

        /// <summary>
        /// Загрузка сцены
        /// </summary>
        private void LoadScene(string path, string name)
        {
            if (path != "")
            {
                if (!EditorApplication.isPlaying)
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(path + ".unity");
                    }
                }
                else
                {
                    LoadSceneIsPlayingMode(name);
                }
            }
            else
            {
                Debug.Log("<color=red>Не назначена сцена</color>");
            }
        }

        /// <summary>
        /// Загрузка сцены в плей моде
        /// </summary>
        private void LoadSceneIsPlayingMode(string sceneName)
        {
            Application.LoadLevel(sceneName);
        }
        #endregion ScenesMethods

        #region AutoSaveMethods
        private void ViewGuiAutoSave()
        {
            if (EH_StaticParametrs.IsActiveAutoSave)
            {
                isViewAutoSave = GUILayout.Toggle(isViewAutoSave,
                                                  (isViewAutoSave == true ? "↑  " : "↓  ") + "AutoSaveScene",
                                                  EditorStyles.boldLabel);
                if (isViewAutoSave)
                {
                    GUILayout.Label("Delay autosave seconds", EditorStyles.boldLabel);
                    classAutoSave.IntervalSave = EditorGUILayout.IntSlider(classAutoSave.IntervalSave, 10, 600);
                    classAutoSave.IsActiveNotification = GUILayout.Toggle(classAutoSave.IsActiveNotification, "Use Notification AutoSave");
                    classAutoSave.IsActive = GUILayout.Toggle(classAutoSave.IsActive, "Use AutoSave");

                    if (!classAutoSave.IsActiveGui)
                    {
                        if (!classAutoSave.IsActive)
                        {
                            classAutoSave.IsActiveGui = true;
                            Debug.Log("<color=red>Deactive autosave</color>");
                            classAutoSave.SetClassAutoSave(classAutoSave);
                        }
                    }
                    else
                    {
                        if (classAutoSave.IsActive)
                        {
                            classAutoSave.IsActiveGui = false;
                            Debug.Log("<color=green>Active autosave</color>");
                            classAutoSave.LastTime = EditorApplication.timeSinceStartup;
                            classAutoSave.SetClassAutoSave(classAutoSave);
                        }
                    }

                    if (!classAutoSave.IsActiveNotificationGui)
                    {
                        if (!classAutoSave.IsActiveNotification)
                        {
                            classAutoSave.IsActiveNotificationGui = true;
                            classAutoSave.SetClassAutoSave(classAutoSave);
                        }
                    }
                    else
                    {
                        if (classAutoSave.IsActiveNotification)
                        {
                            classAutoSave.IsActiveNotificationGui = false;
                            classAutoSave.SetClassAutoSave(classAutoSave);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Пытаемся использовать автосейв в редакторе
        /// </summary>
        private void TrySave()
        {
            if (!classAutoSave.IsActive)
            {
                return;
            }

            //Тк в EditorWindow не реализованны инвок и карутина, делаю через жопу
            //Если кто-то подскажет, как реализовать по пацански буду очень благодарен
            if (!EditorApplication.isPlaying &&
                EditorApplication.timeSinceStartup >= classAutoSave.LastTime + classAutoSave.IntervalSave)
            {
                classAutoSave.LastTime = EditorApplication.timeSinceStartup;
                if (classAutoSave.IsActiveNotification)
                {
                    //Стандартное онко почему-то сохраняет изменения даже если ты отказываешься
                    //Хотя в идеале должно использоваться это окно
                    //if (EditorApplication.SaveCurrentSceneIfUserWantsTo())
                    //{
                    //    Debug.Log("<color=green>AutoSaveScene</color>");
                    //    EditorApplication.SaveScene();
                    //}
                    if (EditorUtility.DisplayDialog("",
                         "Сохранить сцену?",
                         "Да",
                         "Нет"))
                    {
                        Debug.Log("<color=green>AutoSaveScene</color>");
                        EditorApplication.SaveScene();
                    }
                }
                else
                {
                    Debug.Log("<color=green>AutoSaveScene</color>");
                    EditorApplication.SaveScene();
                }
            }
        }
        #endregion AutoSaveMethods

        #region ScreenShotMethods
        /// <summary>
        /// Отрисовываем окно для скринов
        /// </summary>
        private void ViewGuiScreenShot()
        {
            if (EH_StaticParametrs.IsActiveScreenShot)
            {
                isViewScreenShot = GUILayout.Toggle(isViewScreenShot,
                                                    (isViewScreenShot == true ? "↑  " : "↓  ") + "ScreenShot",
                                                    EditorStyles.boldLabel);
                if (isViewScreenShot)
                {
                    isScreenShotDisableInterface = GUILayout.Toggle(isScreenShotDisableInterface,
                                                    "DisableInterface");

                    if (classScreenShot.PathFolderForScreenShot != "")
                    {
                        classScreenShot.PathFolderForScreenShot =
                        EditorGUILayout.TextField("PathFolder: ",
                            classScreenShot.PathFolderForScreenShot);
                    }
                    else
                    {
                        GUILayout.Label("Если ты не укажешь папку\n" +
                                        "куда сохранять скрины\n" +
                                        "они по умолчанию сохраняться\n" +
                                        "в Screenshots в папке с проектом", EditorStyles.boldLabel);
                    }

                    if (classScreenShot.M_ClassResolutionScreenShots.Count > 0)
                    {
                        GUILayout.Label("Resolution for ScreenShots:", EditorStyles.boldLabel);
                        for (int i = 0; i < classScreenShot.M_ClassResolutionScreenShots.Count; i++)
                        {
                            //GUILayout.Label(classScreenShot.M_ClassResolutionScreenShots[i].NameResolution +
                            //"  (" + classScreenShot.M_ClassResolutionScreenShots[i].Width + "/" +
                            //classScreenShot.M_ClassResolutionScreenShots[i].Height + ")");
                            classScreenShot.M_ClassResolutionScreenShots[i].isActive =
                            GUILayout.Toggle(classScreenShot.M_ClassResolutionScreenShots[i].isActive,
                                                "  " + classScreenShot.M_ClassResolutionScreenShots[i].NameResolution +
                                                "  (" + classScreenShot.M_ClassResolutionScreenShots[i].Width + "/" +
                                                classScreenShot.M_ClassResolutionScreenShots[i].Height + ")");
                        }
                    }
                    else
                    {
                        GUILayout.Label("Not resolutions for ScreenShots", EditorStyles.boldLabel);
                    }

                    if (GUILayout.Button("Make Screenshot"))
                    {
                        ActiveScreenShot(true);
                    }
                }
            }
        }

        /// <summary>
        /// Запускаем процесс делания скринов
        /// </summary>
        private void ActiveScreenShot(bool isActive)
        {
            if (isActive)
            {
                classScreenShot.LastTime = EditorApplication.timeSinceStartup - 0.6f;
                classScreenShot.CurrentTimeScale = Time.timeScale;
                Time.timeScale = float.Epsilon;
                classScreenShot.CurrentScreenNumber = -1;
                if (isViewTimeScale)
                {
                    isFixTimeScale = true;
                    isViewTimeScale = false;
                }
            }
            else
            {
                Time.timeScale = classScreenShot.CurrentTimeScale;
                if (isFixTimeScale)
                {
                    isFixTimeScale = false;
                    isViewTimeScale = true;
                }
            }
            CheckDisableInterface(isActive);
            classScreenShot.IsActiveScreen = isActive;
        }
               

        /// <summary>
        /// Делаем скрины на разные разрешения с задержкой в пол секунды 
        /// </summary>
        private void TryMakeScreenShot()
        {
            if (classScreenShot != null && classScreenShot.IsActiveScreen)
            {
                if (EditorApplication.timeSinceStartup >= classScreenShot.LastTime + 0.5f)
                {
                    classScreenShot.LastTime = EditorApplication.timeSinceStartup;
                    classScreenShot.CurrentScreenNumber++;

                    if (classScreenShot.M_ClassResolutionScreenShots.Count == 0)
                    {
                        MakeScreenShot();
                        ActiveScreenShot(false);
                    }
                    else if (classScreenShot.CurrentScreenNumber < classScreenShot.M_ClassResolutionScreenShots.Count)
                    {
                        EditorUtility.DisplayProgressBar("Ждём пока доделаются скрины!",
                                                    classScreenShot.CurrentScreenNumber + " / " + classScreenShot.M_ClassResolutionScreenShots.Count,
                                                    (float)classScreenShot.CurrentScreenNumber / classScreenShot.M_ClassResolutionScreenShots.Count);
                        MakeScreenShot();
                    }
                    else
                    {
                        ActiveScreenShot(false);
                        EditorUtility.ClearProgressBar();
                    }
                }
            }
        }

        /// <summary>
        /// Пытаемся сделать скрин
        /// </summary>
        private void MakeScreenShot()
        {
            try
            {
                string filename = "";
                if (classScreenShot.PathFolderForScreenShot == "")
                {
                    SaveScreenShot(filename, classScreenShot.Dir);
                }
                else
                {
                    SaveScreenShot(filename, classScreenShot.PathFolderForScreenShot);
                }
            }
            catch (Exception e)
            {
                Debug.Log("<color=red>Скрин не вышел( </color>" + e.Message);
            }
        }

        /// <summary>
        /// Делаем скриншот
        /// </summary>
        private void SaveScreenShot(string fileName, string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log("<color=red>Создаём папку </color>" + path);
            }

            if (classScreenShot.M_ClassResolutionScreenShots.Count > 0)
            {
                if (classScreenShot.M_ClassResolutionScreenShots[classScreenShot.CurrentScreenNumber].isActive)
                {
                    AddResolution(classScreenShot.M_ClassResolutionScreenShots[classScreenShot.CurrentScreenNumber].NameResolution,
                    classScreenShot.M_ClassResolutionScreenShots[classScreenShot.CurrentScreenNumber].Width,
                    classScreenShot.M_ClassResolutionScreenShots[classScreenShot.CurrentScreenNumber].Height);
                    ClassScreenShot.SetResolution(classScreenShot.M_ClassResolutionScreenShots[classScreenShot.CurrentScreenNumber].NameResolution);
                    fileName = Directory.GetFiles(path).Length +
                    "_" +
                    classScreenShot.M_ClassResolutionScreenShots[classScreenShot.CurrentScreenNumber].NameResolution +
                    ".png";

                    fileName = Path.Combine(path, fileName);
                    //TODO Старый метод, надо дописать для старых версий юнити
                    //Application.CaptureScreenshot(filename, SUPER_SIZE);
                    ScreenCapture.CaptureScreenshot(fileName, classScreenShot.SuperSize);
                    Debug.Log("<color=green>Добавлен скрин </color>" + fileName + "'");
                }
            }
            else
            {
                fileName = Directory.GetFiles(path).Length +
                "_" +
                Application.productName +
                ".png";

                fileName = Path.Combine(path, fileName);
                //TODO Старый метод, надо дописать для старых версий юнити
                //Application.CaptureScreenshot(filename, SUPER_SIZE);
                ScreenCapture.CaptureScreenshot(fileName, classScreenShot.SuperSize);
                Debug.Log("<color=green>Добавлен скрин в текущем разрешении </color>" + fileName + "'");
                Debug.Log("<color=red>В EditorHelper можно добавить неообходимые\n" +
                    "разрешения и делать сразу пачку скринов</color>");
            }

        }

        /// <summary>
        /// Проверяем делять скрины с интерфейсом или без
        /// </summary>
        private void CheckDisableInterface(bool isDisable)
        {
            if (isScreenShotDisableInterface)
            {
                if (isDisable)
                {
                    classScreenShot.UICanvas = null;
                    classScreenShot.UICanvas = FindObjectsOfType<Canvas>();
                }
                ActiveInterface(isDisable);
            }
        }

        /// <summary>
        /// Включаем/отключаем интерфейс
        /// </summary>
        private void ActiveInterface(bool isDisable)
        {
            if (classScreenShot.UICanvas.Length > 0)
            {
                for (int i = 0; i < classScreenShot.UICanvas.Length; i++)
                {
                    classScreenShot.UICanvas[i].enabled = !isDisable;
                }
            }
        }
        #endregion ScreenShotMethods

        /// <summary>
        /// Обрабатываем управление с клавы
        /// </summary>
        private void KeyboardControl()
        {
            //TODO Всё это дерьмо не работает ( разобраться с клавой в редакторе 
            Debug.Log("KeyboardControl");
            //if (Event.current.Equals(Event.KeyboardEvent("[enter]")))
            //{
            //    Debug.LogError("Enter");
            //}

            //if (Event.current.Equals(Event.KeyboardEvent("[u]")))
            //{
            //    Debug.LogError("u");
            //}

            //if (Input.GetKey(KeyCode.P))
            //{
            //    Debug.Log("P");
            //}
            //if (EditorGUI.actionKey)
            //{
            //    Debug.Log("EditorGUI.actionKey = " + EditorGUI.actionKey);
            //}

            //if (Input.GetKey(KeyCode.Space))
            //{
            //    Debug.Log("Spaфффce");
            //}

            //if (Event.current != null && Event.current.Equals(Event.KeyboardEvent("[p]")))
            //{
            //    Debug.Log("enter");
            //}

            //Event tempEvent = Event.current;
            //if (tempEvent != null)
            //{
            //    Debug.Log("tempEvent = " + tempEvent);
            //    switch (tempEvent.type)
            //    {
            //        case EventType.KeyDown:
            //            {
            //                if (Event.current.keyCode == (KeyCode.P))
            //                {
            //                    Debug.Log("P KeyDown");
            //                }
            //                break;
            //            }
            //        case EventType.KeyUp:
            //            {
            //                if (Event.current.keyCode == (KeyCode.P))
            //                {
            //                    Debug.Log("P KeyUp");
            //                }
            //                break;
            //            }
            //        case EventType.MouseMove:
            //            {
            //                Debug.Log("MouseMove");
            //                break;
            //            }
            //        default:
            //            {
            //                break;
            //            }
            //    }
            //}
        }

        /// <summary>
        /// Класс хранящий параметры сцен
        /// </summary>
        [Serializable]
        public class ClassScenes
        {
            public string NameScene = "";
            public string PathScene = "";
            public UnityEngine.Object SceneObject = null;
        }

        #region ClassPrefs
        /// <summary>
        /// Класс хранящий параметры префсов
        /// </summary>
        [Serializable]
        public class ClassPrefs
        {
            public enum TypePrefs
            {
#if PLAYER_PREFS_HELPER
            Bool,
#endif
                Int,
                Float,
                String
            }

            public string ValuePrefs = "";
            public string NamePrefs = "";
            public TypePrefs TypeThisPrefs = TypePrefs.Int;
#if !PLAYER_PREFS_HELPER
            public bool IsPlayerPrefsHelper = false;
#else
        public bool IsPlayerPrefsHelper = true;
#endif
            public bool IsBool = false;

            public static TypePrefs SetTypePrefs(string stringTypePrefs)
            {
                switch (stringTypePrefs)
                {
                    case "Int":
                        {
                            return TypePrefs.Int;
                            break;
                        }
                    case "Float":
                        {
                            return TypePrefs.Float;
                            break;
                        }
                    case "String":
                        {
                            return TypePrefs.String;
                            break;
                        }
#if PLAYER_PREFS_HELPER
                case "Bool":
                    {
                        return TypePrefs.Bool;
                        break;
                    }
#endif
                    default:
                        {
                            Debug.Log("<color=red>В префсах нет типа </color>" + stringTypePrefs);
                            return TypePrefs.String;
                            break;
                        }
                }
            }

            public static void SetPrefs(TypePrefs typePrefs, string key, string value, bool isPlayerPrefsHelper)
            {
                switch (typePrefs)
                {
                    case TypePrefs.Int:
                        {
                            int checkString = 0;
                            if (int.TryParse(value, out checkString))
                            {
#if !PLAYER_PREFS_HELPER
                                PlayerPrefs.SetInt(key, checkString);
#else
                            if (!isPlayerPrefsHelper)
                            {
                                PlayerPrefs.SetInt(key, checkString);
                            }
                            else
                            {
                                PlayerPrefsHelper.SetInt(key, checkString);
                            }
#endif
                            }
                            else
                            {
                                Debug.Log("<color=red>Это не INT!</color>");
                            }
                            break;
                        }
                    case TypePrefs.Float:
                        {
                            float checkString = 0;
                            if (float.TryParse(value, out checkString))
                            {
#if !PLAYER_PREFS_HELPER
                                PlayerPrefs.SetFloat(key, checkString);
#else
                            if (!isPlayerPrefsHelper)
                            {
                                PlayerPrefs.SetFloat(key, checkString);
                            }
                            else
                            {
                                PlayerPrefsHelper.SetFloat(key, checkString);
                            }
#endif
                            }
                            else
                            {
                                Debug.Log("<color=red>Это не FLOAT!</color>");
                            }
                            break;
                        }
                    case TypePrefs.String:
                        {
#if !PLAYER_PREFS_HELPER
                            PlayerPrefs.SetString(key, value);
#else
                        if (!isPlayerPrefsHelper)
                        {
                            PlayerPrefs.SetString(key, value);
                        }
                        else
                        {
                            PlayerPrefsHelper.SetString(key, value);
                        }
#endif
                            break;
                        }
#if PLAYER_PREFS_HELPER
                case TypePrefs.Bool:
                    {
                        if (isPlayerPrefsHelper)
                        {
                            PlayerPrefsHelper.SetBool(key, value == "True" ? true : false);
                        }
                        break;
                    }
#endif
                    default:
                        {
                            break;
                        }
                }
                PlayerPrefs.Save();
            }

            public static string GetPrefs(TypePrefs typePrefs, string key, bool isPlayerPrefsHelper)
            {
                switch (typePrefs)
                {
                    case TypePrefs.Int:
                        {
#if !PLAYER_PREFS_HELPER
                            return PlayerPrefs.GetInt(key).ToString();
#else
                        if (!isPlayerPrefsHelper)
                        {
                            return PlayerPrefs.GetInt(key).ToString();
                        }
                        else
                        {
                            return PlayerPrefsHelper.GetInt(key).ToString();
                        }
#endif
                            break;
                        }
                    case TypePrefs.Float:
                        {
#if !PLAYER_PREFS_HELPER
                            return PlayerPrefs.GetFloat(key).ToString();
#else
                        if (!isPlayerPrefsHelper)
                        {
                            return PlayerPrefs.GetFloat(key).ToString();
                        }
                        else
                        {
                            return PlayerPrefsHelper.GetFloat(key).ToString();
                        }
#endif
                            break;
                        }
                    case TypePrefs.String:
                        {
#if !PLAYER_PREFS_HELPER
                            return PlayerPrefs.GetString(key);
#else
                        if (!isPlayerPrefsHelper)
                        {
                            return PlayerPrefs.GetString(key);
                        }
                        else
                        {
                            return PlayerPrefsHelper.GetString(key);
                        }
#endif
                            break;
                        }
#if PLAYER_PREFS_HELPER
                case TypePrefs.Bool:
                    {
                        if (!isPlayerPrefsHelper)
                        {
                            return "";
                        }
                        else
                        {
                            return PlayerPrefsHelper.GetBool(key).ToString();
                        }
                        break;
                    }
#endif
                    default:
                        {
                            return "";
                            break;
                        }
                }
            }
        }
        #endregion ClassPrefs

        #region ClassAutoSave
        /// <summary>
        /// Класс хранящий параметры автосэйва
        /// </summary>
        [Serializable]
        public class ClassAutoSave
        {
            public double LastTime = 0;
            public bool IsActive = false;
            public bool IsActiveGui = true;
            [Tooltip("Tooltip text")]
            public bool IsActiveNotification = true;
            public bool IsActiveNotificationGui = false;
            public int IntervalSave = 10;

            public void SetClassAutoSave(ClassAutoSave classAutoSave)
            {
                EditorPrefs.SetInt(Application.productName + "IntervalAutoSave", classAutoSave.IntervalSave);
                EditorPrefs.SetBool(Application.productName + "IsActiveAutoSave", classAutoSave.IsActive);
                EditorPrefs.SetBool(Application.productName + "IsActiveNotificationAutoSave", classAutoSave.IsActiveNotification);
            }

            public void GetClassAutoSave(ClassAutoSave classAutoSave)
            {
                classAutoSave.IntervalSave = EditorPrefs.GetInt(Application.productName + "IntervalAutoSave", 10);
                classAutoSave.IsActive = EditorPrefs.GetBool(Application.productName + "IsActiveAutoSave", false);
                classAutoSave.IsActiveNotification = EditorPrefs.GetBool(Application.productName + "IsActiveNotificationAutoSave", true);
            }
        }
        #endregion ClassAutoSave

        #region ClassScreenShot
        /// <summary>
        /// Класс хранящий параметры скриншотов
        /// </summary>
        [Serializable]
        public class ClassScreenShot
        {
            public Canvas[] UICanvas = null;
            public bool IsActiveScreen = false;
            public double LastTime = 0;
            public int CurrentScreenNumber = 0;

            public float CurrentTimeScale = 0f;
            public string Dir = "Screenshots";
            //ХЗ почему в скринмейкере стнадартном стоит 2 ?!
            public int SuperSize = 1;
            public UnityEngine.Object FolderForScreenShot = null;
            public string PathFolderForScreenShot = "";
            public List<ClassResolution> M_ClassResolutionScreenShots =
                                        new List<ClassResolution>();
            public string NameResolution = "";
            public int Width = 0;
            public int Height = 0;

            public static MethodInfo getGroup;
            public static object GameViewSizesInstance;

            //Все методы в этом классе честно спизженны и я не до конца понимаю, как всё это работает(
            //Но оно работает и делает скрины 
            static ClassScreenShot()
            {
                //GameViewSizesInstance = ScriptableSingleton<GameViewSizes>.instance;
                var sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
                var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
                var instanceProp = singleType.GetProperty("instance");
                getGroup = sizesType.GetMethod("GetGroup");
                GameViewSizesInstance = instanceProp.GetValue(null, null);
            }

            public static void AddCustomSize(GameViewSizeGroupType sizeGroupType, int width, int height, string text)
            {
                var group = GetGroup(sizeGroupType);
                var addCustomSize = getGroup.ReturnType.GetMethod("AddCustomSize"); // or group.GetType().
                var gvsType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
#if UNITY_2017_4_OR_NEWER
                var gameViewSizeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType");
                var constructor = gvsType.GetConstructor(new Type[] { gameViewSizeType, typeof(int), typeof(int), typeof(string) });
#else
                    var constructor = gameViewSize.GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(string) });
#endif
                var newSize = constructor.Invoke(new object[] { 1 /*Тип resolution*/, width, height, text });
                addCustomSize.Invoke(group, new object[] { newSize });
            }

            static object GetGroup(GameViewSizeGroupType type)
            {
                return getGroup.Invoke(GameViewSizesInstance, new object[] { (int)type });
            }

            public static void SetResolution(string nameResolution)
            {
                int idx = FindSize(GetCurrentGroupType(), nameResolution);
                if (idx != -1)
                    SetSize(idx);
            }

            public static int FindSize(GameViewSizeGroupType sizeGroupType, string text)
            {
                // GameViewSizes group = gameViewSizesInstance.GetGroup(sizeGroupType);
                // string[] texts = group.GetDisplayTexts();
                // for loop...

                var group = GetGroup(sizeGroupType);
                var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
                var displayTexts = getDisplayTexts.Invoke(group, null) as string[];
                for (int i = 0; i < displayTexts.Length; i++)
                {
                    string display = displayTexts[i];
                    // the text we get is "Name (W:H)" if the size has a name, or just "W:H" e.g. 16:9
                    // so if we're querying a custom size text we substring to only get the name
                    // You could see the outputs by just logging
                    // Debug.Log(display);
                    int pren = display.IndexOf('(');
                    if (pren != -1)
                        display = display.Substring(0, pren - 1); // -1 to remove the space that's before the prens. This is very implementation-depdenent
                    if (display == text)
                        return i;
                }
                return -1;
            }

            public static void SetSize(int index)
            {
                var gvWndType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
                var selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var gvWnd = EditorWindow.GetWindow(gvWndType);
                selectedSizeIndexProp.SetValue(gvWnd, index, null);
            }

            public static GameViewSizeGroupType GetCurrentGroupType()
            {
                var getCurrentGroupTypeProp = GameViewSizesInstance.GetType().GetProperty("currentGroupType");
                return (GameViewSizeGroupType)(int)getCurrentGroupTypeProp.GetValue(GameViewSizesInstance, null);
            }
        }

        /// <summary>
        /// Класс хранящий параметры скриншотов
        /// </summary>
        [Serializable]
        public class ClassResolution
        {
            public bool isActive = true;
            public string NameResolution = "";
            public int Width = 0;
            public int Height = 0;
        }
        #endregion ClassScreenShot
    }
}