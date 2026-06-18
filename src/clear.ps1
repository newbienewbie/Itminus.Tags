
Remove-Item ".vs" -Recurse -Force
Remove-Item "paket-files" -Recurse -Force

Get-ChildItem "bin" -Recurse | Remove-Item -Recurse -Force 
Get-ChildItem "obj" -Recurse | Remove-Item -Recurse -Force 