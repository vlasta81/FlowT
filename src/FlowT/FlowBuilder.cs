using System;
using System.Collections.Generic;
using FlowT.Contracts;

namespace FlowT
{
    internal sealed class FlowBuilder<TRequest, TResponse> : IFlowBuilder<TRequest, TResponse>
    {
        public List<Type> Specifications { get; } = new();
        public List<Type> Policies { get; } = new();
        public Type Handler { get; private set; }
        public Func<FlowInterrupt<object?>, TResponse>? InterruptMapper { get; private set; }

        public IFlowBuilder<TRequest, TResponse> Check<TSpec>() where TSpec : IFlowSpecification<TRequest>
        {
            Specifications.Add(typeof(TSpec));
            return this;
        }

        public IFlowBuilder<TRequest, TResponse> Use<TPolicy>() where TPolicy : IFlowPolicy<TRequest, TResponse>
        {
            Policies.Add(typeof(TPolicy));
            return this;
        }

        public IFlowBuilder<TRequest, TResponse> Handle<THandler>() where THandler : IFlowHandler<TRequest, TResponse>
        {
            if (Handler is not null)
            {
                throw new InvalidOperationException("Handler is already defined. Handle() can be called only once.");
            }
            Handler = typeof(THandler);
            return this;
        }

        public IFlowBuilder<TRequest, TResponse> OnInterrupt(Func<FlowInterrupt<object?>, TResponse> mapper)
        {
            if (InterruptMapper is not null)
            {
                throw new InvalidOperationException("Interrupt mapper is already defined. OnInterrupt() can be called only once.");
            }
            InterruptMapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            return this;
        }
    }
}
