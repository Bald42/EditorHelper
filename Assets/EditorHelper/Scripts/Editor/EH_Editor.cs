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
    /// Окно редактора для тулзы
    /// </summary>
    public class EH_Editor : EditorWindow
    {
        private Vector2 scrollPosEditor = Vector2.zero;

        private bool isViewTimeScaleEdit = false;
        private bool isViewScenesChange = false;

        /// <summary>
        /// Открываем окно редактора 
        /// </summary>
        public static void Open()
        {
            EH_Editor window = (EH_Editor)EditorWindow.GetWindow(typeof(EH_Editor));
            window.title = "Editor";
            window.Focus();
            //TODO добавить фиксированный размер
            window.Show();
        }

        private void OnDestroy ()
        {
            EH_StaticParametrs.SaveEditorParams();
        }

        /// <summary>
        /// Отрисовка гуя
        /// </summary>
        private void OnGUI()
        {
            scrollPosEditor = GUILayout.BeginScrollView(scrollPosEditor);

            ViewEditor();

            GUILayout.EndScrollView();

            if (GUILayout.Button("For develop"))
            {
                EH_Develop.Open();
            }

            if (GUILayout.Button(EH_StaticParametrs.DOWNLOAD_LAST))
            {
                Application.OpenURL(EH_StaticParametrs.URL_LAST_VERSION);
            }

            if (GUILayout.Button(EH_StaticParametrs.DOWNLOAD_ALL))
            {
                Application.OpenURL(EH_StaticParametrs.URL_ALL_VERSION);
            }
        }

        /// <summary>
        /// Метод редактирования окна
        /// </summary>
        private void ViewEditor()
        {
            GUILayout.Label(EH_StaticParametrs.VERSION, EditorStyles.boldLabel);
            ViewEditorTimeScale();
            ViewEditorScenes();
            ViewEditorAutoSave();
            ViewEditorClearPrefs();            
            ViewEditorScreenShot();
            ViewEditorChets();
        }

        /// <summary>
        /// Отрисовываем настройки тайм скейла
        /// </summary>
        private void ViewEditorTimeScale()
        {
            EditorGUILayout.BeginHorizontal();
            EH_StaticParametrs.IsActiveTimeScale = GUILayout.Toggle(EH_StaticParametrs.IsActiveTimeScale, "isActiveTimeScale");

            //TODO для теста
            if (GUILayout.Button((!EH_StaticParametrs.IsActiveTimeScale).ToString(), GUILayout.MaxWidth(100.0f)))
            {
                EH_StaticParametrs.IsActiveTimeScale = !EH_StaticParametrs.IsActiveTimeScale;
                EditorHelper.Instance.Repaint();
            }

            if (GUILayout.Button("?", GUILayout.MaxWidth(30.0f)))
            {
                EditorUtility.DisplayDialog("",
                                            EH_StaticParametrs.TUTUR_TIME_SCALE,
                                            "Ok");
            }

            EditorGUILayout.EndHorizontal();
            if (EH_StaticParametrs.IsActiveTimeScale)
            {
                TimeScaleEditor();
            }
        }

        /// <summary>
        /// Редактор тайм скейла
        /// </summary>
        private void TimeScaleEditor()
        {
            /*
            isViewTimeScaleEdit = GUILayout.Toggle(isViewTimeScaleEdit,
                (isViewTimeScaleEdit == true ? "↑  " : "↓  ") + "TimeScaleEditor",
                EditorStyles.boldLabel);            

            if (isViewTimeScaleEdit)
            {*/
            EH_StaticParametrs.minTimeScale = EditorGUILayout.FloatField("MinTimeScale = ", EH_StaticParametrs.minTimeScale);
            EH_StaticParametrs.maxTimeScale = EditorGUILayout.FloatField("MaxTimeScale = ", EH_StaticParametrs.maxTimeScale);
            //}
        }

        /// <summary>
        /// Отрисовываем настройки сцен
        /// </summary>
        private void ViewEditorScenes()
        {
            EditorGUILayout.BeginHorizontal();
            EH_StaticParametrs.IsActiveScenes = GUILayout.Toggle(EH_StaticParametrs.IsActiveScenes, "isActiveScenes");

            if (GUILayout.Button((!EH_StaticParametrs.IsActiveScenes).ToString(), GUILayout.MaxWidth(100.0f)))
            {
                EH_StaticParametrs.IsActiveScenes = !EH_StaticParametrs.IsActiveScenes;
                EditorHelper.Instance.Repaint();
            }

            if (GUILayout.Button("?", GUILayout.MaxWidth(30.0f)))
            {
                EditorUtility.DisplayDialog("",
                                                        EH_StaticParametrs.TUTOR_SCENES,
                                                        "Ok");
            }
            EditorGUILayout.EndHorizontal();
            if (EH_StaticParametrs.IsActiveScenes)
            {
                ViewScenesChange();
            }
        }

        /// <summary>
        /// Изменяем сцены
        /// </summary>
        private void ViewScenesChange()
        {
            for (int i = 0; i < EditorHelper.Instance.classScenes.Count; i++)
            {
                EditorHelper.Instance.classScenes[i].NameScene = EditorGUILayout.TextField("NameScene" + i + ": ",
                                                                     EditorHelper.Instance.classScenes[i].NameScene);

                EditorHelper.Instance.classScenes[i].SceneObject = EditorGUILayout.ObjectField(EditorHelper.Instance.classScenes[i].SceneObject,
                                                                         typeof(UnityEngine.Object),
                                                                         true);

                GUILayout.Space(10f);
            }

            if (GUILayout.Button("Add new scene"))
            {
                Debug.Log("<color=red>Назначь сцену для новой кнопки</color>");
                EditorHelper.ClassScenes temp = new EditorHelper.ClassScenes();

                temp.NameScene = "New scene " + EditorHelper.Instance.classScenes.Count;
                temp.PathScene = "";
                temp.SceneObject = null;

                EditorHelper.Instance.classScenes.Add(temp);
            }

            if (GUILayout.Button("Save all scene"))
            {
                for (int i = 0; i < EditorHelper.Instance.classScenes.Count; i++)
                {
                    if (EditorHelper.Instance.classScenes[i].SceneObject)
                    {
                        string tempPath = AssetDatabase.GetAssetPath(EditorHelper.Instance.classScenes[i].SceneObject).Replace(".unity", "");
                        EditorHelper.Instance.classScenes[i].PathScene = tempPath;
                        EditorPrefs.SetString(Application.productName + "PathScene" + i, tempPath);
                        EditorPrefs.SetString(Application.productName + "NameScene" + i, EditorHelper.Instance.classScenes[i].NameScene);
                    }
                    else
                    {
                        Debug.Log("<color=red>Назначь сцену для кнопки </color>" + i);
                    }
                }
            }

            if (GUILayout.Button("Delete last scene"))
            {
                if (EditorUtility.DisplayDialog("Удаление последней сцены из списка",
                                                "",
                                                "ДА", "НЕТ"))
                {
                    if (EditorHelper.Instance.classScenes.Count > 0)
                    {
                        EditorPrefs.DeleteKey(Application.productName + "PathScene" + (EditorHelper.Instance.classScenes.Count - 1));
                        EditorPrefs.DeleteKey(Application.productName + "NameScene" + (EditorHelper.Instance.classScenes.Count - 1));
                        EditorHelper.Instance.classScenes.RemoveAt(EditorHelper.Instance.classScenes.Count - 1);
                    }
                }
            }
            GUILayout.Label("------------------------", EditorStyles.boldLabel);
        }

        /// <summary>
        /// Отрисовываем настройки автосохранения сцен
        /// </summary>
        private void ViewEditorAutoSave()
        {
            EditorGUILayout.BeginHorizontal();
            EH_StaticParametrs.IsActiveAutoSave = GUILayout.Toggle(EH_StaticParametrs.IsActiveAutoSave, "isActiveAutoSave");

            if (GUILayout.Button((!EH_StaticParametrs.IsActiveAutoSave).ToString(), GUILayout.MaxWidth(100.0f)))
            {
                EH_StaticParametrs.IsActiveAutoSave = !EH_StaticParametrs.IsActiveAutoSave;
                EditorHelper.Instance.Repaint();
            }

            if (GUILayout.Button("?", GUILayout.MaxWidth(30.0f)))
            {
                EditorUtility.DisplayDialog("",
                                                        EH_StaticParametrs.TUTOR_AUTO_SAVE,
                                                        "Ok");
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Отрисовываем настройки очистки префсов
        /// </summary>
        private void ViewEditorClearPrefs()
        {
            EditorGUILayout.BeginHorizontal();
            EH_StaticParametrs.IsActiveClearPrefs = GUILayout.Toggle(EH_StaticParametrs.IsActiveClearPrefs, "isActiveClearPrefs");

            if (GUILayout.Button((!EH_StaticParametrs.IsActiveClearPrefs).ToString(), GUILayout.MaxWidth(100.0f)))
            {
                EH_StaticParametrs.IsActiveClearPrefs = !EH_StaticParametrs.IsActiveClearPrefs;
                EditorHelper.Instance.Repaint();
            }

            if (GUILayout.Button("?", GUILayout.MaxWidth(30.0f)))
            {
                EditorUtility.DisplayDialog("",
                                                        EH_StaticParametrs.TUTOR_CLEAR_PREFS,
                                                        "Ok");
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Отрисовываем настройки скринов
        /// </summary>
        private void ViewEditorScreenShot()
        {
            EditorGUILayout.BeginHorizontal();
            EH_StaticParametrs.IsActiveScreenShot = GUILayout.Toggle(EH_StaticParametrs.IsActiveScreenShot, "isActiveScreenShot");

            if (GUILayout.Button((!EH_StaticParametrs.IsActiveScreenShot).ToString(), GUILayout.MaxWidth(100.0f)))
            {
                EH_StaticParametrs.IsActiveScreenShot = !EH_StaticParametrs.IsActiveScreenShot;
                EditorHelper.Instance.Repaint();
            }

            if (GUILayout.Button("?", GUILayout.MaxWidth(30.0f)))
            {
                EditorUtility.DisplayDialog("",
                                                        EH_StaticParametrs.TUTOR_SCREEN_SHOTS,
                                                        "Ok");
            }
            EditorGUILayout.EndHorizontal();
            if (EH_StaticParametrs.IsActiveScreenShot)
            {
                ViewScreenShotParams();
            }
        }

        /// <summary>
        /// Отрисовываем настройки скринов
        /// </summary>
        private void ViewScreenShotParams()
        {
            if (GUILayout.Button("Add Folder For ScreenShots"))
            {
                EditorHelper.Instance.classScreenShot.PathFolderForScreenShot = EditorUtility.OpenFolderPanel("Folder For ScreenShots", "", "");
                EditorPrefs.SetString(Application.productName + "PathForScreenShots", EditorHelper.Instance.classScreenShot.PathFolderForScreenShot);
            }

            EditorHelper.Instance.classScreenShot.NameResolution =
            EditorGUILayout.TextField("NameResolution = ", EditorHelper.Instance.classScreenShot.NameResolution);
            EditorHelper.Instance.classScreenShot.Width =
            EditorGUILayout.IntField("Width = ", EditorHelper.Instance.classScreenShot.Width);
            EditorHelper.Instance.classScreenShot.Height =
            EditorGUILayout.IntField("Height = ", EditorHelper.Instance.classScreenShot.Height);

            GUILayout.Space(10f);

            if (GUILayout.Button("Add Resolution"))
            {
                if (EditorHelper.Instance.classScreenShot.Width != 0 &&
                    EditorHelper.Instance.classScreenShot.Height != 0 &&
                    EditorHelper.Instance.classScreenShot.NameResolution != "")
                {
                    AddResolution();
                }
                else
                {
                    Debug.Log("<color=red>Заполни все поля для нового разрешения</color>");
                }
            }

            if (GUILayout.Button("Delete last Resolution"))
            {
                if (EditorUtility.DisplayDialog("",
                     "Удаляем последнее разрешение?",
                     "Удалить",
                     "Отмена"))
                {
                    if (EditorHelper.Instance.classScreenShot.M_ClassResolutionScreenShots.Count > 0)
                    {
                        EditorPrefs.DeleteKey("Resolution" +
                        (EditorHelper.Instance.classScreenShot.M_ClassResolutionScreenShots.Count - 1) + "NameResolution");
                        EditorPrefs.DeleteKey("Resolution" +
                        (EditorHelper.Instance.classScreenShot.M_ClassResolutionScreenShots.Count - 1) + "Width");
                        EditorPrefs.DeleteKey("Resolution" +
                        (EditorHelper.Instance.classScreenShot.M_ClassResolutionScreenShots.Count - 1) + "Height");

                        EditorHelper.Instance.classScreenShot.M_ClassResolutionScreenShots.RemoveAt(EditorHelper.Instance.classScreenShot.M_ClassResolutionScreenShots.Count - 1);
                    }
                }
            }
            GUILayout.Label("------------------------", EditorStyles.boldLabel);
        }
        

        /// <summary>
        /// Добавляем новое разрешение
        /// </summary>
        public void AddResolution()
        {
            if (EditorHelper.ClassScreenShot.FindSize(EditorHelper.ClassScreenShot.GetCurrentGroupType(), EditorHelper.Instance.classScreenShot.NameResolution) == -1)
            {
                EditorHelper.ClassScreenShot.AddCustomSize(EditorHelper.ClassScreenShot.GetCurrentGroupType(),
                    EditorHelper.Instance.classScreenShot.Width,
                    EditorHelper.Instance.classScreenShot.Height,
                    EditorHelper.Instance.classScreenShot.NameResolution);

                EditorHelper.ClassResolution tempClassResolution = new EditorHelper.ClassResolution();
                EditorHelper.Instance.classScreenShot.M_ClassResolutionScreenShots.Add(tempClassResolution);

                EditorHelper.Instance.classScreenShot.M_ClassResolutionScreenShots
                [EditorHelper.Instance.classScreenShot.M_ClassResolutionScreenShots.Count - 1].Width = EditorHelper.Instance.classScreenShot.Width;

                EditorHelper.Instance.classScreenShot.M_ClassResolutionScreenShots
                [EditorHelper.Instance.classScreenShot.M_ClassResolutionScreenShots.Count - 1].Height = EditorHelper.Instance.classScreenShot.Height;

                EditorHelper.Instance.classScreenShot.M_ClassResolutionScreenShots
                [EditorHelper.Instance.classScreenShot.M_ClassResolutionScreenShots.Count - 1].NameResolution = EditorHelper.Instance.classScreenShot.NameResolution;

                EditorPrefs.SetString("Resolution" + (EditorHelper.Instance.classScreenShot.M_ClassResolutionScreenShots.Count - 1) + "NameResolution",
                EditorHelper.Instance.classScreenShot.NameResolution);
                EditorPrefs.SetInt("Resolution" + (EditorHelper.Instance.classScreenShot.M_ClassResolutionScreenShots.Count - 1) + "Width",
                EditorHelper.Instance.classScreenShot.Width);
                EditorPrefs.SetInt("Resolution" + (EditorHelper.Instance.classScreenShot.M_ClassResolutionScreenShots.Count - 1) + "Height",
                EditorHelper.Instance.classScreenShot.Height);

                EditorHelper.Instance.classScreenShot.Width = 0;
                EditorHelper.Instance.classScreenShot.Height = 0;
                EditorHelper.Instance.classScreenShot.NameResolution = "";
            }
            else
            {
                Debug.Log("<color=red>Разрешение с таким названием уже есть</color>");
            }
        }

        /// <summary>
        /// Отрисовываем настройки читов
        /// </summary>
        private void ViewEditorChets()
        {
            EditorGUILayout.BeginHorizontal();
            EH_StaticParametrs.IsActiveCheats = GUILayout.Toggle(EH_StaticParametrs.IsActiveCheats, "isActiveCheats");

            if (GUILayout.Button((!EH_StaticParametrs.IsActiveCheats).ToString(), GUILayout.MaxWidth(100.0f)))
            {
                EH_StaticParametrs.IsActiveCheats = !EH_StaticParametrs.IsActiveCheats;
                EditorHelper.Instance.Repaint();
            }

            if (GUILayout.Button("?", GUILayout.MaxWidth(30.0f)))
            {
                EditorUtility.DisplayDialog("",
                                                        EH_StaticParametrs.TUTOR_CHEATS,
                                                        "Ok");
            }
            EditorGUILayout.EndHorizontal();
            if (EH_StaticParametrs.IsActiveCheats)
            {
                //TODO перенести весь интерфейс сюда
                //ViewCheatsEdit();
            }
        }
    }
}