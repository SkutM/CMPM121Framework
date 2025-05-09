using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardScreenManager : MonoBehaviour
{
    public GameObject rewardUI;
    public TextMeshProUGUI rewardDescription;
    public Button acceptButton;
    public Button skipButton;
    public Button[] replaceButton;

    private PlayerController playerController;
    private Spell rewardSpell;
    private bool skipRequested = false;

    void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();

        acceptButton.onClick.AddListener(() => HandleAccept()); // CMPM: 146
        skipButton.onClick.AddListener(() =>
        {
            skipRequested = true; // can't figure out how else to do this :L()
            newWave();
        });

        foreach (var button in replaceButton)
            button.gameObject.SetActive(false);
    }

    void Update()
    {
        if (GameManager.Instance.state == GameManager.GameState.WAVEEND && rewardSpell == null && !skipRequested)
            ShowReward();
    }

    void ShowReward()
    {
        var spellBuilder = new SpellBuilder(playerController.spellsJson);
        rewardSpell = spellBuilder.BuildRandomSpell(playerController.spellcasters[0]);

        rewardDescription.text =
        // name, desc, damage, mana.
            $"{spellName(rewardSpell)}\n" +
            $"{spellDesc(rewardSpell)}\n" +
            $"Damage: {rewardSpell.GetDamage()}, Mana: {rewardSpell.GetManaCost()}";

        rewardUI.SetActive(true);
    }

    void HandleAccept()
    {
        int emptySlot = FindFirstEmptySlot();

        if (emptySlot >= 0)
        {
            assignSlot(emptySlot);
            newWave();
        }
        else
        {
            for (int i = 0; i < replaceButton.Length; i++)
            {
                int slot = i;
                replaceButton[i].gameObject.SetActive(true);
                replaceButton[i].onClick.RemoveAllListeners();
                replaceButton[i].onClick.AddListener(() =>
                {
                    assignSlot(slot);
                    newWave();
                });
            }
        }
    }

    int FindFirstEmptySlot() // find the first empty slot in spellcasters array (0), named aptly (future reference)
    {
        for (int i = 0; i < playerController.spellcasters.Length; i++)
        {
            if (playerController.spellcasters[i].spell == null)
                return i;
        }
        return -1;
    }

    void assignSlot(int slot)
    {
        if (slot < 0 || slot >= playerController.spellcasters.Length)
            return;

        playerController.spellcasters[slot].spell = rewardSpell;
        playerController.spellui.UpdateSlot(slot, rewardSpell);
    }

    void newWave() // added some functionality for skipping ... 5-7
    {
        rewardSpell = null;
        skipRequested = false;

        foreach (var button in replaceButton)
            button.gameObject.SetActive(false);

        rewardUI.SetActive(false);
        FindFirstObjectByType<EnemySpawner>().NextWave();
    }

    string spellName(Spell spell)
    {
        if (spell is ModifierSpell modifier)
            return $"{modifier.GetType().Name.Replace("Spell", "")} → {spellName(modifier.innerSpell)}"; // an HOUR. took me an HOUR
        else
            return spell.GetName();
    }

    string spellDesc(Spell spell)
    {
        if (spell is ModifierSpell modifier)
            return spellDesc(modifier.innerSpell);
        else if (spell is BaseSpell baseSpell)
            return baseSpell.GetDescription();
        else
            return "";
    }
}
