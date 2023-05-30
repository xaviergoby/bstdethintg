#!/bin/bash
# Simple script to backup the complete environment
#
# Add the script in a cron job to execute daily
# > sudo crontab -e
# 5 0 * * *  /docker/hodl-td/backup.sh
#
# Restore from root path of the data dir (cd /docker/hodl-td):
# tar -xvpzf /home/admin/backups/2021_02_04/hodl-td_backup.tgz
#

# Database directory
data_dir="/docker/hodl-td"
# Backup directory
backup_dir="/home/admin/backups"
# data_path is path to backup
data_path="*"

app_name=$(basename "$data_dir")
# Removal of old backups
THRESHOLD=$(date -d "22 days ago" +%Y_%m_%d)

## Find all folders in $backup_dir. The -type d means only folders
## and the -maxdepth 1 ensures that any files in subdirectories are
## not included. Combined with -print0 (separate file names with \0),
## IFS= (don't break on whitespace), "-d ''" (records end on '\0') , it can
## deal with all file names.
find ${backup_dir} -maxdepth 1 -type d -print0  | while IFS= read -d '' -r folder
do
    ## Delete the folder if it's older than the $THRESHOLD
    fldr="$(basename "$folder")"
    [ "${fldr//_/}" -le "${THRESHOLD//_/}" ] && rm -r "$folder"
done

# Create backup directory when not existing and set permissions
# Create backup directory and set permissions
backup_date=`date +%Y_%m_%d`
backup_dest="${backup_dir}/${backup_date}"
echo "Backup directory: ${backup_dest}"
mkdir -p "${backup_dest}"
chmod 755 "${backup_dest}"

## Now backup complete folder contents
tmp="${backup_dest}/${app_name}_backup.tmp.tgz"
dst="${backup_dest}/${app_name}_backup.tgz"
# Save current dir
cwd=$(pwd)
cd "${data_dir}"

echo "Dump ${app_name} data"
tar -cvpzf "${tmp}" ${data_path}
cd "${pwd}"

[ -f "${dst}" ] && rm -f "${dst}"
mv "${tmp}" "${dst}"
chmod 644 "${dst}"

echo "Done."
