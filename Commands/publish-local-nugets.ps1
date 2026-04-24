# Clean NuGet cache
Write-Host "Cleaning NuGet cache..."
Remove-Item -Recurse -Force "$env:USERPROFILE\.nuget\packages\notorioustest.core" -ErrorAction SilentlyContinue

# Publish DoggyDog for Windows
Write-Host "Publishing DoggyDog for win-x64..."
dotnet publish ./DoggyDog/DoggyDog.csproj `
    -r win-x64 `
    --self-contained `
    -p:PublishSingleFile=true `
    -o ./artifacts/doggydog/win-x64

# Publish DoggyDog for Linux
Write-Host "Publishing DoggyDog for linux-x64..."
dotnet publish ./DoggyDog/DoggyDog.csproj `
    -r linux-x64 `
    --self-contained `
    -p:PublishSingleFile=true `
    -o ./artifacts/doggydog/linux-x64

# Pack NotoriousTest.Core
Write-Host "Packing nugets..."
dotnet pack ./NotoriousTest.slnx -o ./local-packages --version-suffix local
Write-Host "Done!"
