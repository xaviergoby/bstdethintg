# Deployment instructions

This document describes the installation instructions for the Tradingdesk deployment using the built in CI/CD from Gitlab. The setup deploys the system in a docker environment that will automatically update when an update is merged in the dev- or main branch of the repository.

The host on which the deployment is described should be a Debian or Ubuntu distribution. Next, Docker should be installed and the docker-compose stacks are stored in the ```/docker``` folder. The trading desk stack is stored in ```/docker/hodl-td```. Copy the example stack for the appropriate environment in the hodl-td folder.

## Prepare server installation
First install a basic  Debian system (currently 11). If the installation ask for a user add the user "adminhodl" and an appropriate password. After install run the following commands to add sudo and ssh:
```
apt update
apt install sudo ssh
groupadd -a -G sudo $USER
sudo apt install openssh-server
sudo systemctl start ssh
sudo systemctl enable ssh
sudo usermod -a -G ssh adminhodl
```
Change the ssh settings and allow 
```
sudo nano /etc/ssh/sshd_config
```
At first set the following settings:
```
PermitRootLogin no
MaxSessions 4
PubkeyAuthentication yes
PasswordAuthentication yes
PermitEmptyPasswords no
AllowGroups     ssh
```
Then restart teh server
```
sudo systemctl restart ssh.service
```

Now we can ssh into the server using the adminhodl account. Next we're going to create a key pair to allow login by public key. On the machine where we want to access the server execute ```ssh-keygen```. Create the files in the user folder and copy the public key to the user .ssh folder.
```
nano ~/.ssh/authorized_keys
```
Paste the key.

Now we can login with ssh using the key pair we just added. 
```
ssh -i [PATH_TO_PRIVATE_KEY] [USERNAME]@[HOST]
```
Now go to the host again and remove the option to login with passwords:
```
sudo nano /etc/ssh/sshd_config
sudo systemctl restart ssh.service
```
Set: PasswordAuthentication yes


## Install Docker
```
sudo apt-get install -y \
    ca-certificates \
    curl \
    gnupg \
    lsb-release
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu \
  $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
sudo apt-get update
sudo apt-get install -y docker-ce docker-ce-cli containerd.io

sudo curl -L "https://github.com/docker/compose/releases/download/1.29.2/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
sudo usermod -aG docker $USER
sudo usermod -aG docker adminhodl
```

Limit the logging sizes for the containers
```
sudo nano /etc/docker/daemon.json
```
And fill the following lines:
{
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "10m",
    "max-file": "3",
    "labels": "production_status",
    "env": "os,customer"
  }
}

Then restart the service:
```
sudo service docker restart
```

## Create folders for the docker stacks
```
sudo mkdir -p /docker/hodl-td
sudo mkdir -p /docker/portainer
sudo mkdir -p /docker/gitlab-runner
```

Copy the portainer yaml and the gitlab runner. Repo's are:
- https://gitlab.com/hodl.nl/portainer-stack
- https://gitlab.com/hodl.nl/gitlab-runner

Portainer can be run after the example.env is renamed to .env and a AGENT_SECRET is willed in. When started visit the portainer setup and fill in a password for the environment at http://[HOST]:9000/.

To run the hodl-td stack you first need to login to the container registry (do this also for the deploy user!)
```
docker login -u [USERNAME] -p [TOKEN] registry.gitlab.com
```
Then the stack can be run.

## Running the application stack for the first time
To start the stack for the first time, nginx proxy must be configured. First bring the stack up and then bring the nginx container down again. Copy the configs from this examples folder and create the 
After the config for the nginx
```
docker exec -it nginx nginx -c /config/nginx/nginx.conf -s reload
```

### Add user for ssh for gitlab runner
On the host a user must be created and added to the ssh group and the docker group to execute the docker commands for deployment.
```
sudo adduser deploy
sudo usermod -a -G ssh deploy
sudo usermod -a -G docker deploy
sudo mkdir /home/deploy/.ssh
sudo nano /home/deploy/.ssh/authorized_keys
docker login -u [USERNAME] -p [TOKEN] registry.gitlab.com
```
Add a public key from the pc where you have created a key pair. The private key must be added as var in Gitlab.

The Gitlab runner must be registered on the Gitlab group of Hodl.nl. Start the docker and run in the command prompt:
```
docker exec -it gitlab-runner gitlab-runner register
```

