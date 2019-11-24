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

        private void OnDisable()
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
        }

        /// <summary>
        /// Метод редактирования окна
        /// </summary>
        private void ViewEditor()
        {
            GUILayout.Label(EH_StaticParametrs.VERSION, EditorStyles.boldLabel);
            Debug.Log("0");

            ViewEditorTimeScale();
            ViewEditorScenes();

            /*
            ViewEditorAutoSave();
            ViewEditorClearPrefs();
            ViewEditorScreenShot();
            ViewEditorChets();
            */
        }

        /// <summary>
        /// Отрисовываем настройки тайм скейла
        /// </summary>
        private void ViewEditorTimeScale()
        {
            EditorGUILayout.BeginHorizontal();
            EH_StaticParametrs.IsActiveTimeScale = GUILayout.Toggle(EH_StaticParametrs.IsActiveTimeScale, "isActiveTimeScale");

            //TODO для теста
            if (GUILayout.Button("Change"))
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

            if (GUILayout.Button("Change"))
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
    }
}