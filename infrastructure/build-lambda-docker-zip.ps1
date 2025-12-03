# Move to src folder
Push-Location ../src

# Build Docker image
docker build --no-cache -t gl_posting_interface .

# Return to infrastructure folder
Pop-Location
Push-Location $PSScriptRoot

# Create container from image
docker create --name temp-nativeaot gl_posting_interface

# Copy artifacts from container
docker cp temp-nativeaot:/artifacts/ .

# Remove temp container
docker rm temp-nativeaot

# Remove image
docker rmi gl_posting_interface
