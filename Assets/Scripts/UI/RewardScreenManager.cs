using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardScreenManager : MonoBehaviour
{
    public GameObject rewardUI;
    public Button[] spellButtons;
    public Button skipButton;

    private PlayerController playerController;
    private Spell[] rewardSpells = new Spell[3];

    void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();

        // Setup skip button
        skipButton.onClick.AddListener(() => {
            CleanupAndStartNextWave();
        });
    }

    void Update()
    {
        if (GameManager.Instance.state == GameManager.GameState.WAVEEND)
        {
            ShowReward();
        }
    }

    void ShowReward()
{
    if (rewardSpells[0] != null) return;

    var spellBuilder = new SpellBuilder(playerController.spellsJson);  // FIXED LINE

    for (int i = 0; i < 3; i++)
    {
        rewardSpells[i] = spellBuilder.BuildRandomSpell(playerController.spellcaster);
        int index = i;

        var buttonText = spellButtons[i].GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = $"{GetFullSpellName(rewardSpells[i])} (Dmg: {rewardSpells[i].GetDamage()}, Mana: {rewardSpells[i].GetManaCost()})";

        spellButtons[i].onClick.RemoveAllListeners();
        spellButtons[i].onClick.AddListener(() =>
        {
            playerController.spellcaster.spell = rewardSpells[index];
            CleanupAndStartNextWave();
        });
    }

    rewardUI.SetActive(true);
}


    void CleanupAndStartNextWave()
    {
        // Clear rewards
        for (int i = 0; i < rewardSpells.Length; i++)
        {
            rewardSpells[i] = null;
        }

        rewardUI.SetActive(false);
        FindFirstObjectByType<EnemySpawner>().NextWave();
    }

    string GetFullSpellName(Spell spell)
{
    if (spell is ModifierSpell modifier)
    {
        return $"{modifier.GetType().Name.Replace("Spell", "")} → {GetFullSpellName(modifier.innerSpell)}";
    }
    else
    {
        return spell.GetName();
    }
}


}


