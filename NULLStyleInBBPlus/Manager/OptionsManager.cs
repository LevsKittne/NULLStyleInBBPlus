/*using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.SaveSystem;
using System.IO;
using UnityEngine;
using static NULL.Manager.ModManager;

namespace NULL.Manager {

public static class OptionsManager // Will be removed in a future version. Instead of options menu, the settings in the config file will be used.
{
    public static MenuToggle AmbienceButton;
    public static MenuToggle CharactersButton;
    static bool[] options = new bool[2];
    public static bool DarkAmbience => options[0];
    public static bool Characters => options[1];

    public static void OnMenu(OptionsMenu m)
    {
        GameObject category = CustomOptionsCore.CreateNewCategory(m, "Null Style");
        AmbienceButton = CustomOptionsCore.CreateToggleButton(m, new Vector2(9, 70), "Dark ambience", options[0], "There is no lighting in the school. Suspenseful background ambient track plays.");
        CharactersButton = CustomOptionsCore.CreateToggleButton(m, new Vector2(38, 35), "Other characters", options[1], "Oh no! Null called other characters to help!");
        AmbienceButton.transform.SetParent(category.transform, false);
        CharactersButton.transform.SetParent(category.transform, false);
    }
#pragma warning disable IDE0060
    public static void Save(OptionsMenu m)
    {
        options = [AmbienceButton.Value, CharactersButton.Value];
        SaveOptions();
    }
#pragma warning restore

    internal static void SaveOptions()
    {
        try 
        { 
            File.WriteAllLines(Path.Combine(ModdedSaveSystem.GetCurrentSaveFolder(plug), "options.txt"), [DarkAmbience.ToString(), Characters.ToString()]);
        }
        catch(System.Exception e)
        { 
            Debug.LogError("Error occurred while saving options!");
            Debug.LogException(e);
        }
    }

    internal static void LoadOptions()
    {
        string path = Path.Combine(ModdedSaveSystem.GetCurrentSaveFolder(plug), "options.txt");
        if (!File.Exists(path) || File.ReadAllLines(path).Length >= 3)
            File.WriteAllLines(Path.Combine(ModdedSaveSystem.GetCurrentSaveFolder(plug), "options.txt"), ["false", "false"]);

        var f = File.ReadAllLines(path);
        for (int i = 0; i < f.Length; i++)
            options[i] = bool.Parse(f[i]);
    }
}
}
*/