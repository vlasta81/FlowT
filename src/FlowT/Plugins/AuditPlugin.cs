using FlowT.Abstractions;
using System;
using System.Collections.Generic;

namespace FlowT.Plugins
{
    /// <summary>
    /// Built-in plugin that accumulates structured audit entries for the current flow execution.
    /// Entries are stored in memory for the lifetime of the flow and can be flushed to a
    /// persistent store (database, log, audit service) by the caller after the flow completes.
    /// </summary>
    /// <remarks>
    /// Register via <c>services.AddFlowPlugin&lt;IAuditPlugin, AuditPlugin&gt;()</c>.
    /// <para>
    /// Usage:
    /// <code>
    /// var audit = context.Plugin&lt;IAuditPlugin&gt;();
    /// audit.Record("OrderCreated", new { orderId, userId });
    ///
    /// foreach (var entry in audit.Entries)
    ///     logger.LogInformation("[{Timestamp}] {Action}", entry.Timestamp, entry.Action);
    /// </code>
    /// </para>
    /// </remarks>
    public interface IAuditPlugin
    {
        /// <summary>
        /// Gets all audit entries recorded during the current flow execution, in chronological order.
        /// </summary>
        IReadOnlyList<AuditEntry> Entries { get; }

        /// <summary>
        /// Records an audit action with an optional data payload.
        /// The entry timestamp is set to <see cref="DateTimeOffset.UtcNow"/> at the time of the call.
        /// </summary>
        /// <param name="action">A short description of the action being audited (e.g., "OrderCreated", "PaymentFailed").</param>
        /// <param name="data">An optional object carrying contextual data for the entry. Not serialized automatically.</param>
        void Record(string action, object? data = null);
    }

    /// <summary>
    /// Represents a single audit event recorded during a flow execution.
    /// </summary>
    /// <param name="Action">A short description of the audited action.</param>
    /// <param name="Timestamp">The UTC timestamp when the action was recorded.</param>
    /// <param name="Data">Optional contextual data associated with the action.</param>
    public sealed class AuditEntry
    {
        /// <summary>Gets the short description of the audited action.</summary>
        public string Action { get; }

        /// <summary>Gets the UTC timestamp when the action was recorded.</summary>
        public DateTimeOffset Timestamp { get; }

        /// <summary>Gets optional contextual data associated with the action.</summary>
        public object? Data { get; }

        /// <summary>Initializes a new <see cref="AuditEntry"/>.</summary>
        public AuditEntry(string action, DateTimeOffset timestamp, object? data = null)
        {
            Action = action;
            Timestamp = timestamp;
            Data = data;
        }
    }

    /// <summary>
    /// Default implementation of <see cref="IAuditPlugin"/>.
    /// Inherits from <see cref="FlowPlugin"/> so that <see cref="FlowContext"/> is injected automatically.
    /// </summary>
    public class AuditPlugin : FlowPlugin, IAuditPlugin
    {
        private readonly List<AuditEntry> _entries = new List<AuditEntry>();

        /// <inheritdoc />
        public IReadOnlyList<AuditEntry> Entries => _entries;

        /// <inheritdoc />
        public void Record(string action, object? data = null)
        {
            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentException("Action must not be null or whitespace.", nameof(action));

            _entries.Add(new AuditEntry(action, DateTimeOffset.UtcNow, data));
        }
    }
}
