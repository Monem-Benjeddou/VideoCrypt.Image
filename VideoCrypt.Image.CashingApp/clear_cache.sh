#!/bin/sh

CACHE_DIR=/cache

echo "Clearing cache in $CACHE_DIR..."

# Check if the cache directory exists
if [ -d "$CACHE_DIR" ]; then
  # Remove all files in the cache directory
  rm -rf ${CACHE_DIR:?}/*
  echo "Cache cleared."
else
  echo "Cache directory does not exist."
fi