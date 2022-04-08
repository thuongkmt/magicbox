

$workingDirectory = get-location

# read settings

$jsonConfigFile = "$workingDirectory\settings.json"

$jsonConfig = Get-Content $jsonConfigFile | ConvertFrom-Json

$localAdminAngular = "$workingDirectory"+"\"+ $jsonConfig.LocalAdminConfig.AngularFolder
$localCustomerUI = "$workingDirectory"+"\"+ $jsonConfig.LocalAdminConfig.CustomerUIFolder 


# build local admin angular
Set-Location -Path $localAdminAngular
npm install
ng build  --prod
#ng serve



#build admin net core api
$localAdminNetCoreFolder = "$workingDirectory"+"/"+ $jsonConfig.LocalAdminConfig.NetCoreApiFolder +"/src/KonbiCloud.Web.Host"
$localAdminNetCoreProjectFile = $localAdminNetCoreFolder + "/KonbiCloud.Web.Host.csproj"
dotnet publish $localAdminNetCoreProjectFile


# copy over publishing files 
$adminAngularPubishFolder = $jsonConfig.publishFolder.root + $jsonConfig.publishFolder.adminAngular
$adminApiPubishFolder = $jsonConfig.publishFolder.root + $jsonConfig.publishFolder.adminApi
$customerUiPubishFolder = $jsonConfig.publishFolder.root + $jsonConfig.publishFolder.customerUI
If(!(test-path $adminAngularPubishFolder))
{
      New-Item -ItemType Directory -Force -Path $adminAngularPubishFolder
}
If(!(test-path $adminApiPubishFolder))
{
      New-Item -ItemType Directory -Force -Path $adminApiPubishFolder
}
If(!(test-path $customerUiPubishFolder))
{
      New-Item -ItemType Directory -Force -Path $customerUiPubishFolder
}


#delete old files
 Get-ChildItem -Path "$adminAngularPubishFolder" -include *  -Recurse | remove-item -recurse
  Get-ChildItem -Path "$adminApiPubishFolder" -include *  -Recurse | remove-item -recurse
   Get-ChildItem -Path "$customerUiPubishFolder" -include *  -Recurse | remove-item -recurse
# copy admin angular
Set-Location -Path "$localAdminAngular\dist"

xcopy *.* "$adminAngularPubishFolder" /c /h /k /r /y /e /s


# copy admin api
Set-Location -Path "$localAdminNetCoreFolder\bin\Debug\netcoreapp2.1\publish"
xcopy *.* $adminApiPubishFolder /c /h /k /r /y /e /s

# copy admin api
Set-Location -Path $localCustomerUI
xcopy *.* $customerUiPubishFolder /c /h /k /r /y /e /s

# copy deployment script to output
xcopy "$workingDirectory\DeployWebApps.ps1" $jsonConfig.publishFolder.root /c /h /k /r /y /e /s

Set-location $workingDirectory