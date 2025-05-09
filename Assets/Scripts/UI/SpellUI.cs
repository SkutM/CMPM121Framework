using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SpellUI : MonoBehaviour
{
    public SpellSlotUI[] slotUIs;
    public string description;

    public string GetDescription()
    {
        return description;
    }

    public void UpdateSlot(int slotIndex, Spell spell)
    {
        if (slotIndex >= 0 && slotIndex < slotUIs.Length)
        {
            slotUIs[slotIndex].SetSpell(spell);
        }
    }
}

[System.Serializable] // important
public class SpellSlotUI
{
    public GameObject rootObject;
    public GameObject icon;
    public TextMeshProUGUI manacost;
    public TextMeshProUGUI damage;

    public void SetSpell(Spell spell)
    {
        if (rootObject != null)
            rootObject.SetActive(true);

        GameManager.Instance.spellIconManager.PlaceSprite(spell.GetIcon(), icon.GetComponent<Image>()); // if use...
        manacost.text = spell.GetManaCost().ToString();
        damage.text = spell.GetDamage().ToString();
    }
}
