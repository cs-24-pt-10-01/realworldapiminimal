# install stuff
npm install newman # newman used in run-api-tests.sh


for i in {1..10}
do
    # starting the service
    dotnet run --configuration Release --project src/Api & 

    sleep 10s

    chmod +x run-api-tests.sh

    # run the tests
    APIURL=http://localhost:5000 ./run-api-tests.sh

    # stopping the service (warning this will kill all node processes)
    pkill dotnet

    sleep 10s # wait for the service to stop
done