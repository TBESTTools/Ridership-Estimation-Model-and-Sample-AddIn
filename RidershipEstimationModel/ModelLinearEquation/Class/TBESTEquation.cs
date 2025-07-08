using System;
using System.Collections.Generic;
using TBESTFramework.Management;
using TBESTFramework.RidershipForecasting.ModelEquationSettings;
using TBESTFramework.Utilities;
using TBESTFramework.Data.Network;
using TBESTFramework.Data.Network.Properties;
using TBESTFramework.App;
using TBESTFramework.RidershipForecasting;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace TBESTModelEquation
{

	public class TBESTEquations : MarshalByRefObject
	{
		public static async Task<bool> CalculateBoardings(TransitPattern pRoute, SystemTimePeriod iTimePeriod, System.IO.StreamWriter oWrite, CoefficientList pcoefflist, bool boolDirect, TransitRoute pRouteGroup, ModelParameters pModelParameters, TBESTScenario pScenario)
		{
			bool CalculateBoardingsRet = true;

			try
			{
				string intTP = ((int)iTimePeriod + 1).ToString();
				int TPindex = (int)iTimePeriod;
				var pTBESTEquation = new TBESTModelEquationHelper();

				pTBESTEquation.HelperStreamWriter = oWrite;
				pTBESTEquation.Coefficients = pcoefflist;
				pTBESTEquation.TimePeriod = (int)iTimePeriod;
				pTBESTEquation.RouteGroup = pRouteGroup;

				pTBESTEquation.PerArrivalEstimation = false;
				pTBESTEquation.IsDirectEquation = boolDirect;

				if (pTBESTEquation.Coefficients.ReturnCoefficient("BUS-PERARRIVALEST").CoefficientTimePeriods.List[TPindex].CoeffValue == 1d)
					pTBESTEquation.PerArrivalEstimation = true;

				StopNetworkTimePeriodAttribute pTimePeriodAttributes;
				TransitStop pTransitStop;

				double dEstimatedBoardings = 0d;
				float sngDistanceRatio = 0f;

				double pRouteLength = pRoute.Segments.ReturnTotalLength();

				var pDic = pTBESTEquation.GetReplicationTripReductionDictionary(pRouteGroup, pScenario, pRoute, iTimePeriod);
				var pDicLogReduction = pTBESTEquation.GetReplicationDictionary(pRouteGroup, pScenario, pRoute, iTimePeriod);
				double pRepReduction = 0d;
				int pRepCount = 0;

				for (int i = 0, loopTo = pRoute.PatternTransitStops.List.Count - 1; i <= loopTo; i++)
				{

					pTransitStop = pRoute.PatternTransitStops.List[i];

					sngDistanceRatio = Convert.ToSingle(pRouteLength > 0d ? (double)pTransitStop.StopDistance / pRouteLength : 0);

					dEstimatedBoardings = 0d;
					pTBESTEquation.ResetforNewStop(pTransitStop);
					pTimePeriodAttributes = pTransitStop.TimePeriods.List[TPindex];

					if (pTransitStop.RouteIndex == pRoute.PatternTransitStops.List.Count - 1 & pTransitStop.Interlined == false | pTimePeriodAttributes.Frequency == 0 | pTimePeriodAttributes.Headway == 0)
						goto nextstop;

					if (boolDirect == false)
					{
						if (!(pTimePeriodAttributes.IncomingTransferStops is not null && pTimePeriodAttributes.IncomingTransferStops.List.Count > 0))
						{
							await oWrite.WriteLineAsync(pTransitStop.StopID + "," + intTP + "," + "No Stops that are transferable,0");
							continue;
						}
					}
					pRepReduction = pDic[pTransitStop.StopName];
					pRepCount = pDicLogReduction[pTransitStop.StopName];
					await pTBESTEquation.AddLinearValue("BUS-ONE", "1");
					await pTBESTEquation.AddDummyVariableValue("Route Type", pTBESTEquation.RouteTypeCoeff);
					await pTBESTEquation.AddDummyVariableValue("Technology", pTBESTEquation.TechnologyCoeff);

					if (pTransitStop.Amenities is not null)
						await pTBESTEquation.AddSpecialGeneratorstoEquation(pTransitStop);

					await pTBESTEquation.AddLogSumValue("BUS-LNARRIVALS", pTimePeriodAttributes.Frequency.ToString());
					await pTBESTEquation.AddLinearValue("BUS-SHAREDIS", sngDistanceRatio.ToString());

					double dO2TOT = 0d;
					double dO3Emp = 0d;
					double dO4Emp = 0d;
					double dO6Emp = 0d;
					double dO3Pop = 0d;
					double dO4Pop = 0d;
					double dO6Pop = 0d;
					double dO3Serv = 0d;
					double dO4Serv = 0d;
					double dO6Serv = 0d;
					double dO5EmpTotal = 0d;
					double dO5Pop = 0d;
					double dO2Pop = 0d;
					double dO2EmpTotal = 0d;

					double O1Estimated = 0d;
					NetworkOpportunitySEValue pOvalAtts;
					if (pTimePeriodAttributes.OValues is not null)
					{
						for (int a = 0, loopTo1 = pTimePeriodAttributes.OValues.List.Count - 1; a <= loopTo1; a++)
						{
							pOvalAtts = pTimePeriodAttributes.OValues.List[a];
							switch (pOvalAtts.OValue)
							{
								case 1:
									{
										if (boolDirect == false & pOvalAtts.Population == 0)
										{
											await oWrite.WriteLineAsync(pTransitStop.StopID + "," + intTP + "," + "No O1 riders available,0");
											goto nextstop;
										}
										await pTBESTEquation.AddLogSumValue("BUS-LNO1", pOvalAtts.Population.ToString());
										await pTBESTEquation.AddLinearValue("BUS-O1", pOvalAtts.Population.ToString());
										O1Estimated = pOvalAtts.Population;
										break;
									}
								case 2:
									{

										dO2TOT = pOvalAtts.Population + pOvalAtts.EmployCommercial + pOvalAtts.EmployIndustrial + pOvalAtts.EmployService;
										dO2Pop = pOvalAtts.Population;
										dO2EmpTotal = pOvalAtts.EmployCommercial + pOvalAtts.EmployIndustrial + pOvalAtts.EmployService;

										if (boolDirect & dO2TOT == 0d)
										{
											await oWrite.WriteLineAsync(pTransitStop.StopID + "," + intTP + "," + "No destination accessibility,0");
											goto nextstop;
										}

										await pTBESTEquation.AddLogSumValue("BUS-LNO2POP", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, pOvalAtts.Population).ToString());
										await pTBESTEquation.AddLogSumValue("BUS-LNO2EMP", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, pOvalAtts.EmployCommercial + pOvalAtts.EmployIndustrial + pOvalAtts.EmployService).ToString());
										await pTBESTEquation.AddLogSumValue("BUS-LNO2SERV", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, pOvalAtts.EmployService).ToString());
										await pTBESTEquation.AddLogSumValue("BUS-LNO2COMM", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, pOvalAtts.EmployCommercial).ToString());

										if (dO2TOT > 0d)
											await pTBESTEquation.AddLogSumValue("BUS-LNO2PE", dO2TOT.ToString());

										await pTBESTEquation.AddLinearValue("BUS-O2EMP", (pOvalAtts.EmployCommercial + pOvalAtts.EmployIndustrial + pOvalAtts.EmployService).ToString());
										break;
									}
								case 3:
									{
										dO3Emp = pOvalAtts.EmployCommercial + pOvalAtts.EmployIndustrial + pOvalAtts.EmployService;
										dO3Pop = pOvalAtts.Population;
										dO3Serv = pOvalAtts.EmployService;

										await pTBESTEquation.AddLinearValue("BUS-O3POP", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, pOvalAtts.Population).ToString());
										await pTBESTEquation.AddLinearValue("BUS-O3EMP", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, dO3Emp).ToString());
										await pTBESTEquation.AddLogSumValue("BUS-LNO3SERV", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, pOvalAtts.EmployService).ToString());
										await pTBESTEquation.AddLogSumValue("BUS-LNO3POP", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, pOvalAtts.Population).ToString());
										break;
									}

								case 4:
									{
										dO4Emp = pOvalAtts.EmployCommercial + pOvalAtts.EmployIndustrial + pOvalAtts.EmployService;
										dO4Pop = pOvalAtts.Population;
										dO4Serv = pOvalAtts.EmployService;

										await pTBESTEquation.AddLinearValue("BUS-O4POP", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, pOvalAtts.Population).ToString());
										await pTBESTEquation.AddLinearValue("BUS-O4EMP", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, dO4Emp).ToString());
										await pTBESTEquation.AddLinearValue("BUS-O4TOTAL", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, dO4Emp + dO4Pop).ToString());
										break;
									}
								case 5:
									{
										await pTBESTEquation.AddLinearValue("BUS-O5POP", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, pOvalAtts.Population).ToString());
										dO5Pop = pOvalAtts.Population;
										dO5EmpTotal = pOvalAtts.EmployCommercial + pOvalAtts.EmployIndustrial + pOvalAtts.EmployService;
										break;
									}
								case 6:
									{
										dO6Pop = pOvalAtts.Population;
										dO6Emp = pOvalAtts.EmployCommercial + pOvalAtts.EmployIndustrial + pOvalAtts.EmployService;
										dO6Serv = pOvalAtts.EmployService;
										await pTBESTEquation.AddLinearValue("BUS-O6POP", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, pOvalAtts.Population).ToString());
										break;
									}
								case 10:
									{
										await pTBESTEquation.AddLinearValue("RAIL-O2PNREMP", pOvalAtts.Population.ToString());
										break;
									}
							}
						}
						await pTBESTEquation.AddLinearValue("BUS-O3O4O6", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, dO3Emp + dO4Emp - dO6Emp).ToString());
						await pTBESTEquation.AddLinearValue("BUS-O3O4O6POP", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, dO3Pop + dO4Pop - dO6Pop).ToString());
						await pTBESTEquation.AddLogSumValue("BUS-LNO3O4O6SERV", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, dO3Serv + dO4Serv - dO6Serv).ToString());
						await pTBESTEquation.AddLogSumValue("BUS-LNO3O4O6POP", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, dO3Pop + dO4Pop - dO6Pop).ToString());
						await pTBESTEquation.AddLogSumValue("BUS-LOGO2O5", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, dO2Pop - dO5Pop + (dO2EmpTotal - dO5EmpTotal)).ToString());
						await pTBESTEquation.AddLinearValue("BUS-O4O6TOTAL", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, dO4Pop - dO6Pop + (dO4Emp - dO6Emp)).ToString());

					}

					float sngRatioOriginShareReduction = (float)(1d - pTransitStop.PopEmpIndicators.DestinationOverlapPCT / 100d);

					float[] intTotalParcelTrips = TBESTRidershipForecasting.InitiateModel.mParcelOvaluesTotalList[pTransitStop.StopIndex - 1];
					var pParelItemTotals = TBESTRidershipForecasting.InitiateModel.mParcelItemsTotalList[pTransitStop.StopIndex - 1];
					await pTBESTEquation.AddLogSumValue("LogO2TRIPS_NOCOMP", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, Convert.ToDouble(((double)intTotalParcelTrips[1]).ToString())).ToString());
					await pTBESTEquation.AddLogSumValue("LOG_O4TRIPS", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, Convert.ToDouble((intTotalParcelTrips[3] + 0.00001d).ToString())).ToString());
					await pTBESTEquation.AddLogSumValue("LOG_O2TRIPSTOTAL", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, Convert.ToDouble(((double)(intTotalParcelTrips[1] + intTotalParcelTrips[4])).ToString())).ToString());
					await pTBESTEquation.AddLinearValue("O2TRIPSTOTAL", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, Convert.ToDouble(((double)(intTotalParcelTrips[1] + intTotalParcelTrips[4])).ToString())).ToString());
					await pTBESTEquation.AddLogSumValue("BUS-LOGO6O4TRIPS", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, Convert.ToDouble(((double)(intTotalParcelTrips[3] - intTotalParcelTrips[5]) + 0.00001d).ToString())).ToString());
					await pTBESTEquation.AddLinearValue("O2TRIPS", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, Convert.ToDouble(intTotalParcelTrips[1].ToString())).ToString());
					await pTBESTEquation.AddLinearValue("O4TRIPS", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, Convert.ToDouble(intTotalParcelTrips[3].ToString())).ToString());
					await pTBESTEquation.AddLogSumValue("LOG_BUFFERTRIPS", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, (double)(pParelItemTotals.Trips * sngRatioOriginShareReduction)).ToString());
					await pTBESTEquation.AddLinearValue("DW_DENSITY", Convert.ToString(pParelItemTotals.LandArea == 0L ? 0 : pParelItemTotals.DwellingUnits / pParelItemTotals.LandArea));

					var pPopEmp = pTransitStop.PopEmpIndicators;

					if (pPopEmp is not null)
					{
						await pTBESTEquation.AddLinearValue("BUS-TOTALPOP", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, Convert.ToDouble(pPopEmp.TotalPopulation.ToString())).ToString());
						await pTBESTEquation.AddLinearValue("BUS-TOTALHH", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, Convert.ToDouble(pPopEmp.TotalHouseHolds.ToString())).ToString());
						await pTBESTEquation.AddLinearValue("BUS-SHBLACK", pPopEmp.Black.ToString());
						await pTBESTEquation.AddLinearValue("BUS-SHHISPANIC", pPopEmp.Hispanic.ToString());
						await pTBESTEquation.AddLinearValue("BUS-SHPOPLT16", pPopEmp.LT16.ToString());
						await pTBESTEquation.AddLinearValue("BUS-SHARE_MU", pPopEmp.HHMultiFamilyDU.ToString());
						await pTBESTEquation.AddLinearValue("BUS-SHFORE", pPopEmp.Foreign.ToString());
						await pTBESTEquation.AddLinearValue("BUS-SHHHCHIL", pPopEmp.ShareHHwChildren.ToString());
						await pTBESTEquation.AddLinearValue("BUS-SHFEMALE", pPopEmp.Female.ToString());
						await pTBESTEquation.AddLinearValue("BUS-SHWORKERS", pPopEmp.Worker.ToString());

						if (pPopEmp.AvgHHIncome > 0)
						{
							await pTBESTEquation.AddLogSumValue("BUS-LNHHINC", pPopEmp.AvgHHIncome.ToString());
						}

						double dTotEmp = (double)(pPopEmp.CommercialEmployment + pPopEmp.IndustrialEmployment + pPopEmp.ServiceEmployment);

						await pTBESTEquation.AddLinearValue("BUS-AVGHHINC", pPopEmp.AvgHHIncome.ToString());
						await pTBESTEquation.AddLinearValue("BUS-ZEROVEHHH", pPopEmp.ZeroVehHH.ToString());
						await pTBESTEquation.AddLinearValue("BUS-SERVE", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, Convert.ToDouble(pPopEmp.ServiceEmployment.ToString())).ToString());
						await pTBESTEquation.AddLinearValue("SHARE_SERVEMP", Convert.ToString(dTotEmp == 0d ? 0d : (double)pPopEmp.ServiceEmployment / dTotEmp));
						await pTBESTEquation.AddLinearValue("SHARE_COMMEMP", Convert.ToString(dTotEmp == 0d ? 0 : (double)pPopEmp.CommercialEmployment / dTotEmp));
						await pTBESTEquation.AddLinearValue("BUS-COMMERCIAL", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, Convert.ToDouble(pPopEmp.CommercialEmployment.ToString())).ToString());
						await pTBESTEquation.AddLinearValue("BUS-TOTEMP", pTBESTEquation.ApplyReplication(iTimePeriod, pRepCount, pRepReduction, Convert.ToDouble(dTotEmp.ToString())).ToString());

						if (boolDirect & dTotEmp + (double)pPopEmp.TotalPopulation == 0d)
						{
							await oWrite.WriteLineAsync(pTransitStop.StopID + "," + intTP + "," + "No population or employment in origin buffer,0");
							continue;
						}

						await pTBESTEquation.AddLogSumValue("LOG_BUFFERPE", ((dTotEmp + (double)pPopEmp.TotalPopulation) * (double)sngRatioOriginShareReduction).ToString());
					}


					dEstimatedBoardings = await pTBESTEquation.CalculateEstimatedBoardings(pTimePeriodAttributes);


				nextstop:
					;


					if (pRouteGroup.RouteType == TransitRouteType.BRT | pRouteGroup.RouteType == TransitRouteType.CirculatorBRT)
						dEstimatedBoardings = dEstimatedBoardings * (double)pRouteGroup.BRTAdjustmentFactor;
					if (pRouteGroup.TechnologyType == TransitModeType.LightRail)
						dEstimatedBoardings = dEstimatedBoardings * pModelParameters.LRTAdjustment;
					if (pRouteGroup.TechnologyType == TransitModeType.CommuterRail)
					{
						dEstimatedBoardings = dEstimatedBoardings * pModelParameters.CRAdjustment;
					}

					if (pRouteGroup.TechnologyType == TransitModeType.HeavyRail)
					{
						dEstimatedBoardings = dEstimatedBoardings * pModelParameters.HRAdjustment;
					}

					if (boolDirect)
					{
						int dArrivals = pTransitStop.TimePeriods.List[TPindex].Frequency;
						int lngArrivalCap = dArrivals * pRouteGroup.VehicleSeats;

						if (dEstimatedBoardings > lngArrivalCap)
						{
							dEstimatedBoardings = lngArrivalCap;
							await oWrite.WriteLineAsync(pTransitStop.StopID + "," + intTP + "," + "Constrained Boardings," + Math.Round(dEstimatedBoardings, 2).ToString());
						}

						pTimePeriodAttributes.DirectBoardings = (float)dEstimatedBoardings;
					}
					else
					{
						pTimePeriodAttributes.TransferBoardings = (float)dEstimatedBoardings;
					}

				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error calculating boardings.  To view error details, navigate to the TBEST log file at File->Settings->TBEST Log. ", "TBEST Model Run", MessageBoxButtons.OK, MessageBoxIcon.Information);

				ErrorHandling.ErrHandlerMsg(ex);
				CalculateBoardingsRet = false;
			}

			return CalculateBoardingsRet;
		}

		public static async Task<bool> PopulateBoardings(SystemTimePeriod iTimePeriod, System.IO.StreamWriter owrite, TBESTScenario mModelScenario, ModelParameters mScenarioNetworkParamters)
		{
			bool PopulateBoardingsRet = true;

			float ddirect;
			float dTransfer;
			float dBoardings;
			int iTransferOpps;
			int dArrivals;
			int intInboundTransfers;
			int lngArrivalCap;
			double dbldirectproportion;
			string intTP = ((int)iTimePeriod + 1).ToString();
			int TPindex = (int)iTimePeriod;


			try
			{
				TransitPattern pRoute;
				TransitRoute pRouteGroup;
				TransitStop pTS;
				float sngAdjustmentFactor = 0f;
				int nCapacity = 0;
				int intCurrentLoad = 0;
				for (int i = 0, loopTo = mModelScenario.RoutePatterns.List.Count - 1; i <= loopTo; i++)
				{
					pRoute = mModelScenario.RoutePatterns.List[i];

					pRouteGroup = mModelScenario.Routes.GetTransitRoute(pRoute.ParentTransitRouteID);

					nCapacity = pRouteGroup.VehicleSeats;
					for (int a = 0, loopTo1 = pRoute.PatternTransitStops.List.Count - 1; a <= loopTo1; a++)
					{
						pTS = pRoute.PatternTransitStops.List[a];

						iTransferOpps = 0;
						if (pTS.TimePeriods.List[TPindex].TransferStops is not null)
							iTransferOpps = pTS.TimePeriods.List[TPindex].TransferStops.List.Count;

						intInboundTransfers = 0;
						if (pTS.TimePeriods.List[TPindex].IncomingTransferStops is not null)
							intInboundTransfers = pTS.TimePeriods.List[TPindex].IncomingTransferStops.List.Count;

						dArrivals = pTS.TimePeriods.List[TPindex].Frequency;
						ddirect = pTS.TimePeriods.List[TPindex].DirectBoardings;
						dTransfer = pTS.TimePeriods.List[TPindex].TransferBoardings;
						dBoardings = ddirect + dTransfer;

						if (mScenarioNetworkParamters.ApplyCapacityConstraint)
						{

							intCurrentLoad = pTS.TimePeriods.List[TPindex].OnBoardLoad;
							lngArrivalCap = dArrivals * nCapacity;
							lngArrivalCap = lngArrivalCap - intCurrentLoad;

							if (lngArrivalCap <= 0)
							{
								ddirect = 0f;
								dTransfer = 0f;
								dBoardings = 0f;
							}
							else if (dBoardings > lngArrivalCap)
							{
								dbldirectproportion = (double)(ddirect / dBoardings);
								ddirect = (float)(dbldirectproportion * lngArrivalCap);
								dTransfer = (float)((1d - dbldirectproportion) * lngArrivalCap);
								dBoardings = lngArrivalCap;
							}
						}

						sngAdjustmentFactor = 1f;
						if (pTS.AdjustmentFactors is not null)
						{
							switch (iTimePeriod)
							{
								case SystemTimePeriod.Saturday:
									{
										sngAdjustmentFactor = pTS.AdjustmentFactors.SaturdayFactor;
										break;
									}
								case SystemTimePeriod.Sunday:
									{
										sngAdjustmentFactor = pTS.AdjustmentFactors.SundayFactor;
										break;
									}

								default:
									{
										sngAdjustmentFactor = pTS.AdjustmentFactors.WeekdayFactor;
										break;
									}
							}
						}

						ddirect *= sngAdjustmentFactor;
						dTransfer *= sngAdjustmentFactor;
						dBoardings *= sngAdjustmentFactor;

						await owrite.WriteLineAsync(mModelScenario.ScenarioID + "," + pTS.StopID + "," + intTP + "," + dBoardings + "," + dTransfer + "," + ddirect + "," + dArrivals + "," + iTransferOpps + "," + intInboundTransfers);

					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error Populating Boardings.  To view error details, navigate to the TBEST log file at File->Settings->TBEST Log. ", "TBEST Model Run", MessageBoxButtons.OK, MessageBoxIcon.Information);

				ErrorHandling.ErrHandlerMsg(ex);
				PopulateBoardingsRet = false;
			}

			return PopulateBoardingsRet;

		}

	}

	internal class TBESTModelEquationHelper : MarshalByRefObject
	{
		private System.IO.StreamWriter oWrite;
		private TransitStop mTransitStop;
		private int mStopID = 0;
		private int mTimePeriod = 0;
		private CoefficientList mCoeffList;
		private double dCummulativeLinearBoardingsValue = 0d;
		private TransitRoute mRouteGroup;
		private ModelCoefficient mTechCoeff = null;
		private ModelCoefficient mRouteTypeCoeff = null;
		private bool mboolPerArrivalEstimation = false;
		private bool mDirectEquation = true;
		public bool PerArrivalEstimation
		{
			set
			{
				mboolPerArrivalEstimation = value;
			}
		}
		public bool IsDirectEquation
		{
			get
			{
				return mDirectEquation;
			}
			set
			{
				mDirectEquation = value;
			}
		}
		public TransitRoute RouteGroup
		{
			get
			{
				return mRouteGroup;
			}
			set
			{
				mRouteGroup = value;
			}
		}

		public double CummulativeLinearBoardingsValue
		{
			get
			{
				return dCummulativeLinearBoardingsValue;
			}
			set
			{
				dCummulativeLinearBoardingsValue = value;
			}
		}
		public CoefficientList Coefficients
		{
			get
			{
				return mCoeffList;
			}
			set
			{
				mCoeffList = value;
			}
		}

		public int TimePeriod
		{
			get
			{
				return mTimePeriod;
			}
			set
			{
				mTimePeriod = value;
			}
		}

		public System.IO.StreamWriter HelperStreamWriter
		{
			get
			{
				return oWrite;
			}
			set
			{
				oWrite = value;
			}
		}
		public void ResetforNewStop(TransitStop pTransitStop)
		{
			dCummulativeLinearBoardingsValue = 0d;
			mTransitStop = pTransitStop;
			mStopID = pTransitStop.StopID;
		}
		public ModelCoefficient TechnologyCoeff
		{

			get
			{

				if (mTechCoeff is null)
				{
					{

						switch (mRouteGroup.TechnologyType)
						{
							case TransitModeType.Bus:
								{
									mTechCoeff = mCoeffList.ReturnCoefficient("Bus");
									break;
								}
							case TransitModeType.HeavyRail:
								{
									mTechCoeff = mCoeffList.ReturnCoefficient("HeavyRail");
									break;
								}
							case TransitModeType.PeopleMover:
								{
									mTechCoeff = mCoeffList.ReturnCoefficient("PEPMOVER");
									break;
								}
							case TransitModeType.Streetcar:
								{
									mTechCoeff = mCoeffList.ReturnCoefficient("STREETCAR");
									break;
								}
							case TransitModeType.Other:
								{
									mTechCoeff = mCoeffList.ReturnCoefficient("OtherTechnology");
									break;
								}
							case TransitModeType.CommuterRail:
								{
									mTechCoeff = mCoeffList.ReturnCoefficient("CommuterRail");
									break;
								}
							case TransitModeType.LightRail:
								{
									mTechCoeff = mCoeffList.ReturnCoefficient("LightRail");
									break;
								}
						}
					}
				}

				return mTechCoeff;
			}
		}
		public ModelCoefficient RouteTypeCoeff
		{

			get
			{

				if (mRouteTypeCoeff is null)
				{
					{

						switch (mRouteGroup.RouteType)
						{
							case TransitRouteType.Radial:
								{
									mRouteTypeCoeff = mCoeffList.ReturnCoefficient("RADIAL");
									break;
								}
							case TransitRouteType.Circulator:
								{
									mRouteTypeCoeff = mCoeffList.ReturnCoefficient("Circulator");
									break;
								}
							case TransitRouteType.Crosstown:
								{
									mRouteTypeCoeff = mCoeffList.ReturnCoefficient("Crosstown");
									break;
								}
							case TransitRouteType.Express:
								{
									mRouteTypeCoeff = mCoeffList.ReturnCoefficient("Express");
									break;
								}
							case TransitRouteType.BRT:
								{
									mRouteTypeCoeff = mCoeffList.ReturnCoefficient("BRT");
									break;
								}
							case TransitRouteType.Rapid:
								{
									mRouteTypeCoeff = mCoeffList.ReturnCoefficient("Rapid");
									break;
								}
							case TransitRouteType.Ski:
								{
									mRouteTypeCoeff = mCoeffList.ReturnCoefficient("Ski");
									break;
								}
							case TransitRouteType.CommunityConnector:
								{
									mRouteTypeCoeff = mCoeffList.ReturnCoefficient("Community");
									break;
								}
							case TransitRouteType.CirculatorBRT:
								{
									mRouteTypeCoeff = mCoeffList.ReturnCoefficient("CirculatorBRT");
									break;
								}
							case TransitRouteType.Flex:
								{
									mRouteTypeCoeff = mCoeffList.ReturnCoefficient("Flex");
									break;
								}
						}

					}
				}

				return mRouteTypeCoeff;
			}
		}
		public async Task AddSpecialGeneratorstoEquation(TransitStop pTransitStop)
		{

			for (int a = 0, loopTo = pTransitStop.Amenities.List.Count - 1; a <= loopTo; a++)
			{
				switch (pTransitStop.Amenities.List[a].AmenityID)
				{
					case 1:
						{
							if (mRouteGroup.TechnologyType == TransitModeType.Bus)
								await AddSpecialGeneratorValue("University");
							break;
						}
					case 2:
						{
							if (mRouteGroup.TechnologyType == TransitModeType.Bus)
								await AddSpecialGeneratorValue("MILITARY");
							break;
						}
					case 3:
						{
							if (mRouteGroup.TechnologyType == TransitModeType.Bus)
								await AddSpecialGeneratorValue("MALLS");
							break;
						}
					case 4:
						{
							if (mRouteGroup.TechnologyType == TransitModeType.Bus)
								await AddSpecialGeneratorValue("EventCenter");
							break;
						}
					case 5:
						{
							if (mRouteGroup.RouteType == TransitRouteType.Rapid)
								await AddLinearValue("RAPID-ParkingSpaces", pTransitStop.ParknRideSpaces.ToString());
							if (mRouteGroup.RouteType == TransitRouteType.Express)
								await AddLinearValue("EXPRESS-ParkingSpaces", pTransitStop.ParknRideSpaces.ToString());
							await AddSpecialGeneratorValue("ParknRide");
							break;
						}
					case 6:
						{
							if (mRouteGroup.TechnologyType == TransitModeType.Bus)
								await AddSpecialGeneratorValue("AIRPORT");
							else
								await AddSpecialGeneratorValue("RAIL-AIRPORT");
							break;
						}
					case 7:
						{
							if (mRouteGroup.TechnologyType == TransitModeType.Bus)
								await AddSpecialGeneratorValue("RECPARK");
							break;
						}
					case 8:
						{
							break;
						}

				}

			}
		}
		public async Task AddDummyVariableValue(string strDummaryVariableGroup, ModelCoefficient pcoeff)
		{
			try
			{

				double dblDummyCoeffValue = pcoeff is null ? 0d : pcoeff.CoefficientTimePeriods.List[mTimePeriod].CoeffValue;
				if (dblDummyCoeffValue.ToString() != "0")
				{
					dCummulativeLinearBoardingsValue += dblDummyCoeffValue;
					await oWrite.WriteLineAsync(mStopID.ToString() + "," + (mTimePeriod + 1).ToString() + "," + strDummaryVariableGroup + "," + pcoeff.Description + "," + dblDummyCoeffValue.ToString() + "," + Math.Round(dCummulativeLinearBoardingsValue, 2).ToString());
				}
			}
			catch (Exception)
			{

			}
		}
		public async Task AddLogSumValue(string strCoeffVariableKey, string strVariableValue)
		{
			if (mCoeffList.ReturnCoefficient(strCoeffVariableKey) is null || mCoeffList.ReturnCoefficient(strCoeffVariableKey).CoefficientTimePeriods is null)
			{
				//MessageBox.Show("Unable to locate the following coefficient variable key: " + strCoeffVariableKey + ".  Please revise the coefficient key values.", MsgBoxStyle.Critical, "TBEST Model Equation");

			}
			else
			{
				double dblCoeff = mCoeffList.ReturnCoefficient(strCoeffVariableKey).CoefficientTimePeriods.List[mTimePeriod].CoeffValue;

				if (dblCoeff.ToString() != "0" & Convert.ToDouble(strVariableValue) > 0d)
				{
					dCummulativeLinearBoardingsValue = dCummulativeLinearBoardingsValue + Math.Log(Convert.ToDouble(strVariableValue)) * dblCoeff;
					await oWrite.WriteLineAsync(mStopID.ToString() + "," + (mTimePeriod + 1).ToString() + "," + mCoeffList.ReturnCoefficient(strCoeffVariableKey).Description + "," + strVariableValue + "," + dblCoeff.ToString() + "," + Math.Round(dCummulativeLinearBoardingsValue, 2).ToString());
				}
			}

		}
		public async Task AddLinearValue(string strCoeffVariableKey, string strVariableValue)
		{
			if (mCoeffList.ReturnCoefficient(strCoeffVariableKey) is null || mCoeffList.ReturnCoefficient(strCoeffVariableKey).CoefficientTimePeriods is null)
			{
				//MessageBox.Show("Unable to locate the following coefficient variable key: " + strCoeffVariableKey + ".  Please revise the coefficient key values.", MsgBoxStyle.Critical, "TBEST Model Equation");

			}
			else
			{
				double dblCoeff = mCoeffList.ReturnCoefficient(strCoeffVariableKey).CoefficientTimePeriods.List[mTimePeriod].CoeffValue;
				if (dblCoeff.ToString() != "0")
				{
					dCummulativeLinearBoardingsValue = dCummulativeLinearBoardingsValue + Convert.ToDouble(strVariableValue) * dblCoeff;
					await oWrite.WriteLineAsync(mStopID.ToString() + "," + (mTimePeriod + 1).ToString() + "," + mCoeffList.ReturnCoefficient(strCoeffVariableKey).Description + "," + strVariableValue + "," + dblCoeff.ToString() + "," + Math.Round(dCummulativeLinearBoardingsValue, 2).ToString());
				}
			}

		}
		public async Task AddSpecialGeneratorValue(string strCoeffVariableKey)
		{
			if (mCoeffList.ReturnCoefficient(strCoeffVariableKey) is null || mCoeffList.ReturnCoefficient(strCoeffVariableKey).CoefficientTimePeriods is null)
			{
				//MessageBox.Show("Unable to locate the following coefficient variable key: " + strCoeffVariableKey + ".  Please revise the coefficient key values.", MsgBoxStyle.Critical, "TBEST Model Equation");

			}
			else
			{
				double dblCoeff = mCoeffList.ReturnCoefficient(strCoeffVariableKey).CoefficientTimePeriods.List[mTimePeriod].CoeffValue;
				if (dblCoeff.ToString() != "0")
				{
					dCummulativeLinearBoardingsValue += dblCoeff;
					await oWrite.WriteLineAsync(mStopID.ToString() + "," + (mTimePeriod + 1).ToString() + "," + mCoeffList.ReturnCoefficient(strCoeffVariableKey).Description + ",1," + dblCoeff.ToString() + "," + Math.Round(dCummulativeLinearBoardingsValue, 2).ToString());
				}
			}

		}
		public async Task<double> CalculateEstimatedBoardings(StopNetworkTimePeriodAttribute pTimePeriodAttributes)
		{

			double dEstimatedBoardings = Math.Exp(dCummulativeLinearBoardingsValue);
			if (mboolPerArrivalEstimation)
				dEstimatedBoardings *= pTimePeriodAttributes.Frequency;

			if (mTransitStop.TimePoint.Trim() == "T")
			{
				dEstimatedBoardings = 0d;
				await oWrite.WriteLineAsync(mStopID + "," + (mTimePeriod + 1).ToString() + "," + "TimePoint = T. Stop only has alightings. 0 Boardings");
			}
			else
			{
				await oWrite.WriteLineAsync(mStopID + "," + (mTimePeriod + 1).ToString() + "," + "Boardings," + Math.Round(dEstimatedBoardings, 2).ToString());
			}
			return dEstimatedBoardings;
		}
		public Dictionary<string, int> GetReplicationDictionary(TransitRoute pRouteGroup, TBESTScenario pScenario, TransitPattern pTargetRoute, SystemTimePeriod iTimePeriod)
		{
			TransitPattern nRoute;
			var pZeroCollection = pRouteGroup.Get_DirRoutes(0);
			var pOneCollection = pRouteGroup.Get_DirRoutes(1);
			var pSearchList = pOneCollection;
			foreach (int z in pZeroCollection)
			{
				nRoute = pScenario.RoutePatterns.List[z];
				if (nRoute.RouteNumber == pTargetRoute.RouteNumber & nRoute.Direction == pTargetRoute.Direction)
				{
					pSearchList = pZeroCollection;
					break;
				}
			}
			var pDic = new Dictionary<string, int>();
			int zCount;
			List<string> dontcounttwiceononeRoute;
			int indexTP = (int)iTimePeriod;
			foreach (int z in pSearchList)
			{
				nRoute = pScenario.RoutePatterns.List[z];
				if (nRoute.TimePeriodHeadway[indexTP] == 0)
					continue;
				dontcounttwiceononeRoute = new List<string>();
				foreach (TransitStop pStop in nRoute.PatternTransitStops.List)
				{
					if (dontcounttwiceononeRoute.Contains(pStop.StopName))
						continue;
					dontcounttwiceononeRoute.Add(pStop.StopName);
					if (!pDic.ContainsKey(pStop.StopName))
					{
						pDic.Add(pStop.StopName, 1);
					}
					else
					{
						zCount = pDic[pStop.StopName];
						pDic[pStop.StopName] = zCount + 1;
					}
				}
			}
			return pDic;

		}
		public Dictionary<string, double> GetReplicationTripReductionDictionary(TransitRoute pRouteGroup, TBESTScenario pScenario, TransitPattern pTargetRoute, SystemTimePeriod iTimePeriod)
		{

			TransitPattern nRoute;
			Dictionary<string, double> outDic = [];
			int tpindex = (int)iTimePeriod;
			int pProuteTrips = pTargetRoute.TimePeriodTrips[tpindex];

			try
			{
				if (pProuteTrips == 0)
					return outDic;

				var pZeroCollection = pRouteGroup.Get_DirRoutes(0);
				var pOneCollection = pRouteGroup.Get_DirRoutes(1);
				var pSearchList = pOneCollection;
				foreach (int z in pZeroCollection)
				{
					nRoute = pScenario.RoutePatterns.List[z];
					if (nRoute.RouteNumber == pTargetRoute.RouteNumber & nRoute.Direction == pTargetRoute.Direction)
					{
						pSearchList = pZeroCollection;
						break;
					}
				}
				var pDic = new Dictionary<string, int>();
				int zCount;
				var dontcounttwiceononeRoute = new List<string>();
				string strStopName = "";
				foreach (int z in pSearchList)
				{
					nRoute = pScenario.RoutePatterns.List[z];
					if (nRoute.TimePeriodHeadway[tpindex] == 0)
						continue;
					dontcounttwiceononeRoute = new List<string>();
					foreach (TransitStop pStop in nRoute.PatternTransitStops.List)
					{
						strStopName = pStop.StopName;
						if (dontcounttwiceononeRoute.Contains(strStopName))
							continue;
						dontcounttwiceononeRoute.Add(strStopName);
						if (!pDic.ContainsKey(strStopName))
						{
							pDic.Add(strStopName, pStop.TimePeriods.List[tpindex].Frequency);
						}
						else
						{
							zCount = pDic[strStopName];
							pDic[strStopName] = zCount + pStop.TimePeriods.List[tpindex].Frequency;
						}
					}
				}


				double pTripProportion = 0d;
				int pTotalTrips = 0;

				dontcounttwiceononeRoute = new List<string>();
				foreach (TransitStop pStop in pTargetRoute.PatternTransitStops.List)
				{
					if (dontcounttwiceononeRoute.Contains(pStop.StopName))
						continue;
					dontcounttwiceononeRoute.Add(pStop.StopName);

					pTotalTrips = pDic[pStop.StopName];
					pTripProportion = pProuteTrips / (double)pTotalTrips;
					outDic.Add(pStop.StopName, pTripProportion);

				}
			}
			catch (Exception)
			{
			}

			return outDic;
		}
		public double ApplyReplication(SystemTimePeriod intTimePeriod, int rep, double inReductionPercent, double inValue, bool boolHeadway = false)
		{
			if (rep == 1)
				return inValue;

			double outValue = inValue * inReductionPercent;
			return outValue;
		}
		public TBESTModelEquationHelper() : base()
		{
		}
	}
}