@echo off
echo Stopping PostgreSQL Database...
docker stop eventdriventemplate-postgres-1

echo Waiting for 10 seconds (Simulating outage)...
timeout /t 10

echo Restarting PostgreSQL Database...
docker start eventdriventemplate-postgres-1

echo Database restarted. Check logs for reconnection/recovery.
