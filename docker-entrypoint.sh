#!/bin/bash
set -e

# Wait for PostgreSQL to be ready
echo "Waiting for PostgreSQL to be ready..."
until PGPASSWORD=poolpass psql -h postgres -U pooluser -d poolapp -c '\q'; do
  >&2 echo "PostgreSQL is unavailable - sleeping"
  sleep 1
done

>&2 echo "PostgreSQL is up - continuing"

# Note about migrations
echo "Note: Database migrations need to be applied separately."
echo "You can do this by either:"
echo "1. Running migrations before deploying"
echo "2. Connecting to this container and running migrations manually"

# Start the application
exec dotnet 8-ball-pool.dll 