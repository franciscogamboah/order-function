FROM mcr.microsoft.com/dotnet/sdk:6.0 as build

WORKDIR /app

COPY ./Bope.Fn.Commission.sln /app
COPY ./src /app/src
COPY ./tests /app/tests

WORKDIR /app

CMD dotnet test
