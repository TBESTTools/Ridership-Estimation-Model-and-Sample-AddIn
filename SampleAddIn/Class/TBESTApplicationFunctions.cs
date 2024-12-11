using System;
using TBESTFramework.Management;
using TBESTFramework.Utilities;
using TBESTFramework.RidershipForecasting.ModelEquationSettings;
using System.Threading.Tasks;
using TBESTFramework.App;
using System.IO;
using System.Windows;
using System.Collections.Generic;
using System.Linq;

namespace TBESTScripting
{

	static class TBESTApplicationFunctions
	{
		public static TransitSystem mTransitSystem = null;
		public static TBESTScenario mScenario = null;
		public static TBESTApplication mTBESTApplication = new();
		public static ErrorHandling ErrorHandling = new();
		public static Models mModels;

		public static int mTransitSystemID = 0;
		public static int mScenarioID = 0;


		public static async Task InitializeApp(string[] strArgs)
		{

			try
			{
				Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.Initialize();

				if (mTBESTApplication.RegistryInstallation == TBESTApplication.RegistryInstall.None)
				{
					MessageBox.Show("Unable to access local TBEST Registry keys.", "TBEST Scripting",MessageBoxButton.OK );
					Environment.Exit(0);
				}

				Environment.SetEnvironmentVariable("Path", "");

				if (mTBESTApplication.Load() == false)
					Environment.Exit(0);
				
				if (null == Application.Current)
				{
					new Application();
					Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
				}
				System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Ssl3;

				//Enter Esri Developer Credentials
				//string ArcGISOnlineUrl = "";
				//string AppClientId = "";
				//string OAuthRedirectUrl = "";
				//string APIKey = "";
				//bool ForceEsriLogin = true;

				//if (await TBESTFramework.EsriLogin.ArcGISLoginPrompt.GetArcGISCredentials(Path.GetTempPath(), APIKey, AppClientId, OAuthRedirectUrl, ArcGISOnlineUrl, ForceEsriLogin) == false)
				//{
				//	MessageBox.Show("Unable to access the ArcGIS Portal licensing to run TBEST. Please verify that your organization has access to the ArcGIS Basic, Standard or Advanced license level.", "TBEST", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				//	Environment.Exit(0);
				//}
				mTBESTApplication.Workspaces = new TBESTWorkspaces(mTBESTApplication);
				if ((await mTBESTApplication.Workspaces.Load()) == false)
				{
					Environment.Exit(0);
				}

				List<TBESTWorkspace> nWorkspace = mTBESTApplication.Workspaces.List.Where(n => n.IsActive == true).ToList();

				if (nWorkspace.Count != 1)
				{
					MessageBox.Show("Unable to load TBEST Workspaces. Failed to initialize TBEST.", "TBEST Scripting", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
				else
				{
					mTBESTApplication.ActiveWorkspace = nWorkspace[0];
				}
				mTBESTApplication.TransitSystems = new TransitSystems(mTBESTApplication);
				await mTBESTApplication.LoadDownloadSettings();

				if (await mTBESTApplication.TransitSystems.Load(true) == false)
					Environment.Exit(0);


				mModels = new Models(mTBESTApplication.BaseLayersPath + @"Models\", mTBESTApplication.LocalPath + @"\Models\");
				GdalConfiguration.ConfigureGdal();
				GdalConfiguration.ConfigureOgr();
			}

			catch (Exception ex)
			{
				MessageBox.Show("Unable to initialize TBEST Scripting. Please verify that an ArcGIS Pro license is available.", "TBEST Scripting",MessageBoxButton.OK,MessageBoxImage.Exclamation);
				ErrorHandling.ErrHandlerMsg(ex);
				Environment.Exit(0);
			}
		}
	}
}