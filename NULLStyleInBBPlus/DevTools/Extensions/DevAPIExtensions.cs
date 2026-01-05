using DevTools.Extensions;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Reflection;
using PlusLevelStudio;
using System.Collections.Generic;
using UnityEngine;

namespace DevTools.DevAPI.Extensions
{
    public static class NPCBuilderExtensions
    {
        static Dictionary<string, Sprite> m_spriteStorage = new Dictionary<string, Sprite>();
        public static T BuildNPC<T>(this NPCBuilder<T> b) where T : NPC {
            var npc = b.Build();
            if (m_spriteStorage.ContainsKey(npc.name))
            {
                npc.spriteRenderer[0].sprite = m_spriteStorage[npc.name];
                m_spriteStorage.Remove(npc.name);
            }

            LevelStudioPlugin.Instance.npcDisplays.Add(npc.name, npc.gameObject);
            return npc;
        }
        public static NPCBuilder<T> SetSprite<T>(this NPCBuilder<T> b, Sprite sprite) where T : NPC {
            string name = (string)b.ReflectionGetVariable("objectName");
            m_spriteStorage.Add(name, sprite);
            return b;
        }

        public static NPCBuilder<T> SetSprite<T>(this NPCBuilder<T> b, string sprite) where T : NPC =>
            b.SetSprite(NULL.Manager.ModManager.m.Get<Sprite>(sprite));
    }

}