#!/bin/sh
set -e

OPENSSL_SECLEVEL=1
for f in /etc/ssl/openssl.cnf /usr/lib/ssl/openssl.cnf; do
  if [ -f "$f" ]; then
    sed -i "s/DEFAULT@SECLEVEL=[0-9]/DEFAULT@SECLEVEL=${OPENSSL_SECLEVEL}/g" "$f"
  fi
done

OPENSSL_MIN_PROTOCOL=TLSv1
for f in /etc/ssl/openssl.cnf /usr/lib/ssl/openssl.cnf; do
  if [ -f "$f" ]; then
    sed -i "s/MinProtocol = TLSv1.2/MinProtocol = ${OPENSSL_MIN_PROTOCOL}/g" "$f"
  fi
done

exec dotnet Masa.Scheduler.Services.Worker.dll
