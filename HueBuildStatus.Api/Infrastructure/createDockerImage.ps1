docker-compose build huebuildstatus
docker tag huebuildstatus ghcr.io/rangerchris/huebuildstatus:latest
docker push ghcr.io/rangerchris/huebuildstatus:latest