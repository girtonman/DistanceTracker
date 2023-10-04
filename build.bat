docker build -t distance-tracker .
docker container stop holdboost
docker container rm holdboost
docker run -d -p 80:80 --name holdboost distance-tracker
docker container logs -f holdboost
