using System.Threading;
using System.Threading.Tasks;
using FlowT.Abstractions;

namespace FlowT.Plugins
{
    /// <summary>
    /// Built-in plugin interface that coordinates a database transaction across all pipeline stages
    /// within a single flow execution (specifications → policies → handler).
    /// </summary>
    /// <remarks>
    /// Implement this interface with a concrete class for a specific database provider.
    /// Because the plugin is PerFlow, a policy can begin a transaction that the handler participates in
    /// without any direct coupling between the two.
    /// <para>
    /// Register the concrete implementation via:
    /// <c>services.AddFlowPlugin&lt;ITransactionPlugin, MyTransactionPlugin&gt;()</c>
    /// </para>
    /// <para>
    /// Usage in a transaction policy:
    /// <code>
    /// public class TransactionPolicy : FlowPolicy&lt;TRequest, TResponse&gt;
    /// {
    ///     public override async ValueTask&lt;TResponse&gt; HandleAsync(TRequest request, FlowContext context)
    ///     {
    ///         var tx = context.Plugin&lt;ITransactionPlugin&gt;();
    ///         await tx.BeginAsync(context.CancellationToken);
    ///         try
    ///         {
    ///             var response = await Next!.HandleAsync(request, context);
    ///             await tx.CommitAsync(context.CancellationToken);
    ///             return response;
    ///         }
    ///         catch
    ///         {
    ///             await tx.RollbackAsync(context.CancellationToken);
    ///             throw;
    ///         }
    ///     }
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    public interface ITransactionPlugin
    {
        /// <summary>
        /// Gets a value indicating whether a transaction is currently active.
        /// Set to <c>true</c> after <see cref="BeginAsync"/> and <c>false</c> after
        /// <see cref="CommitAsync"/> or <see cref="RollbackAsync"/>.
        /// </summary>
        bool IsActive { get; }

        /// <summary>Begins a new transaction asynchronously.</summary>
        /// <param name="cancellationToken">Token to observe for cancellation.</param>
        ValueTask BeginAsync(CancellationToken cancellationToken = default);

        /// <summary>Commits the active transaction asynchronously.</summary>
        /// <param name="cancellationToken">Token to observe for cancellation.</param>
        ValueTask CommitAsync(CancellationToken cancellationToken = default);

        /// <summary>Rolls back the active transaction asynchronously.</summary>
        /// <param name="cancellationToken">Token to observe for cancellation.</param>
        ValueTask RollbackAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Abstract base for database transaction plugins. Inherit from this class to provide a concrete
    /// implementation for a specific database provider (e.g., EF Core, Dapper, ADO.NET).
    /// Automatically receives the current <see cref="FlowContext"/> via the <c>protected Context</c> property.
    /// </summary>
    /// <example>
    /// EF Core example:
    /// <code>
    /// public class EfCoreTransactionPlugin : FlowTransactionPlugin
    /// {
    ///     private IDbContextTransaction? _tx;
    ///
    ///     public override async ValueTask BeginAsync(CancellationToken cancellationToken = default)
    ///     {
    ///         _tx = await Context.Service&lt;AppDbContext&gt;().Database.BeginTransactionAsync(cancellationToken);
    ///         IsActive = true;
    ///     }
    ///
    ///     public override async ValueTask CommitAsync(CancellationToken cancellationToken = default)
    ///     {
    ///         await _tx!.CommitAsync(cancellationToken);
    ///         IsActive = false;
    ///     }
    ///
    ///     public override async ValueTask RollbackAsync(CancellationToken cancellationToken = default)
    ///     {
    ///         await _tx!.RollbackAsync(cancellationToken);
    ///         IsActive = false;
    ///     }
    /// }
    ///
    /// // Registration
    /// services.AddFlowPlugin&lt;ITransactionPlugin, EfCoreTransactionPlugin&gt;();
    /// </code>
    /// </example>
    public abstract class TransactionPlugin : FlowPlugin, ITransactionPlugin
    {
        /// <inheritdoc />
        public bool IsActive { get; protected set; }

        /// <inheritdoc />
        public abstract ValueTask BeginAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public abstract ValueTask CommitAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public abstract ValueTask RollbackAsync(CancellationToken cancellationToken = default);
    }
}
