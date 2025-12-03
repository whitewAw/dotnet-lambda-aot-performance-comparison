FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src
COPY . .

RUN apt-get update && \
    apt-get install -y clang zlib1g-dev zip

#Build AOT Lambda 8 (single native executable)
RUN dotnet restore ./LambdaAOTDemo8/LambdaAOTDemo8.csproj && \
    dotnet publish ./LambdaAOTDemo8/LambdaAOTDemo8.csproj -c Release -o /artifacts/publish && \
    mv /artifacts/publish/LambdaAOTDemo8 /artifacts/publish/bootstrap && \
    cd /artifacts/publish && zip -r /artifacts/LambdaAOTDemo8-lambda.zip . -x '*.dbg' -x '*.xml' -x '*.pdb' && \
    rm -rf /artifacts/publish

#Build AOT Lambda 9 (single native executable)
RUN dotnet restore ./LambdaAOTDemo9/LambdaAOTDemo9.csproj && \
    dotnet publish ./LambdaAOTDemo9/LambdaAOTDemo9.csproj -c Release -o /artifacts/publish && \
    mv /artifacts/publish/LambdaAOTDemo9 /artifacts/publish/bootstrap && \
    cd /artifacts/publish && zip -r /artifacts/LambdaAOTDemo9-lambda.zip . -x '*.dbg' -x '*.xml' -x '*.pdb' && \
    rm -rf /artifacts/publish

#Build AOT Lambda 10 (single native executable)
RUN dotnet restore ./LambdaAOTDemo10/LambdaAOTDemo10.csproj && \
    dotnet publish ./LambdaAOTDemo10/LambdaAOTDemo10.csproj -c Release -o /artifacts/publish && \
    mv /artifacts/publish/LambdaAOTDemo10 /artifacts/publish/bootstrap && \
    cd /artifacts/publish && zip -r /artifacts/LambdaAOTDemo10-lambda.zip . -x '*.dbg' -x '*.xml' -x '*.pdb' && \
    rm -rf /artifacts/publish

# Build ReadyToRun Lambda
RUN dotnet restore ./LambdaReadyToRunDemo/LambdaReadyToRunDemo.csproj && \
    dotnet publish ./LambdaReadyToRunDemo/LambdaReadyToRunDemo.csproj -c Release -o /artifacts/publish && \
    cd /artifacts/publish && zip -r /artifacts/LambdaR2RDemo-lambda.zip . -x '*.dbg' -x '*.xml' -x '*.pdb' && \
    rm -rf /artifacts/publish

# Build Regular Lambda
RUN dotnet restore ./LambdaRegularDemo/LambdaRegularDemo.csproj && \
    dotnet publish ./LambdaRegularDemo/LambdaRegularDemo.csproj -c Release -o /artifacts/publish && \
    cd /artifacts/publish && zip -r /artifacts/LambdaRegularDemo-lambda.zip . -x '*.dbg' -x '*.xml' -x '*.pdb' && \
    rm -rf /artifacts/publish

#Build AOT Lambda Invoker (single native executable)
RUN dotnet restore ./LambdaInvoker/LambdaInvoker.csproj && \
    dotnet publish ./LambdaInvoker/LambdaInvoker.csproj -c Release -o /artifacts/publish && \
    mv /artifacts/publish/LambdaInvoker /artifacts/publish/bootstrap && \
    cd /artifacts/publish && zip -r /artifacts/LambdaInvoker-lambda.zip . -x '*.dbg' -x '*.xml' -x '*.pdb' && \
    rm -rf /artifacts/publish

