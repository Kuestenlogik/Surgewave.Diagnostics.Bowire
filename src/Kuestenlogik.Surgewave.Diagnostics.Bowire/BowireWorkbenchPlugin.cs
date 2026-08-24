// Copyright 2026 Küstenlogik
// SPDX-License-Identifier: Apache-2.0

using Kuestenlogik.Bowire;
using Kuestenlogik.Bowire.Protocol.Surgewave;
using Kuestenlogik.Surgewave.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kuestenlogik.Surgewave.Diagnostics.Bowire;

/// <summary>
/// Serves the Bowire workbench at <c>/bowire</c>, speaking <c>surgewave://</c>.
/// </summary>
/// <remarks>
/// This ships as an installable plugin rather than being bundled into the broker,
/// which is what keeps it out of a production deployment: a configuration switch
/// still delivers the endpoints, the embedded resources and the dependencies into
/// the image, whereas an uninstalled plugin is simply not there.
/// </remarks>
public sealed class BowireWorkbenchPlugin : IBrokerPlugin
{
    internal const string RoutePrefix = "/bowire";

    /// <inheritdoc />
    public string FeatureId => "diagnostics.bowire";

    /// <inheritdoc />
    public string DisplayName => "Bowire workbench";

    /// <summary>
    /// Enabled by default: installing the plugin is the opt-in. The switch exists
    /// so an installed workbench can be turned off without uninstalling it.
    /// </summary>
    public bool IsConfigEnabled(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return configuration.GetValue("Surgewave:Bowire:Enabled", true);
    }

    /// <summary>
    /// Initialises Bowire. This is the step the broker's inline wiring skipped:
    /// it only ever called MapBowire, so <c>BowireRecordingSession</c>,
    /// <c>SchemaChangeLogStore</c> and <c>PluginUpdateCheckService</c> were never
    /// registered — and the workbench endpoints resolve all three with
    /// <c>GetRequiredService</c>, so those surfaces threw on first use. AddBowire
    /// also applies the storage root that decides where collections, environments
    /// and recordings live.
    /// </summary>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddBowire();
    }

    /// <inheritdoc />
    public void Configure(object host, IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(host);

        var app = (WebApplication)host;

        // Bowire finds protocols by scanning loaded assemblies, and its force-load
        // pass looks in the ENTRY assembly's directory — the broker's, not this
        // plugin's, so it will not find the adapter shipped in this package.
        // Touching the type loads it into this plugin's load context, where it
        // shares Bowire's IBowireProtocol identity and the scan can see it.
        _ = typeof(BowireSurgewaveProtocol);

        app.MapBowire(RoutePrefix, options =>
        {
            options.Title = "Surgewave gRPC API";
            options.Description = "Interactive gRPC browser for Surgewave Broker";
        });
    }
}
