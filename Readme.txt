# RidershipEstimationModel
Within the equation, each variable value is multiplied by the model coefficient. The Coefficient Key defined in the Model Coefficient dialog for each variable is referenced in the equation to lookup the coefficient value. For example, to apply the coefficient for Share of Hispanic Population at a stop, the code executes the following statement:

 pTBESTEquation.AddLinearValue("BUS-SHHISPANIC",pPopEmp.Hispanic.ToString());
The “BUS-SHHISPANIC ” string is the key defined for Share of Hispanic Population in the Model Coefficient dialog. The pPopemp.Hispanic variable provides the share of Hispanic population for the subject stop. 