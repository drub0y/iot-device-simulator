#
# for quick publishing of application type during development
#

$endpoint = 'localhost:19000'
$applicaitonName = 'fabric:/DeviceSimulation/Devices'
$applicaitonType = 'DeviceSimulationType'
$applicationVersion = '1.0.0'
$packagepath = '..\DeviceSimulation\pkg\Debug'
$imagestorename = 'DeviceSimulation'

Connect-ServiceFabricCluster -ConnectionEndpoint $endpoint

$appType = Get-ServiceFabricApplicationType $applicaitonType

if ($appType){
    $app = Get-ServiceFabricApplication -ApplicationName $applicaitonName
    if ($app){
        Remove-ServiceFabricApplication -ApplicationName $applicaitonName -ErrorAction SilentlyContinue
    }

    Unregister-ServiceFabricApplicationType -ApplicationTypeName $applicaitonType `
										-ApplicationTypeVersion $applicationVersion `
                                        -ErrorAction SilentlyContinue
}

# Copy the application package to the cluster image store.
Copy-ServiceFabricApplicationPackage -ApplicationPackagePath $packagepath `
                                     -ImageStoreConnectionString 'file:C:\SfDevCluster\Data\ImageStoreShare' `
									 -ApplicationPackagePathInImageStore $imagestorename

# Register the application type.
Register-ServiceFabricApplicationType -ApplicationPathInImageStore $imagestorename

# Remove the application package to free system resources.
Remove-ServiceFabricApplicationPackage -ImageStoreConnectionString file:C:\SfDevCluster\Data\ImageStoreShare `
									   -ApplicationPackagePathInImageStore $imagestorename

# don't need to create applicaiton instance
#New-ServiceFabricApplication -ApplicationName $applicaitonName `
#							 -ApplicationTypeName $applicaitonType `
#							 -ApplicationTypeVersion $applicationVersion