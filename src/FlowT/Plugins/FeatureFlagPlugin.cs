using FlowT.Abstractions;
using Microsoft.FeatureManagement;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FlowT.Plugins
{
    /// <summary>
    /// Built-in plugin that evaluates feature flags for the current flow execution using
    /// <see cref="IVariantFeatureManager"/> from <c>Microsoft.FeatureManagement</c>.
    /// Results are cached per feature name for the lifetime of the flow so that the same
    /// feature is never evaluated more than once per pipeline execution.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Prerequisites — register feature management before registering the plugin:
    /// <code>
    /// // Program.cs
    /// builder.Services.AddFeatureManagement();           // reads from appsettings.json
    /// // — or —
    /// builder.Services.AddFeatureManagement()
    ///                 .WithTargeting();                  // adds audience-targeting support
    ///
    /// services.AddFlowPlugin&lt;IFeatureFlagPlugin, FeatureFlagPlugin&gt;();
    /// </code>
    /// </para>
    /// <para>
    /// Configuration (<c>appsettings.json</c>):
    /// <code>
    /// {
    ///   "FeatureManagement": {
    ///     "NewCheckout": true,
    ///     "BetaSearch": false
    ///   }
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// Usage inside a flow handler or specification:
    /// <code>
    /// var ff = context.Plugin&lt;IFeatureFlagPlugin&gt;();
    ///
    /// // Simple on/off gate
    /// if (!await ff.IsEnabledAsync("NewCheckout"))
    ///     return FlowInterrupt&lt;CheckoutResponse&gt;.Fail("Feature not available.", 403);
    ///
    /// // Contextual gate (e.g. targeting a specific user or tenant)
    /// bool enabled = await ff.IsEnabledAsync("BetaSearch", new TargetingContext
    /// {
    ///     UserId = context.GetUserId(),
    ///     Groups = new[] { "Beta" }
    /// });
    ///
    /// // Read from cache populated earlier in the same flow
    /// if (ff.TryGetCached("NewCheckout", out bool cached))
    ///     logger.LogDebug("Cache hit: NewCheckout={Value}", cached);
    /// </code>
    /// </para>
    /// </remarks>
    public interface IFeatureFlagPlugin
    {
        /// <summary>
        /// Checks whether the named feature flag is enabled.
        /// The result is cached per feature name for the duration of the current flow execution.
        /// </summary>
        /// <param name="feature">The name of the feature flag to evaluate.</param>
        /// <param name="cancellationToken">A cancellation token to observe.</param>
        /// <returns><c>true</c> if the feature is enabled; otherwise <c>false</c>.</returns>
        ValueTask<bool> IsEnabledAsync(string feature, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks whether the named feature flag is enabled for the given contextual value
        /// (e.g. a <c>TargetingContext</c> with <c>UserId</c> / <c>Groups</c>).
        /// The result is cached per feature name for the duration of the current flow execution.
        /// </summary>
        /// <typeparam name="TContext">The type of the context object passed to feature filters.</typeparam>
        /// <param name="feature">The name of the feature flag to evaluate.</param>
        /// <param name="featureContext">The context used by contextual feature filters.</param>
        /// <param name="cancellationToken">A cancellation token to observe.</param>
        /// <returns><c>true</c> if the feature is enabled; otherwise <c>false</c>.</returns>
        ValueTask<bool> IsEnabledAsync<TContext>(string feature, TContext featureContext, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a read-only view of all feature flag results evaluated during the current flow.
        /// Keys are feature names; values are the last cached result for that feature.
        /// </summary>
        IReadOnlyDictionary<string, bool> Cache { get; }

        /// <summary>
        /// Attempts to read a previously cached feature flag result without calling
        /// <see cref="IVariantFeatureManager"/>.
        /// </summary>
        /// <param name="feature">The feature flag name.</param>
        /// <param name="value">When this method returns <c>true</c>, the cached value.</param>
        /// <returns><c>true</c> if a cached value exists; otherwise <c>false</c>.</returns>
        bool TryGetCached(string feature, out bool value);
    }

    /// <summary>
    /// Default implementation of <see cref="IFeatureFlagPlugin"/>.
    /// Resolves <see cref="IVariantFeatureManager"/> from the DI container via constructor injection
    /// and inherits from <see cref="FlowPlugin"/> to get access to <see cref="FlowContext"/>.
    /// </summary>
    public class FeatureFlagPlugin : FlowPlugin, IFeatureFlagPlugin
    {
        private readonly IVariantFeatureManager _featureManager;
        private readonly Dictionary<string, bool> _cache = new Dictionary<string, bool>(StringComparer.Ordinal);

        /// <summary>
        /// Initializes a new instance of <see cref="FeatureFlagPlugin"/>.
        /// </summary>
        /// <param name="featureManager">
        /// The <see cref="IVariantFeatureManager"/> used to evaluate feature flags.
        /// Registered automatically when <c>services.AddFeatureManagement()</c> is called.
        /// </param>
        public FeatureFlagPlugin(IVariantFeatureManager featureManager)
        {
            ArgumentNullException.ThrowIfNull(featureManager);
            _featureManager = featureManager;
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, bool> Cache => _cache;

        /// <inheritdoc />
        public async ValueTask<bool> IsEnabledAsync(string feature, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(feature))
                throw new ArgumentException("Feature name must not be null or whitespace.", nameof(feature));

            if (_cache.TryGetValue(feature, out bool cached))
                return cached;

            bool result = await _featureManager.IsEnabledAsync(feature, cancellationToken).ConfigureAwait(false);
            _cache[feature] = result;
            return result;
        }

        /// <inheritdoc />
        public async ValueTask<bool> IsEnabledAsync<TContext>(string feature, TContext featureContext, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(feature))
                throw new ArgumentException("Feature name must not be null or whitespace.", nameof(feature));

            if (_cache.TryGetValue(feature, out bool cached))
                return cached;

            bool result = await _featureManager.IsEnabledAsync(feature, featureContext, cancellationToken).ConfigureAwait(false);
            _cache[feature] = result;
            return result;
        }

        /// <inheritdoc />
        public bool TryGetCached(string feature, out bool value) =>
            _cache.TryGetValue(feature, out value);
    }
}
