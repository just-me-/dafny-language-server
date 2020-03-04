FROM mono:5.20

ARG Z3_RELEASE=https://github.com/Z3Prover/z3/releases/download/z3-4.8.7/z3-4.8.7-x64-ubuntu-16.04.zip
ARG GO_RELEASE=go1.10.3.linux-amd64.tar.gz
ARG NODE_VERSION=10.16.3
ARG SonarScanner_RELEASE=3.0.3.778
ARG BOOGIE_RELEASE=v2.4.2

ENV DOTNET_CLI_TELEMETRY_OPTOUT=true
ENV DISPLAY=:99
ENV XVFB_WHD=${XVFB_WHD:-1280x720x16}
ENV CXX="g++-4.9"
ENV CC="gcc-4.9"
ENV NODE_ENV=dev

# Note: The mkdir is required due to a bug in the debian-slim image.
RUN apt-get update \
 && mkdir -p /usr/share/man/man1\
 && apt-get install -y openjdk-8-jdk-headless gpg apt-transport-https git python3-pip wget unzip\
 && curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > /etc/apt/trusted.gpg.d/microsoft.asc.gpg \
 && curl https://packages.microsoft.com/config/debian/9/prod.list > /etc/apt/sources.list.d/microsoft-prod.list \
 && apt-get update \
 && mkdir -p /usr/share/man/man1 \
 && apt-get install -y openjdk-8-jdk-headless dotnet-sdk-3.0 nunit \
 && apt-get install -y libsecret-1-0 iproute2 xvfb x11-utils libdbus-1-3 libgtk-3-0 libnotify-bin libgnome-keyring0 libgconf2-4 libasound2 libcap2 libcups2 libxtst6 libxss1 libnss3 \
 && rm -rf /var/lib/apt/lists/* /tmp/*

WORKDIR /opt

RUN wget --no-verbose https://dl.google.com/go/${GO_RELEASE} &&\
    tar -xzf ${GO_RELEASE} &&\
    rm -r ${GO_RELEASE}
ENV GOROOT=/opt/go
ENV PATH=${PATH}:${GOROOT}/bin

RUN pip3 install lit OutputCheck pyyaml

RUN wget --no-verbose ${Z3_RELEASE} &&\
    unzip z3*.zip &&\
    rm *.zip &&\
    mv z3* z3
ENV PATH=$PATH:/opt/z3

RUN git clone --branch ${BOOGIE_RELEASE} https://github.com/boogie-org/boogie.git &&\
    msbuild boogie/Source/Boogie.sln
ENV PATH=$PATH:/opt/boogie/Binaries

RUN wget --no-verbose https://nodejs.org/dist/v${NODE_VERSION}/node-v${NODE_VERSION}-linux-x64.tar.xz &&\
    tar -xJf node-v${NODE_VERSION}-linux-x64.tar.xz &&\
    rm node-v${NODE_VERSION}-linux-x64.tar.xz &&\
    ln -s node-v${NODE_VERSION}-linux-x64 node
ENV PATH=$PATH:/opt/node/bin

RUN curl -o sonarscanner.zip -L https://binaries.sonarsource.com/Distribution/sonar-scanner-cli/sonar-scanner-cli-${SonarScanner_RELEASE}-linux.zip && \
    unzip sonarscanner.zip && \
    rm sonarscanner.zip && \
    mv sonar-scanner-${SonarScanner_RELEASE}-linux sonar-scanner
ENV PATH=$PATH:/opt/sonar-scanner/bin

ENV SONAR_RUNNER_HOME=/opt/sonar-scanner


RUN mkdir /mono
WORKDIR /mono
