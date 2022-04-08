$adminAngularAppPool = "adminAngularAppPool"
$adminApiAppPool = "adminApiAppPool"
$customerUiAppPool = "customerUiAppPool"

$adminAngularWebApp = "adminAngularWebApp"
$adminApiWebApp = "adminApiWebApp"
$customerUiWebApp = "customerUiWebApp"



$workingDirectory = get-location

# read settings

$jsonConfigFile = "$workingDirectory\settings.json"

$jsonConfig = Get-Content $jsonConfigFile | ConvertFrom-Json

$adminAngularPubishFolder = $jsonConfig.publishFolder.root + $jsonConfig.publishFolder.adminAngular
$adminApiPubishFolder = $jsonConfig.publishFolder.root + $jsonConfig.publishFolder.adminApi
$customerUiPubishFolder = $jsonConfig.publishFolder.root + $jsonConfig.publishFolder.customerUI

import-module webadministration


#intitialize IIS web apps
if( -not(Test-Path IIS:\\AppPools\$adminAngularAppPool)){
    New-WebAppPool $adminAngularAppPool
}else{
    Write-host $adminAngularAppPool " already exists. ignored"
}

if( -not(Test-Path IIS:\\AppPools\$adminApiAppPool)){
    New-WebAppPool $adminApiAppPool
}
else{
    Write-host $adminApiAppPool " already exists. ignored"
}

if( -not(Test-Path IIS:\\AppPools\$customerUiAppPool)){
    New-WebAppPool $customerUiAppPool
}
else{
    Write-host $customerUiAppPool " already exists. ignored"
}

#creating Web apps if not exists

if( -not(Test-Path IIS:\\Sites\$adminAngularWebApp)){
    New-WebSite -Name  $adminAngularWebApp -Port 4200 -PhysicalPath $adminAngularPubishFolder
}
else{
    Write-host $adminAngularWebApp " already exists. ignored"
}

if( -not(Test-Path IIS:\\Sites\$adminApiWebApp)){
   New-WebSite -Name  $adminApiWebApp -Port 22742 -PhysicalPath  $adminApiPubishFolder
}
else{
    Write-host $adminApiWebApp " already exists. ignored"
}

if( -not(Test-Path IIS:\\Sites\$customerUiWebApp)){
    New-WebSite -Name  $customerUiWebApp -Port 58999 -PhysicalPath $customerUiPubishFolder
}
else{
    Write-host $customerUiWebApp " already exists. ignored"
}

