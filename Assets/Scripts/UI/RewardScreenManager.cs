using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardScreenManager : MonoBehaviour
{
    public GameObject rewardUI;
    public TextMeshProUGUI rewardDescription;
    public Button acceptButton;
    public Button skipButton;
    public Button[] replaceSlotButtons;  // Buttons for slot 1–4 if full

    private PlayerController playerController;
    private Spell rewardSpell;
    private bool skipRequested = false;  // NEW: track if skip was pressed

    void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();

        acceptButton.onClick.AddListener(() => HandleAccept());
        skipButton.onClick.AddListener(() =>
        {
            skipRequested = true;
            CleanupAndStartNextWave();
        });

        foreach (var button in replaceSlotButtons)
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

        rewardDescription.text = $"{GetFullSpellName(rewardSpell)}\nDamage: {rewardSpell.GetDamage()}, Mana: {rewardSpell.GetManaCost()}";

        rewardUI.SetActive(true);
    }

    void HandleAccept()
    {
        int emptySlot = FindFirstEmptySlot();

        if (emptySlot >= 0)
        {
            AssignToSlot(emptySlot);
            CleanupAndStartNextWave();
        }
        else
        {
            // Show replace slot buttons
            for (int i = 0; i < replaceSlotButtons.Length; i++)
            {
                int slot = i;
                replaceSlotButtons[i].gameObject.SetActive(true);
                replaceSlotButtons[i].onClick.RemoveAllListeners();
                replaceSlotButtons[i].onClick.AddListener(() =>
                {
                    AssignToSlot(slot);
                    CleanupAndStartNextWave();
                });
            }
        }
    }

    int FindFirstEmptySlot()
    {
        for (int i = 0; i < playerController.spellcasters.Length; i++)
        {
            if (playerController.spellcasters[i].spell == null)
                return i;
        }
        return -1;  // No empty slot
    }

    void AssignToSlot(int slot)
    {
        if (slot < 0 || slot >= playerController.spellcasters.Length)
        {
            Debug.LogError($"Invalid slot index {slot}! Spellcasters array length: {playerController.spellcasters.Length}");
            return;
        }

        playerController.spellcasters[slot].spell = rewardSpell;
        playerController.spellui.UpdateSlot(slot, rewardSpell);

        Debug.Log($"Assigned reward to slot {slot}");
    }

    void CleanupAndStartNextWave()
    {
        rewardSpell = null;
        skipRequested = false;  // NEW: reset skip flag

        foreach (var button in replaceSlotButtons)
            button.gameObject.SetActive(false);

        rewardUI.SetActive(false);
        FindFirstObjectByType<EnemySpawner>().NextWave();
    }

    string GetFullSpellName(Spell spell)
    {
        if (spell is ModifierSpell modifier)
            return $"{modifier.GetType().Name.Replace("Spell", "")} → {GetFullSpellName(modifier.innerSpell)}";
        else
            return spell.GetName();
    }
}
