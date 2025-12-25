@echo off
echo Starting Resilience Verification...
echo.

echo 1. Stopping existing containers...
docker-compose down

echo.
echo 2. Starting containers...
docker-compose up -d --build

echo.
echo 3. Waiting for services to be ready...
timeout /t 30

echo.
echo 4. Sending a signal to trigger workflow...
curl -X POST http://localhost:8080/api/signals -H "Content-Type: application/json" -d "{\"value\": 100}"

echo.
echo.
echo 5. Checking logs for Aggregation Service (expecting completion or timeout)...
timeout /t 10
docker-compose logs aggregation-service

echo.
echo Verification Triggered. Please review logs above.
pause
