# copy env file (just use example)
cp .env.sample .env

# --- instrument ---

# clone instrumentation tool
cd ..
git clone https://github.com/cs-24-pt-10-01/Using-acorn-to-decorate-JS.git

# stopping instrumentation of knex migration (by moving it out)
mv ./fastify-realworld-example-app/knex knex

# install dependencies
cd ./Using-acorn-to-decorate-JS
npm install package.json
cd ..

# instrumenting
node ./Using-acorn-to-decorate-JS/decorateFolder.js ./fastify-realworld-example-app

# removing instrumentation tool
rm -rf Using-acorn-to-decorate-JS

# adding knex migration back
mv knex ./fastify-realworld-example-app/knex

cd ./fastify-realworld-example-app

# --- run test ---

# install stuff
npm install newman # newman used in run-api-tests.sh
npm install

# starting the service
npm start & 

sleep 5s

# run the tests
APIURL=http://localhost:5000/api ./run-api-tests.sh

# stopping the service (warning this will kill all node processes)
pkill node