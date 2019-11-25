using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor_Helper
{
    /// <summary>
    /// Окно для тестирования и разработки плагина
    /// </summary>
    public class EH_Develop : EditorWindow
    {
        /// <summary>
        /// Открываем окно разработки 
        /// </summary>
        public static void Open()
        {
            EH_Develop window = (EH_Develop)EditorWindow.GetWindow(typeof(EH_Develop));
            window.title = "Develop";
            window.Focus();
            //TODO добавить фиксированный размер
            window.Show();
        }

        /// <summary>
        /// Отрисовка гуя
        /// </summary>
        private void OnGUI()
        {
            ViewClearEditorPrefs();
        }

        /// <summary>
        /// Отрисовка кнопки очистки эдитор префсов
        /// </summary>
        private void ViewClearEditorPrefs ()
        {
            if (GUILayout.Button("Clear EditorPrefs"))
            {
                if (EditorUtility.DisplayDialog(EH_StaticParametrs.ATTENTION,
                         EH_StaticParametrs.CLEAR_EDITOR_PREFS,
                         "OK",
                         "Нахуй нахуй"))
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
}