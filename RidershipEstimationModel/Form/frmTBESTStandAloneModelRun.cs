using System;
using System.Data;
using System.Windows.Forms;
using TBESTFramework.RidershipForecasting.ModelEquationSettings;
using TBESTFramework.App.SEData;
using TBESTFramework.Utilities;
using static TBESTFramework.RidershipForecasting.Validation.ModelValidation;
using static TBESTFramework.Utilities.SQLiteFunctions;
using static TBESTFramework.Utilities.FileFunctions;
using static TBESTFramework.Reporting.ReportFunctions;
using TBESTFramework.App;
using TBESTFramework.Management;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using TBESTRidershipEstimationModel;
using static RidershipEstimationModel.TBESTApplicationFunctions;
using TBESTModelEquation.TBESTRidershipForecasting;
using System.Reflection;

namespace RidershipEstimationModel
{

	public partial class frmTBESTStandAloneModelRun
	{
	
		private ModelParameters mModelParameters = new ModelParameters();
		private short[] lstScenarioIDs = null;
		public frmTBESTStandAloneModelRun()
		{
			InitializeComponent();
		}

		private void frmTBESTStandAloneModelRun_FormClosed(object sender, FormClosedEventArgs e)
		{
			Environment.Exit(0);
		}
		private void frmTBESTScript_Load(object sender, EventArgs e)
		{
			
			Icon = Resources.modelimage_M82_icon;
			cboSystems.ValueMember = "Value";
			cboSystems.DisplayMember = "Description";
			lstScenarios.ValueMember = "Value";
			lstScenarios.DisplayMember = "Description";
			TopLevel = true;

			lstTimePeriods.Items.Add("AM Peak");
			lstTimePeriods.Items.Add("Off-Peak");
			lstTimePeriods.Items.Add("PM Peak");
			lstTimePeriods.Items.Add("Night");
			lstTimePeriods.Items.Add("Saturday");
			lstTimePeriods.Items.Add("Sunday");
			
			if (InitiateSystems() == false)
				Environment.Exit(0);
		}
		public bool InitiateSystems()
		{
			bool InitiateSystemsRet;

			try
			{

				var loopTo = mTBESTApplication.TransitSystems.List.Count - 1;
				for (int i = 0; i <= loopTo; i++)
				{
					if (mTransitSystemID != 0 & mTBESTApplication.TransitSystems.List[i].TransitSystemID != mTransitSystemID)
						continue;
					cboSystems.Items.Add(new ValueDescriptionPair(mTBESTApplication.TransitSystems.List[i].TransitSystemID, mTBESTApplication.TransitSystems.List[i].TransitSystemName));
				}
				if (mTBESTApplication.TransitSystems.List.Count == 1)
				{
					cboSystems.Text = mTBESTApplication.TransitSystems.List[0].TransitSystemName;
				}
				else if (mTransitSystem is not null)
				{
					cboSystems.Text = mTransitSystem.TransitSystemName;
				}
				else if (mTBESTApplication.TransitSystems.List.Count > 1)
				{
					cboSystems.Text = "- Select One -";
				}
				else
				{
					cboSystems.Text = "No Transit Systems Available";
				}
				if (mScenario is not null)
				{
					for (int a = 0, loopTo1 = lstScenarios.Items.Count - 1; a <= loopTo1; a++)
					{
						if ((lstScenarios.Items[a].ToString() ?? "") == (mScenario.ScenarioName ?? ""))
						{
							lstScenarios.SetItemChecked(a, true);
							lstScenarios.SelectedIndex = a;
							break;
						}
					}
				}
				InitiateSystemsRet = true;
			}
			catch (Exception )
			{
				MessageBox.Show("Error loading Systems.  To view error details, navigate to the TBEST log file at File->Settings->TBEST Log.", "TBEST Ridership Estimation Model Run", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				InitiateSystemsRet = false;

			}

			return InitiateSystemsRet;

		}

		private async void cboSystems_SelectedIndexChanged(object sender, EventArgs e)
		{
			int pSystemID = Convert.ToInt32(((ValueDescriptionPair)cboSystems.SelectedItem).Value);
			lstScenarios.Items.Clear();


			mTransitSystem = mTBESTApplication.TransitSystems.ReturnSystem(pSystemID);

			if (mTransitSystem.Scenarios is null || mTransitSystem.Scenarios.List.Count == 0)
			{
				mTransitSystem.Scenarios = new TBESTScenarios(mTransitSystem);
				await mTransitSystem.Scenarios.Load();
			}
			TBESTScenario pScenario;
			for (int i = 0, loopTo = mTransitSystem.Scenarios.List.Count - 1; i <= loopTo; i++)
			{
				pScenario = mTransitSystem.Scenarios.List[i];
				if (mScenarioID != 0 & pScenario.ScenarioID != mScenarioID)
					continue;

				if (!pScenario.IsBaseYear & !pScenario.IsBaseActiveScenario)
				{
					lstScenarios.Items.Add(new ValueDescriptionPair(pScenario.ScenarioID, pScenario.ScenarioName));
				}

			}
			mModelParameters = new ModelParameters();

			bool boolIsModValidated = await mTransitSystem.IsModelValidated();
			mModelParameters.SyncApplicationScenarioRouteswithBaseYearCollections = boolIsModValidated;

			if (lstScenarios.Items.Count == 1)
			{
				lstScenarios.SetItemChecked(0, true);
				await SetScenarioProperties();
			}
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			Close();
		}


		private async Task<bool> ExecuteModel()
		{
			bool boolOutValue = false;

			try
			{
				if (mTransitSystem is null)
				{
					MessageBox.Show("Please select a Transit System and Scenario to run the ridership estimation model.", "TBEST Ridership Estimation Model Run",MessageBoxButtons.OK,  MessageBoxIcon.Information );
					return boolOutValue;
				}

				if (lstTimePeriods.CheckedIndices.Count == 0)
				{
					MessageBox.Show("Please select the Time Period(s) to run.", "TBEST Ridership Estimation Model Run", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return boolOutValue;
				}

				if (lstScenarioIDs is null)
				{
					MessageBox.Show("Please select the scenarios to run.", "TBEST Ridership Estimation Model Run", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return boolOutValue;
				}

				if (await mTransitSystem.IsModelValidated() == false & mModelParameters.BRTSettings == ModelParameters.ValidatedBRTSettings.ApplyBRTValidationFactor)
				{
					MessageBox.Show("To apply model validation factors to new BRT routes, the system must first be validated.", "TBEST Ridership Estimation Model Run", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return boolOutValue;
				}

				if (MessageBox.Show("Are you sure you want to initiate a model run?", "TBEST Ridership Estimation Model Run", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
				{
					return boolOutValue;
				}

				List<SystemTimePeriod> iRunPeriods = new List<SystemTimePeriod>();
				for (int a = 0, loopTo = lstTimePeriods.Items.Count - 1; a <= loopTo; a++)
				{
					if (lstTimePeriods.GetItemChecked(a))
					{
						iRunPeriods.Add((SystemTimePeriod)a);
					}
				}
				mModelParameters.TimePeriods = iRunPeriods;

				TBESTScenario pScenario = null;
				bool boolSuccess = false;

				for (int a = 0, loopTo1 = lstScenarioIDs.GetLength(0) - 1; a <= loopTo1; a++)
				{
					boolSuccess = false;
					pScenario = mTransitSystem.Scenarios.ReturnScenario(lstScenarioIDs[a]).CloneItem();

					if (string.IsNullOrEmpty(pScenario.ModelName))
					{
						MessageBox.Show("The " + pScenario.ScenarioName + " TBEST scenario does not have a model selected. Please use the Scenario Properties to select a model and then re-execute the model run.", "TBEST Ridership Estimation Model Run", MessageBoxButtons.OK, MessageBoxIcon.Information);
						break;
					}

					StreamWriter oWrite;
					try
					{
						oWrite = OpenStream(pScenario.ScenarioPath + @"\Log\DirectBoardings.csv", true);
						oWrite.Close();
					}
					catch (Exception )
					{
						MessageBox.Show("The TBEST Direct Boardings Log file is currently open. Please close the file before continuing with the model run.", "TBEST Ridership Estimation Model Run", MessageBoxButtons.OK, MessageBoxIcon.Information);
						break;
					}

					try
					{
						oWrite = OpenStream(pScenario.ScenarioPath + @"\Log\TransferBoardings.csv", true);
						oWrite.Close();
					}
					catch (Exception )
					{
						MessageBox.Show("The TBEST Transfer Boardings Log file is currently open. Please close the file before continuing with the model run.", "TBEST Ridership Estimation Model Run", MessageBoxButtons.OK, MessageBoxIcon.Information);
						break;
					}

					if (!await SocioEconomicGrowthFunctions.AreGrowthRateRecordsUptoDate(pScenario))
					{
						break;
					}
					ModelSettings pModelSettings = null;


					string exeLocation = Assembly.GetExecutingAssembly().Location;

					string modelLocation = exeLocation.Substring(0,exeLocation.IndexOf("bin")) + @"\ModelLinearEquation\";

					if (pScenario.TransitSystem.ValidationYear != "0")
					{
						pModelSettings = new ModelSettings("ModelToDeploy", modelLocation, mTBESTApplication.LocalPath + @"\Models\");
						if (mModelParameters.SyncApplicationScenarioRouteswithBaseYearCollections)
						{
							await SyncApplicationScenarioRouteswithBaseYearCollections(pScenario);
						}
					}
					else
					{
						pModelSettings = new ModelSettings("ModelToDeploy", modelLocation, mTBESTApplication.LocalPath + @"\Models\");
					}

					pScenario.TransitSystem.CloseConnection();

					if (pModelSettings is null)
					{
						MessageBox.Show("TBEST could not process the model selected for this scenario. The '" + pScenario.ModelName + "' model files are not present on this machine. Please reference an existing model structure to this scenario and try again.", "TBEST Ridership Estimation Model Run", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
					else
					{
						WindowState = FormWindowState.Normal;
						EnableUI(false);
				        boolSuccess = (bool)await InitiateModel.LoadModel(pScenario, mModelParameters, pModelSettings,"Processing Model " + (a + 1).ToString() + " of " + lstScenarioIDs.GetLength(0).ToString());
					}
					GC.Collect();
					if (boolSuccess)
					{
						if (mModelParameters.AssignAllSERecords)
						{
							pScenario.CreateScenarioLog();
							await mModelParameters.LoadEditableModelParameters(await pModelSettings.GetSQLDBConnection());
							if (await BuildRouteGroupPopulationandEmployment(pScenario, mModelParameters.MarketCaptureDistance) == false)
							{
								MessageBox.Show("The TBEST Model Route Socio-Economic Summary post process did not complete. The Ridership Forecating Model Run estimations are complete but the Route Socio-Economic Summary was either canceled or failed in processing. This means that Route-Level Socio-Econoomic summaries will not be available in the TBEST Query Tool and the Scenario Summary Tool.", "TBEST Ridership Estimation Model Run", MessageBoxButtons.OK, MessageBoxIcon.Information);
								boolSuccess = false;
							}
						}
					}
					await pScenario.Close();
					pModelSettings.CloseConnection();
					if (boolSuccess == false)
					{
						MessageBox.Show("Ridership Forecating Model Run canceled.", "TBEST Ridership Estimation Model Run", MessageBoxButtons.OK, MessageBoxIcon.Information);
						return boolOutValue;
					}

				}
				WindowState = FormWindowState.Normal;
				EnableUI(true);
				if (boolSuccess)
				{
					MessageBox.Show("Ridership Forecating Model Run Complete!", "TBEST Ridership Estimation Model Run", MessageBoxButtons.OK, MessageBoxIcon.Information);
					if (MessageBox.Show("Would you like to close the Model Run dialog now?", "TBEST Ridership Estimation Model Run",MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						Close();
					}
				}
				boolOutValue = true;
			}
			catch (Exception )
			{
			}
			finally
			{
				WindowState = FormWindowState.Normal;
				EnableUI(true);
			}

			return boolOutValue;
		}

		private void EnableUI(bool boolEnable)
		{
			btnClose.Enabled = boolEnable;
			btnGo.Enabled = boolEnable;
			Frame1.Enabled = boolEnable;
			GroupBox1.Enabled = boolEnable;
			pgModelOptions.Enabled = boolEnable;

		}


		private async void btnGo_Click(object sender, EventArgs e)
		{
			await ExecuteModel();
		}

		private static void SetLabelColumnWidth(Azuria.Common.Controls.FilteredPropertyGrid grid, int width)
		{
			var propertyGridView = grid.Controls[2];
			var mi = propertyGridView.GetType().GetMethod("MoveSplitterTo", BindingFlags.Instance | BindingFlags.NonPublic);
			if (mi is null)
			{
				return;
			}
			mi.Invoke(propertyGridView, [width]);

		}

		private async void lstScenarios_SelectedIndexChanged(object sender, EventArgs e)
		{
			await SetScenarioProperties();

		}
		private async Task SetScenarioProperties()
		{
			string strScenIDList = "";
			int i = 0;
			lstScenarioIDs = null;

			for (int a = 0, loopTo = lstScenarios.Items.Count - 1; a <= loopTo; a++)
			{
				if (lstScenarios.GetItemChecked(a))
				{
					Array.Resize(ref lstScenarioIDs, i + 1);
					lstScenarioIDs[i] = Convert.ToInt16(((ValueDescriptionPair)lstScenarios.Items[a]).Value);
					strScenIDList += lstScenarioIDs[i].ToString() + ",";
					i++;
				}
			}
			if (!(strScenIDList.Length == 0))
			{
				strScenIDList = strScenIDList.TrimEnd(',');
			}

			mModelParameters.AssignAllSERecords = true;
			Attribute pFilter = new System.ComponentModel.CategoryAttribute("Model Run Options");
			pgModelOptions.BrowsableAttributes = new System.ComponentModel.AttributeCollection(pFilter);

			SetLabelColumnWidth(pgModelOptions, 300);

			pgModelOptions.PropertySort = PropertySort.Categorized;
			pgModelOptions.SelectedObject = mModelParameters;

			bool boolValidRun;
			int iTimePeriod;

			var loopTo1 = lstTimePeriods.Items.Count - 1;
			for (i = 0; i <= loopTo1; i++)
				lstTimePeriods.SetItemChecked(i, false);

			DataTable rs = await GetDataTable("Select DISTINCT TIMEPERIOD, VALIDRUN FROM VALIDRUNS WHERE SCEN_ID IN (" + strScenIDList + ") ORDER BY timeperiod",await mTransitSystem.GetSQLDBConnection());
			foreach (DataRow row in rs.Rows)
			{
				iTimePeriod = Convert.ToInt32(row["TimePeriod"]);
				boolValidRun = Convert.ToBoolean(row["VALIDRUN"]);
				if (boolValidRun == false & iTimePeriod < 7 & iTimePeriod > 0)
					lstTimePeriods.SetItemChecked(iTimePeriod - 1, true);
			}
			rs.Clear();
			rs.Dispose();

		}
	}
}