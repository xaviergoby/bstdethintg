## Introduction

This is a simple pipeline example for a .NET Core application, showing just
how easy it is to get up and running with .NET development using GitLab.

# Reference links

- [GitLab CI Documentation](https://docs.gitlab.com/ee/ci/)

## What's contained in this project

The root of the repository contains the out of the `dotnet new console` command,
which generates a new console application that just prints out "Hello, World."
It's a simple example, but great for demonstrating how easy GitLab CI is to
use with .NET. Check out the `Program.cs` and `dotnetcore.csproj` files to
see how these work.

In addition to the .NET Core content, there is a ready-to-go `.gitignore` file
sourced from the the .NET Core [.gitignore](https://github.com/dotnet/core/blob/master/.gitignore). This
will help keep your repository clean of build files and other configuration.

Finally, the `.gitlab-ci.yml` contains the configuration needed for GitLab
to build your code. Let's take a look, section by section.

First, we note that we want to use the official Microsoft .NET SDK image
to build our project.

```
image: microsoft/dotnet:latest
```

We're defining two stages here: `build`, and `test`. As your project grows
in complexity you can add more of these.

```
stages:
    - build
    - test
```

Next, we define our build job which simply runs the `dotnet build` command and
identifies the `bin` folder as the output directory. Anything in the `bin` folder
will be automatically handed off to future stages, and is also downloadable through
the web UI.

```
build:
    stage: build
    script:
        - "dotnet build"
    artifacts:
      paths:
        - bin/
```

Similar to the build step, we get our test output simply by running `dotnet test`.

```
test:
    stage: test
    script: 
        - "dotnet test"
```

This should be enough to get you started. There are many, many powerful options 
for your `.gitlab-ci.yml`. You can read about them in our documentation 
[here](https://docs.gitlab.com/ee/ci/yaml/).

## Developing with Gitpod

This template repository also has a fully-automated dev setup for [Gitpod](https://docs.gitlab.com/ee/integration/gitpod.html).

The `.gitpod.yml` ensures that, when you open this repository in Gitpod, you'll get a cloud workspace with .NET Core pre-installed, and your project will automatically be built and start running.

## Entity Framework migrations

When datamodels or data seeding change, the migrations must be updated so the patest model is reflected to the database. To do this execute the followin commands in a Powershell command propt:
```
cd .\src\Hodl.Api\
dotnet ef migrations add [MigrationName]
dotnet ef database update
```

## Appsettings and configuration external services

In the appsettings.json, settings for the running environment are configures. This varies from background service timeouts to database and email configurations. For each environment the settings must be configured. Be aware that production settings must be different from the testing environment, for security reasons.

### Google email configuration

To send e-mails using Google Workspace accounts, you must activate two factor authentication on the account where you want to send mail from. Then an app password can be generated to give access without the 2-factor authentication. The app password can be generated on the following page: https://myaccount.google.com/apppasswords

### Google OAuth2 configuration

To enable the OAuth authentication from Google, enable the OAuth API in the Google developers console: https://console.cloud.google.com/apis/credentials?project=omega-metric-341612

Use "Create credentials" to create an "OAuth client ID". Choose "Web application" and name it recognizable for the application and running environment. Then add the front-end URI in the Authorized Origins, and create the credential. Copy the ClientId in the appsettings.json, and in the front-end config.json.

## Running a local dev environment

To build a docker from the source, open a command prompt and navigate to the project folder. Then go in to the src directory and execute the followin command:
```
docker build --pull -f Dockerfile-debug -t hodl-td-api:local .
```

To run a local environment you can pull the latest hodl-td-api container from the Gitlab container registry. 
Create a folder to start the environment from, like ```C:\docker\hodl-td\```. Copy the ```docker-compose.yml```
in that folder and a copy of the appsettings.json from ```./src/Hodl.Api/```. Change the database connection string from
```"TradingDeskConnectionString": "Host=localhost;Database=hodlTD;username=hodlTD;password=[CHANGE]"``` to 
```"TradingDeskConnectionString": "Host=hodl-td-db-dev;Database=hodlTD;username=hodlTD;password=[CHANGE]"```. Then add an 
environment file (```.env```) and put the following values in there:
```
POSTGRES_USER=hodlTD
POSTGRES_PASSWORD=[CHANGE]
POSTGRES_DB=hodlTD
```
In the docker-compose file, you can either run the locally build docker, of the docker from the Gitlab container registry.
If you pull from the registry for the first time, you have to setup the login for the repository in docker. Do that using 
the following command:
```
docker login -u [USERNAMR] -p [PASSWORD] registry.gitlab.com
```

Now execute the following commands from the command-line from the where you saved the docker-compose.yml and settings files:
```
docker-compose pull
docker-compose up -d
```

You can now access the API on http://localhost:5001/.

## Upgrade Postgres version in Docker
When using the official Postgres docker without version tag, there might be an update that upgrades the database version where the stored files must be migrated to the new server version. In this case, a db dump must be made using the old version. That dump can be imported again in the new version.

The following scripts can be run:
1. In the old version:
```
docker exec -it hodl-td-db.dev /usr/bin/pg_dumpall -U hodlTD > ~/hodl_db.sql
```
2. In the new version (make sure the api did not create the database already!!):
```
docker exec -i hodl-td-db.dev psql -U hodlTD < ~/hodl_db.sql
```

## Notification System Configuration

### Discord Notification Handler Integration Setup

In order to get the integration of Discord into the _Notification System_ of _Trading Desk_ you need to the acquire the Webhook of the speicifc channel which you wish 
to send alerts/notifications. First, under *Text Channels*, select *Edit Channel* (the gear icon) for the specific chanel you to have alerts/notifcations sent to. Next,
click on *Integrations* | *View Webhooks*. Now you may either click on *New Webhook* or use the Webhook which is might be already present. Next click on *Copy Webhook URL*
and use this as the value for "WebhookUrl" in appsettings.json.

```
  "DiscordOptions": {
    "WebhookUrl": "[DISCORD_CHANNEL_WEBHOOK_URL]"
  }
```
