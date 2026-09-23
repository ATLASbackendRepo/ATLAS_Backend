$roots = @('ATLAS', 'ATLASDomain', 'ATLAsBLL', 'ATLAsDCL')
$files = Get-ChildItem $roots -Recurse -Filter *.cs | Where-Object { $_.FullName -notmatch '\\Migrations\\' }

foreach ($file in $files) {
	$text = [System.IO.File]::ReadAllText($file.FullName)
	$original = $text

	# Pattern: capture XML doc lines, then capture one or more attribute lines, and swap them
	# This handles: /// ... /// lines followed by [attrs] lines
	do {
		$updated = $text -replace '(?ms)^([ \t]*)((?:///.*\r?\n)+)([ \t]*\[)', "`$3`$2`$1"
		if ($updated -eq $text) { break }
		$text = $updated
	} while ($true)

	if ($text -ne $original) {
		[System.IO.File]::WriteAllText($file.FullName, $text)
		Write-Host "Fixed: $($file.Name)"
	}
}
