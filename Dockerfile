# ─── Stage 1: Build ───────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY *.sln ./
COPY FIXIT.API/*.csproj ./FIXIT.API/
COPY FIXIT.Application/*.csproj ./FIXIT.Application/
COPY FIXIT.Infrastructure/*.csproj ./FIXIT.Infrastructure/
COPY FIXIT.Presentation/*.csproj ./FIXIT.Presentation/
COPY FIXIT.Domain/*.csproj ./FIXIT.Domain/

RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish FIXIT.API/FIXIT.API.csproj -c Release -o /out

# ─── Stage 2: Runtime ─────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /out .

ARG CLIENTGOOGLEID
ARG CLIENTGOOGLESECRET
ARG CONSTR
ARG EMAILPASSWORD
ARG FATAWEREK_API_KEY
ARG HMAC_SECRET_KEY
ARG JWTKEY
ARG PAYMOP_API_KEY
ARG PAYMOP_API_PUBLICKEY
ARG PAYMOP_API_SECRETKEY
ARG PAYMOP_IFRAME_ID
ARG PAYMOP_INTEGRATION_ID
ARG PROVIDER_KEY

ENV CLIENTGOOGLEID=$CLIENTGOOGLEID
ENV CLIENTGOOGLESECRET=$CLIENTGOOGLESECRET
ENV CONSTR=$CONSTR
ENV EMAILPASSWORD=$EMAILPASSWORD
ENV FATAWEREK_API_KEY=$FATAWEREK_API_KEY
ENV HMAC_SECRET_KEY=$HMAC_SECRET_KEY
ENV JWTKEY=$JWTKEY
ENV PAYMOP_API_KEY=$PAYMOP_API_KEY
ENV PAYMOP_API_PUBLICKEY=$PAYMOP_API_PUBLICKEY
ENV PAYMOP_API_SECRETKEY=$PAYMOP_API_SECRETKEY
ENV PAYMOP_IFRAME_ID=$PAYMOP_IFRAME_ID
ENV PAYMOP_INTEGRATION_ID=$PAYMOP_INTEGRATION_ID
ENV PROVIDER_KEY=$PROVIDER_KEY

EXPOSE 8080

ENTRYPOINT ["dotnet", "FIXIT.API.dll"]