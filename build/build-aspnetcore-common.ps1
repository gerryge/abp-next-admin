# COMMON PATHS 

$rootFolder = (Get-Item -Path "./" -Verbose).FullName

# List of solutions used only in development mode
[PsObject[]]$serviceArray = @()

$serviceArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/services/LY.MicroService.BackendAdmin.HttpApi.Host/"; Service = "admin" }
$serviceArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/services/LY.MicroService.AuthServer/"; Service = "authserver" }
$serviceArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/services/LY.MicroService.AuthServer.HttpApi.Host/"; Service = "authserver-api" }
$serviceArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/services/LY.MicroService.identityServer/"; Service = "identityserver" }
$serviceArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/services/LY.MicroService.identityServer.HttpApi.Host/"; Service = "identityserver4-admin" }
$serviceArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/services/LY.MicroService.LocalizationManagement.HttpApi.Host/"; Service = "localization" }
$serviceArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/services/LY.MicroService.PlatformManagement.HttpApi.Host/"; Service = "platform" }
$serviceArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/services/LY.MicroService.RealtimeMessage.HttpApi.Host/"; Service = "messages" }
$serviceArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/services/LY.MicroService.TaskManagement.HttpApi.Host/"; Service = "task-management" }
$serviceArray += [PsObject]@{ Path = $rootFolder + "/../gateways/internal/LINGYUN.MicroService.Internal.ApiGateway/src/LINGYUN.MicroService.Internal.ApiGateway/"; Service = "internal-apigateway" }

[PsObject[]]$solutionArray = @()
$solutionArray += [PsObject]@{ File = $rootFolder + "/../aspnet-core/LINGYUN.MicroService.All.sln" }
$solutionArray += [PsObject]@{ File = $rootFolder + "/../aspnet-core/LINGYUN.MicroService.Common.sln" }
$solutionArray += [PsObject]@{ File = $rootFolder + "/../aspnet-core/LINGYUN.MicroService.TaskManagement.sln" }
$solutionArray += [PsObject]@{ File = $rootFolder + "/../aspnet-core/LINGYUN.MicroService.WebhooksManagement.sln" }
$solutionArray += [PsObject]@{ File = $rootFolder + "/../aspnet-core/LINGYUN.MicroService.Workflow.sln" }

Write-host ""
Write-host ":::::::::::::: !!! You are in development mode !!! ::::::::::::::" -ForegroundColor red -BackgroundColor  yellow
Write-host "" 