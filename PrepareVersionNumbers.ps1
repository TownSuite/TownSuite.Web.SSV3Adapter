$cwd = $PSScriptRoot


& "./tools/AssemblyInfoUtil.exe" -inc:3 "$cwd/TownSuite.Web.SSV3Adapter/TownSuite.Web.SSV3Adapter.csproj"
& "./tools/AssemblyInfoUtil.exe" -inc:3 "$cwd/TownSuite.Web.SSV3Adapter.Interfaces/TownSuite.Web.SSV3Adapter.Interfaces.csproj"
& "./tools/AssemblyInfoUtil.exe" -inc:3 "$cwd/TownSuite.Web.SSV3Adapter.Prometheus/TownSuite.Web.SSV3Adapter.Prometheus.csproj"
