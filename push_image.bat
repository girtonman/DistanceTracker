@REM https://docs.digitalocean.com/products/container-registry/how-to/use-registry-docker-kubernetes/
@REM https://docs.digitalocean.com/reference/doctl/how-to/install/
@REM https://docs.digitalocean.com/reference/api/create-personal-access-token/

@REM doctl auth init distance
@REM doctl registry login

docker build distance_tracker:2.1
docker tag distance_tracker:2.1 registry.digitalocean.com/noodle/distance_tracker:2.1
docker push registry.digitalocean.com/noodle/distance_tracker:2.1

@REM In droplet:
@REM docker pull registry.digitalocean.com/noodle/distance_tracker:2.1
@REM docker run -d -p 80:80 --name tracker_2_1 registry.digitalocean.com/noodle/distance_tracker:2.1
