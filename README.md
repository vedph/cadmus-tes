# Cadmus TES

Backend for the Cadmus TES project.

🐋 Quick Docker image build:

```sh
docker buildx create --use

docker buildx build . --platform linux/amd64,linux/arm64,windows/amd64 -t vedph2020/cadmus-tes-api:0.0.1 -t vedph2020/cadmus-tes-api:latest --push
```

(replace with the current version).
