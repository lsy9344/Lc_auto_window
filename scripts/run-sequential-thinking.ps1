[CmdletBinding()]
param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]] $RemainingArgs
)

$ErrorActionPreference = 'Stop'

function Get-NpxPath {
    $commandNames = @('npx.cmd', 'npx.exe', 'npx.ps1', 'npx')
    foreach ($name in $commandNames) {
        $command = Get-Command $name -ErrorAction SilentlyContinue
        if ($command) {
            return $command.Path
        }
    }

    $candidateRoots = @(
        $env:ProgramFiles,
        ${env:ProgramFiles(x86)},
        $env:LOCALAPPDATA,
        $env:APPDATA
    ) | Where-Object { $_ }

    $candidateSuffixes = @(
        'nodejs\npx.cmd',
        'nodejs\npx.exe',
        'nodejs\npx.ps1',
        'npm\npx.cmd',
        'npm\npx.ps1'
    )

    foreach ($root in $candidateRoots) {
        foreach ($suffix in $candidateSuffixes) {
            $fullPath = Join-Path $root $suffix
            if (Test-Path $fullPath) {
                return $fullPath
            }
        }
    }

    throw 'Unable to locate the npx executable. Install Node.js and ensure npx is available on PATH.'
}

$npxPath = Get-NpxPath
$arguments = @('-y', '@modelcontextprotocol/server-sequential-thinking')
if ($RemainingArgs) {
    $arguments += $RemainingArgs
}

& $npxPath @arguments

