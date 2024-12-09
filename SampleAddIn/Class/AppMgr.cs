using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;
using System.Reflection;

namespace TBESTScripting
{

	class AppMgr
	{

		[STAThread()]
		public static void Main(string[] strArgs)
		{
			AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
		
			try
			{
				 
				AppMgr p = new AppMgr();
				p.ExitRequested += p_ExitRequested;
				Task programStart = p.StartAsync(strArgs);
				HandleExceptions(programStart);

				Application.Run();
				OSGeo.GDAL.Gdal.GDALDestroyDriverManager();
			}

			catch (Exception ex)
			{
				MessageBox.Show("TBEST Scripting has stopped working. The internal error message is:" + ex.Message,"TBEST Scripting",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
			}
			finally
			{
				Environment.Exit(0);
			}
		}

		private readonly frmTBESTScript m_mainForm;
		private AppMgr()
		{
			m_mainForm = new frmTBESTScript();
			m_mainForm.FormClosed += m_mainForm_FormClosed;
		}
		public event EventHandler<EventArgs> ExitRequested;
		void m_mainForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			OnExitRequested(EventArgs.Empty);
		}
		protected virtual void OnExitRequested(EventArgs e)
		{
			if (ExitRequested != null)
				ExitRequested(this, e);
		}
		public async Task StartAsync(string[] strArgs)
		{
			await TBESTApplicationFunctions.InitializeApp(strArgs);
			m_mainForm.Show();
		}
		static void p_ExitRequested(object sender, EventArgs e)
		{
			Application.ExitThread();
		}
		private static async void HandleExceptions(Task task)
		{
			try
			{
				await Task.Yield();
				await task;
			}
			catch (Exception)
			{

				Application.Exit();
			}
		}
		private static string GetTBESTInstallLocation()
		{
			try
			{
				string stringbuffer = ""; // receives data read from the registry"
										  // Set the name of the new key and the default security settings
										  // Create or open the registry key
				RegistryKey pRegSubKey;
				RegistryKey pRegKey;

				pRegKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

				pRegSubKey = pRegKey.OpenSubKey(@"SOFTWARE\TBEST\5.0");
				if (pRegSubKey == null)
				{
					pRegSubKey = pRegKey.OpenSubKey(@"SOFTWARE\WOW6432Node\TBEST\5.0");
					if (pRegSubKey == null)
					{

						return "";
					}
				}
				if (pRegSubKey.GetValue("InstallPath") != null)
					stringbuffer = pRegSubKey.GetValue("InstallPath").ToString();
				pRegKey.Close();
				pRegSubKey.Close();
				return stringbuffer;
			}
			catch (Exception)
			{
				return "";
			}

		}
		private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			// Path to the common folder containing ArcGIS Runtime DLLs
			string commonFolder = Path.Combine(GetTBESTInstallLocation(), "bin");

			// Get the name of the assembly that needs to be resolved
			string assemblyName = new AssemblyName(args.Name).Name;

			// Construct the full path to the assembly in the common folder
			string assemblyPath = Path.Combine(commonFolder, assemblyName + ".dll");
			Console.WriteLine(assemblyPath);
			// Check if the assembly file exists and load it
			if (File.Exists(assemblyPath))
			{
				return Assembly.LoadFrom(assemblyPath);
			}

			// Return null if the assembly could not be resolved
			return null;
		}
	}
}