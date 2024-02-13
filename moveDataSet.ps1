param (
    [string]$userFolder = ".\Datasets"
)

$destinationFolder = ".\bin\x64\Debug\BTRProcessor"

# Check if the source folder exists
if (Test-Path $userFolder -PathType Container) {
    # Create the destination folder if it doesn't exist
    if (-not (Test-Path $destinationFolder -PathType Container)) {
        New-Item -ItemType Directory -Path $destinationFolder | Out-Null
    }

    # Copy the contents of the userFolder to the destination folder
    Copy-Item -Path $userFolder -Destination $destinationFolder -Recurse -Force

    Write-Host "Folder '$userFolder' and its contents copied to '$destinationFolder'."
} else {
    Write-Host "Source folder '$userFolder' does not exist."
}
