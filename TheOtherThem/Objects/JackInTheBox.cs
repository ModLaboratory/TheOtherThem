using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TheOtherThem.TheOtherRoles;

namespace TheOtherThem.Objects
{

    public class JackInTheBox
    {
        public static List<JackInTheBox> AllJackInTheBoxes { get; } = new();
        public static bool BoxesConvertedToVents { get; set; } = false;
        public static Sprite[] BoxAnimationSprites { get; } = new Sprite[18];

        public const int JackInTheBoxLimit = 3;

        public static Sprite GetBoxAnimationSprite(int index)
        {
            if (BoxAnimationSprites == null || BoxAnimationSprites.Length == 0)
                return null;

            index = Mathf.Clamp(index, 0, BoxAnimationSprites.Length - 1);
            if (BoxAnimationSprites[index] == null)
                BoxAnimationSprites[index] = Helpers.LoadSpriteFromResources($"TheOtherThem.Resources.TricksterAnimation.trickster_box_00{index + 1:00}.png", 175f);

            return BoxAnimationSprites[index];
        }

        public static void StartAnimation(int ventId)
        {
            JackInTheBox box = AllJackInTheBoxes.FirstOrDefault((x) => x?._vent != null && x._vent.Id == ventId);
            if (box == null) return;
            Vent vent = box._vent;

            HudManager.Instance.StartCoroutine(Effects.Lerp(0.6f, new Action<float>((p) =>
            {
                if (vent != null && vent.myRend != null)
                {
                    vent.myRend.sprite = GetBoxAnimationSprite((int)(p * BoxAnimationSprites.Length));
                    if (p == 1f) vent.myRend.sprite = GetBoxAnimationSprite(0);
                }
            })));
        }

        private GameObject _gameObject;
        public Vent _vent;

        public JackInTheBox(Vector2 p)
        {
            _gameObject = new GameObject("JackInTheBox");
            Vector3 position = new Vector3(p.x, p.y, PlayerControl.LocalPlayer.transform.position.z + 1f);
            position += (Vector3)PlayerControl.LocalPlayer.Collider.offset; // Add collider offset that DoMove moves the player up at a valid position
            // Create the marker
            _gameObject.transform.position = position;
            var boxRenderer = _gameObject.AddComponent<SpriteRenderer>();
            boxRenderer.sprite = GetBoxAnimationSprite(0);

            // Create the vent
            var referenceVent = UnityEngine.Object.FindObjectOfType<Vent>();
            _vent = UnityEngine.Object.Instantiate<Vent>(referenceVent);
            _vent.transform.position = _gameObject.transform.position;
            _vent.Left = null;
            _vent.Right = null;
            _vent.Center = null;
            _vent.EnterVentAnim = null;
            _vent.ExitVentAnim = null;
            _vent.Offset = new Vector3(0f, 0.25f, 0f);
            _vent.GetComponent<PowerTools.SpriteAnim>()?.Stop();
            _vent.Id = ShipStatus.Instance.AllVents.Select(x => x.Id).Max() + 1; // Make sure we have a unique id
            var ventRenderer = _vent.GetComponent<SpriteRenderer>();
            ventRenderer.sprite = GetBoxAnimationSprite(0);
            _vent.myRend = ventRenderer;
            var allVentsList = ShipStatus.Instance.AllVents.ToList();
            allVentsList.Add(_vent);
            ShipStatus.Instance.AllVents = allVentsList.ToArray();
            _vent.gameObject.SetActive(false);
            _vent.name = "JackInTheBoxVent_" + _vent.Id;

            // Only render the box for the Trickster
            var playerIsTrickster = PlayerControl.LocalPlayer == Trickster.trickster;
            _gameObject.SetActive(playerIsTrickster);

            AllJackInTheBoxes.Add(this);
        }

        public static void UpdateStates()
        {
            if (BoxesConvertedToVents == true) return;
            foreach (var box in AllJackInTheBoxes)
            {
                var playerIsTrickster = PlayerControl.LocalPlayer == Trickster.trickster;
                box._gameObject.SetActive(playerIsTrickster);
            }
        }

        public void ConvertToVent()
        {
            _gameObject.SetActive(false);
            _vent.gameObject.SetActive(true);
            return;
        }

        public static void ConvertToVents()
        {
            AllJackInTheBoxes.ForEach(box => box.ConvertToVent());
            ConnectVents();

            BoxesConvertedToVents = true;
            return;
        }

        public static bool HasJackInTheBoxLimitReached() => AllJackInTheBoxes.Count >= JackInTheBoxLimit;

        private static void ConnectVents()
        {
            for (var i = 0; i < AllJackInTheBoxes.Count - 1; i++)
            {
                var a = AllJackInTheBoxes[i];
                var b = AllJackInTheBoxes[i + 1];
                a._vent.Right = b._vent;
                b._vent.Left = a._vent;
            }
            // Connect first with last
            AllJackInTheBoxes.First()._vent.Left = AllJackInTheBoxes.Last()._vent;
            AllJackInTheBoxes.Last()._vent.Right = AllJackInTheBoxes.First()._vent;
        }

        public static void ClearJackInTheBoxes()
        {
            BoxesConvertedToVents = false;
            AllJackInTheBoxes.Clear();
        }

    }

}