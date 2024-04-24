#!/bin/bash

# Check if the correct number of arguments is provided
if [ "$#" -ne 2 ]; then
    echo "Usage: $0 <input_file> <output_file>"
    exit 1
fi

# Store input and output file names
input_file="$1"
output_file="$2"

# Add '[' character at the start of the input file
echo '[' > "$output_file"

# Add a comma to the end of each line using sed
sed 's/\r*$//;s/$/,/' "$input_file" >> "$output_file"

# Add ']' character to the end of the output file
echo ']' >> "$output_file"