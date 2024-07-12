using System.Collections.Generic;
using Input;
using Player;
using UnityEngine;

namespace Upgrades
{
    public class UpgradeManager : MonoBehaviour
    {
        public List<UpgradeCard> cards;
        public List<UpgradeInfo> upgrades;
        public ThirdPersonController player;

        public bool DEBUG_Present;
        // Start is called before the first frame update

        private int _selected = 0;
        private int lastSelected = 0;
        void Start()
        {
            Inputs.Select += ChangeSelection;
            Inputs.Confirm += ConfirmCard;
        }

        private void ChangeSelection(Vector2 input)
        {
            if (input.x != 0) _selected = Mathf.Clamp(_selected + (int) input.x, 0, 2);
            // Indicate which is selected
            // _selected.SetSelected(true);
            // upgrades[lastSelected].SetSelected(false);
            // lastSelected = _selected;
        }

        private void ConfirmCard()
        {
            // player.SelectedUpgrade = cards[_selected].UpgradePrefab;
            foreach (var card in cards)
            {
                card.gameObject.SetActive(false);
            }
        }
        
        public void PresentUpgrades()
        {
            
            var indices = DrawRandomUpgrade(2);
            for (var i = 0; i < cards.Count; i++)
            {
                cards[i].SetUpgradeInfo(upgrades[indices[i]]);
                cards[i].gameObject.SetActive(true);
            }

        }

        private List<int> DrawRandomUpgrade(int amount)
        {
            var result = new HashSet<int>();
            while (result.Count < amount) {
                 result.Add(Random.Range(0, upgrades.Count));
            }
            return new List<int>(result);
        }

        // Update is called once per frame
        void Update()
        {
            if (DEBUG_Present)
            {
                DEBUG_Present = false;
                PresentUpgrades();
            }
        }
    }
}
