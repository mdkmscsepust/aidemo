#!/bin/bash
# TableVine — start backend + open Android Studio
set -e

cd "$(dirname "$0")"

# Kill any existing backend on port 5000
echo "Clearing port 5000..."
lsof -i :5000 2>/dev/null | awk 'NR>1 {print $2}' | xargs -r kill -9
sleep 1

# Start backend in background
echo "Starting API on 0.0.0.0:5000..."
dotnet run --project src/ReservationPlatform.API --launch-profile http &
API_PID=$!
echo "API running (PID $API_PID)"

# Open Android Studio
echo "Opening Android Studio..."
export CAPACITOR_ANDROID_STUDIO_PATH=/opt/android-studio/bin/studio.sh
cd client
npx cap open android

echo ""
echo "Done! In Android Studio:"
echo "  1. Wait for Gradle sync"
echo "  2. Select your phone in the device dropdown"
echo "  3. Press Run ▶"
