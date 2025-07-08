using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;
using System.Threading.Tasks;
using TBESTFramework.App;
using TBESTFramework.Analysis.NetworkAccessible;
using TBESTFramework.Data.Parcels;
using TBESTFramework.Analysis.Parcels;
using TBESTFramework.Data.SocioEconomic;
using TBESTFramework.Management;
using TBESTFramework.Data.Network;
using TBESTFramework.Data.Network.Properties;
using TBESTFramework.App.SEData;
using TBESTFramework.Utilities;
using TBESTFramework.RidershipForecasting;
using TBESTFramework.RidershipForecasting.ModelEquationSettings;
using TBESTFramework.RidershipForecasting.Validation;
using TBESTFramework.RidershipForecasting.BRTFunctions;
using static TBESTFramework.RidershipForecasting.ModelEquationSettings.ModelParameters;
using static TBESTFramework.Utilities.SQLiteFunctions;
using static TBESTFramework.Utilities.NetworkUtilities;
using static TBESTFramework.Utilities.FileFunctions;
using System.IO;

namespace TBESTModelEquation.TBESTRidershipForecasting
{

	public class InitiateModel : MarshalByRefObject
	{
		public static List<float[]> mParcelOvaluesTotalList = new List<float[]>();
		public static List<ParcelItem> mParcelItemsTotalList = new List<ParcelItem>();

		public static async Task<object> LoadModel(TBESTScenario mOpenScenario, ModelParameters pModelParameters, ModelSettings pModelSettings, string strModelMessage)
		{
			var pModel = new TBESTModelCode();
			return await pModel.ExecuteModelAsync(mOpenScenario, pModelParameters, pModelSettings, strModelMessage);
		}
	}

	public class TBESTModelCode
	{
		private ModelSettings mSettings;
		private StreamWriter m_objOvalueTextStream;
		private StreamWriter m_objBoardingsTS;
		private StreamWriter m_objTransfers;
		private StreamWriter m_objParcelOvalueTextStream;
		private StreamWriter m_objParcelGroupOvalueTextStream;
		public SQLiteConnection m_SQLConnection;
		private CSparseMatrix m_ImpedanceSparseMatrix;
		private CSparseMatrix m_DecaySparseMatrix;
		public string m_BaseLayerPath;
		public string m_DecayMatrixPath;
		private OValueSEDataSummary m_O2OveralappingSE2;
		private OValueSEDataSummary m_O2NonOveralappingSE2;
		private FareLookup m_FareLookUp;
		private int[,] intLocationArrayO4 = null;
		private int[,] intLocationArrayO2 = null;
		private int[,] intLocationArrayOriginMarketOverlap = null;
		private OValueSEDataSummary mPopEmpOverlappedO4 = new OValueSEDataSummary();
		private OValueSEDataSummary mpPopEmpOverlappedCluster = new OValueSEDataSummary();
		private ParcelItems mParcelOverlappedO4 = new ParcelItems();
		private ParcelItems mParcelOverlappedO6 = new ParcelItems();
		private OValueSEDataSummary mPopEmpOverlappedO6 = new OValueSEDataSummary();
		public string mSQLInstanceName = "";
		public string mSQLFileName = "";
		private List<TransitStop> mStopList = new List<TransitStop>();

		public async Task<object> ExecuteModelAsync(TBESTScenario pScenario, ModelParameters pModelParameters, ModelSettings pModelInputSettings, string strModelMessage)
		{

			bool ModelRunRet = false;
			m_BaseLayerPath = pScenario.TransitSystem.TBESTApplication.BaseLayersPath;

			ModelSettings pModelSettings = pModelInputSettings;
			TransitSystems pTSs = new(pScenario.TransitSystem.TBESTApplication);

			await pTSs.Load(false);

			TransitSystem pTS = pTSs.ReturnSystem(pScenario.TransitSystem.TransitSystemID);


			TBESTApplication inAPP = pScenario.TransitSystem.TBESTApplication;
			TBESTApplication pTBAPP = new()
			{
				LocalPath = inAPP.LocalPath,
				BaseLayersPath = inAPP.BaseLayersPath,
				BaseMapLayerFile = inAPP.BaseMapLayerFile,
				CensusLayerName = inAPP.CensusLayerName,
				EmploymentLayerName = inAPP.EmploymentLayerName,
				DatabaseFileName = inAPP.DatabaseFileName,
				ActiveWorkspace = inAPP.ActiveWorkspace,
				ParcelLayerName = inAPP.ParcelLayerName,
				TransitSystems = pTSs
			};

			pTS.TBESTApplication = pTBAPP;

			TBESTScenario mOpenScenario = new(pTS)
			{
				ScenarioPath = pScenario.ScenarioPath,
				ScenarioName = pScenario.ScenarioName,
				BaseYear = pScenario.BaseYear,
				ScenarioDescription = pScenario.ScenarioDescription,
				ScenarioYear = pScenario.ScenarioYear,
				WageRate = pScenario.WageRate,
				WageGrowthRate = pScenario.WageGrowthRate,
				SiteBufferSize = pScenario.SiteBufferSize,
				EmploymentSourceYear = pScenario.EmploymentSourceYear,
				ParcelSourceYear = pScenario.ParcelSourceYear,
				ParcelVersionID = pScenario.ParcelVersionID,
				ModelName = pScenario.ModelName,
				ScenarioID = pScenario.ScenarioID,
				OriginalScenarioID = pScenario.ScenarioID,
				IsBaseYear = pScenario.IsBaseYear,
				LastEmploymentRunFlag = pScenario.LastEmploymentRunFlag,
				LocalEditPath = pScenario.LocalEditPath,
				ZDATAEnabled = pScenario.ZDATAEnabled,
				ZDATAsource = pScenario.ZDATAsource,
				EmploymentType = pScenario.EmploymentType
			};

			m_SQLConnection = await OpenSQLiteConnection(mOpenScenario.TransitSystem.SourcePath + @"\" + mOpenScenario.TransitSystem.TBESTApplication.DatabaseFileName);
			if (string.IsNullOrEmpty(mOpenScenario.ModelName))
			{
				MessageBox.Show("The " + mOpenScenario.ScenarioName + " TBEST scenario does not have a model selected. Please use the Scenario Properties to select a model and then re-execute the model run.", "TBEST Model Run", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return ModelRunRet;
			}
			if (mOpenScenario.TransitSystem.ValidationYear != "0")
			{
				if (await pModelSettings.LoadCoefficients() == false)
				{
					return ModelRunRet;
				}
				await pModelSettings.TBESTModelParameters.LoadEditableModelParameters(await pModelSettings.GetSQLDBConnection());
			}
			else
			{
				if (await pModelSettings.LoadCoefficients() == false)
				{
					return ModelRunRet;
				}
				await pModelSettings.TBESTModelParameters.LoadEditableModelParameters(await pModelSettings.GetSQLDBConnection());
			}

			pModelSettings.TBESTModelParameters.ApplyCapacityConstraint = pModelParameters.ApplyCapacityConstraint;
			pModelSettings.TBESTModelParameters.AssignAllSERecords = pModelParameters.AssignAllSERecords;
			pModelSettings.TBESTModelParameters.WriteTempTableRecords = pModelParameters.WriteTempTableRecords;
			pModelSettings.TBESTModelParameters.TimePeriods = pModelParameters.TimePeriods;
			pModelSettings.TBESTModelParameters.BRTSettings = pModelParameters.BRTSettings;
			pModelSettings.TBESTModelParameters.PerformParknRideMarketShedCapture = pModelParameters.PerformParknRideMarketShedCapture;

			pModelSettings.CloseConnection();

			if (await mOpenScenario.Open(true) == false)
			{
				return ModelRunRet;
			}

			mOpenScenario.RoutePatterns.IndextoRouteGroups(mOpenScenario.Routes);

			if (pModelSettings.CancelForm is not null)
				pModelSettings.CancelForm.Close();

			m_DecayMatrixPath = mOpenScenario.LocalEditPath + @"\DecayMatrix";
			if (!Directory.Exists(m_DecayMatrixPath))
			{
				Directory.CreateDirectory(m_DecayMatrixPath);
			}

			pModelSettings.ModelScenario = mOpenScenario;
			pModelSettings.CancelForm = new TBESTFramework.Forms.frmCancelPro();
			pModelSettings.CancelForm.mTBESTProcess = pModelSettings;
			pModelSettings.ModelProgressBar = pModelSettings.CancelForm._tsProgbar;
			pModelSettings.ModelStatusBar = pModelSettings.CancelForm._tsLabel;
			pModelSettings.CancelForm.TopMost = false;
			pModelSettings.CancelForm._lblModels.Text = mOpenScenario.TransitSystem.TransitSystemName + " - " + mOpenScenario.ScenarioName;
			pModelSettings.CancelForm._lblModelstoProcess.Text = strModelMessage;
			pModelSettings.CancelForm.FormatNotificationGrid();
			pModelSettings.CancelForm.Show(null);
			pModelSettings.ModelScenario.TransitSystem.CloseConnection();

			ModelRunRet = await RunTBESTModel(pModelSettings);
			pModelSettings.CancelForm?.Close();

			return ModelRunRet;
		}
		public ModelSettings ScenarioModelSettings
		{
			get { return mSettings; }
			set { mSettings = value; }
		}

		private void ModelAccessiblity(TransitPattern pRoute, TransitStop pTransitStop, SystemTimePeriod iTimePeriod)
		{
			float dImpedance;
			float dFirstWaitTime;
			TransitRoute pRouteGroup;
			StopNetworkTimePeriodAttribute pTimePeriodAtts;
			ModelStopProcess pModelStopProcess_1;
			try
			{
				if (pTransitStop is null)
					return;

				pRouteGroup = mSettings.ModelScenario.Routes.List[pRoute.ParentRouteIndex];
				dImpedance = (float)m_FareLookUp.GetFareImpedance(pRouteGroup, true);

				pTimePeriodAtts = pTransitStop.TimePeriods.GetTimePeriodAttribute(iTimePeriod);
				if (pTimePeriodAtts is null)
					return;

				if (pTimePeriodAtts.Frequency == 0)
					return;

				dFirstWaitTime = (float)GetFirstWaitTime(pTimePeriodAtts.Headway, iTimePeriod);
				dFirstWaitTime *= mSettings.TBESTModelParameters.CompositeDivisor;
				dFirstWaitTime *= mSettings.TBESTModelParameters.FirstWaitWeight;

				dImpedance += dFirstWaitTime;
				var pstopa = new ProcessStops.ProcessStopAccessiblity();
				var pModeDecaySummary = new ModeTripLength();

				ProcessStops.m_DecaySparseMatrix = m_DecaySparseMatrix;
				ProcessStops.mRouteGroups = mSettings.ModelScenario.Routes;
				ProcessStops.m_ImpedanceSparseMatrix = m_ImpedanceSparseMatrix;
				ProcessStops.mFareLookup = m_FareLookUp;

				pModelStopProcess_1 = new ModelStopProcess()
				{
					Impedance = dImpedance,
					OriginIndex = pTransitStop.StopIndex,
					RoutePattern = pRoute,
					TransitStop = pTransitStop,
					TransferCount = 0,
					MinStop = pTransitStop.RouteIndex + 1,
					MaxStop = pRoute.PatternTransitStops.List.Count - 1,
					OriginRouteGroup = pRoute.ParentTransitRouteID,
					CurrentRouteGroupIndex = pRoute.ParentRouteIndex,
					CurrentOriginaWaitTime = (int)Math.Round(dFirstWaitTime),
					TripLengthSummary = pModeDecaySummary
				};

				pstopa.ProcessStop(pModelStopProcess_1, mSettings.ModelScenario, mSettings.TBESTModelParameters);
				pModelStopProcess_1 = null;
				pstopa = null;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error processing Model Accessibility. To view error details, navigate to the TBEST log file at File->Settings->TBEST Log.", "TBEST Model Run", MessageBoxButtons.OK, MessageBoxIcon.Information);
				ErrorHandling.ErrHandlerMsg(ex);
				mSettings.CancelProcess = true;
			}

		}
		private double GetFirstWaitTime(int iHeadway, SystemTimePeriod iTimePeriod)
		{
			var dFirstWaitTime = default(double);

			if (iTimePeriod != SystemTimePeriod.Saturday & iTimePeriod != SystemTimePeriod.Sunday)
			{
				if (iHeadway <= 30)
				{
					if (iHeadway <= 4)
					{
						dFirstWaitTime = 2d;
					}
					else
					{
						dFirstWaitTime = 2d + 0.5d * (iHeadway - 4);
					}
				}
				else if (iHeadway <= 60 & iHeadway > 30)
				{
					dFirstWaitTime = 15d + 0.167d * (iHeadway - 30);
				}
				else if (iHeadway > 60)
				{
					dFirstWaitTime = 15d + 0.167d * (60 - 30);
				}
			}
			else if (iHeadway <= 30)
			{
				if (iHeadway <= 4)
				{
					dFirstWaitTime = 2d;
				}
				else
				{
					dFirstWaitTime = 2d + 0.5d * (iHeadway - 4);
				}
			}
			else if (iHeadway <= 60 & iHeadway > 30)
			{
				dFirstWaitTime = 15d + 0.167d * (iHeadway - 30);
			}
			else if (iHeadway <= 90 & iHeadway > 60)
			{
				dFirstWaitTime = 20d + 0.083d * (iHeadway - 60);
			}
			else if (iHeadway <= 120 & iHeadway > 90)
			{
				dFirstWaitTime = 22.5d + 0.05d * (iHeadway - 90);
			}
			else if (iHeadway <= 150 & iHeadway > 120)
			{
				dFirstWaitTime = 24d + 0.033d * (iHeadway - 120);
			}
			else if (iHeadway <= 180 & iHeadway > 150)
			{
				dFirstWaitTime = 25d + 0.024d * (iHeadway - 150);
			}
			else if (iHeadway <= 210 & iHeadway > 180)
			{
				dFirstWaitTime = 25.71d + 0.018d * (iHeadway - 180);
			}
			else
			{
				dFirstWaitTime = 25.71d + 0.018d * (210 - 180);
			}

			return dFirstWaitTime;

		}

		private async Task<object> BuildIncomingAccessibleStops(SystemTimePeriod iTimePeriod, TransitStop pOriginTransitStop)
		{
			object BuildIncomingAccessibleStopsRet = null;

			int i;
			int b;

			byte iImpedance;

			byte[] zIndex;
			byte[] zIndexDecay;

			TransferStops pTransferStops;
			var pLocalNeighbors = new NeighborStops();
			TransferStop pTransferStop;
			List<int> intO1Filter = new List<int>();
			int pOriginID = pOriginTransitStop.StopID;
			float sngExpImpedance;
			var lUpstreamBoardings = default(float);
			var lBusLoadAccessibility = default(float);
			var lBusLoadAccessibilityObserbved = default(float);
			var lAPCAccessiblity = default(float);
			StopNetworkTimePeriodAttribute nSTPAOrigin = pOriginTransitStop.TimePeriods.GetTimePeriodAttribute(iTimePeriod);

			try
			{
				NeighborStop pNeighborStop;
				i = pOriginTransitStop.StopIndex;
				pNeighborStop = new NeighborStop();
				pNeighborStop.StopIndex = pOriginTransitStop.StopIndex;

				pLocalNeighbors.List.Add(pNeighborStop);

				if (mSettings.TBESTModelParameters.EmployO1FilteringMethod)
				{
					pTransferStops = nSTPAOrigin.IncomingTransferStops;
					if (pTransferStops is not null)
					{
						for (int Z = 0, loopTo = pTransferStops.List.Count - 1; Z <= loopTo; Z++)
						{
							pTransferStop = pTransferStops.List[Z];
							for (int li = 0, loopTo1 = pTransferStop.RouteIndex; li <= loopTo1; li++)
								intO1Filter.Add(mSettings.ModelScenario.RoutePatterns.List[pTransferStop.RouteItem].PatternTransitStops.List[li].StopID);
						}
					}
				}

				zIndex = new byte[(mSettings.ModelScenario.StopIDList.GetLength(0))];
				zIndexDecay = new byte[(mSettings.ModelScenario.StopIDList.GetLength(0))];
				for (int a = 0, loopTo2 = pLocalNeighbors.List.Count - 1; a <= loopTo2; a++)
				{
					pNeighborStop = pLocalNeighbors.List[a];
					i = pNeighborStop.StopIndex;

					foreach (TransitPattern pRoute in mSettings.ModelScenario.RoutePatterns.List)
					{
						foreach (TransitStop pStop in pRoute.PatternTransitStops.List)
						{
							b = pStop.StopIndex;

							iImpedance = m_ImpedanceSparseMatrix.get_Cell(b, i);

							if (iImpedance > 0)
							{
								if (zIndex[b - 1] == 0 | zIndex[b - 1] > iImpedance)
								{
									zIndex[b - 1] = iImpedance;
									zIndexDecay[b - 1] = m_DecaySparseMatrix.get_Cell(b, i);
								}
							}

						}
					}
				}

				TransitDecayVariable pTripDecayConstant;
				StopNetworkTimePeriodAttribute nSTPA;
				foreach (TransitPattern pDestRoute in mSettings.ModelScenario.RoutePatterns.List)
				{
					foreach (TransitStop pDestTransitStop in pDestRoute.PatternTransitStops.List)
					{
						b = pDestTransitStop.StopIndex - 1;
						iImpedance = zIndex[b];
						pTripDecayConstant = (TransitDecayVariable)zIndexDecay[b];

						if (iImpedance == 0)
							continue;

						sngExpImpedance = (float)Math.Exp((double)(Convert.ToSingle(iImpedance / (double)mSettings.TBESTModelParameters.CompositeDivisor) * mSettings.TBESTModelParameters.GetDecayConstant(pTripDecayConstant)));

						if (pDestTransitStop is not null)
						{
							nSTPA = pDestTransitStop.TimePeriods.GetTimePeriodAttribute(iTimePeriod);
							if (mSettings.TBESTModelParameters.EmployO1FilteringMethod == false | (mSettings.TBESTModelParameters.EmployO1FilteringMethod && intO1Filter.Contains(pDestTransitStop.StopID)))
							{
								lAPCAccessiblity += sngExpImpedance * nSTPA.APCOns;
								lUpstreamBoardings += sngExpImpedance * nSTPA.DirectBoardings;
							}

							if ((pDestTransitStop.StopID.ToString().Substring(0, 5) ?? "") == (pOriginTransitStop.StopID.ToString().Substring(0, 5) ?? "") & pDestTransitStop.StopID < pOriginTransitStop.StopID)
							{
								lBusLoadAccessibility += sngExpImpedance * nSTPA.DirectBoardings;
								lBusLoadAccessibilityObserbved += sngExpImpedance * nSTPA.APCOns;
							}
						}
					}
				}

				if (mSettings.TBESTModelParameters.WriteTempTableRecords)
				{
					await m_objOvalueTextStream.WriteLineAsync(mSettings.ModelScenario.ScenarioID + ", " + pOriginID.ToString() + ", " + ((int)iTimePeriod + 1).ToString() + ", 1, " + (long)Math.Round(lUpstreamBoardings) + ",0,0,0");
					await m_objOvalueTextStream.WriteLineAsync(mSettings.ModelScenario.ScenarioID + ", " + pOriginID.ToString() + ", " + ((int)iTimePeriod + 1).ToString() + ", 7 , " + (long)Math.Round(lAPCAccessiblity) + ",0,0,0");
					await m_objOvalueTextStream.WriteLineAsync(mSettings.ModelScenario.ScenarioID + ", " + pOriginID.ToString() + ", " + ((int)iTimePeriod + 1).ToString() + ", 11 , " + (long)Math.Round(lBusLoadAccessibility) + ",0,0,0");
					await m_objOvalueTextStream.WriteLineAsync(mSettings.ModelScenario.ScenarioID + ", " + pOriginID.ToString() + ", " + ((int)iTimePeriod + 1).ToString() + ", 12 , " + (long)Math.Round(lBusLoadAccessibilityObserbved) + ",0,0,0");
				}
				nSTPAOrigin.OnBoardLoad = (int)Math.Round(lBusLoadAccessibility);

				NetworkOpportunitySEValue pOvalues = new NetworkOpportunitySEValue();

				pOvalues.Population = (int)Math.Round(lUpstreamBoardings);
				pOvalues.EmployService = 0;
				pOvalues.EmployIndustrial = 0;
				pOvalues.EmployCommercial = 0;
				pOvalues.OValue = 1;

				if (nSTPAOrigin.OValues is null)
					pOriginTransitStop.TimePeriods.List[(int)iTimePeriod].OValues = new NetworkOpportunitySEValues();

				pOriginTransitStop.TimePeriods.List[(int)iTimePeriod].OValues.List.Add(pOvalues);

			}

			catch (Exception ex)
			{
				MessageBox.Show("Error Building Accessible Stops. To view error details, navigate to the TBEST log file at File->Settings->TBEST Log.", "TBEST Model Run", MessageBoxButtons.OK, MessageBoxIcon.Information);
				ErrorHandling.ErrHandlerMsg(ex);
				mSettings.CancelProcess = true;
			}

			return BuildIncomingAccessibleStopsRet;
		}
		private void SetLocationAccessiblityArray(TransitStop pOriginTransitStop, SystemTimePeriod iTimePeriod)
		{
			int i;

			NeighborStop pNeighborStop;
			TransitStop pTransitStop;
			TransferStops pTransferStops;
			NeighborStops pLocalNeighbors;
			TransferStop pTransferStop;
			StopNetworkTimePeriodAttribute nSTPAOrigin = pOriginTransitStop.TimePeriods.GetTimePeriodAttribute(iTimePeriod);
			var pNeighborStops = nSTPAOrigin.NeighboringStops;

			try
			{
				intLocationArrayO2 = null;
				intLocationArrayO4 = null;
				intLocationArrayOriginMarketOverlap = null;
				pLocalNeighbors = new NeighborStops();

				i = pOriginTransitStop.StopIndex;
				if (pOriginTransitStop.Interlined == true)
				{
					pTransferStops = nSTPAOrigin.TransferStops;
					if (pTransferStops is not null)
					{
						for (int Z = 0, loopTo = pTransferStops.List.Count - 1; Z <= loopTo; Z++)
						{
							pTransferStop = pTransferStops.List[Z];
							pTransitStop = mStopList[pTransferStop.StopIndex - 1];
							if (pTransitStop.Interlined)
							{
								if (NetworkAccessibleFunctions.AreStopsInterlined(mSettings.ModelScenario, pOriginTransitStop.StopID, pTransferStop.StopID))
								{
									if (pTransitStop.TimePeriods.List[(int)iTimePeriod].Frequency != 0)
									{
										pNeighborStop = new NeighborStop();
										pNeighborStop.StopIndex = pTransferStop.StopIndex;
										pLocalNeighbors.List.Add(pNeighborStop);
									}
								}
							}
						}
					}
				}

				pNeighborStop = new NeighborStop();
				if (i <= mSettings.ModelScenario.StopIDList.GetLength(0))
				{
					pTransitStop = mStopList[i - 1];
					if ((pTransitStop.StopID.ToString().Substring(0, 5) ?? "") == (pOriginTransitStop.StopID.ToString().Substring(0, 5) ?? "") & pTransitStop.TimePeriods.List[(int)iTimePeriod].Frequency != 0)
					{
						pNeighborStop.StopIndex = pTransitStop.StopIndex;
						pLocalNeighbors.List.Add(pNeighborStop);
					}
				}

				intLocationArrayO2 = BuildLocationArray(pLocalNeighbors, false, iTimePeriod);

				pLocalNeighbors = new NeighborStops();
				if (pNeighborStops is not null)
				{
					for (int Z = 0, loopTo1 = pNeighborStops.List.Count - 1; Z <= loopTo1; Z++)
					{
						if (pNeighborStops.List[Z].StopType == 3)
						{
							pNeighborStop = new NeighborStop();
							pNeighborStop.StopIndex = pNeighborStops.List[Z].StopIndex;
							pLocalNeighbors.List.Add(pNeighborStop);
						}
					}
				}

				SummarizeO4(pLocalNeighbors, pOriginTransitStop, iTimePeriod);
			}


			catch (Exception)
			{
			}
		}
		private void SummarizeO4(NeighborStops pLocalNeighbors, TransitStop pOriginStop, SystemTimePeriod intTimePeriod)
		{
			byte intImpedanceValue;
			byte intLocationImpedance;
			int[,] intLocationArray;
			NeighborStop pNeighborStop;
			TransitStop pTransitStop;
			intLocationArrayO4 = new int[mSettings.ModelScenario.LocationCount + 1, 4];
			intLocationArrayOriginMarketOverlap = new int[mSettings.ModelScenario.LocationCount + 1, 4];
			StopPopEmpValues pPopEmp;

			float sngExpImpedance;
			TransitDecayVariable pTripDecayConstant;
			ParcelItem pDestParcelItem;

			mPopEmpOverlappedO4 = null;
			mpPopEmpOverlappedCluster = null;
			mPopEmpOverlappedO6 = null;
			mParcelOverlappedO4 = null;
			mParcelOverlappedO6 = null;


			mPopEmpOverlappedO4 = new OValueSEDataSummary();
			mpPopEmpOverlappedCluster = new OValueSEDataSummary();
			mPopEmpOverlappedO6 = new OValueSEDataSummary();
			mParcelOverlappedO4 = new ParcelItems();
			mParcelOverlappedO6 = new ParcelItems();
			int n;
			byte iO2Impedance;

			TransitStop pTransitStopM;

			for (int a = 0, loopTo = pLocalNeighbors.List.Count - 1; a <= loopTo; a++)
			{
				pNeighborStop = pLocalNeighbors.List[a];

				pTransitStop = mStopList[pNeighborStop.StopIndex - 1];

				intLocationArray = new int[mSettings.ModelScenario.LocationCount + 1, 3];

				short intOriginStopFrequency = pTransitStop.TimePeriods.List[(int)intTimePeriod].Frequency;

				foreach (TransitPattern pRouteW in mSettings.ModelScenario.RoutePatterns.List)
				{
					foreach (TransitStop pTransitStopW in pRouteW.PatternTransitStops.List)
					{
						intImpedanceValue = m_ImpedanceSparseMatrix.get_Cell(pTransitStop.StopIndex, pTransitStopW.StopIndex);
						if (intImpedanceValue == 0)
							continue;
						intLocationImpedance = (byte)intLocationArray[pTransitStopW.LocationID, 1];

						if (intImpedanceValue < intLocationImpedance & intLocationImpedance != 0 | intLocationImpedance == 0)
						{
							intLocationArray[pTransitStopW.LocationID, 1] = intImpedanceValue;
							intLocationArray[pTransitStopW.LocationID, 0] = pTransitStopW.StopIndex;
							intLocationArray[pTransitStopW.LocationID, 2] = m_DecaySparseMatrix.get_Cell(pTransitStop.StopIndex, pTransitStopW.StopIndex);
						}
					}
				}

				for (int i = 1, loopTo1 = intLocationArray.GetLength(0) - 1; i <= loopTo1; i++)
				{
					intImpedanceValue = (byte)intLocationArray[i, 1];
					if (intImpedanceValue == 0)
						continue;
					pTransitStopM = mStopList[intLocationArray[i, 0] - 1];
					n = pTransitStopM.LocationID;

					pTripDecayConstant = (TransitDecayVariable)intLocationArray[n, 2];
					sngExpImpedance = (float)Math.Exp((double)Convert.ToSingle(intImpedanceValue * mSettings.TBESTModelParameters.GetDecayConstant(pTripDecayConstant)));
					pPopEmp = pTransitStopM.PopEmpIndicators;

					// this is to determine the impact of overlap of O4 with O2

					intLocationImpedance = (byte)intLocationArrayO4[n, 1];
					// so, if this orgin stop produces the shortest path to this destination location, then it needs to be added to O4
					if (intImpedanceValue < intLocationImpedance & intLocationImpedance != 0 | intLocationImpedance == 0)
					{
						intLocationArrayO4[n, 0] = intLocationArray[n, 0];
						intLocationArrayO4[n, 1] = intLocationArray[n, 1];
						intLocationArrayO4[n, 2] = intLocationArray[n, 2];
					}
					intLocationArrayO4[n, 3] = intLocationArrayO4[n, 3] + intOriginStopFrequency;

					if (pTransitStop.LocationID == pOriginStop.LocationID)
					{
						intLocationImpedance = (byte)intLocationArrayOriginMarketOverlap[n, 1];
						if (intImpedanceValue < intLocationImpedance & intLocationImpedance != 0 | intLocationImpedance == 0)
						{
							intLocationArrayOriginMarketOverlap[n, 0] = intLocationArray[n, 0];
							intLocationArrayOriginMarketOverlap[n, 1] = intLocationArray[n, 1];
							intLocationArrayOriginMarketOverlap[n, 2] = intLocationArray[n, 2];
						}
						intLocationArrayOriginMarketOverlap[n, 3] = intLocationArrayOriginMarketOverlap[n, 3] + intOriginStopFrequency;
						mpPopEmpOverlappedCluster.AddSummaryForStop(pTransitStopM, intOriginStopFrequency, sngExpImpedance, pPopEmp);
					}

				}
			}
			ParcelItem pParcelItem;

			for (int i = 1, loopTo2 = intLocationArrayO4.GetLength(0) - 1; i <= loopTo2; i++)
			{
				if (intLocationArrayO4[i, 1] == 0)
					continue;
				pTransitStopM = mStopList[intLocationArrayO4[i, 0] - 1];


				n = pTransitStopM.LocationID;
				intImpedanceValue = (byte)intLocationArrayO4[n, 1];

				if (intImpedanceValue == 0)
					continue;

				pTripDecayConstant = (TransitDecayVariable)intLocationArrayO4[n, 2];
				sngExpImpedance = (float)Math.Exp((double)Convert.ToSingle(intImpedanceValue * mSettings.TBESTModelParameters.GetDecayConstant(pTripDecayConstant)));
				pPopEmp = pTransitStopM.PopEmpIndicators;

				iO2Impedance = (byte)intLocationArrayO2[n, 1];
				// this is to determine the impact of overlap of O4 with O2
				// add the destination population and employment to the O4 overlapped summary and O6
				mPopEmpOverlappedO4.AddSummaryForStop(pTransitStopM, 1, sngExpImpedance, pPopEmp);
				if (iO2Impedance > 0)
					mPopEmpOverlappedO6.AddSummaryForStop(pTransitStopM, 1, sngExpImpedance, pPopEmp);

				pDestParcelItem = InitiateModel.mParcelItemsTotalList[pTransitStopM.StopIndex - 1];

				pParcelItem = new ParcelItem()
				{
					DORCode = pDestParcelItem.DORCode,
					Population = pDestParcelItem.Population * sngExpImpedance * 1f,
					StructSqFeet = pDestParcelItem.StructSqFeet * sngExpImpedance * 1f,
					DwellingUnits = pDestParcelItem.DwellingUnits * sngExpImpedance * 1f,
					LandArea = (long)Math.Round(pDestParcelItem.LandArea * sngExpImpedance * 1f),
					Trips = pDestParcelItem.Trips * sngExpImpedance * 1f
				};

				mParcelOverlappedO4.AddtoSummary(pParcelItem, mSettings.ModelScenario.TransitSystem.ParcelTripRates, true);
				if (iO2Impedance > 0)
					mParcelOverlappedO6.AddtoSummary(pParcelItem, mSettings.ModelScenario.TransitSystem.ParcelTripRates, true);

				pParcelItem = null;
			}
		}

		private int[,] BuildLocationArray(NeighborStops pLocalNeighbors, bool boolBuildMultiOriginTripOverlap, SystemTimePeriod intTimePeriod)
		{
			byte intImpedanceValue;
			byte intLocationImpedance;
			int[,] intLocationArray = new int[mSettings.ModelScenario.LocationCount + 1, 3];
			TransitStop pTransitStop;
			int[,] intLocationArrayTotals = new int[mSettings.ModelScenario.LocationCount + 1, 4];

			short intOriginStopFrequency;
			for (int a = 0, loopTo = pLocalNeighbors.List.Count - 1; a <= loopTo; a++)
			{
				pTransitStop = mStopList[pLocalNeighbors.List[a].StopIndex - 1];
				intOriginStopFrequency = pTransitStop.TimePeriods.List[(int)intTimePeriod].Frequency;

				foreach (TransitPattern pRouteW in mSettings.ModelScenario.RoutePatterns.List)
				{
					foreach (TransitStop pTransitStopW in pRouteW.PatternTransitStops.List)
					{
						intImpedanceValue = m_ImpedanceSparseMatrix.get_Cell(pTransitStop.StopIndex, pTransitStopW.StopIndex);
						if (intImpedanceValue == 0)
							continue;
						intLocationImpedance = (byte)intLocationArray[pTransitStopW.LocationID, 1];

						if (intImpedanceValue < intLocationImpedance & intLocationImpedance != 0 | intLocationImpedance == 0)
						{
							intLocationArray[pTransitStopW.LocationID, 1] = intImpedanceValue;
							intLocationArray[pTransitStopW.LocationID, 0] = pTransitStopW.StopIndex;
							intLocationArray[pTransitStopW.LocationID, 2] = m_DecaySparseMatrix.get_Cell(pTransitStop.StopIndex, pTransitStopW.StopIndex);
						}

					}
				}

				if (boolBuildMultiOriginTripOverlap)
				{
					for (int n = 0, loopTo1 = intLocationArray.GetLength(0) - 1; n <= loopTo1; n++)
					{
						intImpedanceValue = (byte)intLocationArray[n, 1];
						if (intImpedanceValue == 0)
							continue;
						intLocationImpedance = (byte)intLocationArrayTotals[n, 1];
						if (intImpedanceValue < intLocationImpedance & intLocationImpedance != 0 | intLocationImpedance == 0)
						{
							intLocationArrayTotals[n, 0] = intLocationArray[n, 0];
							intLocationArrayTotals[n, 1] = intLocationArray[n, 1];
							intLocationArrayTotals[n, 2] = intLocationArray[n, 2];
						}
						intLocationArrayTotals[n, 3] = intLocationArrayTotals[n, 3] + intOriginStopFrequency;
					}
				}
				else
				{
					intLocationArrayTotals = intLocationArray;
				}

			}

			return intLocationArrayTotals;
		}

		private async Task<object> BuildAccessibleStopsO4(SystemTimePeriod iTimePeriod, TransitStop pOriginTransitStop)
		{
			object BuildAccessibleStopsO4Ret = null;
			string intTP = ((int)iTimePeriod + 1).ToString();
			int k = 4;

			ParcelTripRates pr = mSettings.ModelScenario.TransitSystem.ParcelTripRates;

			try
			{

				var pParcelGroupSummaryItems = new ParcelItems();
				var pOverlapParcelGroupSummaryItems = new ParcelItems();
				ParcelItem pItem;
				if (mSettings.TBESTModelParameters.WriteTempTableRecords)
				{
					await m_objOvalueTextStream.WriteLineAsync(mSettings.ModelScenario.ScenarioID.ToString() + ", " + pOriginTransitStop.StopID.ToString() + ", " + intTP + ", " + k.ToString() + ", " + (Math.Round(mPopEmpOverlappedO4.Population)).ToString() + ", " + (Math.Round(mPopEmpOverlappedO4.IndustrialEmployment)).ToString() + ", " + (Math.Round(mPopEmpOverlappedO4.CommercialEmployment)).ToString() + ", " + (Math.Round(mPopEmpOverlappedO4.ServiceEmployment)).ToString());
				}

				if (mSettings.TBESTModelParameters.RunParcelSEAssign)
				{
					for (int zz = 0, loopTo = mParcelOverlappedO4.List.Count - 1; zz <= loopTo; zz++)
					{
						pItem = mParcelOverlappedO4.List[zz];
						if (pItem.GroupCode > 0)
						{
							if (mSettings.TBESTModelParameters.WriteTempTableRecords)
								await m_objParcelOvalueTextStream.WriteLineAsync(mSettings.ModelScenario.ScenarioID.ToString() + "," + pOriginTransitStop.StopID.ToString() + "," + intTP + "," + k.ToString() + "," + pItem.GroupCode.ToString() + "," + "0," + pItem.Population.ToString() + "," + Convert.ToInt32(pItem.Trips).ToString() + "," + pItem.DwellingUnits.ToString() + ",0");

							pParcelGroupSummaryItems.AddtoSummary(mParcelOverlappedO4.List[zz], pr, false);
						}

					}
					if (mSettings.TBESTModelParameters.WriteTempTableRecords)
					{
						for (int zz = 0, loopTo1 = pParcelGroupSummaryItems.List.Count - 1; zz <= loopTo1; zz++)
						{
							pItem = pParcelGroupSummaryItems.List[zz];
							await m_objParcelGroupOvalueTextStream.WriteLineAsync(mSettings.ModelScenario.ScenarioID.ToString() + "," + pOriginTransitStop.StopID.ToString() + "," + intTP + "," + k.ToString() + "," + pItem.GroupCode.ToString() + ",0," + pItem.Population.ToString() + "," + Convert.ToInt32(pItem.Trips).ToString() + "," + pItem.DwellingUnits.ToString() + ",0");
						}
					}
				}

				var pParcelOvalue = new NetworkOpportuntiyParcel();
				NetworkOpportunitySEValue pOvalues = new NetworkOpportunitySEValue();

				if (pOriginTransitStop.TimePeriods.List[(int)iTimePeriod].OValues is null)
					pOriginTransitStop.TimePeriods.List[(int)iTimePeriod].OValues = new NetworkOpportunitySEValues();

				pOvalues.Population = (int)Math.Round(mPopEmpOverlappedO4.Population);
				pOvalues.EmployService = (int)Math.Round(mPopEmpOverlappedO4.ServiceEmployment);
				pOvalues.EmployIndustrial = (int)Math.Round(mPopEmpOverlappedO4.IndustrialEmployment);
				pOvalues.EmployCommercial = (int)Math.Round(mPopEmpOverlappedO4.CommercialEmployment);
				pOvalues.OValue = (byte)k;

				pOriginTransitStop.TimePeriods.List[(int)iTimePeriod].OValues.List.Add(pOvalues);

				if (mSettings.TBESTModelParameters.RunParcelSEAssign)
				{
					if (pOriginTransitStop.TimePeriods.List[(int)iTimePeriod].ParcelOvalues is null)
						pOriginTransitStop.TimePeriods.List[(int)iTimePeriod].ParcelOvalues = new NetworkOpportuntiyParcels();

					for (int zz = 0, loopTo2 = pParcelGroupSummaryItems.List.Count - 1; zz <= loopTo2; zz++)
					{
						pItem = pParcelGroupSummaryItems.List[zz];
						pParcelOvalue = new NetworkOpportuntiyParcel()
						{
							StructSqFeet = pItem.StructSqFeet,
							LandArea = pItem.LandArea,
							Population = pItem.Population,
							DwellingUnits = pItem.DwellingUnits,
							Trips = pItem.Trips,
							OValue = (byte)k,
							DORCode = pItem.DORCode,
							GroupCode = pItem.GroupCode,
						};

						pOriginTransitStop.TimePeriods.List[(int)iTimePeriod].ParcelOvalues.List.Add(pParcelOvalue);
					}
				}

			}
			catch (Exception ex)
			{
				MessageBox.Show("Error Building Accessible Stops. To view error details, navigate to the TBEST log file at File->Settings->TBEST Log.", "TBEST Model Run", MessageBoxButtons.OK, MessageBoxIcon.Information);
				ErrorHandling.ErrHandlerMsg(ex);
				mSettings.CancelProcess = true;

			}

			return BuildAccessibleStopsO4Ret;
		}

		private async Task<object> BuildAccessibleStopsO6(SystemTimePeriod iTimePeriod, TransitStop pOriginTransitStop)
		{
			object BuildAccessibleStopsO6Ret = null;
			string intTP = ((int)iTimePeriod + 1).ToString();
			int k = 6;

			try
			{
				var pParcelGroupSummaryItems = new ParcelItems();
				var pOverlapParcelGroupSummaryItems = new ParcelItems();
				ParcelItem item;
				if (mSettings.TBESTModelParameters.WriteTempTableRecords)
				{
					await m_objOvalueTextStream.WriteLineAsync(mSettings.ModelScenario.ScenarioID.ToString() + ", " + pOriginTransitStop.StopID.ToString() + ", " + intTP + ", " + k.ToString() + ", " + ((long)Math.Round(mPopEmpOverlappedO6.Population)).ToString() + ", " + ((long)Math.Round(mPopEmpOverlappedO6.IndustrialEmployment)).ToString() + ", " + ((long)Math.Round(mPopEmpOverlappedO6.CommercialEmployment)).ToString() + ", " + ((long)Math.Round(mPopEmpOverlappedO6.ServiceEmployment)).ToString());
				}

				if (mSettings.TBESTModelParameters.RunParcelSEAssign)
				{
					for (int zz = 0, loopTo = mParcelOverlappedO6.List.Count - 1; zz <= loopTo; zz++)
					{
						item = mParcelOverlappedO6.List[zz];
						if (item.GroupCode > 0)
						{
							if (mSettings.TBESTModelParameters.WriteTempTableRecords)
								await m_objParcelOvalueTextStream.WriteLineAsync(mSettings.ModelScenario.ScenarioID.ToString() + "," + pOriginTransitStop.StopID.ToString() + "," + intTP + "," + k.ToString() + "," + item.GroupCode.ToString() + "," + "0," + item.Population.ToString() + "," + Convert.ToInt32(item.Trips).ToString() + "," + item.DwellingUnits.ToString() + ",0");

							pParcelGroupSummaryItems.AddtoSummary(mParcelOverlappedO6.List[zz], mSettings.ModelScenario.TransitSystem.ParcelTripRates, false);
						}
					}

					if (mSettings.TBESTModelParameters.WriteTempTableRecords)
					{
						for (int zz = 0, loopTo1 = pParcelGroupSummaryItems.List.Count - 1; zz <= loopTo1; zz++)
						{
							item = pParcelGroupSummaryItems.List[zz];
							await m_objParcelGroupOvalueTextStream.WriteLineAsync(mSettings.ModelScenario.ScenarioID.ToString() + "," + pOriginTransitStop.StopID.ToString() + "," + intTP + "," + k.ToString() + "," + item.GroupCode.ToString() + ",0," + item.Population.ToString() + "," + Convert.ToInt32(item.Trips).ToString() + "," + item.DwellingUnits.ToString() + ",0");
						}
					}
				}

				var pParcelOvalue = new NetworkOpportuntiyParcel();

				if (pOriginTransitStop.TimePeriods.List[(int)iTimePeriod].OValues is null)
					pOriginTransitStop.TimePeriods.List[(int)iTimePeriod].OValues = new NetworkOpportunitySEValues();

				NetworkOpportunitySEValue pOvalues = new NetworkOpportunitySEValue()
				{
					Population = (int)Math.Round(mPopEmpOverlappedO6.Population),
					EmployService = (int)Math.Round(mPopEmpOverlappedO6.ServiceEmployment),
					EmployIndustrial = (int)Math.Round(mPopEmpOverlappedO6.IndustrialEmployment),
					EmployCommercial = (int)Math.Round(mPopEmpOverlappedO6.CommercialEmployment),
					OValue = (byte)k
				};

				pOriginTransitStop.TimePeriods.List[(int)iTimePeriod].OValues.List.Add(pOvalues);

				if (mSettings.TBESTModelParameters.RunParcelSEAssign)
				{
					if (pOriginTransitStop.TimePeriods.List[(int)iTimePeriod].ParcelOvalues is null)
						pOriginTransitStop.TimePeriods.List[(int)iTimePeriod].ParcelOvalues = new NetworkOpportuntiyParcels();

					for (int zz = 0, loopTo2 = pParcelGroupSummaryItems.List.Count - 1; zz <= loopTo2; zz++)
					{
						item = pParcelGroupSummaryItems.List[zz];
						pParcelOvalue = new NetworkOpportuntiyParcel()
						{
							StructSqFeet = item.StructSqFeet,
							LandArea = item.LandArea,
							Population = item.Population,
							DwellingUnits = item.DwellingUnits,
							Trips = item.Trips,
							OValue = (byte)k,
							DORCode = item.DORCode,
							GroupCode = item.GroupCode
						};

						pOriginTransitStop.TimePeriods.List[(int)iTimePeriod].ParcelOvalues.List.Add(pParcelOvalue);
					}
				}

			}
			catch (Exception ex)
			{
				MessageBox.Show("Error Building Accessible Stops.  To view error details, navigate to the TBEST log file at File->Settings->TBEST Log.", "TBEST Model Run", MessageBoxButtons.OK, MessageBoxIcon.Information);
				ErrorHandling.ErrHandlerMsg(ex);
				mSettings.CancelProcess = true;

			}

			return BuildAccessibleStopsO6Ret;
		}
		private async Task<object> BuildAccessibleStopsO2(SystemTimePeriod iTimePeriod, TransitStop pOriginTransitStop)
		{
			object BuildAccessibleStopsO2Ret = null;
			byte iImpedance;
			NetworkOpportuntiyParcel pParcelOvalue;
			float sngExpImpedance;
			ParcelItem pDestParcelItem;
			StopPopEmpValues pPopEmp;
			int k = 2;

			try
			{

				TransitDecayVariable pTripDecayConstant;
				short intOriginStopFrequency = pOriginTransitStop.TimePeriods.List[(int)iTimePeriod].Frequency;
				var pPopEmpOverlapped = new OValueSEDataSummary();
				var pPopEmpNonOverlapped = new OValueSEDataSummary();
				var pParcelSummaryItemsNonOverlapped = new ParcelItems();
				var pParcelSummaryItemsOverlapped = new ParcelItems();
				var pPopEmpOverlapped2 = new OValueSEDataSummary();
				var pPopEmpNonOverlapped2 = new OValueSEDataSummary();
				ParcelItem nitem;
				short intDestClusterArrivalsO4 = 0;
				short intDestClusterArrivals = 0;
				var pParcelItem = new ParcelItem();
				TransitStop pTransitStopM = null;
				var tr = mSettings.ModelScenario.TransitSystem.ParcelTripRates;
				for (int nte = 1, loopTo = intLocationArrayO2.GetLength(0) - 1; nte <= loopTo; nte++)
				{
					if (intLocationArrayO2[nte, 1] == 0)
						continue;
					pTransitStopM = mStopList[intLocationArrayO2[nte, 0] - 1];
					iImpedance = (byte)intLocationArrayO2[pTransitStopM.LocationID, 1];
					pTripDecayConstant = (TransitDecayVariable)intLocationArrayO2[pTransitStopM.LocationID, 2];
					pPopEmp = pTransitStopM.PopEmpIndicators;
					sngExpImpedance = (float)Math.Exp((double)Convert.ToSingle(iImpedance * mSettings.TBESTModelParameters.GetDecayConstant(pTripDecayConstant)));

					intDestClusterArrivalsO4 = (short)intLocationArrayO4[pTransitStopM.LocationID, 3];  // this is the number of arrivals at the O4 destination.  If > the # of O2 arrivals then O4 reaches this destination as well
					intDestClusterArrivals = (short)intLocationArrayOriginMarketOverlap[pTransitStopM.LocationID, 3]; // this is the number of arrivals at the cluster destination.  If > the # of O2 arrivals then cluster reaches this destination as well

					// if the share the o2 destination does not have an O4 destination in common, then there is no overlap
					if (intDestClusterArrivalsO4 == 0)
					{
						// 'this is the overlap split of total population and employment for the summary originating from O4 stop location clusters
						pPopEmpNonOverlapped.AddSummaryForStop(pTransitStopM, 1, sngExpImpedance, pPopEmp);
					}
					else
					{
						pPopEmpOverlapped.AddSummaryForStop(pTransitStopM, 1, sngExpImpedance, pPopEmp);
					}

					if (intDestClusterArrivals == 0)
					{
						pPopEmpNonOverlapped2.AddSummaryForStop(pTransitStopM, intOriginStopFrequency, sngExpImpedance, pPopEmp);
					}
					else
					{
						pPopEmpOverlapped2.AddSummaryForStop(pTransitStopM, intOriginStopFrequency, sngExpImpedance, pPopEmp);
					}

					if (mSettings.TBESTModelParameters.RunParcelSEAssign)
					{
						pDestParcelItem = InitiateModel.mParcelItemsTotalList[pTransitStopM.StopIndex - 1];

						pParcelItem = new ParcelItem()
						{
							DORCode = pDestParcelItem.DORCode,
							Population = pDestParcelItem.Population * sngExpImpedance * 1f,
							StructSqFeet = pDestParcelItem.StructSqFeet * sngExpImpedance * 1f,
							DwellingUnits = pDestParcelItem.DwellingUnits * sngExpImpedance * 1f,
							LandArea = (long)Math.Round(pDestParcelItem.LandArea * sngExpImpedance * 1f),
							Trips = pDestParcelItem.Trips * sngExpImpedance * 1f
						};

						if (intDestClusterArrivalsO4 == intOriginStopFrequency)
						{
							pParcelSummaryItemsNonOverlapped.AddtoSummary(pParcelItem, tr, true);
						}
						else
						{
							pParcelSummaryItemsOverlapped.AddtoSummary(pParcelItem, tr, true);
						}
						pParcelItem = null;
					}
				}

				var pParcelGroupSummaryItems = new ParcelItems();
				var pOverlapParcelGroupSummaryItems = new ParcelItems();

				string intTP = ((int)iTimePeriod + 1).ToString();
				if (mSettings.TBESTModelParameters.WriteTempTableRecords)
				{

					await m_objOvalueTextStream.WriteLineAsync(mSettings.ModelScenario.ScenarioID + ", " + pOriginTransitStop.StopID + ", " + intTP + ", " + k + ", " +
						(long)Math.Round(pPopEmpNonOverlapped.Population + pPopEmpOverlapped.Population) + ", " + (long)Math.Round(pPopEmpNonOverlapped.IndustrialEmployment +
						pPopEmpOverlapped.IndustrialEmployment) + ", " + (long)Math.Round(pPopEmpNonOverlapped.CommercialEmployment + pPopEmpOverlapped.CommercialEmployment) + ", " +
						(long)Math.Round(pPopEmpNonOverlapped.ServiceEmployment + pPopEmpOverlapped.ServiceEmployment));

					await m_objOvalueTextStream.WriteLineAsync(mSettings.ModelScenario.ScenarioID + ", " + pOriginTransitStop.StopID + ", " + intTP + ",5, " +
						(long)Math.Round(pPopEmpOverlapped.Population) + ", " + (long)Math.Round(pPopEmpOverlapped.IndustrialEmployment) + ", " +
						(long)Math.Round(pPopEmpOverlapped.CommercialEmployment) + ", " + (long)Math.Round(pPopEmpOverlapped.ServiceEmployment));

					if (mSettings.TBESTModelParameters.WriteTempTableRecords)
					{
						await m_objOvalueTextStream.WriteLineAsync(mSettings.ModelScenario.ScenarioID + ", " + pOriginTransitStop.StopID + ", " + intTP + ", 8 , " + (long)(pPopEmpNonOverlapped.ParkingSpacesAvailable + pPopEmpOverlapped.ParkingSpacesAvailable) + ",0,0,0");
						await m_objOvalueTextStream.WriteLineAsync(mSettings.ModelScenario.ScenarioID + ", " + pOriginTransitStop.StopID + ", " + intTP + ", 9 , " + (long)Math.Round(pPopEmpNonOverlapped.ParkingPopulation + pPopEmpOverlapped.ParkingPopulation) + ",0,0,0");
						await m_objOvalueTextStream.WriteLineAsync(mSettings.ModelScenario.ScenarioID + ", " + pOriginTransitStop.StopID + ", " + intTP + ", 10 , " + (long)Math.Round(pPopEmpNonOverlapped.ParkingEmployment + pPopEmpOverlapped.ParkingEmployment) + ",0,0,0");
					}

				}

				if (mSettings.TBESTModelParameters.RunParcelSEAssign)
				{
					for (int zz = 0, loopTo1 = pParcelSummaryItemsNonOverlapped.List.Count - 1; zz <= loopTo1; zz++)
					{
						nitem = pParcelSummaryItemsNonOverlapped.List[zz];
						if (nitem.GroupCode > 0)
						{
							if (mSettings.TBESTModelParameters.WriteTempTableRecords)
								await m_objParcelOvalueTextStream.WriteLineAsync(mSettings.ModelScenario.ScenarioID + "," + pOriginTransitStop.StopID + "," + intTP + "," + k + "," + nitem.DORCode + "," + nitem.StructSqFeet + "," + nitem.Population + "," + Convert.ToInt32(nitem.Trips) + "," + nitem.DwellingUnits + "," + nitem.LandArea);

							pParcelGroupSummaryItems.AddtoSummary(pParcelSummaryItemsNonOverlapped.List[zz], mSettings.ModelScenario.TransitSystem.ParcelTripRates, false);
						}
					}

					if (mSettings.TBESTModelParameters.WriteTempTableRecords)
					{
						for (int zz = 0, loopTo2 = pParcelGroupSummaryItems.List.Count - 1; zz <= loopTo2; zz++)
						{
							nitem = pParcelGroupSummaryItems.List[zz];
							await m_objParcelGroupOvalueTextStream.WriteLineAsync(mSettings.ModelScenario.ScenarioID.ToString() + "," + pOriginTransitStop.StopID.ToString() + "," + intTP + "," + k.ToString() + "," + nitem.GroupCode.ToString() + ",0," + nitem.Population.ToString() + "," + Convert.ToInt32(nitem.Trips).ToString() + "," + nitem.DwellingUnits.ToString() + ",0");
						}
					}

					for (int zz = 0, loopTo3 = pParcelSummaryItemsOverlapped.List.Count - 1; zz <= loopTo3; zz++)
					{
						nitem = pParcelSummaryItemsOverlapped.List[zz];
						if (nitem.GroupCode > 0)
						{
							if (mSettings.TBESTModelParameters.WriteTempTableRecords)
								await m_objParcelOvalueTextStream.WriteLineAsync(mSettings.ModelScenario.ScenarioID + "," + pOriginTransitStop.StopID + "," + intTP + ",5," + nitem.DORCode + "," + nitem.StructSqFeet + "," + nitem.Population + "," + Convert.ToInt32(nitem.Trips) + "," + nitem.DwellingUnits + "," + nitem.LandArea);
							pOverlapParcelGroupSummaryItems.AddtoSummary(pParcelSummaryItemsOverlapped.List[zz], mSettings.ModelScenario.TransitSystem.ParcelTripRates, false);
						}
					}

					if (mSettings.TBESTModelParameters.WriteTempTableRecords)
					{
						for (int zz = 0, loopTo4 = pOverlapParcelGroupSummaryItems.List.Count - 1; zz <= loopTo4; zz++)
						{
							nitem = pOverlapParcelGroupSummaryItems.List[zz];
							await m_objParcelGroupOvalueTextStream.WriteLineAsync(mSettings.ModelScenario.ScenarioID.ToString() + "," + pOriginTransitStop.StopID.ToString() + "," + intTP + ",5," + nitem.GroupCode.ToString() + ",0," + nitem.Population.ToString() + "," + Convert.ToInt32(nitem.Trips).ToString() + "," + nitem.DwellingUnits.ToString() + ",0");
						}
					}
				}


				if (pOriginTransitStop.TimePeriods.List[(int)iTimePeriod].OValues is null)
					pOriginTransitStop.TimePeriods.List[(int)iTimePeriod].OValues = new NetworkOpportunitySEValues();

				NetworkOpportunitySEValue pOvalues = new NetworkOpportunitySEValue()
				{
					Population = (int)Math.Round(pPopEmpNonOverlapped.Population + pPopEmpOverlapped.Population),
					EmployService = (int)Math.Round(pPopEmpNonOverlapped.ServiceEmployment + pPopEmpOverlapped.ServiceEmployment),
					EmployIndustrial = (int)Math.Round(pPopEmpNonOverlapped.IndustrialEmployment + pPopEmpOverlapped.IndustrialEmployment),
					EmployCommercial = (int)Math.Round(pPopEmpNonOverlapped.CommercialEmployment + pPopEmpOverlapped.CommercialEmployment),
					OValue = (byte)k
				};

				pOriginTransitStop.TimePeriods.List[(int)iTimePeriod].OValues.List.Add(pOvalues);

				pOvalues = new NetworkOpportunitySEValue
				{
					Population = (int)Math.Round(pPopEmpOverlapped.Population),
					EmployService = (int)Math.Round(pPopEmpOverlapped.ServiceEmployment),
					EmployIndustrial = (int)Math.Round(pPopEmpOverlapped.IndustrialEmployment),
					EmployCommercial = (int)Math.Round(pPopEmpOverlapped.CommercialEmployment),
					OValue = 5
				};


				pOriginTransitStop.TimePeriods.List[(int)iTimePeriod].OValues.List.Add(pOvalues);

				if (mSettings.TBESTModelParameters.RunParcelSEAssign)
				{
					if (pOriginTransitStop.TimePeriods.List[(int)iTimePeriod].ParcelOvalues is null)
						pOriginTransitStop.TimePeriods.List[(int)iTimePeriod].ParcelOvalues = new NetworkOpportuntiyParcels();

					for (int zz = 0, loopTo5 = pParcelGroupSummaryItems.List.Count - 1; zz <= loopTo5; zz++)
					{

						nitem = pParcelGroupSummaryItems.List[zz];
						pParcelOvalue = new NetworkOpportuntiyParcel()
						{
							StructSqFeet = nitem.StructSqFeet,
							LandArea = nitem.LandArea,
							Population = nitem.Population,
							DwellingUnits = nitem.DwellingUnits,
							Trips = nitem.Trips,
							OValue = (byte)k,
							DORCode = nitem.DORCode,
							GroupCode = nitem.GroupCode,
						};

						if (mSettings.TBESTModelParameters.WriteTempTableRecords)
							await m_objParcelOvalueTextStream.WriteLineAsync(mSettings.ModelScenario.ScenarioID + "," + pOriginTransitStop.StopID + "," + intTP + "," + k.ToString() + "," + pParcelOvalue.DORCode + "," + pParcelOvalue.StructSqFeet + "," + pParcelOvalue.Population + "," + Convert.ToInt32(pParcelOvalue.Trips) + "," + pParcelOvalue.DwellingUnits + "," + pParcelOvalue.LandArea);

						pOriginTransitStop.TimePeriods.List[(int)iTimePeriod].ParcelOvalues.List.Add(pParcelOvalue);
					}

					for (int zz = 0, loopTo6 = pOverlapParcelGroupSummaryItems.List.Count - 1; zz <= loopTo6; zz++)
					{
						nitem = pOverlapParcelGroupSummaryItems.List[zz];
						pParcelOvalue = new NetworkOpportuntiyParcel()
						{
							StructSqFeet = nitem.StructSqFeet,
							LandArea = nitem.LandArea,
							Population = nitem.Population,
							DwellingUnits = nitem.DwellingUnits,
							Trips = nitem.Trips,
							OValue = 5,
							DORCode = nitem.DORCode,
							GroupCode = nitem.GroupCode
						};
						if (mSettings.TBESTModelParameters.WriteTempTableRecords)
							await m_objParcelOvalueTextStream.WriteLineAsync(mSettings.ModelScenario.ScenarioID + "," + pOriginTransitStop.StopID + "," + intTP + ",5," + pParcelOvalue.DORCode + "," + pParcelOvalue.StructSqFeet + "," + pParcelOvalue.Population + "," + Convert.ToInt32(pParcelOvalue.Trips) + "," + pParcelOvalue.DwellingUnits + "," + pParcelOvalue.LandArea);

						pOriginTransitStop.TimePeriods.List[(int)iTimePeriod].ParcelOvalues.List.Add(pParcelOvalue);
					}

				}
				m_O2OveralappingSE2 = pPopEmpOverlapped2;
				m_O2NonOveralappingSE2 = pPopEmpNonOverlapped2;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error Building Accessible Stops. To view error details, navigate to the TBEST log file at File->Settings->TBEST Log.", "TBEST Model Run", MessageBoxButtons.OK, MessageBoxIcon.Information);
				ErrorHandling.ErrHandlerMsg(ex);
				mSettings.CancelProcess = true;
			}

			return BuildAccessibleStopsO2Ret;
		}

		private async Task BuildOverlapDestinationsAccessibleStops(SystemTimePeriod iTimePeriod, TransitStop pOriginTransitStop)
		{

			try
			{
				double O2Overlapped = 0d;
				double intClusterTotal = 0d;
				double O2NonOverlapped = 0d;
				byte bytOverlapPCT = 0;
				string intTP = ((int)iTimePeriod + 1).ToString();
				O2Overlapped = (double)(m_O2OveralappingSE2.CommercialEmployment + m_O2OveralappingSE2.IndustrialEmployment + m_O2OveralappingSE2.ServiceEmployment + m_O2OveralappingSE2.Population);
				intClusterTotal = (double)(mpPopEmpOverlappedCluster.CommercialEmployment + mpPopEmpOverlappedCluster.IndustrialEmployment + mpPopEmpOverlappedCluster.ServiceEmployment + mpPopEmpOverlappedCluster.Population);
				O2NonOverlapped = (double)(m_O2NonOveralappingSE2.CommercialEmployment + m_O2NonOveralappingSE2.IndustrialEmployment + m_O2NonOveralappingSE2.ServiceEmployment + m_O2NonOveralappingSE2.Population);
				bytOverlapPCT = Convert.ToByte(intClusterTotal == 0d ? 0 : 100d * (O2Overlapped / (O2Overlapped + O2NonOverlapped + intClusterTotal)));
				pOriginTransitStop.PopEmpIndicators.DestinationOverlapPCT = bytOverlapPCT;

				if (mSettings.TBESTModelParameters.WriteTempTableRecords)
				{

					await m_objOvalueTextStream.WriteLineAsync(mSettings.ModelScenario.ScenarioID.ToString() + ", " + pOriginTransitStop.StopID.ToString() + ", " + intTP + ", 3, " + bytOverlapPCT.ToString() + ",0,0,0");
					//m_objOvalueTextStream.Flush();
				}
			}

			catch (Exception ex)
			{
				MessageBox.Show("Error Building Accessible Stops. To view error details, navigate to the TBEST log file at File->Settings->TBEST Log.", "TBEST Model Run", MessageBoxButtons.OK, MessageBoxIcon.Information);
				ErrorHandling.ErrHandlerMsg(ex);
				mSettings.CancelProcess = true;

			}
		}
		private async Task<bool> RunStopDistance()
		{
			bool RunStopDistanceRet = false;

			DataTable rs = await GetDataTable("Select STOPID from SPATIAL_EDIT_LOG WHERE (EDITTYPE = 'Move Stop' OR EDITTYPE = 'New Stop' OR EDITTYPE = 'ADD' OR EDITTYPE = 'Deleted Stop') AND SCEN_ID = " + mSettings.ModelScenario.ScenarioID, m_SQLConnection);

			if (rs.Rows.Count > 0)
			{
				RunStopDistanceRet = true;
			}
			rs.Clear();
			rs.Dispose();
			if (RunStopDistanceRet == false)
			{
				rs = await GetDataTable("Select ACCEPTED from SYSTEM_EDIT_LOG where SCEN_ID = " + mSettings.ModelScenario.ScenarioID + " and ACCEPTED = 0", m_SQLConnection);
				if (rs.Rows.Count > 0)
				{
					RunStopDistanceRet = true;
				}
				rs.Clear();
				rs.Dispose();
			}
			rs = await GetDataTable("Select STOPID from SPATIAL_EDIT_LOG WHERE (EDITTYPE = 'Deleted Stop') AND SCEN_ID = " + mSettings.ModelScenario.ScenarioID, m_SQLConnection);
			if (rs.Rows.Count > 0)
			{
				RunStopDistanceRet = true;
				mSettings.TBESTModelParameters.AssignAllSERecords = true;
			}
			rs.Clear();
			rs.Dispose();
			return RunStopDistanceRet;

		}
		private bool InitializeModelTextFiles()
		{
			bool InitializeModelTextFilesRet = false;

			try
			{

				m_objOvalueTextStream = OpenStream(mSettings.ModelScenario.LocalEditPath + @"\O_VALUES.txt", true, false, true);
				if (m_objOvalueTextStream is null)
				{
					mSettings.CancelProcess = true;
					return InitializeModelTextFilesRet;
				}

				m_objBoardingsTS = OpenStream(mSettings.ModelScenario.LocalEditPath + @"\ANALYSIS_BOARDINGS.txt", true, false, true);
				if (m_objBoardingsTS is null)
				{
					mSettings.CancelProcess = true;
					return InitializeModelTextFilesRet;
				}

				m_objTransfers = OpenStream(mSettings.ModelScenario.LocalEditPath + @"\TRANSFERS.txt", true, false, true);

				if (m_objTransfers is null)
				{
					mSettings.CancelProcess = true;
					return InitializeModelTextFilesRet;
				}

				m_objParcelOvalueTextStream = OpenStream(mSettings.ModelScenario.LocalEditPath + @"\PARCEL_OVALUES.txt", true, false, true);
				if (m_objParcelOvalueTextStream is null)
				{
					mSettings.CancelProcess = true;
					return InitializeModelTextFilesRet;
				}

				m_objParcelGroupOvalueTextStream = OpenStream(mSettings.ModelScenario.LocalEditPath + @"\PARCEL_GROUP_OVALUES.txt", true, false, true);

				if (m_objParcelGroupOvalueTextStream is null)
				{
					mSettings.CancelProcess = true;
					return InitializeModelTextFilesRet;
				}

				InitializeModelTextFilesRet = true;
			}

			catch (Exception ex)
			{
				MessageBox.Show("Error initializing Model files. To view error details, navigate to the TBEST log file at File->Settings->TBEST Log.", "TBEST Model Run", MessageBoxButtons.OK, MessageBoxIcon.Information);
				ErrorHandling.ErrHandlerMsg(ex);
			}

			return InitializeModelTextFilesRet;

		}
		public async Task<bool> RunTBESTModel(ModelSettings pModelSettings)
		{
			bool RunTBESTModelRet = false;
			string sTimePeriod;

			try
			{
				mSettings = pModelSettings;
				if (mSettings.CancelProcess)
					return RunTBESTModelRet;

				var pMarketAssessment = new ModelMarketAssessmentFunctions();

				await mSettings.ModelScenario.ScenarioLog.WriteLineAsync("*** Scenario Model Run Began:" + Convert.ToString(DateTime.Now));
				await mSettings.ModelScenario.ScenarioLog.WriteLineAsync("");
				await mSettings.ModelScenario.ScenarioLog.WriteLineAsync("*** Model Parameters ***");
				await mSettings.ModelScenario.ScenarioLog.WriteLineAsync("Model Name: " + mSettings.ModelScenario.ModelName);
				await mSettings.TBESTModelParameters.WriteModelParameters(mSettings.ModelScenario.ScenarioLog);
				await mSettings.ModelScenario.ScenarioLog.WriteLineAsync("");

				if (await ReturnRecordCount("growthrate", "Tract", "SCEN_ID = " + mSettings.ModelScenario.ScenarioID, m_SQLConnection) == 0)
				{
					var p = new SocioEconomicDataUpdate
					{
						SESourceYear = mSettings.ModelScenario.BaseYear,
						EmploymentSourceYear = mSettings.ModelScenario.EmploymentSourceYear
					};
					await p.ResetGrowthRates(false, mSettings.ModelScenario.ScenarioID, mSettings.ModelScenario.TransitSystem);
				}

				Cursor.Current = Cursors.WaitCursor;

				mSettings.CancelProcess = false;
				m_FareLookUp = new FareLookup(mSettings.ModelScenario, mSettings.TBESTModelParameters);

				List<string> strNotificationList =
				[
					"Indexing Stops",
					"Calculating Stop Distance",
					"Generating Socio-Economic Data",
					"Loading Model Data",
					"Compiling Network",
				];

				foreach (SystemTimePeriod t in mSettings.TBESTModelParameters.TimePeriods)
				{
					sTimePeriod = LookupTimePeriod(t);
					strNotificationList.Add(sTimePeriod + " Model");
				}

				string[] strNotificationList2 = strNotificationList.ToArray();
				mSettings.CancelForm.InitializeList(ref strNotificationList2);

				mSettings.CancelForm.NotificationUpdate();

				await NetworkAccessibleFunctions.IndexStopsTable(mSettings.ModelScenario);

				Application.DoEvents();
				if (mSettings.CancelProcess)
					goto cleanup;

				await mSettings.ModelScenario.ScenarioLog.WriteLineAsync("*** Scenario Stop Indexing Completed :" + Convert.ToString(DateTime.Now));
				mSettings.CancelForm.NotificationUpdate();

				if (!mSettings.TBESTModelParameters.AssignAllSERecords)
					await RunStopDistance();

				Application.DoEvents();
				if (mSettings.CancelProcess)
					goto cleanup;

				if (mSettings.TBESTModelParameters.WriteTempTableRecords)
				{
					string strDropQuery = "";
					foreach (SystemTimePeriod t in mSettings.TBESTModelParameters.TimePeriods)
					{
						strDropQuery += "TIMEPERIOD = " + ((int)t + 1) + " or ";
					}

					strDropQuery = strDropQuery.Substring(0, strDropQuery.Length - 4);
					await ExecuteQuery(m_SQLConnection, "DELETE FROM O_VALUES WHERE SCEN_ID = " + mSettings.ModelScenario.ScenarioID + " AND (" + strDropQuery + ")");
					await ExecuteQuery(m_SQLConnection, "DELETE FROM PARCEL_OVALUES WHERE SCEN_ID = " + mSettings.ModelScenario.ScenarioID + " AND (" + strDropQuery + ")");
					await ExecuteQuery(m_SQLConnection, "DELETE FROM TRANSFERS WHERE SCEN_ID = " + mSettings.ModelScenario.ScenarioID + " AND (" + strDropQuery + ")");
				}

				mSettings.ModelScenario.StopPoints = await NetworkAccessibleFunctions.LoadPointCollection(mSettings.ModelScenario);

				bool val = await NetworkAccessibleFunctions.CalculateStopDistances(mSettings.ModelScenario, mSettings.TBESTModelParameters, mSettings.CancelForm, mSettings.ModelProgressBar, mSettings.ModelStatusBar);
				mSettings.CancelProcess = !val;
				SystemTables pSystemTables = new SystemTables();
				await pSystemTables.LoadAsync(mSettings.ModelScenario.TransitSystem, ReturnModelTables: false);

				await RebuildClusteredIndexes(m_SQLConnection);
				Application.DoEvents();

				if (mSettings.CancelProcess)
					goto cleanup;

				mSettings.CancelForm.NotificationUpdate();
				await mSettings.ModelScenario.ScenarioLog.WriteLineAsync("*** Scenario Stop Indexing Completed :" + Convert.ToString(DateTime.Now));
				mSettings.ModelScenario.TransitSystem.CloseConnection();

				if (mSettings.ModelScenario.TransitSystem.ValidationYear != "0")
				{
					mSettings.ModelScenario.TransitSystem.ParcelTripRates = await TripGenerationRates.LoadParcelTripRatesfromExcel(mSettings.ModelScenario.TransitSystem, mSettings.ModelScenario.TransitSystem.SourcePath + @"\Model\" + mSettings.ModelScenario.ModelName);
				}
				else
				{
					mSettings.ModelScenario.TransitSystem.ParcelTripRates = await TripGenerationRates.LoadParcelTripRatesfromExcel(mSettings.ModelScenario.TransitSystem, m_BaseLayerPath + @"\Models\" + mSettings.ModelScenario.ModelName);
				}

				val = await pMarketAssessment.GeneratePopEmpNumbers(mSettings.ModelScenario, mSettings.TBESTModelParameters, mSettings.CancelForm, mSettings.ModelProgressBar, mSettings.ModelStatusBar, m_SQLConnection);
				mSettings.CancelProcess = !val;
				mSettings.ModelScenario.TransitSystem.CloseConnection();

				DetachAllSQLiteConnections();
				GC.Collect();

				if (mSettings.CancelProcess)
					goto cleanup;
				mSettings.CancelForm.NotificationUpdate();
				await mSettings.ModelScenario.ScenarioLog.WriteLineAsync("*** Scenario Socio-Economic Data Update Completed :" + Convert.ToString(DateTime.Now));

				var pModelVal = new ModelValidation(mSettings.ModelScenario);
				pModelVal.ProgressBar = mSettings.ModelProgressBar;
				pModelVal.StatusLabel = (ToolStripStatusLabel)mSettings.ModelStatusBar;
				pModelVal.ModelParameters = mSettings.TBESTModelParameters;
				await pModelVal.CalculateValidationbyCollection(false, !(await mSettings.ModelScenario.TransitSystem.IsModelValidated()), m_SQLConnection);


				if (await mSettings.ModelScenario.LoadTransitNetwork(TBESTScenario.LoadTypes.Routes, mSettings.TBESTModelParameters.TimePeriods, mSettings.ModelProgressBar, mSettings.ModelStatusBar) == false)
				{
					mSettings.CancelProcess = true;
					goto cleanup;
				}

				if (mSettings.ModelScenario.LoadHeadwayforRoutes() == false)
				{
					mSettings.CancelProcess = true;
					goto cleanup;
				}

				Application.DoEvents();

				if (mSettings.CancelProcess)
					goto cleanup;

				mSettings.CancelForm.NotificationUpdate();

				if (mSettings.ModelStatusBar is not null)
					mSettings.ModelStatusBar.Text = "Compiling the TBEST Network...";

				if (mSettings.TBESTModelParameters.PerformParknRideMarketShedCapture)
				{
					if (mSettings.ModelStatusBar is not null)
						mSettings.ModelStatusBar.Text = "Generating Park-n-Ride Market Shed...";
					await pMarketAssessment.AssignParknRideSEData(mSettings.ModelScenario, m_SQLConnection);
				}

				Application.DoEvents();
				if (mSettings.CancelProcess)
					goto cleanup;

				if (await mSettings.ModelScenario.CopyRouteandSegmentFeatureClasses(mSettings.ModelScenario.LocalEditPath + @"\TBEST_LAYERS.geodatabase", mSettings.ModelScenario.ScenarioPath + @"\TBEST_LAYERS.geodatabase") == false)
				{
					mSettings.CancelProcess = true;
					goto cleanup;
				}

				mSettings.CancelForm.NotificationUpdate();
				await mSettings.ModelScenario.ScenarioLog.WriteLineAsync("*** Compiled Network:" + Convert.ToString(DateTime.Now));
				int intUseMemory = 1;

				foreach (TransitPattern pRoute in mSettings.ModelScenario.RoutePatterns.List)
				{
					foreach (TransitStop pTransitStop in pRoute.PatternTransitStops.List)
					{
						if (pTransitStop.StopIndex - 1 >= mStopList.Count)
						{
							mStopList.Add(pTransitStop);
						}
						else
						{
							mStopList.Insert(pTransitStop.StopIndex - 1, pTransitStop);
						}
					}
				}

				StreamWriter oWrite;
				oWrite = OpenStream(mSettings.ModelScenario.ScenarioPath + @"\Log\DirectBoardings.csv", true, false, true);
				await oWrite.WriteLineAsync("StopID,TimePeriod,Variable,Value,Coeff,EquationValue");
				oWrite.Close();

				oWrite = OpenStream(mSettings.ModelScenario.ScenarioPath + @"\Log\TransferBoardings.csv", true, false, true);
				await oWrite.WriteLineAsync("StopID,TimePeriod,Variable,Value,Coeff,EquationValue");
				oWrite.Close();
				bool boolValidated = await mSettings.ModelScenario.TransitSystem.IsModelValidated();

				string argoutDir = mSettings.ModelScenario.LocalEditPath;
				int argnRow = mSettings.ModelScenario.StopIDList.GetLength(0);
				TBESTScenario.LoadTypes argiEnumValues = (TBESTScenario.LoadTypes)((int)TBESTScenario.LoadTypes.TimePeriods + (int)TBESTScenario.LoadTypes.Transfers + (int)TBESTScenario.LoadTypes.NeigborStops + (int)TBESTScenario.LoadTypes.Parcels);

				foreach (SystemTimePeriod t in mSettings.TBESTModelParameters.TimePeriods)
				{

					sTimePeriod = LookupTimePeriod(t);
					mSettings.TBESTModelParameters.TimePeriod = t;
					string intTP = ((int)t + 1).ToString();
					int TPindex = (int)t;

					m_ImpedanceSparseMatrix = new CSparseMatrix();
					m_DecaySparseMatrix = new CSparseMatrix();

					m_ImpedanceSparseMatrix.initMat(ref argnRow, ref argnRow, ref intUseMemory, ref argoutDir, false);
					m_DecaySparseMatrix.initMat(ref argnRow, ref argnRow, ref intUseMemory, ref m_DecayMatrixPath, false);

					if (InitializeModelTextFiles() == false)
						goto cleanup;

					string TransferTempTableName = await StoredProcedures.CreateTransferFilter(mSettings.ModelScenario.ScenarioID, mSettings.TBESTModelParameters.TransferWalkDistance, t, m_SQLConnection, TransfersDisallowedOntheSame.Route);
					string N2TempTableName = await StoredProcedures.CreateNeightborStopFilter(mSettings.ModelScenario.ScenarioID, mSettings.TBESTModelParameters.NeighborDistance, t, m_SQLConnection, NeighborStopsFilter.OntheSameRouteButNotOnSamePattern);
					string N3TempTableName = await StoredProcedures.CreateNeightborStopFilter(mSettings.ModelScenario.ScenarioID, mSettings.TBESTModelParameters.NeighborDistance, t, m_SQLConnection, NeighborStopsFilter.NotontheSameRoute);

					if (await mSettings.ModelScenario.LoadTransitNetwork(argiEnumValues, new List<SystemTimePeriod>() { t }, mSettings.ModelProgressBar, mSettings.ModelStatusBar) == false)
					{
						mSettings.CancelProcess = true;
						goto cleanup;
					}

					if (mSettings.TBESTModelParameters.TransferOption == TransferSettings.PrimaryDirection)
					{
						NetworkAccessibleFunctions.FilterTransfersforDirectionOnly(mSettings.ModelScenario, t);
					}

					NetworkAccessibleFunctions.CleanTransfers(mSettings.ModelScenario, t);
					if (mSettings.TBESTModelParameters.WriteTempTableRecords)
						await WriteTransfers(t);

					if (mSettings.ModelProgressBar is not null)
					{
						mSettings.ModelProgressBar.Visible = true;
						mSettings.ModelProgressBar.Minimum = 0;
						mSettings.ModelProgressBar.Value = 0;
						mSettings.ModelProgressBar.Maximum = mSettings.ModelScenario.StopIDList.GetLength(0);
						mSettings.ModelStatusBar.Text = "Calculating Accessibility (" + sTimePeriod + ")...";
					}

					foreach (TransitPattern pRoute in mSettings.ModelScenario.RoutePatterns.List)
					{
						foreach (TransitStop pTransitStop in pRoute.PatternTransitStops.List)
						{
							mSettings.ModelProgressBar?.Increment(1);

							ModelAccessiblity(pRoute, pTransitStop, t);

							Application.DoEvents();
							if (mSettings.CancelProcess)
								goto cleanup;
						}
					}

					if (mSettings.ModelStatusBar is not null)
						mSettings.ModelStatusBar.Text = "Summarizing Accessibility (" + sTimePeriod + ")...";

					Application.DoEvents();
					if (m_ImpedanceSparseMatrix.SmallMatrix == false)
					{
						m_ImpedanceSparseMatrix.CommitAll();
						m_ImpedanceSparseMatrix.EnableSpeedyReadMode();
					}
					if (m_DecaySparseMatrix.SmallMatrix == false)
					{
						m_DecaySparseMatrix.CommitAll();
						m_DecaySparseMatrix.EnableSpeedyReadMode();
					}

					InitiateModel.mParcelOvaluesTotalList.Clear();
					InitiateModel.mParcelItemsTotalList.Clear();
					Application.DoEvents();
					ParcelItem pParcelItem;
					foreach (TransitPattern pRoute in mSettings.ModelScenario.RoutePatterns.List)
					{
						foreach (TransitStop pTransitStop in pRoute.PatternTransitStops.List)
						{
							InitiateModel.mParcelOvaluesTotalList.Add([0f, 0f, 0f, 0f, 0f, 0f, 0f]);
							pParcelItem = new ParcelItem()
							{
								AttractionTrips = pTransitStop.ParcelItems.ReturnTotal(ParcelVariable.Trips),
								Population = pTransitStop.ParcelItems.ReturnTotal(ParcelVariable.TotalPopulation),
								DwellingUnits = pTransitStop.ParcelItems.ReturnTotal(ParcelVariable.DwellingUnits),
								StructSqFeet = pTransitStop.ParcelItems.ReturnTotal(ParcelVariable.BuildingSQFT),
								LandArea = (long)Math.Round(pTransitStop.ParcelItems.ReturnTotal(ParcelVariable.LandArea)),
								Trips = pTransitStop.ParcelItems.TotalTrips,
								ProductionTrips = pTransitStop.ParcelItems.ReturnTotal(ParcelVariable.ProductionTrips)
							};

							InitiateModel.mParcelItemsTotalList.Add(pParcelItem);
							pTransitStop.ParcelItems.List.Clear();
						}
					}
					Application.DoEvents();
					GC.Collect();

					if (mSettings.ModelProgressBar is not null)
						mSettings.ModelProgressBar.Value = 0;

					if (mSettings.ModelScenario.LocationCount == 0) { await mSettings.ModelScenario.SetLocationCountAsync(); }
					Application.DoEvents();

					foreach (TransitPattern pRoute in mSettings.ModelScenario.RoutePatterns.List)
					{
						foreach (TransitStop pTransitStop in pRoute.PatternTransitStops.List)
						{
							mSettings.ModelProgressBar?.Increment(1);

							m_O2OveralappingSE2 = null;
							m_O2NonOveralappingSE2 = null;

							SetLocationAccessiblityArray(pTransitStop, t);

							if (intLocationArrayO2 is not null)
								await BuildAccessibleStopsO2(t, pTransitStop);
							if (intLocationArrayO4 is not null)
							{
								await BuildAccessibleStopsO4(t, pTransitStop);
								await BuildAccessibleStopsO6(t, pTransitStop);
							}

							if (intLocationArrayOriginMarketOverlap is not null)
								await BuildOverlapDestinationsAccessibleStops(t, pTransitStop);

							InitiateModel.mParcelOvaluesTotalList[pTransitStop.StopIndex - 1] = pTransitStop.TimePeriods.List[TPindex].ParcelOvalues.ReturnParcelTripTotals();

							if (pTransitStop.TimePeriods.List[TPindex].ParcelOvalues is not null && pTransitStop.TimePeriods.List[TPindex].ParcelOvalues.List.Count > 0)
							{
								pTransitStop.TimePeriods.List[TPindex].ParcelOvalues.List.Clear();
							}
							if (pTransitStop.TimePeriods.List[TPindex].NeighboringStops is not null && pTransitStop.TimePeriods.List[TPindex].NeighboringStops.List.Count > 0)
							{
								pTransitStop.TimePeriods.List[TPindex].NeighboringStops.List.Clear();
							}

							pTransitStop.TimePeriods.List[TPindex].NeighboringStops = null;
							pTransitStop.TimePeriods.List[TPindex].ParcelOvalues = null;
							Application.DoEvents();
							if (mSettings.CancelProcess)
								goto cleanup;
						}
					}

					TransitRoute pRouteGroup;

					await m_objOvalueTextStream.FlushAsync();
					await m_objParcelOvalueTextStream.FlushAsync();
					await m_objParcelGroupOvalueTextStream.FlushAsync();
					await m_objTransfers.FlushAsync();

					oWrite = OpenStream(mSettings.ModelScenario.ScenarioPath + @"\Log\DirectBoardings.csv", false, false, true);

					foreach (TransitPattern pEstRoute in mSettings.ModelScenario.RoutePatterns.List)
					{
						if (mSettings.CancelProcess)
						{
							oWrite.Close();
							goto cleanup;
						}
						pRouteGroup = mSettings.ModelScenario.Routes.List[pEstRoute.ParentRouteIndex];

						if ((pRouteGroup.RouteType == TransitRouteType.BRT | pRouteGroup.RouteType == TransitRouteType.CirculatorBRT) & pRouteGroup.BRTAdjustmentFactor == 0f)
						{
							var pBRTAdjustment = new BRTRouteAdjustment();
							await pBRTAdjustment.CreateAsync(mSettings.ModelScenario, pRouteGroup.RouteID);
							pBRTAdjustment.CalculateBRTAdjustment();
							pBRTAdjustment.CalculateValidatedBRTAdjustment();
							pRouteGroup.BRTAdjustmentFactor = 1f + (pBRTAdjustment.TotalAdjusment - pBRTAdjustment.TotalValidatedAdjusment);

						}

						if (await TBESTEquations.CalculateBoardings(pEstRoute, t, oWrite, mSettings.DirectBoardingsCoefficients, true, pRouteGroup, mSettings.TBESTModelParameters, mSettings.ModelScenario) == false)
						{
							MessageBox.Show("Unable to execute the " + mSettings.ModelName + " equation.  Please make sure the model is correctly referenced and compiled.", "TBEST Model Run Canceled...", MessageBoxButtons.OK, MessageBoxIcon.Information);
							mSettings.CancelProcess = true;
							oWrite.Close();
							goto cleanup;
						}
					}
					oWrite.Close();

					if (m_ImpedanceSparseMatrix.SmallMatrix == false)
					{
						object argboolAccess = true;
						m_ImpedanceSparseMatrix.SetColAccess(ref argboolAccess);
					}
					if (m_DecaySparseMatrix.SmallMatrix == false)
					{
						object argboolAccess1 = true;
						m_DecaySparseMatrix.SetColAccess(ref argboolAccess1);
					}

					oWrite = OpenStream(mSettings.ModelScenario.ScenarioPath + @"\Log\TransferBoardings.csv", false, false, true);

					if (mSettings.ModelProgressBar is not null)
						mSettings.ModelProgressBar.Value = 0;

					foreach (TransitPattern pRoute in mSettings.ModelScenario.RoutePatterns.List)
					{
						foreach (TransitStop pTransitStop in pRoute.PatternTransitStops.List)
						{
							mSettings.ModelProgressBar?.Increment(1);

							await BuildIncomingAccessibleStops(t, pTransitStop);

							Application.DoEvents();
							if (mSettings.CancelProcess)
							{
								oWrite.Close();
								goto cleanup;
							}
						}
						pRouteGroup = mSettings.ModelScenario.Routes.List[pRoute.ParentRouteIndex];

						if (await TBESTEquations.CalculateBoardings(pRoute, t, oWrite, mSettings.TransferBoardingsCoefficients, false, pRouteGroup, mSettings.TBESTModelParameters, mSettings.ModelScenario) == false)
						{
							MessageBox.Show("Unable to execute the " + mSettings.ModelName + " equation.  Please make sure the model is correctly referenced and compiled.", "TBEST Model Run Canceled...", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							mSettings.CancelProcess = true;
							oWrite.Close();
							goto cleanup;
						}
					}

					oWrite.Close();

					await ExecuteQuery(m_SQLConnection, "DELETE FROM ANALYSIS_BOARDINGS WHERE TIMEPERIOD = " + intTP + " AND SCEN_ID = " + mSettings.ModelScenario.ScenarioID);
					await ExecuteQuery(m_SQLConnection, "DELETE FROM ANALYSIS_PERFORMANCE WHERE TIMEPERIOD = " + intTP + " AND SCEN_ID = " + mSettings.ModelScenario.ScenarioID);

					if (await TBESTEquations.PopulateBoardings(t, m_objBoardingsTS, mSettings.ModelScenario, mSettings.TBESTModelParameters) == false)
					{
						MessageBox.Show("TBEST was unable to populate estimated boardings information.", "TBEST Model Run", MessageBoxButtons.OK, MessageBoxIcon.Information);
						mSettings.CancelProcess = true;
						goto cleanup;
					}

					foreach (TransitPattern pRoute in mSettings.ModelScenario.RoutePatterns.List)
					{
						foreach (TransitStop pTransitStop in pRoute.PatternTransitStops.List)
						{
							pTransitStop.TimePeriods.List[TPindex].NeighboringStops = null;
							pTransitStop.TimePeriods.List[TPindex].OValues = null;
							pTransitStop.TimePeriods.List[TPindex].TransferStops = null;
							pTransitStop.TimePeriods.List[TPindex].IncomingTransferStops = null;
							pTransitStop.TimePeriods.List[TPindex].AccessibleStops = null;
							pTransitStop.TimePeriods.List[TPindex].ParcelOvalues = null;
						}
					}

					m_ImpedanceSparseMatrix.delMat();
					m_ImpedanceSparseMatrix = null;
					m_DecaySparseMatrix.delMat();
					m_DecaySparseMatrix = null;

					m_objBoardingsTS.Close();

					await BulkInsert(m_SQLConnection, "ANALYSIS_BOARDINGS", mSettings.ModelScenario.LocalEditPath + @"\ANALYSIS_BOARDINGS.txt", false, ",");

					await ExecuteQuery(m_SQLConnection, "DELETE FROM ANALYSIS_BOARDINGS WHERE SCEN_ID = " + mSettings.ModelScenario.OriginalScenarioID + " AND TIMEPERIOD = " + intTP);

					await CopyDatabaseRecords("ANALYSIS_BOARDINGS", mSettings.ModelScenario.ScenarioID, mSettings.ModelScenario.OriginalScenarioID, new SQLiteCommand(m_SQLConnection), " AND TIMEPERIOD = " + intTP);

					File.Delete(mSettings.ModelScenario.LocalEditPath + @"\ANALYSIS_BOARDINGS.txt");

					m_objOvalueTextStream.Close();
					m_objParcelOvalueTextStream.Close();
					m_objParcelGroupOvalueTextStream.Close();
					m_objTransfers.Close();

					if (mSettings.TBESTModelParameters.WriteTempTableRecords)
					{

						await BulkInsert(m_SQLConnection, "O_VALUES", mSettings.ModelScenario.LocalEditPath + @"\O_VALUES.txt", false, ",");
						await BulkInsert(m_SQLConnection, "TRANSFERS", mSettings.ModelScenario.LocalEditPath + @"\TRANSFERS.txt", false, ",");
						await BulkInsert(m_SQLConnection, "PARCEL_OVALUES", mSettings.ModelScenario.LocalEditPath + @"\PARCEL_GROUP_OVALUES.txt", false, ",");

						await ExecuteQuery(m_SQLConnection, "DELETE FROM O_VALUES WHERE SCEN_ID = " + mSettings.ModelScenario.OriginalScenarioID + " AND TIMEPERIOD = " + intTP);
						await ExecuteQuery(m_SQLConnection, "DELETE FROM TRANSFERS WHERE SCEN_ID = " + mSettings.ModelScenario.OriginalScenarioID + " AND TIMEPERIOD = " + intTP);
						await ExecuteQuery(m_SQLConnection, "DELETE FROM PARCEL_OVALUES WHERE SCEN_ID = " + mSettings.ModelScenario.OriginalScenarioID + " AND TIMEPERIOD = " + intTP);
						await CopyDatabaseRecords("O_VALUES", mSettings.ModelScenario.ScenarioID, mSettings.ModelScenario.OriginalScenarioID, new SQLiteCommand(m_SQLConnection), " AND TIMEPERIOD = " + intTP);
						await CopyDatabaseRecords("TRANSFERS", mSettings.ModelScenario.ScenarioID, mSettings.ModelScenario.OriginalScenarioID, new SQLiteCommand(m_SQLConnection), " AND TIMEPERIOD = " + intTP);
						await CopyDatabaseRecords("PARCEL_OVALUES", mSettings.ModelScenario.ScenarioID, mSettings.ModelScenario.OriginalScenarioID, new SQLiteCommand(m_SQLConnection), " AND TIMEPERIOD = " + intTP);


						File.Delete(mSettings.ModelScenario.LocalEditPath + @"\O_VALUES.txt");
						File.Delete(mSettings.ModelScenario.LocalEditPath + @"\TRANSFERS.txt");
						File.Delete(mSettings.ModelScenario.LocalEditPath + @"\PARCEL_OVALUES.txt");

					}

					await mSettings.ModelScenario.BuildRoutePerformanceTable(mSettings.ModelScenario.OriginalScenarioID, t);

					await ExecuteQuery(m_SQLConnection, "DELETE FROM EDIT_LOG WHERE EDITTYPE = 'NETWORK' AND TIMEPERIOD = " + intTP + " and SCEN_ID = " + mSettings.ModelScenario.OriginalScenarioID);
					await ExecuteQuery(m_SQLConnection, "DELETE from VALIDRUNS WHERE SCEN_ID in (" + mSettings.ModelScenario.OriginalScenarioID + "," + mSettings.ModelScenario.ScenarioID + ") AND TIMEPERIOD = " + intTP);
					await ExecuteQuery(m_SQLConnection, "Insert into VALIDRUNS (SCEN_ID, TIMEPERIOD, VALIDRUN) VALUES (" + mSettings.ModelScenario.ScenarioID + "," + intTP + ",-1)");
					await ExecuteQuery(m_SQLConnection, "Insert into VALIDRUNS (SCEN_ID, TIMEPERIOD, VALIDRUN) VALUES (" + mSettings.ModelScenario.OriginalScenarioID + "," + intTP + ",-1)");


					if (await DoesTableExist(TransferTempTableName, m_SQLConnection)) await ExecuteQuery(m_SQLConnection, "Drop Table " + TransferTempTableName);
					if (await DoesTableExist(N3TempTableName, m_SQLConnection)) await ExecuteQuery(m_SQLConnection, "Drop Table " + N3TempTableName);
					if (await DoesTableExist(N2TempTableName, m_SQLConnection)) await ExecuteQuery(m_SQLConnection, "Drop Table " + N2TempTableName);


					await mSettings.ModelScenario.ScenarioLog.WriteLineAsync("*** Scenario " + sTimePeriod + " Model Completed :" + Convert.ToString(DateTime.Now));

					mSettings.CancelForm.NotificationUpdate();
					Application.DoEvents();
					GC.Collect();
				}

				await mSettings.ModelScenario.BuildRouteGroupMiles(mSettings.ModelProgressBar, mSettings.ModelStatusBar);

				await ExecuteQuery(m_SQLConnection, "UPDATE SCENARIOS SET [EMP_RUN] = '" + mSettings.ModelScenario.EmploymentType.ToString() + "' WHERE SCEN_ID = " + mSettings.ModelScenario.OriginalScenarioID);
				await ExecuteQuery(m_SQLConnection, "DELETE FROM STOP_DISTANCE WHERE SCEN_ID = " + mSettings.ModelScenario.ScenarioID);

				await mSettings.ModelScenario.ScenarioLog.WriteLineAsync("*** Scenario Model Run Complete:" + Convert.ToString(DateTime.Now));
				mSettings.CancelForm.NotificationUpdate();

			cleanup:
				;
			}

			catch (Exception ex)
			{
				mSettings.CancelForm.NotificationError();
				MessageBox.Show("Error Running the TBEST Ridership Estimation Model. To view error details, navigate to the TBEST log file at File->Settings->TBEST Log.", "TBEST Model Run", MessageBoxButtons.OK, MessageBoxIcon.Information);
				ErrorHandling.ErrHandlerMsg(ex);
				mSettings.CancelProcess = true;
				RunTBESTModelRet = false;
			}
			finally
			{

				if (mSettings.ModelProgressBar is not null)
					mSettings.ModelProgressBar.Visible = false;

				if (mSettings.CancelProcess)
				{
					await mSettings.ModelScenario.ScenarioLog.WriteLineAsync("*** Scenario Model Run Canceled:" + Convert.ToString(DateTime.Now));
					mSettings.ModelScenario.ScenarioID = 0;
					mSettings.CancelForm.NotificationCancel();
					m_objOvalueTextStream?.Close();
					m_objBoardingsTS?.Close();
					m_objTransfers?.Close();
					m_objParcelOvalueTextStream?.Close();
					m_objParcelGroupOvalueTextStream?.Close();
				}

				await ExecuteQuery(m_SQLConnection, "Delete from Validation");
				if (m_SQLConnection.State == ConnectionState.Open)
				{
					m_SQLConnection.Close();
				}
				mSettings.ModelScenario.ScenarioLog.Close();
				m_objBoardingsTS = null;
				m_objOvalueTextStream = null;
				m_objTransfers = null;
				RunTBESTModelRet = !mSettings.CancelProcess;
			}

			return RunTBESTModelRet;

		}

		private async Task WriteTransfers(SystemTimePeriod bytTimePeriod)
		{
			string strOriginID;
			TransferStops pOriginTransferstops;
			int x;
			TransferStop pOriginTransferStop;
			int xCount;
			string intTP = ((int)bytTimePeriod + 1).ToString();
			int TPindex = (int)bytTimePeriod;
			foreach (TransitPattern pRoute in mSettings.ModelScenario.RoutePatterns.List)
			{
				foreach (TransitStop pTransitStop in pRoute.PatternTransitStops.List)
				{
					strOriginID = pTransitStop.StopID.ToString();
					pOriginTransferstops = pTransitStop.TimePeriods.List[TPindex].TransferStops;
					if (pOriginTransferstops is not null)
					{
						xCount = pOriginTransferstops.List.Count - 1;
						for (x = xCount; x >= 0; x--)
						{
							pOriginTransferStop = pOriginTransferstops.List[x];
							await m_objTransfers.WriteLineAsync(mSettings.ModelScenario.ScenarioID.ToString() + "," + strOriginID + "," + pOriginTransferStop.StopID.ToString() + "," + intTP);
						}
					}
				}
			}
		}
	}

	public static class ProcessStops
	{
		public static TransitRoutes mRouteGroups;
		public static CSparseMatrix m_ImpedanceSparseMatrix;
		public static CSparseMatrix m_DecaySparseMatrix;
		public static FareLookup mFareLookup;

		internal class ProcessStopAccessiblity
		{

			public void ProcessStop(ModelStopProcess pModelProcessStop, TBESTScenario pScenario, ModelParameters pScenarioModelParameters)
			{

				int x;
				ModelStopProcess pModelStopProcess_1;
				ProcessStopAccessiblity pstopa;
				short intFWT;
				float dTransferWaitTime;
				double dTransferFareImpedance;
				float dTravelTime;
				short dTransferIMpedance;
				StopNetworkTimePeriodAttribute pProcessAtts;
				StopNetworkTimePeriodAttribute pTransferAtts;
				TransitPattern pRouteinLoop;
				TransitStop pGetAttTransfer;
				short intRouteHeadway;

				TransitStop pProcessstop = null;
				byte bytCellValue;

				int TPIndex = (int)pScenarioModelParameters.TimePeriod;
				TransitRoute pRouteGroup = mRouteGroups.List[pModelProcessStop.CurrentRouteGroupIndex];
				TransitRouteType pRouteType = pRouteGroup.RouteType;
				TransitModeType pTechType = pRouteGroup.TechnologyType;

				TransferStops pTransferSTops;
				x = pModelProcessStop.MinStop;

				byte bytTransferStopImpedance;
				while (x <= pModelProcessStop.MaxStop)
				{
					pProcessstop = pModelProcessStop.RoutePattern.PatternTransitStops.List[x];

					bytCellValue = m_ImpedanceSparseMatrix.get_Cell(pModelProcessStop.OriginIndex, pProcessstop.StopIndex);

					if (bytCellValue != 0 & bytCellValue <= pModelProcessStop.Impedance)
					{
						return;
					}

					pProcessAtts = pProcessstop.TimePeriods.List[TPIndex];
					if (pProcessAtts.TravelTime < 0f)
						pProcessAtts.TravelTime = 0f;

					dTravelTime = pProcessAtts.TravelTime;

					if (pModelProcessStop.Impedance + dTravelTime > pScenarioModelParameters.MaxImpedance)
					{
						pModelProcessStop = null;
						return;
					}

					pModelProcessStop.Impedance += dTravelTime;
					pModelProcessStop.TripLengthSummary.AddMileage(pTechType, pRouteType, dTravelTime);

					if (pProcessAtts.Frequency > 0 & pProcessstop.StopIndex != 0)
					{

						if (bytCellValue == 0 | bytCellValue > pModelProcessStop.Impedance)
						{
							if (pModelProcessStop.TransferArray is not null)
							{
								pModelProcessStop.TransferArray[pProcessstop.StopIndex] = pModelProcessStop.TransferCount;
							}

							m_ImpedanceSparseMatrix.set_Cell(pModelProcessStop.OriginIndex, pProcessstop.StopIndex, (byte)Math.Round(pModelProcessStop.Impedance));
							m_DecaySparseMatrix.set_Cell(pModelProcessStop.OriginIndex, pProcessstop.StopIndex, (byte)pModelProcessStop.TripLengthSummary.ReturnPrimaryModeforPath());

						}

						if (string.IsNullOrEmpty(pProcessstop.TimePoint) & pProcessstop.Interlined == false & (pScenarioModelParameters.TransferOption == TransferSettings.StationsOnly | pScenarioModelParameters.TransferOption == TransferSettings.StationsandTimepoints))
						{
							x++;
							continue;
						}

						if (pScenarioModelParameters.MaxTransfers == pModelProcessStop.TransferCount & pProcessstop.Interlined == false)
						{
							x++;
							continue;
						}

						if (pModelProcessStop.TransferCount <= pScenarioModelParameters.MaxTransfers)
						{
							pTransferSTops = pProcessAtts.TransferStops;
							if (pTransferSTops is not null)
							{
								foreach (TransferStop pTransferStop in pProcessAtts.TransferStops.List)
								{
									pRouteinLoop = pScenario.RoutePatterns.List[pTransferStop.RouteItem];
									pGetAttTransfer = pRouteinLoop.PatternTransitStops.List[pTransferStop.RouteIndex];
									pTransferAtts = pGetAttTransfer.TimePeriods.List[TPIndex];

									if (pTransferAtts.Frequency > 0 & pRouteinLoop.ParentTransitRouteID != pModelProcessStop.OriginRouteGroup)
									{
										if (pProcessstop.Interlined & pGetAttTransfer.Interlined)
										{
											if (NetworkAccessibleFunctions.AreStopsInterlined(pScenario, pProcessstop.StopID, pTransferStop.StopID))
											{
												pstopa = new ProcessStopAccessiblity();
												pModelStopProcess_1 = new ModelStopProcess()
												{
													Impedance = pModelProcessStop.Impedance,
													OriginIndex = pModelProcessStop.OriginIndex,
													RoutePattern = pRouteinLoop,
													TransitStop = pGetAttTransfer,
													TransferCount = pModelProcessStop.TransferCount,
													MinStop = pGetAttTransfer.RouteIndex + 1,
													MaxStop = pRouteinLoop.PatternTransitStops.List.Count - 1,
													TransferArray = pModelProcessStop.TransferArray,
													OriginRouteGroup = pModelProcessStop.OriginRouteGroup,
													CurrentRouteGroupIndex = pRouteinLoop.ParentRouteIndex,
													TripLengthSummary = (ModeTripLength)pModelProcessStop.TripLengthSummary.CloneItem()
												};

												pstopa.ProcessStop(pModelStopProcess_1, pScenario, pScenarioModelParameters);
											}
										}
										else
										{
											if (pTransferStop.Timed == false)
											{
												if (pScenarioModelParameters.TransferOption == TransferSettings.StationsOnly | string.IsNullOrEmpty(pGetAttTransfer.TimePoint) & pScenarioModelParameters.TransferOption == TransferSettings.StationsandTimepoints)
													continue;
												dTransferWaitTime = 30f;

												intRouteHeadway = Convert.ToInt16(pScenario.Routes.List[pRouteinLoop.ParentRouteIndex].Get_DirHeadway(pRouteinLoop.DirectionCode, pScenarioModelParameters.TimePeriod));
												dTransferWaitTime = (float)(intRouteHeadway * 0.5d);

											}
											else
											{
												dTransferWaitTime = pScenarioModelParameters.TimedTransferWaitTime;
											}

											dTransferWaitTime *= pScenarioModelParameters.TransferWaitWeight;

											intFWT = (short)Math.Round(pScenarioModelParameters.CalcWalkTime(pTransferStop.Distance));

											var pTransferRouteGroup = mRouteGroups.List[pRouteinLoop.ParentRouteIndex];

											dTransferFareImpedance = mFareLookup.GetFareImpedance(pTransferRouteGroup, false);
											if ((double)pModelProcessStop.Impedance + dTransferFareImpedance + (double)dTransferWaitTime + intFWT + pScenarioModelParameters.TransferPenaltyWeight > pScenarioModelParameters.MaxImpedance)
											{
												dTransferIMpedance = pScenarioModelParameters.MaxImpedance;
											}
											else
											{
												dTransferIMpedance = (short)Math.Round((double)pModelProcessStop.Impedance + dTransferFareImpedance + (double)dTransferWaitTime + intFWT + pScenarioModelParameters.TransferPenaltyWeight);
											}

											if (dTransferIMpedance < pScenarioModelParameters.MaxImpedance & pModelProcessStop.TransferCount + 1 <= pScenarioModelParameters.MaxTransfers)
											{

												bytTransferStopImpedance = m_ImpedanceSparseMatrix.get_Cell(pModelProcessStop.OriginIndex, pTransferStop.StopIndex);

												if (bytTransferStopImpedance > dTransferIMpedance | bytTransferStopImpedance == 0)
												{
													m_ImpedanceSparseMatrix.set_Cell(pModelProcessStop.OriginIndex, pTransferStop.StopIndex, (byte)dTransferIMpedance);
													m_DecaySparseMatrix.set_Cell(pModelProcessStop.OriginIndex, pTransferStop.StopIndex, (byte)pModelProcessStop.TripLengthSummary.ReturnPrimaryModeforPath());

													if (pModelProcessStop.TransferArray is not null)
													{
														pModelProcessStop.TransferArray[pTransferStop.StopIndex] = (byte)(pModelProcessStop.TransferCount + 1);
													}

													pstopa = new ProcessStopAccessiblity();

													pModelStopProcess_1 = new ModelStopProcess()
													{
														Impedance = dTransferIMpedance,
														OriginIndex = pModelProcessStop.OriginIndex,
														RoutePattern = pRouteinLoop,
														TransitStop = pGetAttTransfer,
														TransferCount = (byte)(pModelProcessStop.TransferCount + 1),
														MinStop = pGetAttTransfer.RouteIndex + 1,
														MaxStop = pRouteinLoop.PatternTransitStops.List.Count - 1,
														TransferArray = pModelProcessStop.TransferArray,
														OriginRouteGroup = pModelProcessStop.OriginRouteGroup,
														CurrentRouteGroupIndex = pRouteinLoop.ParentRouteIndex,
														TripLengthSummary = (ModeTripLength)pModelProcessStop.TripLengthSummary.CloneItem()

													};
													pstopa.ProcessStop(pModelStopProcess_1, pScenario, pScenarioModelParameters);

												}
											}
										}
									}
								}
							}
						}
					}
					x++;
				}

				if (pProcessstop is not null)
				{
					if ((pRouteGroup.RouteType == TransitRouteType.Circulator | pRouteGroup.RouteType == TransitRouteType.CirculatorBRT) & pProcessstop.RouteIndex != pModelProcessStop.TransitStop.RouteIndex)
					{

						pstopa = new ProcessStopAccessiblity();
						pModelStopProcess_1 = new ModelStopProcess()
						{
							Impedance = pModelProcessStop.Impedance,
							OriginIndex = pModelProcessStop.OriginIndex,
							RoutePattern = pModelProcessStop.RoutePattern,
							TransitStop = pModelProcessStop.RoutePattern.PatternTransitStops.List[0],
							TransferCount = pModelProcessStop.TransferCount,
							MinStop = 0,
							MaxStop = pModelProcessStop.TransitStop.RouteIndex,
							TransferArray = pModelProcessStop.TransferArray,
							OriginRouteGroup = pModelProcessStop.OriginRouteGroup,
							CurrentRouteGroupIndex = pModelProcessStop.CurrentRouteGroupIndex,
							TripLengthSummary = (ModeTripLength)pModelProcessStop.TripLengthSummary.CloneItem()
						};

						pstopa.ProcessStop(pModelStopProcess_1, pScenario, pScenarioModelParameters);
					}
				}
				pProcessstop = null;
			}
		}
	}

	internal class ModelStopProcess
	{
		private TransitStop mStop;
		private int mOriginIndex;
		private TransitPattern mRoute;
		private byte mTransferCount;
		private float mImpedance;
		private int mMinStop;
		private int mMaxStop;
		private byte[] mtransferArray;
		private int mOriginRouteGroup;
		private int mCurrentRouteGroupIndex;
		private int mCurrentLoadHeadway;
		private ModeTripLength mTripLengthSummary;
		public int CurrentOriginaWaitTime
		{
			get { return mCurrentLoadHeadway; }
			set { mCurrentLoadHeadway = value; }
		}

		public ModeTripLength TripLengthSummary
		{
			get { return mTripLengthSummary; }
			set { mTripLengthSummary = value; }
		}

		public int OriginRouteGroup
		{
			get { return mOriginRouteGroup; }
			set { mOriginRouteGroup = value; }
		}

		public int CurrentRouteGroupIndex
		{
			get { return mCurrentRouteGroupIndex; }
			set { mCurrentRouteGroupIndex = value; }
		}

		public byte[] TransferArray
		{
			get { return mtransferArray; }
			set { mtransferArray = value; }
		}

		public int MinStop
		{
			get { return mMinStop; }
			set { mMinStop = value; }
		}

		public int MaxStop
		{
			get { return mMaxStop; }
			set { mMaxStop = value; }
		}

		public float Impedance
		{
			get { return mImpedance; }
			set { mImpedance = value; }
		}

		public byte TransferCount
		{
			get { return mTransferCount; }
			set { mTransferCount = value; }
		}

		public TransitStop TransitStop
		{
			get { return mStop; }
			set { mStop = value; }
		}

		public int OriginIndex
		{
			get { return mOriginIndex; }
			set { mOriginIndex = value; }
		}

		public TransitPattern RoutePattern
		{
			get { return mRoute; }
			set { mRoute = value; }
		}
	}

	public class FareLookup
	{
		private readonly RouteTypeFares mRouteFares;
		private readonly double mDollarsperHour;
		private readonly double mFareGrowthRate;
		private readonly double mWageRate;
		private readonly bool mBoolEnforceFareImpedance;
		public FareLookup(TBESTScenario pScenario, ModelParameters pNetworkParmeters)
		{
			mRouteFares = pScenario.RouteTypeFares;
			short pYearDif = (short)(pScenario.ScenarioYear - pScenario.BaseYear);

			if (pScenario.WageGrowthRate != 0d)
			{
				pScenario.InflationWage = Math.Pow(1d + pScenario.WageGrowthRate / 100d, pYearDif);
			}
			else
			{
				pScenario.InflationWage = 1d;
			}


			pScenario.WageRate = pScenario.InflationWage * pScenario.WageRate;


			if (mRouteFares.InflationRate != 0d)
			{
				mFareGrowthRate = Math.Pow(1d + mRouteFares.InflationRate / 100d, pYearDif);
			}
			else
			{
				mFareGrowthRate = 1d;
			}

			mWageRate = pScenario.WageRate;
			mDollarsperHour = 0.5d * (pScenario.WageRate / 2080d);
			mBoolEnforceFareImpedance = pNetworkParmeters.EnforceFareImpedance;

		}

		public double GetFareImpedance(TransitRoute pRouteGroup, bool boolFirstFare)
		{
			double outImpedance;
			double pFare = 0d;

			if (pRouteGroup.RouteFareException != null)
			{
				pFare = Convert.ToDouble((boolFirstFare ? pRouteGroup.RouteFareException.BaseFare : pRouteGroup.RouteFareException.TransferFare));
			}
			else
			{
				switch (pRouteGroup.TechnologyType)
				{
					case TransitModeType.Ferry:
						{
							pFare = Convert.ToDouble(boolFirstFare ? mRouteFares.GetFirstFare(TransitDecayVariable.Ferry) : mRouteFares.GetTransferFare(TransitDecayVariable.Ferry));
							break;
						}

					case TransitModeType.CommuterRail:
						{
							pFare = Convert.ToDouble(boolFirstFare ? mRouteFares.GetFirstFare(TransitDecayVariable.CommuterRail) : mRouteFares.GetTransferFare(TransitDecayVariable.CommuterRail));
							break;
						}

					case TransitModeType.HeavyRail:
						{
							pFare = Convert.ToDouble(boolFirstFare ? mRouteFares.GetFirstFare(TransitDecayVariable.HeavyRail) : mRouteFares.GetTransferFare(TransitDecayVariable.HeavyRail));
							break;
						}

					case TransitModeType.LightRail:
						{
							pFare = Convert.ToDouble(boolFirstFare ? mRouteFares.GetFirstFare(TransitDecayVariable.LightRail) : mRouteFares.GetTransferFare(TransitDecayVariable.LightRail));
							break;
						}

					case TransitModeType.PeopleMover:
						{
							pFare = Convert.ToDouble(boolFirstFare ? mRouteFares.GetFirstFare(TransitDecayVariable.PeopleMover) : mRouteFares.GetTransferFare(TransitDecayVariable.PeopleMover));
							break;
						}

					case TransitModeType.Streetcar:
						{
							pFare = Convert.ToDouble(boolFirstFare ? mRouteFares.GetFirstFare(TransitDecayVariable.Streetcar) : mRouteFares.GetTransferFare(TransitDecayVariable.Streetcar));
							break;
						}

					case TransitModeType.Other:
						{
							pFare = Convert.ToDouble(boolFirstFare ? mRouteFares.GetFirstFare(TransitDecayVariable.Other) : mRouteFares.GetTransferFare(TransitDecayVariable.Other));
							break;
						}

					case TransitModeType.Bus:
						{
							switch (pRouteGroup.RouteType)
							{
								case TransitRouteType.BRT:
									{
										pFare = Convert.ToDouble(boolFirstFare ? mRouteFares.GetFirstFare(TransitDecayVariable.BRT) : mRouteFares.GetTransferFare(TransitDecayVariable.BRT));
										break;
									}

								case TransitRouteType.Express:
									{
										pFare = Convert.ToDouble(boolFirstFare ? mRouteFares.GetFirstFare(TransitDecayVariable.Express) : mRouteFares.GetTransferFare(TransitDecayVariable.Express));
										break;
									}

								case TransitRouteType.Circulator:
									{
										pFare = Convert.ToDouble(boolFirstFare ? mRouteFares.GetFirstFare(TransitDecayVariable.Circulator) : mRouteFares.GetTransferFare(TransitDecayVariable.Circulator));
										break;
									}

								case TransitRouteType.Crosstown:
									{
										pFare = Convert.ToDouble(boolFirstFare ? mRouteFares.GetFirstFare(TransitDecayVariable.Crosstown) : mRouteFares.GetTransferFare(TransitDecayVariable.Crosstown));
										break;
									}

								case TransitRouteType.Radial:
									{
										pFare = Convert.ToDouble(boolFirstFare ? mRouteFares.GetFirstFare(TransitDecayVariable.Radial) : mRouteFares.GetTransferFare(TransitDecayVariable.Radial));
										break;
									}

								case TransitRouteType.Rapid:
									{
										pFare = Convert.ToDouble(boolFirstFare ? mRouteFares.GetFirstFare(TransitDecayVariable.Rapid) : mRouteFares.GetTransferFare(TransitDecayVariable.Rapid));
										break;
									}

								case TransitRouteType.Ski:
									{
										pFare = Convert.ToDouble(boolFirstFare ? mRouteFares.GetFirstFare(TransitDecayVariable.Ski) : mRouteFares.GetTransferFare(TransitDecayVariable.Ski));
										break;
									}

								case TransitRouteType.Flex:
									{
										pFare = Convert.ToDouble(boolFirstFare ? mRouteFares.GetFirstFare(TransitDecayVariable.Flex) : mRouteFares.GetTransferFare(TransitDecayVariable.Flex));
										break;
									}

								case TransitRouteType.CommunityConnector:
									{
										pFare = Convert.ToDouble(boolFirstFare ? mRouteFares.GetFirstFare(TransitDecayVariable.CommunityConnector) : mRouteFares.GetTransferFare(TransitDecayVariable.CommunityConnector));
										break;
									}

								case TransitRouteType.CirculatorBRT:
									{
										pFare = Convert.ToDouble(boolFirstFare ? mRouteFares.GetFirstFare(TransitDecayVariable.CirculatorBRT) : mRouteFares.GetTransferFare(TransitDecayVariable.CirculatorBRT));
										break;
									}
							}

							break;
						}
				}

			}


			pFare *= mFareGrowthRate;
			if (pFare == 0d | mWageRate == 0d | mBoolEnforceFareImpedance == false)
			{
				outImpedance = 0d;
			}
			else
			{
				outImpedance = pFare / (mDollarsperHour / 60d);
			}

			return outImpedance;
		}

	}

	public class ModeTripLength
	{
		private float mCommuterRail = 0f;
		private float mHeavyRail = 0f;
		private float mLightRail = 0f;
		private float mExpress = 0f;
		private float mBRT = 0f;
		private float mRadial = 0f;
		private float mOther = 0f;
		private float mStreetCar = 0f;
		private float mPeopleMover = 0f;
		private float mRapid = 0f;
		private float mCrosstown = 0f;
		private float mCirculator = 0f;
		private float mFerry = 0f;
		private float mCirculatorBRT = 0f;
		private float mFlex = 0f;
		private float mCommunity = 0f;
		private float mSki = 0f;
		public object CloneItem()
		{
			return (ModeTripLength)MemberwiseClone();
		}
		public TransitDecayVariable ReturnPrimaryModeforPath()
		{

			float mTotalMiles = mCirculatorBRT + mFlex + mCommunity + mSki + mCrosstown + mCirculator + mRadial + mBRT + mExpress + mLightRail + mHeavyRail + mCommuterRail + mOther + mStreetCar + mPeopleMover + mRapid + mFerry;
			float mRatio = 0f;
			var pReturnVariable = default(TransitDecayVariable);

			if (mHeavyRail / mTotalMiles > mRatio)
			{
				pReturnVariable = TransitDecayVariable.HeavyRail;
				mRatio = mHeavyRail / mTotalMiles;
			}
			if (mCommuterRail / mTotalMiles > mRatio)
			{
				pReturnVariable = TransitDecayVariable.CommuterRail;
				mRatio = mCommuterRail / mTotalMiles;
			}
			if (mRadial / mTotalMiles > mRatio)
			{
				pReturnVariable = TransitDecayVariable.Radial;
				mRatio = mRadial / mTotalMiles;
			}
			if (mBRT / mTotalMiles > mRatio)
			{
				pReturnVariable = TransitDecayVariable.BRT;
				mRatio = mBRT / mTotalMiles;
			}
			if (mExpress / mTotalMiles > mRatio)
			{
				pReturnVariable = TransitDecayVariable.Express;
				mRatio = mExpress / mTotalMiles;
			}
			if (mLightRail / mTotalMiles > mRatio)
			{
				pReturnVariable = TransitDecayVariable.LightRail;
				mRatio = mLightRail / mTotalMiles;
			}
			if (mStreetCar / mTotalMiles > mRatio)
			{
				pReturnVariable = TransitDecayVariable.Streetcar;
				mRatio = mStreetCar / mTotalMiles;
			}
			if (mPeopleMover / mTotalMiles > mRatio)
			{
				pReturnVariable = TransitDecayVariable.PeopleMover;
				mRatio = mPeopleMover / mTotalMiles;
			}
			if (mOther / mTotalMiles > mRatio)
			{
				pReturnVariable = TransitDecayVariable.Other;
				mRatio = mOther / mTotalMiles;
			}
			if (mFerry / mTotalMiles > mRatio)
			{
				pReturnVariable = TransitDecayVariable.Ferry;
				mRatio = mFerry / mTotalMiles;
			}

			if (mRapid / mTotalMiles > mRatio)
			{
				pReturnVariable = TransitDecayVariable.Rapid;
				mRatio = mRapid / mTotalMiles;
			}
			if (mCrosstown / mTotalMiles > mRatio)
			{
				pReturnVariable = TransitDecayVariable.Crosstown;
				mRatio = mCrosstown / mTotalMiles;
			}
			if (mCirculatorBRT / mTotalMiles > mRatio)
			{
				pReturnVariable = TransitDecayVariable.CirculatorBRT;
				mRatio = mCirculatorBRT / mTotalMiles;
			}
			if (mCirculator / mTotalMiles > mRatio)
			{
				pReturnVariable = TransitDecayVariable.Circulator;
				mRatio = mCirculator / mTotalMiles;
			}
			if (mCommunity / mTotalMiles > mRatio)
			{
				pReturnVariable = TransitDecayVariable.CommunityConnector;
				mRatio = mCommunity / mTotalMiles;
			}
			if (mFlex / mTotalMiles > mRatio)
			{
				pReturnVariable = TransitDecayVariable.Flex;
				mRatio = mFlex / mTotalMiles;
			}
			if (mSki / mTotalMiles > mRatio)
			{
				pReturnVariable = TransitDecayVariable.Ski;
				mRatio = mSki / mTotalMiles;
			}
			if (pReturnVariable == 0)
				pReturnVariable = TransitDecayVariable.Radial;


			return pReturnVariable;

		}
		public void AddMileage(TransitModeType modetype, TransitRouteType routetype, float miles)
		{
			switch (modetype)
			{
				case TransitModeType.CommuterRail:
					{
						mCommuterRail += miles;
						break;
					}
				case TransitModeType.HeavyRail:
					{
						mHeavyRail += miles;
						break;
					}
				case TransitModeType.LightRail:
					{
						mLightRail += miles;
						break;
					}
				case TransitModeType.PeopleMover:
					{
						mPeopleMover += miles;
						break;
					}
				case TransitModeType.Streetcar:
					{
						mStreetCar += miles;
						break;
					}
				case TransitModeType.Other:
					{
						mOther += miles;
						break;
					}
				case TransitModeType.Ferry:
					{
						mFerry += miles;
						break;
					}
				case TransitModeType.Bus:
					{
						switch (routetype)
						{
							case TransitRouteType.BRT:
								{
									mBRT += miles;
									break;
								}
							case TransitRouteType.Express:
								{
									mExpress += miles;
									break;
								}
							case TransitRouteType.Circulator:
								{
									mCirculator += miles;
									break;
								}
							case TransitRouteType.Crosstown:
								{
									mCrosstown += miles;
									break;
								}
							case TransitRouteType.Radial:
								{
									mRadial += miles;
									break;
								}
							case TransitRouteType.Rapid:
								{
									mRapid += miles;
									break;
								}
							case TransitRouteType.CommunityConnector:
								{
									mCommunity += miles;
									break;
								}
							case TransitRouteType.Flex:
								{
									mFlex += miles;
									break;
								}
							case TransitRouteType.Ski:
								{
									mSki += miles;
									break;
								}
							case TransitRouteType.CirculatorBRT:
								{
									mCirculatorBRT += miles;
									break;
								}

						}

						break;
					}
			}
		}
	}

	internal class OValueSEDataSummary
	{
		private float mPopuation = 0f;
		private float mIEMP = 0f;
		private float mCEMP = 0f;
		private float mSEMP = 0f;
		private short mParkingSpaces = 0;
		private float mParkingPopulation = 0f;
		private float mParkingEmployment = 0f;
		public OValueSEDataSummary() : base()
		{
			mPopuation = 0f;
			mIEMP = 0f;
			mCEMP = 0f;
			mSEMP = 0f;
			mParkingSpaces = 0;
			mParkingPopulation = 0f;
			mParkingEmployment = 0f;
		}

		public void AddSummaryForStop(TransitStop pTransitStop, int intTrips, float sngExpImpedance, StopPopEmpValues pPopEmp)
		{
			Population += ((sngExpImpedance * pPopEmp.TotalPopulation) * intTrips);
			IndustrialEmployment += ((sngExpImpedance * pPopEmp.IndustrialEmployment) * intTrips);
			CommercialEmployment += ((sngExpImpedance * pPopEmp.CommercialEmployment) * intTrips);
			ServiceEmployment += ((sngExpImpedance * pPopEmp.ServiceEmployment) * intTrips);

			ParkingSpacesAvailable += (short)(sngExpImpedance * Convert.ToSingle(pTransitStop.ParknRideSpaces));
			ParkingPopulation += sngExpImpedance * pTransitStop.ParknRidePopulationMarket;
			ParkingEmployment += sngExpImpedance * pTransitStop.ParknRideEmployeeMarket;
		}
		public float Population
		{
			get { return mPopuation; }
			set { mPopuation = value; }
		}
		public float IndustrialEmployment
		{
			get { return mIEMP; }
			set { mIEMP = value; }
		}

		public float CommercialEmployment
		{
			get { return mCEMP; }
			set { mCEMP = value; }
		}

		public float ServiceEmployment
		{
			get { return mSEMP; }
			set { mSEMP = value; }
		}

		public short ParkingSpacesAvailable
		{
			get { return mParkingSpaces; }
			set { mParkingSpaces = value; }
		}

		public float ParkingEmployment
		{
			get { return mParkingPopulation; }
			set { mParkingPopulation = value; }
		}

		public float ParkingPopulation
		{
			get { return mParkingEmployment; }
			set { mParkingEmployment = value; }
		}
	}
}