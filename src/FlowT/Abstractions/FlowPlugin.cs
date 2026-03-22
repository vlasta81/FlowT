namespace FlowT.Abstractions
{
    /// <summary>
    /// Abstract base class for plugins that need direct access to the <see cref="FlowContext"/>.
    /// Inherit from this class to get an automatically injected <see cref="Context"/> property
    /// without exposing initialization details to external callers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When <see cref="FlowContext.Plugin{T}"/> resolves a plugin that inherits from <see cref="FlowPlugin"/>,
    /// it automatically binds the current <see cref="FlowContext"/> before returning the instance.
    /// The <see cref="Context"/> property is available for all subsequent method calls within the same flow execution.
    /// </para>
    /// <para>
    /// The plugin is <strong>PerFlow</strong> — one instance per <see cref="FlowContext"/> execution,
    /// shared across all pipeline stages (specifications, policies, handler).
    /// </para>
    /// <para>
    /// Plugins that do not need <see cref="FlowContext"/> access do not have to inherit from this class.
    /// </para>
    /// <example>
    /// <code>
    /// public class AuditPlugin : FlowPlugin, IAuditPlugin
    /// {
    ///     private readonly ILogger&lt;AuditPlugin&gt; _logger;
    ///
    ///     public AuditPlugin(ILogger&lt;AuditPlugin&gt; logger) => _logger = logger;
    ///
    ///     public void Record(string action)
    ///     {
    ///         _logger.LogInformation("[{FlowId}] {Action}", Context.GetFlowIdString(), action);
    ///         Context.Set(new AuditEntry(action, Context.StartedAt));
    ///         Context.PublishInBackground(new AuditEvent(action), Context.CancellationToken);
    ///     }
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    public abstract class FlowPlugin
    {
        /// <summary>
        /// Gets the <see cref="FlowContext"/> this plugin is bound to.
        /// Available after the plugin is resolved via <see cref="FlowContext.Plugin{T}"/>.
        /// </summary>
        protected FlowContext Context { get; private set; } = null!;

        /// <summary>
        /// Binds the plugin to its <see cref="FlowContext"/>. Called once by the framework immediately
        /// after the plugin instance is created. Not intended to be called by user code.
        /// </summary>
        /// <param name="context">The <see cref="FlowContext"/> to bind to this plugin.</param>
        internal void Initialize(FlowContext context) => Context = context;
    }

}
