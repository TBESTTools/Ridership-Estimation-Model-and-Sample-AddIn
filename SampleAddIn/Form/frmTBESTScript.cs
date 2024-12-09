using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using TBESTFramework.App;
using TBESTFramework.Data.Network;
using TBESTFramework.Management;
using TBESTFramework.Utilities;
using static TBESTScripting.TBESTApplicationFunctions;
using static TBESTFramework.Utilities.ErrorHandling;
using TBESTFramework.Forms;
namespace TBESTScripting
{

	public partial class frmTBESTScript
	{
		public frmTBESTScript()
		{
			InitializeComponent();
		}
		private void frmTBESTScript_Load(object sender, EventArgs e)
		{
			cboScenarios.DisplayMember = "Description";
			cboScenarios.ValueMember = "Value";
			cboSystems.DisplayMember = "Description";
			cboSystems.ValueMember = "Value";
			InitiateSystems();
		}
		public bool InitiateSystems()
		{
			bool InitiateSystemsRet;

			try
			{

				for (int i = 1, loopTo = mTBESTApplication.TransitSystems.List.Count - 1; i <= loopTo; i++)
					cboSystems.Items.Add(new ValueDescriptionPair(mTBESTApplication.TransitSystems.List[i].TransitSystemID, mTBESTApplication.TransitSystems.List[i].TransitSystemName));
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

				InitiateSystemsRet = true;
			}
			catch (Exception ex)
			{
				ErrHandlerMsg(ex);
				MessageBox.Show("Error loading Systems. To view error details, navigate to the TBEST log file at File->Settings->TBEST Log.","TBEST",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
				InitiateSystemsRet = false;

			}

			return InitiateSystemsRet;

		}

		private async void cboSystems_SelectedIndexChanged(object sender, EventArgs e)
		{
			int pSystemID = Convert.ToInt32(((ValueDescriptionPair)cboSystems.SelectedItem).Value);
			cboScenarios.Items.Clear();
			cboScenarios.Text = "- Select One -";
			cboScenarios.Enabled = true;
		
			mTransitSystem = mTBESTApplication.TransitSystems.ReturnSystem(pSystemID);
		
			if (mScenario is not null && mScenario.TransitSystem.TransitSystemID != mTransitSystem.TransitSystemID) { await mScenario.Close(); mScenario = null;   }
			if (mTransitSystem.Scenarios is null || mTransitSystem.Scenarios.List.Count == 0)
			{
				mTransitSystem.Scenarios = new TBESTScenarios(mTransitSystem);
				await mTransitSystem.Scenarios.Load();
			}
			TBESTScenario pScenario;
			for (int i = 1, loopTo = mTransitSystem.Scenarios.List.Count - 1; i <= loopTo; i++)
			{
				pScenario = mTransitSystem.Scenarios.List[i];
				if (!pScenario.IsBaseYear)
					cboScenarios.Items.Add(new ValueDescriptionPair(pScenario.ScenarioID, pScenario.ScenarioName));
			}
			if (cboScenarios.Items.Count == 1)
			{
				cboScenarios.Text = ((ValueDescriptionPair)cboScenarios.Items[0]).Description.ToString();
			}
			else if (mScenario is not null)
			{
				cboScenarios.Text = mScenario.ScenarioName;
			}
			else if (mTransitSystem.Scenarios.List.Count - 1 > 1)
			{
				cboScenarios.Text = "- Select One -";
			}
			else
			{
				cboScenarios.Text = "No Scenarios Available";
			}
			cboScenarios.Enabled = true;
			cboAction.Enabled = true;

		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void cboScenarios_SelectedIndexChanged(object sender, EventArgs e)
		{
			int pScenarioID = Convert.ToInt32(((ValueDescriptionPair)cboScenarios.SelectedItem).Value);
			if (mScenario is not null && mScenario.ScenarioID > 10000)
			{
				mScenario.Close();
				mScenario = null;

			}
			DataGridViewRoutes.Rows.Clear();

			mScenario = mTransitSystem.Scenarios.ReturnScenario(pScenarioID);

			cboAction.Enabled = true;

		}

		private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
		{
			foreach (DataGridViewRow dr in DataGridViewRoutes.Rows)
				dr.Cells[0].Value = chkSelectAll.Checked;
		}

		private async Task LoadRoutes()
		{
			if (mTransitSystem is null | mScenario is null)
			{
				MessageBox.Show("Please select a Transit System and Scenario to load routes", "TBEST Scripting", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}


			frmMiniCancel pfrmMiniCancel = new frmMiniCancel();
			try
			{
				pfrmMiniCancel.btnCancel.Visible = false;
				pfrmMiniCancel.prgImport.Style = ProgressBarStyle.Marquee;
				pfrmMiniCancel.TopMost = true;
				pfrmMiniCancel.lblProgressDisplay.Text = "Opening " + mScenario.ScenarioName;

				pfrmMiniCancel.prgImport.Value = 0;
				pfrmMiniCancel.Show();
				pfrmMiniCancel.Refresh();
				pfrmMiniCancel.Update();
				if (await mScenario.Open(true))
				{
					TBESTScenario.LoadTypes argiEnumValues = (TBESTScenario.LoadTypes)((int)TBESTScenario.LoadTypes.Routes + (int)TBESTScenario.LoadTypes.TimePeriods);

					if (await mScenario.LoadTransitNetwork(argiEnumValues, [SystemTimePeriod.AMPeak, SystemTimePeriod.OffPeak, SystemTimePeriod.PMPeak, SystemTimePeriod.Night, SystemTimePeriod.Saturday, SystemTimePeriod.Sunday], null, null))
					{
						foreach (TransitRoute pRouteGroup in mScenario.Routes.List)
						{
							DataGridViewRoutes.Rows.Add(chkSelectAll.Checked, pRouteGroup.RouteName + " - " + pRouteGroup.RouteDescription);
							DataGridViewRoutes.Rows[DataGridViewRoutes.Rows.Count - 1].Tag = pRouteGroup.RouteID;
						}
					}
					await mScenario.Close();
					mScenario = null;
				}
			}
			catch (Exception)
			{

			}
			finally 
			{
				if (FormFunctions.IsFormLoaded("frmMiniCancel")) pfrmMiniCancel.Close();
				mScenario = null; 
			}
			
			
		}

		private async void btnGo_Click(object sender, EventArgs e)
		{

			switch (cboAction.Text ?? "")
			{
				case "Load Routes":
					{
						await LoadRoutes();
						break;
					}

			}
		}
	}
}