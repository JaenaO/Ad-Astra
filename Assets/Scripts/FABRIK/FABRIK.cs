using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Transform))]
public class FABRIK : MonoBehaviour
{
    private FABRIKChain rootChain;

    private readonly List<FABRIKChain> chains = new List<FABRIKChain>();
    private readonly Dictionary<string, FABRIKChain> endChains = new Dictionary<string, FABRIKChain>();
    private string primaryEndChainName;

    [SerializeField] private bool autoSolve = true;

    public void Awake()
    {
        Rebuild();
    }

    public void Rebuild()
    {
        chains.Clear();
        endChains.Clear();
        primaryEndChainName = null;

        rootChain = LoadSystem(transform);

        // Inversely sort by layer, greater-first for backward pass.
        chains.Sort((x, y) => y.Layer.CompareTo(x.Layer));

        foreach (FABRIKChain chain in chains)
        {
            chain.CalculateSummedWeight();
        }

        // Initialize each end-chain target to its current effector position.
        foreach (KeyValuePair<string, FABRIKChain> item in endChains)
        {
            item.Value.Target = item.Value.EndEffector.Position;
        }

        foreach (string key in endChains.Keys)
        {
            primaryEndChainName = key;
            break;
        }
    }

    private FABRIKChain LoadSystem(Transform transform, FABRIKChain parent = null, int layer = 0)
    {
        List<FABRIKEffector> effectors = new List<FABRIKEffector>();

        // Use parent chain's end effector as our sub-base effector, e.g:
        //                 [D]---[E]
        //        1       /    2        1 = [A, B, C]
        // [A]---[B]---[C]              2 = [C, D, E]
        //                \    3        3 = [C, F, G]
        //                 [F]---[G]
        if (parent != null)
        {
            effectors.Add(parent.EndEffector);
        }

        // childCount > 1 is a new sub-base
        // childCount = 0 is an end chain (added to our list below)
        // childCount = 1 is continuation of chain
        while (transform != null)
        {
            FABRIKEffector effector = transform.GetComponent<FABRIKEffector>();

            if (effector == null)
            {
                break;
            }

            effectors.Add(effector);

            if (transform.childCount != 1)
            {
                break;
            }
            
            transform = transform.GetChild(0);
        }

        FABRIKChain chain = new FABRIKChain(parent, effectors, layer);

        chains.Add(chain);

        // Add to our end chain list if it is an end chain
        if (chain.IsEndChain)
        {
            string chainName = transform.gameObject.name;
            if (!endChains.ContainsKey(chainName))
            {
                endChains.Add(chainName, chain);
            }
        }
        // Else iterate over each of the end effector's children to create a new chain in the layer above
        else foreach (Transform child in transform)
        {
            LoadSystem(child, chain, layer + 1);
        }

        return chain;
    }

    public void Update()
    {
        if (autoSolve)
        {
            Solve();
        }
    }

    public bool TrySetTarget(string endChainName, Vector3 target)
    {
        FABRIKChain chain;
        if (!TryGetEndChain(endChainName, out chain))
        {
            return false;
        }

        chain.Target = target;
        return true;
    }

    public bool SetPrimaryTarget(Vector3 target)
    {
        if (string.IsNullOrEmpty(primaryEndChainName))
        {
            return false;
        }

        return TrySetTarget(primaryEndChainName, target);
    }

    public bool TryGetPrimaryEndEffectorPosition(out Vector3 position)
    {
        position = Vector3.zero;
        if (string.IsNullOrEmpty(primaryEndChainName))
        {
            return false;
        }

        FABRIKChain chain;
        if (!TryGetEndChain(primaryEndChainName, out chain))
        {
            return false;
        }

        position = chain.EndEffector.Position;
        return true;
    }

    public void Solve()
    {
        if (rootChain == null || chains.Count == 0)
        {
            return;
        }

        // We must iterate by layer in the first stage, working from target(s) to root
        foreach (FABRIKChain chain in chains)
        {
            chain.Backward();
        }

        // Provided our hierarchy, the second stage doesn't directly require an iterator
        rootChain.ForwardMulti();
    }

    public FABRIKChain GetEndChain(string name)
    {
        return endChains[name];
    }

    private bool TryGetEndChain(string name, out FABRIKChain chain)
    {
        return endChains.TryGetValue(name, out chain);
    }
}
