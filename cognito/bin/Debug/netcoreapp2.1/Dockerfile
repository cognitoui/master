FROM microsoft/dotnet:2.1-aspnetcore-runtime-alpine
ADD publish /root/app
RUN apk update && apk add python && apk add --update py-pip && pip install awscli && mkdir /root/.aws
COPY config /root/.aws/config
COPY credentials /root/.aws/credentials
RUN cd /root/app
WORKDIR /root/app
ENTRYPOINT ["dotnet", "/root/app/cognito.dll"]
