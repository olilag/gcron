#!/bin/bash

umask u=rwx,go=rx
sudo mkdir -p /var/spool/gcron
user_id=$(id -u)
sudo chown "$user_id" /var/spool/gcron 
