using HarmonyLib;
using static TheOtherThem.TheOtherRoles;
using TheOtherThem.Objects;
using UnityEngine;
using System.Collections.Generic;

namespace TheOtherThem.Patches

{
    [HarmonyPatch]
    class MapBehaviorPatch
    {
        public static Dictionary<byte, SpriteRenderer> mapIcons = null;
        public static Dictionary<byte, SpriteRenderer> corpseIcons = null;

        public static Sprite corpseSprite;
        private static Vector3 useButtonPos;

        public static Sprite getCorpseSprite()
        {
            if (corpseSprite) return corpseSprite;
            corpseSprite = Helpers.LoadSpriteFromResources("TheOtherThem.Resources.CorpseIcon.png", 115f);
            return corpseSprite;
        }

        public static void resetIcons()
        {
            if (mapIcons != null)
            {
                foreach (SpriteRenderer r in mapIcons.Values)
                    Object.Destroy(r.gameObject);
                mapIcons.Clear();
                mapIcons = null;
            }

            if (corpseIcons != null)
            {
                foreach (SpriteRenderer r in corpseIcons.Values)
                    Object.Destroy(r.gameObject);
                corpseIcons.Clear();
                corpseIcons = null;
            }
        }

        static void initializeIcons(MapBehaviour __instance, PlayerControl pc = null)
        {
            List<PlayerControl> players = new List<PlayerControl>();
            if (pc == null)
            {
                mapIcons = new Dictionary<byte, SpriteRenderer>();
                corpseIcons = new Dictionary<byte, SpriteRenderer>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    players.Add(p);
                }
            } else
            {
                players.Add(pc);
            }

            foreach (PlayerControl p in players)
            {
                if (p.IsGM()) continue;

                byte id = p.PlayerId;
                mapIcons[id] = UnityEngine.Object.Instantiate(__instance.HerePoint, __instance.HerePoint.transform.parent);
                p.SetPlayerMaterialColors(mapIcons[id]);

                
                corpseIcons[id] = UnityEngine.Object.Instantiate(__instance.HerePoint, __instance.HerePoint.transform.parent);
                corpseIcons[id].sprite = getCorpseSprite();
                corpseIcons[id].transform.localScale = Vector3.one * 0.20f;
                p.SetPlayerMaterialColors(corpseIcons[id]);
            }
        }

        [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate))]
        class MapBehaviourFixedUpdatePatch
        {
            static void Postfix(MapBehaviour __instance)
            {
                if (PlayerControl.LocalPlayer.IsGM())
                {
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (p == null || p.IsGM()) continue;

                        byte id = p.PlayerId;
                        if (!mapIcons.ContainsKey(id))
                        {
                            continue;
                        }

                        bool enabled = !p.Data.IsDead;
                        if (enabled)
                        {
                            Vector3 vector = p.transform.position;
                            vector /= ShipStatus.Instance.MapScale;
                            vector.x *= Mathf.Sign(ShipStatus.Instance.transform.localScale.x);
                            vector.z = -1f;
                            mapIcons[id].transform.localPosition = vector;

                        }

                        mapIcons[id].enabled = enabled;
                    }

                    foreach (SpriteRenderer r in corpseIcons.Values) { r.enabled = false; }
                    foreach (DeadBody b in Object.FindObjectsOfType<DeadBody>())
                    {
                        byte id = b.ParentId;
                        Vector3 vector = b.transform.position;
                        vector /= ShipStatus.Instance.MapScale;
                        vector.x *= Mathf.Sign(ShipStatus.Instance.transform.localScale.x);
                        vector.z = -1f;

                        if (!corpseIcons.ContainsKey(id))
                        {
                            continue;
                        }

                        corpseIcons[id].transform.localPosition = vector;
                        corpseIcons[id].enabled = true;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.GenericShow))]
        class MapBehaviourShowNormalMapPatch
        {
            static void Prefix(MapBehaviour __instance)
            {
                if (PlayerControl.LocalPlayer.IsGM())
                {
                    useButtonPos = HudManager.Instance.UseButton.transform.localPosition;
                }
                CustomOverlays.hideInfoOverlay();
            }

            static void Postfix(MapBehaviour __instance)
            {
                if (PlayerControl.LocalPlayer.IsGM())
                {
                    if (mapIcons == null || corpseIcons == null)
                        initializeIcons(__instance);

                    __instance.taskOverlay.Hide();
                    foreach (byte id in mapIcons.Keys)
                    {
                        NetworkedPlayerInfo playerById = GameData.Instance.GetPlayerById(id);
                        PlayerMaterial.SetColors(playerById.Object.CurrentOutfit.ColorId, mapIcons[id]);
                        mapIcons[id].enabled = !playerById.IsDead;
                    }

                    foreach (DeadBody b in Object.FindObjectsOfType<DeadBody>())
                    {
                        byte id = b.ParentId;
                        NetworkedPlayerInfo playerById = GameData.Instance.GetPlayerById(id);
                        PlayerMaterial.SetColors(playerById.Object.CurrentOutfit.ColorId, corpseIcons[id]);
                        corpseIcons[id].enabled = true;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Close))]
        class MapBehaviorClosePatch
        {
            static void Postfix(MapBehaviour __instance)
            {
                if (PlayerControl.LocalPlayer.IsGM())
                {
                    HudManager.Instance.UseButton.transform.localPosition = useButtonPos;
                }
            }
        }
    }
}