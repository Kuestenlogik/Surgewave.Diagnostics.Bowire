# Surgewave.Diagnostics.Bowire

The [Bowire](https://github.com/Kuestenlogik/Bowire) workbench for Surgewave, shipped as an installable broker plugin. It serves the interactive browser at `/bowire` speaking `surgewave://`.

> **A development and staging aid.** It is deliberately not part of a production deployment. That is why it is installed rather than bundled: a configuration switch still ships the endpoints, the embedded resources and the dependencies into the image, whereas an uninstalled plugin is not there at all.

## Install

```bash
surgewave plugins install path/to/Kuestenlogik.Surgewave.Diagnostics.Bowire.swpkg
surgewave plugins list
```

The workbench is then served at `/bowire`. Set `Surgewave:Bowire:Enabled=false` to switch an installed workbench off without uninstalling it.

## What it contains

| Assembly | Origin |
|---|---|
| `Kuestenlogik.Surgewave.Diagnostics.Bowire` | this repository — the `IBrokerPlugin` |
| `Kuestenlogik.Bowire` | bundled: the broker does not carry it |
| `Kuestenlogik.Bowire.Protocol.Surgewave` | bundled: the `surgewave://` adapter |

Everything else — `Surgewave.Plugins`, `Surgewave.Client`, `Core`, `Protocol`, `Transport` — is **provided by the host** and is kept out of the package.

The broker's own activation path (`BrokerPluginActivator`) loads plugins with `Assembly.LoadFrom` into the default context and skips assemblies already loaded by simple name, so it tolerates host copies in a package. The isolated path (`PluginLoader`, `PluginDiscovery` with `useDefaultContext: false`) does not: it resolves plugin-first, so a bundled `Surgewave.Plugins` gives the plugin a different `IPlugin` — still discovered, since that check compares namespace and name as strings, but never activated, because `IsAssignableFrom` fails. Excluding them is correct under both, and stops a build-time copy from being preferred over the running broker's.

The csproj enforces this for the whole `Kuestenlogik.Surgewave.*` prefix rather than per reference, because the adapter pulls several of them in transitively.

## Why a separate repository

It sits between two independently versioned products. Built here, it is released *after* both, so it always compiles against the current Bowire and the current Surgewave — neither has to wait for it, and nothing has to exist before the broker's installer is built.

See [Surgewave#154](https://github.com/Kuestenlogik/Surgewave/issues/154).
