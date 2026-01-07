using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using NULL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DevTools.Extensions {
    public static class GenericExtensions {
        public static Color SetAlpha(this Image img, float a) {
            var c = img.color;
            c.a = a;
            img.color = c;
            return c;
        }

        static IEnumerable<string> GetFiles(string directory) {
            foreach (var s in Directory.GetFiles(Path.Combine(BasePlugin.ModPath, directory)))
                yield return Path.GetFileNameWithoutExtension(s);
        }

        public static string[] GetAllFiles(string directory) => new List<string>(GetFiles(directory)).ToArray();

        public static IEnumerable<T> RemoveAllAndReturn<T>(this IEnumerable<T> values, Predicate<T> predicate) {
            using (var enumerator = values.GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    if (!predicate(enumerator.Current))
                        yield return enumerator.Current;
                }
            }
        }

        public static IEnumerable<T> ReplaceAllAndReturn<T>(this IEnumerable<T> values, Predicate<T> predicate, T replacement) {
            using (var enumerator = values.GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    if (predicate(enumerator.Current))
                        yield return replacement;

                    yield return enumerator.Current;
                }
            }
        }

        public static int IndexAt<T>(this IEnumerable<T> values, T val) {
            int index = 0;
            using (IEnumerator<T> enumerator = values.GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    if (ReferenceEquals(enumerator.Current, val) || Equals(val, enumerator.Current))
                        return index;

                    index++;
                }
            }
            return -1;
        }

        public static int LastIndexAt<T>(this IEnumerable<T> values, T val) {
            int curIndex = -1;
            int index = 0;
            using (IEnumerator<T> enumerator = values.GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    if (ReferenceEquals(enumerator.Current, val) || Equals(val, enumerator.Current))
                        curIndex = index;

                    index++;
                }
            }
            return curIndex;
        }

        public static int IndexAt<T>(this IEnumerable<T> values, Predicate<T> func) {
            int index = 0;
            using (IEnumerator<T> enumerator = values.GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    if (func(enumerator.Current))
                        return index;

                    index++;
                }
            }
            return -1;
        }

        public static int LastIndexAt<T>(this IEnumerable<T> values, Predicate<T> func) {
            int curIndex = -1;
            int index = 0;
            using (IEnumerator<T> enumerator = values.GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    if (func(enumerator.Current))
                        curIndex = index;

                    index++;
                }
            }
            return curIndex;
        }

        public static T ElementWithName<T>(this List<T> list, string name) => list.Find(x => x.ToString().Equals(name));

        public static T ElementContainingName<T>(this List<T> list, string name) => list.Find(x => x.ToString().Contains(name));

        public static List<T> ElementsWithName<T>(this List<T> list, string name) => list.FindAll(x => x.ToString().Equals(name));

        public static List<T> ElementsContainingName<T>(this List<T> list, string name) => list.FindAll(x => x.ToString().Contains(name));

        public static void Replace<T>(this IList<T> values, int index, T value) {
            if (index < 0 || index >= values.Count || values.Count == 0)
                throw new ArgumentOutOfRangeException($"The index: {index} is out of the list range (Length: {values.Count})");

            values.RemoveAt(index);
            values.Insert(index, value);
        }

        public static IEnumerable<T> DoAndReturn<T>(this IEnumerable<T> values, Func<T, T> func) {
            using (var enumerator = values.GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    yield return func(enumerator.Current);
                }
            }
        }

        public static IEnumerable<T> RemoveIn<T>(this IEnumerable<T> values, T val) => values.Where(x => !ReferenceEquals(x, val) && !Equals(val, x));

        public static IEnumerable<T> RemoveInAt<T>(this IEnumerable<T> values, int index) {
            int numeration = 0;
            using (var enumerator = values.GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    if (numeration++ != index)
                        yield return enumerator.Current;
                }
            }
        }

        public static T Next<T>(this T src) where T : struct {
            if (!typeof(T).IsEnum) {
                throw new ArgumentException(string.Format("Argumnent {0} is not an Enum", typeof(T).FullName));
            }

            T[] array = (T[])Enum.GetValues(src.GetType());
            int num = Array.IndexOf<T>(array, src) + 1;

            if (array.Length != num) {
                return array[num];
            }

            return array[0];
        }

        public static void Swap<T>(this List<T> list, int i, int j) => (list[j], list[i]) = (list[i], list[j]);

        public static List<T> FindAndRemove<T>(this List<T> lst, Predicate<T> match) {
            List<T> result = lst.FindAll(match);
            lst.RemoveAll(match);
            return result;
        }

        public static T GetRandom<T>(this IList<T> array) => array[UnityEngine.Random.Range(0, array.Count)];

        public static T ParseEnum<T>(this string value) => (T)Enum.Parse(typeof(T), value, true);

        public static void LoadAll<T>(this AssetManager man) where T : UnityEngine.Object {
            var t = typeof(T);

            if (t == typeof(Sprite))
                Utils.GetAllFilesFromFolder("Texture2D").Do(x => man.Add(x.Split(new char[] { '\\' }).Last(), ContentManager.CreateSprite(x)));

            if (t == typeof(SoundObject))
                Utils.GetAllFilesFromFolder("AudioClip").Do(x => man.Add(x.Split(new char[] { '\\' }).Last(), ContentManager.CreateSoundObject(x, "", type: SoundType.Voice)));
        }

        public static void LoadAll(this AssetManager man) {
            man.LoadAll<Sprite>();
            man.LoadAll<SoundObject>();
        }

        public static T ToEnum<T>(this string text) where T : Enum => EnumExtensions.ExtendEnum<T>(text);
#pragma warning disable IDE0060
        public static T GetRandom<T>(this T value) where T : Enum {
            var array = Enum.GetValues(typeof(T));
            List<T> values = new List<T>();
            foreach (T en in array)
                values.Add(en);
            return values.GetRandom();
        }
#pragma warning restore
        public static bool Contains<T>(this IList<T> list, Predicate<T> predicate) {
            foreach (var b in list) {
                if (predicate(b))
                    return true;
            }
            return false;
        }

        public static void RemoveChildsContainingNames(this Transform t, IList<string> names) {
            var e = t.transform.GetEnumerator();
            while (e.MoveNext()) {
                foreach (var name in names)
                    if (((Transform)e.Current).ToString().Contains(name))
                        UnityEngine.Object.Destroy(((Transform)e.Current).gameObject);
            }
        }

        public static StandardMenuButton AddText(this StandardMenuButton but, string text, float size, Color? color = null, Vector2? offset = null, FontStyles fontStyle = FontStyles.Normal) {
            var txt = new GameObject("CustomButText").AddComponent<TextMeshProUGUI>();
            txt.transform.SetParent(but.transform, false);
            txt.fontSize = size;
            txt.color = color ?? Color.black;
            txt.text = text;
            txt.fontStyle = fontStyle;
            txt.transform.localPosition += (Vector3)(offset ?? Vector3.zero);
            return but;
        }

        public static void TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue val) {
            if (!dict.ContainsKey(key))
                dict.Add(key, val);
            else
                Debug.LogWarning("The dictionary already contains the key: " + key);
        }

        public static int CountSubs(this string input, string substring) {
            int count = 0;
            int index = 0;

            while ((index = input.IndexOf(substring, index)) != -1) {
                count++;
                index += substring.Length;
            }
            return count;
        }

        public static T TryAddComponent<T>(this GameObject obj) where T : Component {
            if (obj == null)
                throw new NullReferenceException("Yo, man! We don't accept objects equal to null! Bye!");

            if (obj.GetComponent<T>() is null)
                return obj.AddComponent<T>();

            return obj.GetComponent<T>();
        }

        public static string ArrayToString<T>(this IList<T> list) {
            string res = string.Empty;
            using var en = list.GetEnumerator();
            while (en.MoveNext())
                res += en.Current.ToString() + "\n";

            return res;
        }

        public static SceneObject SetLevel(this SceneObject scene, LevelObject ld) {
            scene.levelObject = ld;
            scene.additionalNPCs = ld.additionalNPCs;
            return scene;
        }
    }
}