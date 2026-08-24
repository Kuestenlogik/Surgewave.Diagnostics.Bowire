// Copyright 2026 Küstenlogik
// SPDX-License-Identifier: Apache-2.0

using Kuestenlogik.Bowire;
using Kuestenlogik.Bowire.Recording;
using Kuestenlogik.Surgewave.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kuestenlogik.Surgewave.Diagnostics.Bowire.Tests;

public sealed class BowireWorkbenchPluginTests
{
    private static IConfiguration Config(params (string Key, string Value)[] values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(v => new KeyValuePair<string, string?>(v.Key, v.Value)))
            .Build();

    [Fact]
    public void Plugin_IsDiscoverableAsABrokerPlugin()
    {
        Assert.IsAssignableFrom<IBrokerPlugin>(new BowireWorkbenchPlugin());
    }

    [Fact]
    public void IsConfigEnabled_DefaultsToEnabled_BecauseInstallingIsTheOptIn()
    {
        Assert.True(new BowireWorkbenchPlugin().IsConfigEnabled(Config()));
    }

    [Fact]
    public void IsConfigEnabled_HonoursAnExplicitOff()
    {
        var configuration = Config(("Surgewave:Bowire:Enabled", "false"));

        Assert.False(new BowireWorkbenchPlugin().IsConfigEnabled(configuration));
    }

    /// <summary>
    /// The workbench endpoints resolve these three with GetRequiredService, and only
    /// AddBowire registers them. The broker's inline wiring called MapBowire without
    /// AddBowire, so those surfaces threw on first use — this pins that the plugin
    /// does the initialisation the inline call site skipped.
    /// </summary>
    [Theory]
    [InlineData(typeof(BowireRecordingSession))]
    [InlineData(typeof(SchemaChangeLogStore))]
    public void ConfigureServices_RegistersWhatTheWorkbenchEndpointsRequire(Type required)
    {
        var services = new ServiceCollection();

        new BowireWorkbenchPlugin().ConfigureServices(services, Config());

        Assert.Contains(services, d => d.ServiceType == required);
    }
}
