# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:6.0
COPY . /app
WORKDIR /app
RUN ["dotnet", "restore"]
RUN ["dotnet", "build"]
EXPOSE 80/tcp
RUN chmod +x ./entrypoint.sh
CMD /bin/bash ./entrypoint.sh

