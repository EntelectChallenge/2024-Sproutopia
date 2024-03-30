#!/bin/bash

IMAGE_NAME="sproutopia"
FORCE_BUILD=false

# Check for command-line argument to force build
while [[ "$#" -gt 0 ]]; do
    case $1 in
        -b|--build) FORCE_BUILD=true;;
        *) 
            echo "Unknown parameter passed: $1"
            echo "Usage: $0 [OPTION]"
            echo "OPTIONS:"
            echo "  -b, --build                  force build of docker image even if it already exists"
            exit 1;;
    esac
    shift
done

# Function to build the Docker image
build_image() {
    echo "Building Docker image '$IMAGE_NAME'..."
    docker build --no-cache --build-arg CONFIGURATION=Debug -t "$IMAGE_NAME" .
}

# Check if the Docker image already exists or force build
if $FORCE_BUILD || ! docker image inspect "$IMAGE_NAME" &> /dev/null; then
    build_image
else
    echo "Docker image '$IMAGE_NAME' already exists. Skipping build."
fi

# Run the Docker container
docker run -it --rm -p 5000:5000 --name "$IMAGE_NAME" "$IMAGE_NAME"
