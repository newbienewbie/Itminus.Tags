
Remove-Item ".vs" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "paket-files" -Recurse -Force -ErrorAction SilentlyContinue

Get-ChildItem "bin" -Recurse -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
Get-ChildItem "obj" -Recurse -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue