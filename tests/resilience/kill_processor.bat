@echo off
echo Killing Data Processor 1...
docker stop eventdriventemplate-data-processor-1-1

echo Waiting for 10 seconds...
timeout /t 10

echo Restarting Data Processor 1...
docker start eventdriventemplate-data-processor-1-1

echo Processor restarted. Check logs for recovery.
