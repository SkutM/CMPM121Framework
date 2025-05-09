using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SpellUI : MonoBehaviour
{
    public SpellSlotUI[] slotUIs;  // array holding per-slot UI blocks

    public void UpdateSlot(int slotIndex, Spell spell)
    {
        if (slotIndex >= 0 && slotIndex < slotUIs.Length)
        {
            Debug.Log("Updating slot " + slotIndex + " with spell " + spell.GetName());

            slotUIs[slotIndex].SetSpell(spell);
        }
    }
}

[System.Serializable]
public class SpellSlotUI
{
    public GameObject rootObject;  // ← the parent GameObject you want to show/hide
    public GameObject icon;
    public TextMeshProUGUI manacost;
    public TextMeshProUGUI damage;

public void SetSpell(Spell spell)
{
    if (rootObject != null)
        rootObject.SetActive(true);  // Make sure the UI slot is visible

    GameManager.Instance.spellIconManager.PlaceSprite(spell.GetIcon(), icon.GetComponent<Image>());
    manacost.text = spell.GetManaCost().ToString();
    damage.text = spell.GetDamage().ToString();
}

}


