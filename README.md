# FDOT TBEST Ridership Estimation Model and Sample Add-In
This repository provides the TBEST 5.0 scripting interface within Microsoft Visual Studio 2022,
allowing users to leverage the TBEST API and data to create unique Add-Ins as well as customize
the TBEST Ridership Estimation Model. Add-Ins can reference additional data sources and include new 
methodologies and logic to solve outside the box problems. 

## Getting Started
### System Requirements
Developing a TBEST Add-In or the TBEST ridership Estimation Model requires a machine running Windows 10 or greater along with the following:
- [TBEST 5.0](https://tbest.org/downloads/?dl_cat=15)
- Licensed copy of [Microsoft Visual Studio 2022](https://visualstudio.microsoft.com/vs/)

Note that deploying an Add-In or ridership Estimation Model for use in TBEST requires an
[Esri Developer Bundle](https://www.esri.com/en-us/arcgis/products/develop-with-arcgis/buy#arcgis_developer_bundle)
supporting [ArcGIS Maps SDK for .NET](https://developers.arcgis.com/net/).

### Preparing the Solution
Download the project files using the GitHub interface or with [git](https://git-scm.com/downloads/win) 
```
git clone https://github.com/TBESTTools/Ridership-Estimation-Model-and-Sample-AddIn.git
```

### Set Esri Developer Credentials
Deployment of TBEST Add-Ins and models as well as the usage of ArcGIS base maps requires Esri developer credentials.
Both the `TBESTSampeAddIn` and `RidershipEstimationModel` projects contain separate initialization files at
`Class/TBESTApplicationFunctions.cs`. In these files, specify Esri credentials by uncommenting and editing
the following lines as necessary.

```cs
//Enter Esri Developer Credentials
string ArcGISOnlineUrl = "";
string AppClientId = "";
string OAuthRedirectUrl = "";
string APIKey = "";
bool ForceEsriLogin = false;

if (await TBESTFramework.EsriLogin.ArcGISLoginPrompt.GetArcGISCredentials(Path.GetTempPath(), APIKey, AppClientId, OAuthRedirectUrl, ArcGISOnlineUrl,ForceEsriLogin) == false)
{
    MessageBox.Show("Unable to access the ArcGIS Portal licensing to run TBEST. Please verify that your organization has access to the ArcGIS Basic, Standard or Advanced license level.", "TBEST", MessageBoxButton.OK, MessageBoxImage.Exclamation);
    Environment.Exit(0);
}
```
Note that `ForceEsriLogin` is `false` for the `RidershipEstimationModel` project and `true` for
the `TBESTSampleAddIn` project.

## Build and Deploy Add-Ins to TBEST
### Running and Debugging an Add-In in Visual Studio
Run the `TBESTSampleAddIn` project in Visual Studio and step through the project as desired.
Esri developer credentials are not required for debugging unless the project makes us of ArcGIS base maps.

### Building an Add-In
Use Visual Studio to build the `TBESTSampleAddIn` project in Release mode. Building the project
places your compiled Add-In in the `SampleAddin/AddInToDeploy` directory.

### Deploying and running the Add-In in TBEST
Install and run the Add-In in TBEST:
1. Close any open instances of TBEST.
2. Copy the `SampleAddin/AddInToDeploy` directory to `C:/TBEST50/Tools/AddIns`.
3. Rename the new directory at `C:/TBEST50/Tools/AddIns/AddInToDeploy` to the desired Add-In name.
For example, if we want to call our Add-In "Demo Add-In", we would rename the directory to
`"C:/TBEST50/Tools/AddIns/Demo Add-In"`
4. Launch TBEST and run the Add-In from the Tools menu in the top navigation bar `Tools > Demo Add-In`.

## Build and Deploy TBEST Ridership Estimation Model
### Running and Debugging an Add-In in Visual Studio
Run the `RidershipEstimationModel` project in Visual Studio and step through the project as desired.
### Building the Model
Use Visual Studio to build the `RidershipEstimationModel`
project in Release mode. Building the project places your compiled model in the
`RidershipEstimationModel/ModelLinearEquation/ModelToDeploy` directory.

### Deploying and Running the Model in TBEST
Add the new model to TBEST:
1. Close any open instances of TBEST.
2. Copy the `RidershipEstimationModel/ModelLinearEquation/ModelToDeploy` directory to `C:/TBEST50/Models`.
3. Rename the newly created directory at `C:/TBEST50/Models/ModelToDeploy` to the desired model name.
For example, if we want to call our Model "Demo Model", we would rename the directory to
`"C:/TBEST50/Tools/AddIns/Demo Model"`
4. Launch TBEST and view the new model by navigating to the *TBEST Explorer* window and expanding the
*Ridership Estimation Models* folder. *Demo Model* should be listed as one of the models in the folder.
For information on configuring and running the model, in TBEST, consult the
[TBEST User Guide](https://tbest.org/wp-content/files/TBESTUserGuide_50.pdf)

## Resources
- [FDOT Public Transit Office](https://www.fdot.gov/fdottransit/transitofficehome)
- [TBEST Website](https://tbest.org)
- [TBEST User Guide](https://tbest.org/wp-content/files/TBESTUserGuide_50.pdf)
- [Esri ArcGIS Maps SDK for .NET](https://developers.arcgis.com/net/)
