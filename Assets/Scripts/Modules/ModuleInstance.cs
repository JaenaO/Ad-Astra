using UnityEngine;
using System.Collections;

public class ModuleInstance : MonoBehaviour
{
    public ModuleDefinition definition;
    private bool isRunning = false;

    public void Activate()
    {
        if (isRunning) return;
        isRunning = true;

        switch (definition.category)
        {
            case ModuleCategory.Generator:
                StartCoroutine(GeneratorLoop());
                break;
            case ModuleCategory.Converter:
                StartCoroutine(ConverterLoop());
                break;
            // Engines and Claws are passive — no coroutine needed
        }
    }

    // ── GENERATOR ────────────────────────────────────────────
    // Simple:   +5 AD / 10s
    // Advanced: +5 AD +2 SC / 15s
    // Enhanced: +10 AD +5 SC +2 F / 20s
    // Unrivaled: +20 AD +10 SC +5 F +5 N / 25s
    IEnumerator GeneratorLoop()
    {
        while (true)
        {
            float interval = definition.tickInterval > 0 ? definition.tickInterval : 10f;
            yield return new WaitForSeconds(interval);

            if (definition.passiveAD > 0)
                GameManager.Instance.AddMaterial(MaterialTier.AsteroidDust,  definition.passiveAD);
            if (definition.passiveSC > 0)
                GameManager.Instance.AddMaterial(MaterialTier.SpaceCrystal,  definition.passiveSC);
            if (definition.passiveF  > 0)
                GameManager.Instance.AddMaterial(MaterialTier.StarFragment,  definition.passiveF);
            if (definition.passiveN  > 0)
                GameManager.Instance.AddMaterial(MaterialTier.Novaflare,     definition.passiveN);

            Debug.Log($"[{definition.moduleName}] Generated resources.");
        }
    }

    // ── CONVERTER ────────────────────────────────────────────
    // Simple:   10 AD → 1 SC / 25s
    // Advanced: 10 SC → 2 F  / 30s
    // Enhanced: 10 F  → 2 N  / 35s
    IEnumerator ConverterLoop()
    {
        while (true)
        {
            float interval = definition.convertInterval > 0
                ? definition.convertInterval / definition.convertSpeedMultiplier
                : 25f;

            yield return new WaitForSeconds(interval);

            bool success = GameManager.Instance.Spend(
                definition.convertFrom,
                definition.convertFromAmount);

            if (success)
            {
                GameManager.Instance.AddMaterial(
                    definition.convertTo,
                    definition.convertToAmount);

                Debug.Log($"[{definition.moduleName}] Converted " +
                    $"{definition.convertFromAmount} {definition.convertFrom} → " +
                    $"{definition.convertToAmount} {definition.convertTo}");
            }
            else
            {
                Debug.Log($"[{definition.moduleName}] Not enough " +
                    $"{definition.convertFrom} to convert.");
            }
        }
    }
}