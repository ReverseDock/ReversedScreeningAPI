#!/bin/bash

# Set the directory containing the files that you want to send
FILES_DIRECTORY="/home/fk/dev/web/reversed-screening/testrun_big/receptors/"

# Set the URL for the POST request
POST_URL="http://localhost:1337/admin/receptors"

# Loop through all files in the directory
for file in $FILES_DIRECTORY/*; do
  filebasename=$(basename $file)
  id=$(perl -lne 'print $1 if /-([\d\w]+)-F/' <<< $filebasename)
  echo $id
  # id=${filebasename:3:6}
  # Send a POST request containing the file
  curl -F "formFile=@$file" -F "UniProtId=$id" $POST_URL
done
