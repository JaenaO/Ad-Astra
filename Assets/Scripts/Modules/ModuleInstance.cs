using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ModuleInstance : MonoBehaviour
{
    public ModuleDefinition definition;
    private bool isRunning = false;

    public void Activate()
    {
        if (isRunning) return;
        isRunning = true;

        if (definition.category == ModuleCategory.Generator && definition.tickInterval > 0)
            StartCoroutine(GeneratorLoop());

        if (definition.category == ModuleCategory.Converter && definition.convertInterval > 0)
            StartCoroutine(ConverterLoop());
    }

    IEnumerator GeneratorLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(definition.tickInterval);
            if (definition.passiveAD > 0) GameManager.Instance.AddMaterial(MaterialTier.AsteroidDust,  definition.passiveAD);
            if (definition.passiveSC > 0) GameManager.Instance.AddMaterial(MaterialTier.SpaceCrystal,  definition.passiveSC);
            if (definition.passiveF  > 0) GameManager.Instance.AddMaterial(MaterialTier.StarFragment,   definition.passiveF);
            if (definition.passiveN  > 0) GameManager.Instance.AddMaterial(MaterialTier.Novaflare,      definition.passiveN);
        }
    }

    IEnumerator ConverterLoop()
    {
        while (true)
        {
            float interval = definition.convertInterval / definition.convertSpeedMultiplier;
            yield return new WaitForSeconds(interval);

            bool success = GameManager.Instance.Spend(definition.convertFrom, definition.convertFromAmount);
            if (success)
                GameManager.Instance.AddMaterial(definition.convertTo, definition.convertToAmount);
        }
    }
}