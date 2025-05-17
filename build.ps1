[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

Write-Host "▶ Проверка наличия .NET Framework 4.7.2..."

$regPath = "HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full"
$version = (Get-ItemProperty -Path $regPath -Name Release -ErrorAction SilentlyContinue).Release

if ($version -lt 461808) {
    Write-Warning "❌ Не установлен .NET Framework 4.7.2. Установка через winget..."
    winget install Microsoft.DotNet.Framework.DeveloperPack_4
    exit 1
} else {
    Write-Host "✅ .NET Framework 4.7.2 установлен."
}

# Проверка наличия базы данных
$dbPath = "\\THE_DEATH_STAR\SharedDB\ExpensesDatabase2.mdb"
if (-Not (Test-Path $dbPath)) {
    Write-Warning "⚠️ Файл базы данных не найден по пути: $dbPath"
    exit 1
} else {
    Write-Host "✅ База данных найдена: $dbPath"
}

# Сборка проекта
Write-Host "▶ Сборка проекта..."

$msbuildPath = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
& $msbuildPath .\UchetRashodov.sln /p:Configuration=Release

if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Ошибка при сборке."
    exit 1
} else {
    Write-Host "✅ Сборка завершена успешно."
}
