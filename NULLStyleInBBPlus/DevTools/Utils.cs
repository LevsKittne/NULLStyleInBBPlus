using NULL;
using NULL.Manager;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace DevTools {
    public static class Utils {
        public static Color GetRandomColor() => new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        public static string[] GetAllFilesFromFolder(string folder) {
            var files = new List<string>();

            foreach (var file in Directory.GetFiles(Path.Combine(BasePlugin.ModPath, folder))) {
                files.Add(Path.GetFileNameWithoutExtension(file));
            }

            foreach (var subfolder in Directory.GetDirectories(Path.Combine(BasePlugin.ModPath, folder))) {
                var relativeSubfolder = Path.Combine(folder, new DirectoryInfo(subfolder).Name);
                var subfolderFiles = GetAllFilesFromFolder(relativeSubfolder);
                foreach (var subfile in subfolderFiles) {
                    files.Add(Path.Combine(new DirectoryInfo(relativeSubfolder).Name, subfile));
                }
            }
            return files.ToArray();
        }

        /*public static SceneObject LoadCustomSceneObject(string lvl, string title = "WIP", int lvlNo = -1, Cubemap skybox = null, string folder = "ExtraLevels") {
            var path = Path.Combine(BasePlugin.ModPath, folder, lvl + ".cbld");
            try {
                return null;
            }
            catch (System.Exception e) {
                Debug.LogError($"Loading custom level failed.\nCheck that the file path is correct: {path}");
                Debug.LogException(e);
            }

            return null;
        }*/

        public static T GetObject<T>(string name) where T : Object => FindResourceObjectContainingName<T>(name) ?? ModManager.m.Get<T>(name);
        public static T[] GetObjects<T>() where T : Object => FindResourceObjects<T>().Union(ModManager.m.GetAll<T>()).ToArray();
        public static T FindResourceObjectContainingName<T>(string name) where T : Object => Resources.FindObjectsOfTypeAll<T>().First(x => x.name.ToLower().Contains(name.ToLower()));
        public static T FindResourceObjectWithName<T>(string name) where T : Object => Resources.FindObjectsOfTypeAll<T>().First(x => x.name.ToLower() == name.ToLower());
        public static T[] FindResourceObjectsContainingName<T>(string name) where T : Object => Resources.FindObjectsOfTypeAll<T>().Where(x => x.name.ToLower().Contains(name.ToLower())).ToArray();
        public static T FindResourceObject<T>() where T : Object => Resources.FindObjectsOfTypeAll<T>()[0];
        public static T[] FindResourceObjects<T>() where T : Object => Resources.FindObjectsOfTypeAll<T>();
    }
}