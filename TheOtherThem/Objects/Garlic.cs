using System.Collections.Generic;
using UnityEngine;

namespace TheOtherThem.Objects
{
    class Garlic
    {
        public static List<Garlic> garlics = new List<Garlic>();

        public GameObject garlic;
        private GameObject background;

        private static Sprite garlicSprite;
        public static Sprite GetGarlicSprite()
        {
            if (garlicSprite) return garlicSprite;
            garlicSprite = Helpers.LoadSpriteFromResources("TheOtherThem.Resources.Garlic.png", 300f);
            return garlicSprite;
        }

        private static Sprite backgroundSprite;
        public static Sprite GetBackgroundSprite()
        {
            if (backgroundSprite) return backgroundSprite;
            backgroundSprite = Helpers.LoadSpriteFromResources("TheOtherThem.Resources.GarlicBackground.png", 60f);
            return backgroundSprite;
        }

        public Garlic(Vector2 p)
        {
            garlic = new GameObject("Garlic");
            background = new GameObject("Background");
            background.transform.SetParent(garlic.transform);
            Vector3 position = new Vector3(p.x, p.y, PlayerControl.LocalPlayer.transform.localPosition.z + 0.001f); // just behind player
            garlic.transform.position = position;
            garlic.transform.localPosition = position;
            background.transform.localPosition = new Vector3(0, 0, -0.01f); // before player

            var garlicRenderer = garlic.AddComponent<SpriteRenderer>();
            garlicRenderer.sprite = GetGarlicSprite();
            var backgroundRenderer = background.AddComponent<SpriteRenderer>();
            backgroundRenderer.sprite = GetBackgroundSprite();


            garlic.SetActive(true);
            garlics.Add(this);
        }

        public static void ClearGarlics()
        {
            garlics = new List<Garlic>();
        }

        public static void UpdateAll()
        {
            foreach (Garlic garlic in garlics)
            {
                if (garlic != null)
                    garlic.Update();
            }
        }

        public void Update()
        {
            if (background != null)
                background.transform.Rotate(Vector3.forward * 6 * Time.fixedDeltaTime);
        }
    }
}