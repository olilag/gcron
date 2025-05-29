#!/bin/bash

umask u=rwx,go=rx
# for users configuration
sudo mkdir -p /var/spool/gcron
# log file
sudo touch /var/log/gcron.log
user_id=$(id -u)
sudo chown "$user_id" /var/spool/gcron
sudo chown "$user_id" /var/log/gcron.log
