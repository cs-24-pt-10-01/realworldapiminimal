# install stuff
npm install newman # newman used in run-api-tests.sh

# starting the service
dotnet run --configuration Release --project src/Api & 

sleep 10s

# run the tests
APIURL=http://localhost:5000 ./run-api-tests.sh

# stopping the service (warning this will kill all node processes)
pkill dotnet