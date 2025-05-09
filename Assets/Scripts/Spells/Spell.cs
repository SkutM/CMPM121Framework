using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class Spell 
{
    public float last_cast;
    public SpellCaster owner;
    public Hittable.Team team;

    public string description;

    public Spell(SpellCaster owner)
    {
        this.owner = owner;
    }

    public virtual string GetName()
    {
        return "Bolt";
    }

    public virtual int GetManaCost()
    {
        return 10;
    }

    public virtual int GetDamage()
    {
        return 100;
    }

    public virtual float GetCooldown()
    {
        return 0.75f;
    }

    public virtual int GetIcon()
    {
        return 0;
    }

    // ✅ ADD THIS METHOD:
    public virtual string GetDescription()
    {
        return description ?? "";
    }

    public bool IsReady()
    {
        return (last_cast + GetCooldown() < Time.time);
    }

    public virtual IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;
        GameManager.Instance.projectileManager.CreateProjectile(0, "straight", where, target - where, 15f, OnHit);
        yield return new WaitForEndOfFrame();
    }

    protected virtual void OnHit(Hittable other, Vector3 impact)
    {
        if (other.team != team)
        {
            other.Damage(new Damage(GetDamage(), Damage.Type.ARCANE));
        }
    }
}

public class BaseSpell : Spell
{
protected int damage;
protected int manaCost;
protected float cooldown;
protected string trajectory;
protected float speed;
protected int spriteIndex;

    public string Trajectory
    {
        get => trajectory;
        set => trajectory = value;
    }

    public float Speed
    {
        get => speed;
        set => speed = value;
    }


    public BaseSpell(SpellCaster owner) : base(owner) {}

    public override int GetDamage() => damage;
    public override int GetManaCost() => manaCost;
    public override float GetCooldown() => cooldown;



    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;
        Vector3 direction = target - where;
        GameManager.Instance.projectileManager.CreateProjectile(spriteIndex, trajectory, where, direction, speed, OnHit);
        yield return new WaitForEndOfFrame();
    }

    public virtual void SetAttributes(JObject attributes)
    {
        damage = RPN.ParseInt(attributes["damage"]?["amount"]?.ToString(), owner.spellPower, GameManager.Instance.wave);
        manaCost = RPN.ParseInt(attributes["mana_cost"]?.ToString(), owner.spellPower, GameManager.Instance.wave);
        cooldown = RPN.ParseFloat(attributes["cooldown"]?.ToString(), owner.spellPower, GameManager.Instance.wave);


        var proj = attributes["projectile"];
        if (proj != null)
        {
            trajectory = proj["trajectory"]?.ToString();
            speed = RPN.ParseFloat(proj["speed"]?.ToString(), owner.spellPower);
            spriteIndex = proj["sprite"]?.ToObject<int>() ?? 0;
        }
    }
}


public class ModifierSpell : Spell
{
    public Spell innerSpell;

    public ModifierSpell(Spell innerSpell) : base(innerSpell.owner)
    {
        this.innerSpell = innerSpell;
    }

    public override int GetManaCost()
    {
        return innerSpell.GetManaCost();
    }

    public override int GetDamage()
    {
        return innerSpell.GetDamage();
    }

    public override float GetCooldown()
    {
        return innerSpell.GetCooldown();
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        Debug.Log("Applying modifier...");
        yield return innerSpell.Cast(where, target, team);
    }
}

public class ArcaneSpraySpell : BaseSpell
{
    private int projectileCount;

    public ArcaneSpraySpell(SpellCaster owner) : base(owner) {}

    public override string GetName()
    {
        return "Arcane Spray";
    }


    public override void SetAttributes(JObject attributes)
    {
        base.SetAttributes(attributes);
        projectileCount = RPN.ParseInt(attributes["N"]?.ToString(), owner.spellPower);
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;
        for (int i = 0; i < projectileCount; i++)
        {
            Vector3 randomOffset = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-10f, 10f)) * (target - where);
            GameManager.Instance.projectileManager.CreateProjectile(spriteIndex, trajectory, where, randomOffset, speed, OnHit);
        }
        yield return new WaitForEndOfFrame();
    }
}

public class MagicMissileSpell : BaseSpell
{
    public MagicMissileSpell(SpellCaster owner) : base(owner) {}

    public override string GetName()
    {
        return "Magic Missile";
    }


    public override void SetAttributes(JObject attributes)
    {
        base.SetAttributes(attributes);
        trajectory = "homing";  // homing
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;
        Vector3 direction = target - where;
        GameManager.Instance.projectileManager.CreateProjectile(spriteIndex, trajectory, where, direction, speed, OnHit);
        yield return new WaitForEndOfFrame();
    }
}

public class ArcaneExplosionSpell : BaseSpell
{
    private int secondaryCount;
    private float secondarySpeed;
    private int secondarySpriteIndex;

    public ArcaneExplosionSpell(SpellCaster owner) : base(owner) {}

    public override string GetName()
    {
        return "Arcane Explosion";
    }


    public override void SetAttributes(JObject attributes)
    {
        base.SetAttributes(attributes);

        secondaryCount = RPN.ParseInt(attributes["N"]?.ToString(), owner.spellPower);
        secondarySpeed = RPN.ParseFloat(attributes["secondary_projectile"]?["speed"]?.ToString(), owner.spellPower);
        secondarySpriteIndex = attributes["secondary_projectile"]?["sprite"]?.ToObject<int>() ?? 0;
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;
        Vector3 direction = target - where;

        GameManager.Instance.projectileManager.CreateProjectile(spriteIndex, trajectory, where, direction, speed, OnExplosionHit);
        yield return new WaitForEndOfFrame();
    }

    private void OnExplosionHit(Hittable other, Vector3 impact)
    {
        if (other.team != team)
        {
            other.Damage(new Damage(GetDamage(), Damage.Type.ARCANE));

            for (int i = 0; i < secondaryCount; i++)
            {
                float angle = UnityEngine.Random.Range(0, 360);
                Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector3.right;
                GameManager.Instance.projectileManager.CreateProjectile(secondarySpriteIndex, "straight", impact, dir, secondarySpeed, OnHit);
            }
        }
    }
}

public class SplitterSpell : ModifierSpell
{
    public SplitterSpell(Spell inner) : base(inner) {}

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;

        Vector3 direction = target - where;
        Vector3 dir1 = Quaternion.Euler(0, 0, 10f) * direction;
        Vector3 dir2 = Quaternion.Euler(0, 0, -10f) * direction;

        yield return innerSpell.Cast(where, where + dir1, team);
        yield return innerSpell.Cast(where, where + dir2, team);
    }
}

public class DoublerSpell : ModifierSpell
{
    public DoublerSpell(Spell inner) : base(inner) {}

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;

        yield return innerSpell.Cast(where, target, team);
        yield return new WaitForSeconds(0.2f);  // short delay
        yield return innerSpell.Cast(where, target, team);
    }
}

public class DamageMagnifierSpell : ModifierSpell
{
    private float damageMultiplier = 1.5f;
    private float manaMultiplier = 1.2f;

    public DamageMagnifierSpell(Spell inner) : base(inner) {}

    public override int GetDamage() => Mathf.RoundToInt(innerSpell.GetDamage() * damageMultiplier);
    public override int GetManaCost() => Mathf.RoundToInt(innerSpell.GetManaCost() * manaMultiplier);
}


public class SpeedModifierSpell : ModifierSpell
{
    private float speedMultiplier = 1.5f;

    public SpeedModifierSpell(Spell inner) : base(inner) {}

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        if (innerSpell is BaseSpell baseSpell)
        {
            float originalSpeed = baseSpell.Speed;
            baseSpell.Speed *= speedMultiplier;

            yield return innerSpell.Cast(where, target, team);

            baseSpell.Speed = originalSpeed;  // reset after cast
        }
        else
        {
            yield return innerSpell.Cast(where, target, team);
        }
    }
}

public class ChaosModifierSpell : ModifierSpell
{
    private float damageMultiplier = 2.0f;

    public ChaosModifierSpell(Spell inner) : base(inner) {}

    public override int GetDamage() => Mathf.RoundToInt(innerSpell.GetDamage() * damageMultiplier);

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        if (innerSpell is BaseSpell baseSpell)
        {
            string originalTrajectory = baseSpell.Trajectory;
            baseSpell.Trajectory = "spiraling";

            yield return innerSpell.Cast(where, target, team);

            baseSpell.Trajectory = originalTrajectory;  // reset after cast (again)
        }
        else
        {
            yield return innerSpell.Cast(where, target, team);
        }
    }
}

public class HomingModifierSpell : ModifierSpell
{
    private float damageMultiplier = 0.75f;
    private float manaMultiplier = 1.2f;

    public HomingModifierSpell(Spell inner) : base(inner) {}

    public override int GetDamage() => Mathf.RoundToInt(innerSpell.GetDamage() * damageMultiplier);
    public override int GetManaCost() => Mathf.RoundToInt(innerSpell.GetManaCost() * manaMultiplier);

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        if (innerSpell is BaseSpell baseSpell)
        {
            string originalTrajectory = baseSpell.Trajectory;
            baseSpell.Trajectory = "homing";

            yield return innerSpell.Cast(where, target, team);

            baseSpell.Trajectory = originalTrajectory;  // reset after cast (againx2)
        }
        else
        {
            yield return innerSpell.Cast(where, target, team);
        }
    }
}

public class ChainingLightningSpell : BaseSpell
{
    private int maxJumps;
    private float jumpRange;

    public ChainingLightningSpell(SpellCaster owner) : base(owner) {}

    public override string GetName()
    {
        return "Chain Lightning";
    }


    public override void SetAttributes(JObject attributes)
    {
        base.SetAttributes(attributes);
        maxJumps = RPN.ParseInt(attributes["N"]?.ToString(), owner.spellPower);
        jumpRange = RPN.ParseFloat(attributes["jump_range"]?.ToString(), owner.spellPower);
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;
        GameObject firstTarget = GameManager.Instance.GetClosestEnemy(target);
        if (firstTarget != null)
        {
            yield return ChainHit(firstTarget, maxJumps);
        }
        yield return new WaitForEndOfFrame();
    }

    private IEnumerator ChainHit(GameObject current, int jumpsLeft)
    {
        if (jumpsLeft <= 0) yield break;

        var enemyController = current.GetComponent<EnemyController>();
        var hittable = enemyController != null ? enemyController.hp : null;

        if (hittable != null && hittable.team != team)
        {
            hittable.Damage(new Damage(GetDamage(), Damage.Type.ARCANE));
        }

        yield return new WaitForSeconds(0.1f);  // short delay for visual effect

        // Find next closest enemy within jump range
        GameObject nextTarget = null;
        float closestDistance = float.MaxValue;
        foreach (var enemy in GameManager.Instance.GetAllEnemies())
        {
            if (enemy == current) continue;
            float dist = Vector3.Distance(current.transform.position, enemy.transform.position);
            if (dist < closestDistance && dist <= jumpRange)
            {
                closestDistance = dist;
                nextTarget = enemy;
            }
        }

        if (nextTarget != null)
        {
            yield return ChainHit(nextTarget, jumpsLeft - 1);
        }
    }

}

public class FireballSpell : BaseSpell
{
    private float radius;

    public FireballSpell(SpellCaster owner) : base(owner) {}

    public override string GetName()
    {
        return "Fireball";
    }


    public override void SetAttributes(JObject attributes)
    {
        base.SetAttributes(attributes);
        radius = RPN.ParseFloat(attributes["radius"]?.ToString(), owner.spellPower);
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
{
    this.team = team;
    float radius = 5f; // example explosion radius

    var enemies = new List<GameObject>(GameManager.Instance.GetAllEnemies());
    foreach (var enemy in enemies)
    {
        float dist = Vector3.Distance(where, enemy.transform.position);
        if (dist <= radius)
        {
            var enemyController = enemy.GetComponent<EnemyController>();
            var hittable = enemyController != null ? enemyController.hp : null;

            if (hittable != null && hittable.team != team)
            {
                hittable.Damage(new Damage(GetDamage(), Damage.Type.FIRE));
            }
        }
    }

    yield return new WaitForEndOfFrame();
}
}

public class SlowOnHitModifierSpell : ModifierSpell
{
    private float slowFactor = 0.5f;  // Reduces speed to 50%
    private float slowDuration = 2f;  // Lasts 2 seconds

    public SlowOnHitModifierSpell(Spell inner) : base(inner) {}

    protected override void OnHit(Hittable other, Vector3 impact)
    {
        base.OnHit(other, impact);

        if (other.owner != null)
        {
            var enemyController = other.owner.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.StartCoroutine(ApplySlow(enemyController));
            }
        }
    }

    private IEnumerator ApplySlow(EnemyController enemy)
    {
        var unit = enemy.GetComponent<Unit>();
        if (unit != null)
        {
            float originalSpeed = unit.movement.magnitude;

            // Slow down (reduce vector magnitude)
            unit.movement *= slowFactor;

            yield return new WaitForSeconds(slowDuration);

            // Restore original speed
            unit.movement = unit.movement.normalized * originalSpeed;
        }
    }
}



public class KnockbackModifierSpell : ModifierSpell
{
    private float knockbackForce = 5f;  // Adjust as needed

    public KnockbackModifierSpell(Spell inner) : base(inner) {}

    protected override void OnHit(Hittable other, Vector3 impact)
    {
        base.OnHit(other, impact);

        if (other.owner != null)
        {
            var rb = other.owner.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 knockbackDirection = ((Vector2)(other.owner.transform.position) - (Vector2)impact).normalized;
                rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }
}





