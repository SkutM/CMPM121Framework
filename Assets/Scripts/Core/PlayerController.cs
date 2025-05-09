using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public Hittable hp;
    public HealthBar healthui;
    public ManaBar manaui;
    public SpellCaster[] spellcasters = new SpellCaster[4];
    public SpellUI spellui;
    public int speed;
    public Unit unit;
    public TextAsset spellsJson;

    void Start()
    {
        unit = GetComponent<Unit>();
        GameManager.Instance.player = gameObject;
    }

    public void StartLevel()
    {
        for (int i = 0; i < 4; i++)
        {
            spellcasters[i] = new SpellCaster(125, 8, Hittable.Team.PLAYER, spellsJson);
            StartCoroutine(spellcasters[i].ManaRegeneration());
        }

        hp = new Hittable(100, Hittable.Team.PLAYER, gameObject);
        hp.OnDeath += Die;
        hp.team = Hittable.Team.PLAYER;

        healthui.SetHealth(hp);
        manaui.SetSpellCaster(spellcasters[0]);

        SpellBuilder builder = new SpellBuilder(spellsJson);
        Spell startingSpell = builder.Build("arcane_bolt", spellcasters[0]);
        spellcasters[0].spell = startingSpell;
        spellui.UpdateSlot(0, startingSpell);
    }

    void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            StartCoroutine(spellcasters[0].Cast(transform.position, GetMouseWorldPosition())); // supposedly. Now no mouseClick simply key presses (MAKE NOTE)

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            StartCoroutine(spellcasters[1].Cast(transform.position, GetMouseWorldPosition()));

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            StartCoroutine(spellcasters[2].Cast(transform.position, GetMouseWorldPosition()));

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
            StartCoroutine(spellcasters[3].Cast(transform.position, GetMouseWorldPosition()));
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector2 mouseScreen = Mouse.current.position.value;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0;
        return mouseWorld;
    }

    void OnAttack(InputValue value)
    {
        if (GameManager.Instance.state == GameManager.GameState.PREGAME || GameManager.Instance.state == GameManager.GameState.GAMEOVER) return;
        Vector2 mouseScreen = Mouse.current.position.value;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0;
    }

    void OnMove(InputValue value)
    {
        if (GameManager.Instance.state == GameManager.GameState.PREGAME || GameManager.Instance.state == GameManager.GameState.GAMEOVER) return;
        unit.movement = value.Get<Vector2>() * speed;
    }

    void Die()
    {
    }

    public void ApplyWaveScaling(int wave) // will finish this 5-8
    {
        int maxHP = RPN.ParseInt("95 wave 5 * +", wave);
        int mana = RPN.ParseInt("90 wave 10 * +", wave);
        int manaRegen = RPN.ParseInt("10 wave +", wave);
        int spellPower = RPN.ParseInt("wave 10 *", wave);

        float hpPercentage = (float)hp.hp / hp.max_hp;
        hp.SetMaxHP(maxHP);
        hp.hp = Mathf.RoundToInt(hpPercentage * maxHP);

        for (int i = 0; i < 4; i++)
        {
            spellcasters[i].max_mana = mana;
            spellcasters[i].mana = mana;
            spellcasters[i].mana_reg = manaRegen;
            spellcasters[i].spellPower = spellPower;
        }

        // finished 5-9 :)))))))))
    }
}
