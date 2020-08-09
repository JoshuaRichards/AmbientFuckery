#Requires -RunAsAdministrator

openssl genrsa -out root.key 4096
openssl req -new -x509 -key root.key -out root.crt -subj "/CN=cephissus-root"

$openSslDir = "$(openssl version -d)" -replace '^OPENSSLDIR: "|"$', ''
Copy-Item "$openSslDir/openssl.cnf" ./ssl.conf
$computerName = $env:COMPUTERNAME
$localDns = $computerName.ToLower() + ".local"
"[SAN]`nsubjectAltName=DNS.1:localhost,DNS.2:$computerName,DNS.3:$localDns,IP:127.0.0.1`n" |
    Out-File -Encoding utf8 -Append -NoNewline ssl.conf

openssl genrsa -out cephissus.key 4096
openssl req -subj "/CN=cephissus" -new -key cephissus.key -out cephissus.csr -extensions SAN -config ssl.conf -days 90

openssl x509 -req -in cephissus.csr -CA root.crt -CAkey root.key -CAcreateserial `
    -out cephissus.crt -extensions SAN -extfile ssl.conf

$password = "$(openssl rand -base64 64)"
$securePassword = ConvertTo-SecureString -String $password -Force -AsPlainText
openssl pkcs12 -export -out cephissus.pfx -inkey cephissus.key -in cephissus.crt -password "pass:$password"

function shred([string] $file) {
    openssl rand 8192 | out-file $file
    rm $file
}

shred root.key
shred cephissus.csr
shred ssl.conf

[Environment]::SetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Password", $password, "User")
[Environment]::SetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path", "$(pwd)/cephissus.pfx", "User")

Import-Certificate -FilePath root.crt -CertStoreLocation Cert:\LocalMachine\Root