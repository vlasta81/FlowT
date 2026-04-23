using FlowT.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FlowT.Plugins
{
    /// <summary>
    /// Built-in plugin that measures and accumulates elapsed time for named operations within a flow execution.
    /// Use <see cref="Measure"/> in a <c>using</c> block to time a section of code; results are accessible
    /// via <see cref="Elapsed"/> after the measurement completes.
    /// </summary>
    /// <remarks>
    /// Register via <c>services.AddFlowPlugin&lt;IPerformancePlugin, PerformancePlugin&gt;()</c>.
    /// <para>
    /// Usage:
    /// <code>
    /// var perf = context.Plugin&lt;IPerformancePlugin&gt;();
    ///
    /// using (perf.Measure("db-query"))
    /// {
    ///     orders = await dbContext.Orders.ToListAsync();
    /// }
    ///
    /// logger.LogInformation("DB query took {Elapsed}", perf.Elapsed["db-query"]);
    /// </code>
    /// </para>
    /// </remarks>
    public interface IPerformancePlugin
    {
        /// <summary>
        /// Gets the elapsed times for all completed measurements, keyed by the name passed to <see cref="Measure"/>.
        /// When the same name is used more than once, the last measurement overwrites the previous value.
        /// </summary>
        IReadOnlyDictionary<string, TimeSpan> Elapsed { get; }

        /// <summary>
        /// Starts timing the named operation. The elapsed time is recorded when the returned
        /// <see cref="IDisposable"/> is disposed (i.e., at the end of the <c>using</c> block).
        /// </summary>
        /// <param name="name">A unique label for the operation being timed.</param>
        /// <returns>A disposable that stops the timer and records the elapsed time when disposed.</returns>
        IDisposable Measure(string name);
    }

    /// <summary>
    /// Default implementation of <see cref="IPerformancePlugin"/>.
    /// Inherits from <see cref="FlowPlugin"/> so that <see cref="FlowContext"/> is injected automatically.
    /// </summary>
    public class PerformancePlugin : FlowPlugin, IPerformancePlugin
    {
        private readonly Dictionary<string, TimeSpan> _elapsed = new Dictionary<string, TimeSpan>();

        /// <inheritdoc />
        public IReadOnlyDictionary<string, TimeSpan> Elapsed => _elapsed;

        /// <inheritdoc />
        public IDisposable Measure(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Measurement name must not be null or whitespace.", nameof(name));

            return new MeasurementScope(name, _elapsed);
        }

        private sealed class MeasurementScope : IDisposable
        {
            private readonly string _name;
            private readonly Dictionary<string, TimeSpan> _target;
            private readonly long _start;
            private bool _disposed;

            internal MeasurementScope(string name, Dictionary<string, TimeSpan> target)
            {
                _name = name;
                _target = target;
                _start = Stopwatch.GetTimestamp();
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;
                _target[_name] = Stopwatch.GetElapsedTime(_start);
            }
        }
    }
}
