using System;
using UnityEngine;

public static class GameEvents
{
    // Combat hits
    public static event Action<Monster, float> OnClickHit;
    public static event Action                 OnClickMiss;
    public static event Action<Monster, float> OnUltSlashHit;

    // Ultimate state
    public static event Action OnUltStart;
    public static event Action OnUltEnd;

    // Animator control (routed by AnimatorRouter on Player)
    public static event Action<string>       OnTrigger;
    public static event Action<string, bool> OnBool;
    public static event Action<string, int>  OnInt;

    // Monster state (optional, for UI/VFX)
    public static event Action<Monster>      OnMonsterBroken;
    public static event Action<Monster, int> OnMonsterPhaseChanged;

    // Rune finishers (optional extra flavor)
    public static event Action<RuneType>     OnRuneFinisher;

    // Helper raisers
    public static void RaiseClickHit(Monster m, float damage) => OnClickHit?.Invoke(m, damage);
    public static void RaiseClickMiss()                       => OnClickMiss?.Invoke();
    public static void RaiseUltSlashHit(Monster m, float dmg) => OnUltSlashHit?.Invoke(m, dmg);

    public static void RaiseUltStart() => OnUltStart?.Invoke();
    public static void RaiseUltEnd()   => OnUltEnd?.Invoke();

    public static void RaiseTrigger(string name)          => OnTrigger?.Invoke(name);
    public static void RaiseBool(string name, bool value) => OnBool?.Invoke(name, value);
    public static void RaiseInt(string name, int value)   => OnInt?.Invoke(name, value);

    public static void RaiseMonsterBroken(Monster m)             => OnMonsterBroken?.Invoke(m);
    public static void RaiseMonsterPhaseChanged(Monster m, int phase) => OnMonsterPhaseChanged?.Invoke(m, phase);

    public static void RaiseRuneFinisher(RuneType rune) => OnRuneFinisher?.Invoke(rune);
}

public enum SlashType
{
    None      = 0,
    Vertical  = 1,
    Horizontal= 2,
    Diagonal  = 3
}

public enum RuneType
{
    None = 0,
    X    = 1,
    V    = 2,
    Z    = 3
}
