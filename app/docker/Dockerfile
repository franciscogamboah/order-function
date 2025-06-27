FROM mcr.microsoft.com/dotnet/sdk:6.0 as build

RUN mkdir /app
WORKDIR /app

COPY ./app/Bope.Fn.Commission.sln /app
COPY ./app/src /app/src

WORKDIR /app/src/AWSLambda/
RUN dotnet restore -r linux-x64 /p:PublishReadyToRun=true
RUN dotnet publish -c Release -o /var/task --no-restore -r linux-x64 /p:PublishReadyToRun=true

FROM public.ecr.aws/lambda/dotnet:6 AS base

COPY --from=build /var/task /var/task

ENV TZ=America/Lima
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

EXPOSE 8080

CMD [ "AWSLambda::AWSLambda.Function::FunctionHandler" ]
