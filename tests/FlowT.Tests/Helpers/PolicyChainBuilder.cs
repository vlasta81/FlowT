using FlowT.Abstractions;
using FlowT.Contracts;
using System.Reflection;

namespace FlowT.Tests.Helpers;

/// <summary>
/// Helper for manually building policy chains in tests
/// Uses reflection to call protected internal SetNext method
/// </summary>
public static class PolicyChainBuilder
{
    private static readonly MethodInfo SetNextMethod = typeof(FlowPolicy<,>)
        .GetMethod("SetNext", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        ?? throw new InvalidOperationException("SetNext method not found");

    /// <summary>
    /// Chains policies together: policy1 -> policy2 -> ... -> handler
    /// </summary>
    public static TPolicy ChainPolicies<TRequest, TResponse, TPolicy>(
        TPolicy outerPolicy,
        IFlowHandler<TRequest, TResponse> innerHandler)
        where TPolicy : FlowPolicy<TRequest, TResponse>
    {
        CallSetNext(outerPolicy, innerHandler);
        return outerPolicy;
    }

    /// <summary>
    /// Chains multiple policies together
    /// </summary>
    public static FlowPolicy<TRequest, TResponse> ChainPolicies<TRequest, TResponse>(
        IFlowHandler<TRequest, TResponse> handler,
        params FlowPolicy<TRequest, TResponse>[] policies)
    {
        if (policies.Length == 0)
        {
            throw new ArgumentException("At least one policy is required", nameof(policies));
        }

        // Chain from innermost to outermost (reverse order)
        IFlowHandler<TRequest, TResponse> current = handler;
        for (int i = policies.Length - 1; i >= 0; i--)
        {
            CallSetNext(policies[i], current);
            current = policies[i];
        }

        return policies[0]; // Return outermost policy
    }

    /// <summary>
    /// Creates a simple two-policy chain
    /// </summary>
    public static TOuterPolicy Chain<TRequest, TResponse, TOuterPolicy, TInnerPolicy>(
        TOuterPolicy outer,
        TInnerPolicy inner,
        IFlowHandler<TRequest, TResponse> handler)
        where TOuterPolicy : FlowPolicy<TRequest, TResponse>
        where TInnerPolicy : FlowPolicy<TRequest, TResponse>
    {
        CallSetNext(inner, handler);
        CallSetNext(outer, inner);
        return outer;
    }

    private static void CallSetNext<TRequest, TResponse>(
        FlowPolicy<TRequest, TResponse> policy,
        IFlowHandler<TRequest, TResponse> next)
    {
        var policyType = policy.GetType();
        while (policyType != null && !policyType.IsGenericType)
        {
            policyType = policyType.BaseType;
        }

        if (policyType == null)
        {
            throw new InvalidOperationException("Could not find FlowPolicy<,> base type");
        }

        var method = policyType.GetMethod("SetNext", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        method?.Invoke(policy, new object[] { next });
    }
}
