#!/bin/sh
#
# shalla_update.sh, v 0.3 20070418
# done by kapivie at sil.at under FreeBSD
# without any warranty
#
#--------------------------------------------------
# little script (for crond)
# to fetch and modify new list from shalla.de
#--------------------------------------------------
#
# *check* paths and squidGuard-owner on your system
# try i.e. "which squid" to find out the path for squid
# try "ps aux | grep squid" to find out the owner for squidGuard
#     *needs wget*
#

httpget="/usr/local/bin/wget"
tarpath="/usr/bin/tar"
chownpath="/usr/sbin/chown"

##########################################

workdir="/home/groups/s/si/sift/htdocs/listupdates"
shallalist="http://www.shallalist.de/Downloads/shallalist.tar.gz"

# download actual shalla's blacklist
# thanks for the " || exit 1 " hint to Rich Wales
#wget $shallalist $workdir/shallalist.tar.gz || exit 1
tar -xzf $workdir/shallalist.tar.gz -C $workdir || exit 1

# remove entry from porn-list: "krone.at"
# (too popular in Austria to be forbidden ;-)
# grep -vx krone.at $workdir/BL/porn/domains > $workdir/domains
# cp $workdir/domains $workdir/BL/porn/

# create new local database
#rm -r $workdir/BL
